namespace Kinlo.Services.Handlers;

[DeviceConnec(ProcessTypeEnum.回流扫码, [CommunicationEnum.None])]
public class ScanCodeReworkFlowHandler : ServiceHandlerBase
{
   public ScanCodeReworkFlowHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken) { }

   protected override async Task HandleCore(short plcValue)
   {
      var device = _devicesConfig.GetRunDevice(Context, Context.DeviceStartIndex);
      string logHeader1 = Context.ToProcessLogHeader(plcValue);
      if (device == null)
      {
         _isDeviceAlarm = true;
         $"找不到设备，不给PLC发结果及完成信号，请停机检查！;".LogProcess(logHeader1, Log4NetLevelEnum.错误, true);
         return;
      }

      var scanCodeResults = ScanBarcodeHelpre.ScanCode(
         device,
         _parameterConfig.RunParameter.RetryCountScanCode,
         Context.DataLength,
         _taskLogHeader,
         _parameterConfig.AdvancedConfig.BatteryBarcodeValidationRule
      );
      await Parallel.ForAsync(
         0,
         Context.DataLength,
         new ParallelOptions { MaxDegreeOfParallelism = Context.DataLength },
         async (i, _) =>
         {
            int lane = plcValue + i; //通道
            var resultPath = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[{i}]");
            var logHeader = Context.ToProcessLogHeader(lane);
            if (scanCodeResults.Length <= i)
            {
               $"扫码最大个数[{scanCodeResults.Length}] 小于设定个数[{i + 1}]!".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               resultPath.WritePlcResult(ResultTypeEnum.索引超界, ResultTypeEnum._, _plc, _parameterConfig, logHeader);
               return;
            }
            IBatMainModel GetNgBattery(int l, ResultTypeEnum reslut)
            {
               var bat = Activator.CreateInstance(_displayDataCollection.CompleteBatteryDatas.RuntimeBatteryType) as IBatMainModel;
               var batReworkScan = bat as IBatReworkScanModel;
               batReworkScan.ReworkScanTime = DateTime.Now;
               batReworkScan.ReworkScanIndex = (byte)l;
               batReworkScan.ReworkScanResult = reslut;
               return bat;
            }
            if (scanCodeResults[i].ScanStatus != ResultTypeEnum.OK)
            {
               var bat = GetNgBattery(lane, ResultTypeEnum.扫码失败);
               $"扫码失败!".LogProcess(logHeader, Log4NetLevelEnum.警告, true);
               resultPath.WritePlcResult(ResultTypeEnum.扫码失败, ResultTypeEnum._, _plc, _parameterConfig, logHeader);
               AddDisplayData(bat);
               return;
            }
            logHeader = Context.ToProcessLogHeader(lane, barcode: scanCodeResults[i].Code);
            var batMain = await _batteryCache.GetByBarcodeAsync(scanCodeResults[i].Code, logHeader);

            if (batMain == null)
            {
               var bat = GetNgBattery(lane, ResultTypeEnum.数据库找不到电池);
               AddDisplayData(bat);
               resultPath.WritePlcResult(ResultTypeEnum.数据库找不到电池, ResultTypeEnum._, _plc, _parameterConfig, logHeader);
               return;
            }

            logHeader = Context.ToProcessLogHeader(lane, batMain.Id, batMain.Barcode);
            var batReworkScan = (IBatReworkScanModel)batMain;
            batReworkScan.ReworkScanTime = DateTime.Now;
            batReworkScan.ReworkScanIndex = (byte)lane;
            batReworkScan.ReworkScanResult = ResultTypeEnum.OK;
            //更新指定列
            var upDic = new Dictionary<string, object>
            {
               [nameof(BatMainModel.Id)] = batMain.Id,
               [nameof(IBatReworkScanModel.ReworkScanTime)] = batReworkScan.ReworkScanTime,
               [nameof(IBatReworkScanModel.ReworkScanIndex)] = batReworkScan.ReworkScanIndex,
               [nameof(IBatReworkScanModel.ReworkScanResult)] = batReworkScan.ReworkScanResult,
            };

            if (!await _sugarDB.UpdateColumnsAsync(upDic, batMain.Id, batMain.Barcode, logHeader))
            {
               batReworkScan.ReworkScanResult = ResultTypeEnum.保存数据库失败;
            }
            AddDisplayData(batMain);
            var tag = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[{i}].Code", 0);
            for (int n = 1; n < 4; n++)
            {
               if (_plc.WriteValue(batMain.Barcode, tag, logHeader))
               {
                  $"第[{n}]次 地址[{JsonSerializer.Serialize(tag)}]写入条码[{batMain.Barcode}] 成功".LogProcess(logHeader);
                  break;
               }
               else
               {
                  $"第[{n}]次 地址[{JsonSerializer.Serialize(tag)}]写入条码[{batMain.Barcode}] 失败".LogProcess(logHeader);
               }
            }
            var idTag = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPCData[{i}].ID", 0);
            for (int n = 1; n < 4; n++)
            {
               if (_plc.WriteValue(batMain.Id, idTag, logHeader))
               {
                  $"第[{n}]次地址[{JsonSerializer.Serialize(idTag)}]写入ID[{batMain.Id}] 成功,条码：{batMain.Barcode};".LogProcess(
                     logHeader
                  );
                  break;
               }
               else
               {
                  $"第[{n}]次地址[{JsonSerializer.Serialize(idTag)}]写入ID[{batMain.Id}] 失败,条码：{batMain.Barcode};".LogProcess(
                     logHeader
                  );
               }
            }
            resultPath.WritePlcResult(batReworkScan.ReworkScanResult, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
         }
      );
   }
}
