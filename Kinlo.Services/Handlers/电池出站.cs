namespace Kinlo.Services.Handlers;

/// <summary>
///
/// </summary>
[DeviceConnec(ProcessTypeEnum.出站, [CommunicationEnum.None])] //指定工艺，可指定多个
[DeviceConnec(ProcessTypeEnum.清洗机出站, [CommunicationEnum.None])] //指定工艺，可指定多个
public class OutboundHandler : ServiceHandlerBase
{
   public OutboundHandler(IContainer container, IDevice plc, PLCInteractAddressModel plcInteractAddress, CancellationTokenSource taskToken)
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
            string logHeader = Context.ToProcessLogHeader(plcData.Index, plcData.ID);
            var mainBattery = await _batteryCache.GetByIdAsync(plcData.ID, logHeader); //取缓存

            if (mainBattery == null)
            {
               plcData.DataAddress.WritePlcResult(ResultTypeEnum.数据库找不到电池, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
               return;
            }

            logHeader = Context.ToProcessLogHeader(plcData.Index, plcData.ID, mainBattery.Barcode);

            if (Context.ProcessesType == ProcessTypeEnum.清洗机出站)
            {
               CleaningMachineMaterDataDtu materData = new CleaningMachineMaterDataDtu();
               var MeterDatas = _plc.ReadClass<CleaningMachineMaterDataDtu>(Context.ExtraDataAddress, materData, logHeader);
               var batCleaningMachine = (IBatCleaningMachineExitModel)mainBattery;
               batCleaningMachine.CleaningWaterTemp1 = materData.CleaningWaterTemp1 / 10;
               batCleaningMachine.CleaningWaterTemp2 = materData.CleaningWaterTemp2 / 10;
               batCleaningMachine.CleaningWaterTemp3 = materData.CleaningWaterTemp3 / 10;
               batCleaningMachine.DryingTemp1 = materData.DryingTemp1 / 10;
               batCleaningMachine.DryingTemp2 = materData.DryingTemp2 / 10;
               batCleaningMachine.SetCleaningWaterTemp1 = materData.SetCleaningWaterTemp1 / 10;
               batCleaningMachine.SetCleaningWaterTemp2 = materData.SetCleaningWaterTemp2 / 10;
               batCleaningMachine.SetCleaningWaterTemp3 = materData.SetCleaningWaterTemp3 / 10;
               batCleaningMachine.SetDryingTemp1 = materData.SetDryingTemp1 / 10;
               batCleaningMachine.SetDryingTemp2 = materData.SetDryingTemp2 / 10;
               batCleaningMachine.PhValue = materData.PHValue;
               batCleaningMachine.PhTestVoltage = materData.PHTestVoltage;
               batCleaningMachine.PhTestTemp = materData.PHTestTemp;
               batCleaningMachine.PhORPValule = materData.PHORPValue;
               batCleaningMachine.CleaningTime = DateTime.Now;
            }
            await MesOutput(mainBattery, logHeader);

            if (_parameterConfig.AdvancedConfig.ProductionType == ProductionTypeEnum.清洗机)
            {
               if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
               {
                  mainBattery.MesOutputStatus = ResultTypeEnum.保存数据库失败;
               }
            }
            else
            {
               //更新指定列
               var upDic = new Dictionary<string, object>
               {
                  [nameof(BatMainModel.Id)] = mainBattery.Id,
                  [nameof(BatMainModel.OutputTime)] = mainBattery.OutputTime,
                  [nameof(BatMainModel.MesOutputStatus)] = mainBattery.MesOutputStatus,
                  [nameof(BatMainModel.FinalStatus)] = mainBattery.FinalStatus,
               };
               if (!await _sugarDB.UpdateColumnsAsync(upDic, mainBattery.Id, mainBattery.Barcode, logHeader))
               {
                  mainBattery.MesOutputStatus = ResultTypeEnum.保存数据库失败;
               }
            }
            plcData.DataAddress.WritePlcResult(ResultTypeEnum._, mainBattery.MesOutputStatus, _plc, _parameterConfig, logHeader); //写入PLC结果
            AddDisplayData(mainBattery); //更新界面显示
         }
      );
   }
}
