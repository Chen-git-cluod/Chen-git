namespace Kinlo.Services.Handlers;

public partial class WeightAfterHandler
{
   /// <summary>
   /// 注液过少回流称重
   /// </summary>
   /// <param name="mainBattery"></param>
   /// <param name="weiging"></param>
   /// <param name="plcData"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public async Task RefillHandle(
      IBatMainModel mainBattery,
      DeviceResult<double> deviceResult,
      ReceivePlcDataModel plcData,
      string logHeader
   )
   {
      var batBefScan = (IBatScanBeforeModel)mainBattery;
      var batAftWeight = (IBatWeightAfterModel)mainBattery;
      var autoRefillWeight = (IBatWeightAutoRefillModel)mainBattery;

      autoRefillWeight.AutoRefillTime = DateTime.Now;
      autoRefillWeight.AutoRefillWeightIndex = plcData.Index;

      var sendPlcResult = autoRefillWeight.AutoRefillResult = batAftWeight.FinalWeighingResult = deviceResult.Status.ToProductResult();
      //如果称重取到值， 再计算其它，如果称重未取到值直接NG
      if (sendPlcResult == ResultTypeEnum.OK)
      {
         batAftWeight.FinalAfterWeight = autoRefillWeight.AutoRefillWeight = deviceResult.Value;
         autoRefillWeight.AutoRefillVolume = autoRefillWeight.AutoRefillWeight - batAftWeight.FirstInjectWeight;
         batAftWeight.FinalWeighingResult = mainBattery.FinalWeightRangeCheck(_parameterConfig, logHeader);
         batAftWeight.TotalInjectionVolume = mainBattery.GetTotalInjectVolume(_parameterConfig, logHeader);
         batAftWeight.TotalInjectionVolumeDeviation = mainBattery.GetTotalInjectionVolumeDeviation(_parameterConfig, logHeader);
         autoRefillWeight.AutoRefillResult = mainBattery.GetTotalInjectionVolumeResult(_parameterConfig, logHeader);
         //发送补液量至PLC
         var supplementaryRes = SupplementaryElectrolyteToPlc(batAftWeight.TotalInjectionVolumeDeviation, plcData, logHeader);
         //单纯获取发给PLC结果，此过程不会给电池赋值
         sendPlcResult = ClaculatePlcResult(
            autoRefillWeight.AutoRefillResult,
            batAftWeight.FinalWeighingResult,
            supplementaryRes,
            mainBattery,
            logHeader
         );

         //如果是注液量偏少，就会回流，标记回流原因，真实回流后记录回流次数
         if (sendPlcResult == ResultTypeEnum.注液量偏少)
            AddOrUpdateReworkReason(mainBattery);
      }
      $"少液回流称重:[{autoRefillWeight.AutoRefillWeight}],称重结果为:[{batAftWeight.FinalWeighingResult}];回流注液结果为:[{autoRefillWeight.AutoRefillResult}]；".LogProcess(
         logHeader
      );

      if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
      {
         autoRefillWeight.AutoRefillResult = ResultTypeEnum.保存数据库失败;
         sendPlcResult = ResultTypeEnum.保存数据库失败;
      }

      var processesDatas = _displayDataCollection.ProcessesDatas.FirstOrDefault(x => x.Processes == ProcessTypeEnum.回流补液);
      processesDatas?.AddDisplayData(mainBattery); //更新至补液界面显示

      plcData.DataAddress.WritePlcResult(sendPlcResult, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
   }
}
