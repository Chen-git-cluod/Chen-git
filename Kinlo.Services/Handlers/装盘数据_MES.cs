namespace Kinlo.Services.Handlers;

/// <summary>
///  装盘数据
/// </summary>
[DeviceConnec(ProcessTypeEnum.MES装盘数据, [CommunicationEnum.None])] //指定工艺，可指定多个
public class LoadTraySendMesHandler : ServiceHandlerBase
{
   public LoadTraySendMesHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken) { }

   protected override async Task HandleCore(short plcValue)
   {
      //if (!TryReadPlcData(out List<ReceivePlcDataModel> plcDatas))
      //    return;

      //var crateCode = _plc.ReadValue<string>(new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[0].Code"), _taskLogHeader);
      //$"读取Plc记忆，料框码：[{crateCode}], ID：{string.Join(";", plcDatas.Select(x => x.ID))}".LogProcess(_taskLogHeader);
      //var outBatteries = new List<IBatMainModel>();
      //foreach (var plcData in plcDatas)
      //{
      //    if (plcData.ID < 1) continue;
      //    string logHeader = Context.ToProcessLogHeader(plcValue + plcData.Index - 1, plcData.ID);
      //    var mainBattery = await _batteryCache.GetByIdAsync(plcData.ID, logHeader);//取缓存
      //    if (mainBattery == null)
      //    {
      //        var address = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[{plcData.Index - 1}]", 0);
      //        ResultTypeEnum.数据库找不到电池.WritePlcResult(address, _plc, _parameterConfig, logHeader);//写入PLC结果
      //        continue;
      //    }
      //    logHeader = Context.ToProcessLogHeader(plcValue + plcData.Index - 1, plcData.ID, mainBattery.Barcode);
      //    mainBattery.MesExitTime = DateTime.Now;
      //    outBatteries.Add(mainBattery);
      //}

      //var checkResult = LoadTraySendWmsHandler.CheckDuplicate(outBatteries); //检查重复数据
      //if (!checkResult.result)//如果有重复数据则不发送MES
      //{
      //    checkResult.msg.LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
      //    foreach (var batt in outBatteries)
      //    {
      //        //var address = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[{batt.EntryCrateSlot - 1}]", 0);
      //        //WriteResult(ResultTypeEnum.NG,  address, batt.Barcode, batt.EntryCrateSlot);//写入PLC结果
      //    }
      //    return;
      //}

      //if (_parameterConfig.AdvancedConfig.MESStatus != MESStatusEnum.关闭)
      //{
      //    foreach (var batt in outBatteries)
      //    {
      //        //var mesRequest = _mesService.GetRequestMessage(MesInterfaceNameEnum.产出消耗原材料绑定, batt);
      //        //await _mesService.SendRequestAsync(MesInterfaceNameEnum.产出消耗原材料绑定, mesRequest, batt.Barcode);
      //    }
      //}
      //await MesExit(_taskLogHeader, outBatteries.ToArray());

      //foreach (var batMain in outBatteries)
      //{
      //    string logHeader = Context.ToProcessLogHeader(id: batMain.Id, barcode: batMain.Barcode);
      //    if (batMain.MesExitStatus is ResultTypeEnum.OK or ResultTypeEnum._)
      //        Interlocked.Increment(ref okCount);
      //    else
      //        Interlocked.Increment(ref ngCount);

      //    //更新指定列
      //    var upDic = new Dictionary<string, object>
      //    {
      //        [nameof(BatMainModel.Id)] = batMain.Id,
      //        //[nameof(BatMainModel.ExitCrateNumber)] = batMain.ExitCrateNumber,
      //        //[nameof(BatMainModel.ExitCrateSlot)] = batMain.ExitCrateSlot,
      //        [nameof(BatMainModel.MesExitTime)] = batMain.MesExitTime,
      //        [nameof(BatMainModel.MesExitStatus)] = batMain.MesExitStatus,
      //        [nameof(BatMainModel.FinalStatus)] = batMain.FinalStatus,
      //    };

      //    if (!await _sugarDB.UpdateColumnsAsync(upDic, batMain.Id, batMain.Barcode, logHeader))
      //    {
      //        batMain.MesExitStatus = ResultTypeEnum.保存数据库失败;
      //    }

      //    //var address = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[{batMain.ExitCrateSlot - 1}]", 0);
      //    //WriteResult(batMain.FinalStatus, address, batMain.ExitCrateNumber ?? "", 0);//写入PLC结果
      //}

      //AddDisplayData(outBatteries.ToArray());
   }
}
