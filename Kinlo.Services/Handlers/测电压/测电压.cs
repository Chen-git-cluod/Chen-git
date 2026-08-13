namespace Kinlo.Services.Handlers;

[DeviceConnec(ProcessTypeEnum.测电压, [CommunicationEnum.None])] //指定工艺，可指定多个
public class TestVoltageHandler : ServiceHandlerBase
{
   public TestVoltageHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken) { }

   protected override async Task HandleCore(short plcValue)
   {
      if (!TryReadPlcData(out List<ReceivePlcDataModel> plcDatas))
         return;
      await Parallel.ForEachAsync(
         plcDatas,
         new ParallelOptions { MaxDegreeOfParallelism = Context.DataLength },
         async (plcData, _) =>
         {
            bool isSpotCheck = plcData.ID == -1; //是否为点检
            string logHeader = Context.ToProcessLogHeader(plcValue, plcData.ID);
            IBatMainModel? mainBattery = isSpotCheck switch
            {
               true => Activator.CreateInstance(_displayDataCollection.CompleteBatteryDatas.RuntimeBatteryType) as IBatMainModel, //点检
               _ => await _batteryCache.GetByIdAsync(plcData.ID, logHeader), //取缓存
            };
            if (mainBattery == null)
            {
               if (isSpotCheck)
                  "创建点检电池对象失败；".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               plcData.DataAddress.WritePlcResult(ResultTypeEnum.数据库找不到电池, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
               return;
            }
            logHeader = Context.ToProcessLogHeader(plcValue, plcData.ID, mainBattery.Barcode);

            var device = _devicesConfig.GetRunDevice(Context, plcData.Index);

            IBatVoltageTestModel voltage = (IBatVoltageTestModel)mainBattery!;
            if (device == null)
            {
               voltage.VoltageTestResult = ResultTypeEnum.未找到设备;
               AddDisplayData(mainBattery);
               ; //更新界面显示
               _isDeviceAlarm = true;
               $"找不到设备，不给PLC发结果及完成信号，请停机检查！;".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               return;
            }
            $"开始检测".LogProcess(logHeader);
            var deviceRs = device.ReadValue<float>(
               null,
               logHeader,
               new DeviceOperationOptions(retryCount: _parameterConfig.RunParameter.VoltageTestCount + 1)
            );

            if (deviceRs.Status != DeviceStatus.Success)
               $"采集数据失败！".LogProcess(logHeader, Log4NetLevelEnum.警告);
            else
               voltage.TestVoltageValue = (float)Math.Round(deviceRs.Value, 5);

            voltage.TestVoltageTime = DateTime.Now;
            voltage.TestVoltageIndex = (byte)plcValue;
            voltage.VoltageRange = $"{_parameterConfig.RunParameter.VoltageUpper}~{_parameterConfig.RunParameter.VoltageLower}";
            voltage.VoltageTestResult = deviceRs switch
            {
               var dr
                  when dr.Status == DeviceStatus.Success
                     && dr.Value >= _parameterConfig.RunParameter.VoltageLower
                     && dr.Value <= _parameterConfig.RunParameter.VoltageUpper => ResultTypeEnum.OK,
               _ => ResultTypeEnum.NG,
            };
            ;

            if (isSpotCheck) //点检
            {
               float checkValue = deviceRs.Value == null ? 0 : (float)deviceRs.Value;
               voltage.VoltageTestResult = Context.ProcessesType.RangeInspection(
                  plcValue,
                  _inspectionConfig,
                  deviceRs.Status == DeviceStatus.Success,
                  checkValue
               );
               OnSupplementaryElectrolyteToPlc(plcData, checkValue, logHeader, "[点检发送电压]");
               plcData.DataAddress.WritePlcResult(voltage.VoltageTestResult, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
               //  AddDisplayData(mainBattery);//更新界面显示
               return;
            }
            #region 上传MES
            if (voltage.VoltageTestResult != ResultTypeEnum.OK)
            {
               // mainBattery.MesOutputTime = DateTime.Now;
               await MesOutput(mainBattery, logHeader);
            }
            #endregion

            if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
            {
               voltage.VoltageTestResult = ResultTypeEnum.保存数据库失败;
            }

            AddDisplayData(mainBattery);
            plcData.DataAddress.WritePlcResult(voltage.VoltageTestResult, mainBattery.MesOutputStatus, _plc, _parameterConfig, logHeader);
         }
      );
   }
}
