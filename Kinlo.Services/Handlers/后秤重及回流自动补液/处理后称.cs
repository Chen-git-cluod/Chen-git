namespace Kinlo.Services.Handlers;

public partial class WeightAfterHandler
{
   protected async Task AfterWeightHanler(
      IBatMainModel mainBattery,
      DeviceResult<double> deviceResult,
      ReceivePlcDataModel plcData,
      string logHeader
   )
   {
      IBatWeightBeforeModel batBefWeight =
         _parameterConfig.AdvancedConfig.ProductionType == ProductionTypeEnum.回氦
            ? new BatWeightBeforeModel()
            : (IBatWeightBeforeModel)mainBattery;

      var batBefScan = (IBatScanBeforeModel)mainBattery;
      var batTestLeak = (IBatTestLeakModel)mainBattery;
      var batAftWeight = (IBatWeightAfterModel)mainBattery;

      batAftWeight.AfterWeightTime = DateTime.Now;
      batAftWeight.AfterWeightIndex = plcData.Index;

      var sendPlcResult = batAftWeight.FirstInjectResult = batAftWeight.FinalWeighingResult = deviceResult.Status.ToProductResult();
      //如果称重取到值， 再计算其它，如果称重未取到值直接NG
      if (sendPlcResult == ResultTypeEnum.OK)
      {
         batAftWeight.FinalAfterWeight = batAftWeight.FirstInjectWeight = deviceResult.Value;
         batAftWeight.ActualInjectionVolume = batAftWeight.FirstInjectWeight.GetInjectVolume(batBefWeight.BeforeWeight, _parameterConfig);

         batAftWeight.TargetInjectionVolumeDeviation = Math.Round(
            batAftWeight.ActualInjectionVolume - batBefWeight.TargetInjectionVolume,
            3
         );

         batAftWeight.TotalInjectionVolume = mainBattery.GetTotalInjectVolume(_parameterConfig, logHeader);

         batAftWeight.TotalInjectionVolumeDeviation = mainBattery.GetTotalInjectionVolumeDeviation(_parameterConfig, logHeader);

         batAftWeight.FirstInjectResult = mainBattery.GetTotalInjectionVolumeResult(_parameterConfig, logHeader);
         //最终称重检测
         batAftWeight.FinalWeighingResult = mainBattery.FinalWeightRangeCheck(_parameterConfig, logHeader);

         //发送补液量到PLC（注液OK或NG都发，如果OK可以覆盖之前的）
         var supplementaryRes = SupplementaryElectrolyteToPlc(batAftWeight.TotalInjectionVolumeDeviation, plcData, logHeader);

         //单纯计算发给PLC结果，此过程不会给电池赋值
         sendPlcResult = ClaculatePlcResult(
            batAftWeight.FirstInjectResult,
            batAftWeight.FinalWeighingResult,
            supplementaryRes,
            mainBattery,
            logHeader
         );

         //如果是注液量偏少，就会回流，标记回流原因，真实回流后记录回流次数
         if (sendPlcResult == ResultTypeEnum.注液量偏少)
            AddOrUpdateReworkReason(mainBattery);

         _ = SaveInjectTableAsync(mainBattery, batBefWeight, batAftWeight); //写入注液量表，不等待
      }

      #region 上传MES   260107去掉NG上传MES，260323重新加上出站
      if (batAftWeight.FinalInjectResult != ResultTypeEnum.OK || batAftWeight.FinalWeighingResult != ResultTypeEnum.OK)
      {
         //mainBattery.MesOutputTime = DateTime.Now;//还要过补液，不算出站
         await MesOutput(mainBattery, logHeader);
      }
      #endregion

      await HandleBatteryResultAsync(mainBattery, sendPlcResult, plcData.DataAddress, logHeader);
   }

   /// <summary>
   /// 处理电池结果
   /// </summary>
   /// <param name="mainBattery"></param>
   /// <param name="sendPlcResult"></param>
   /// <param name="address"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   private async Task HandleBatteryResultAsync(
      IBatMainModel mainBattery,
      ResultTypeEnum sendPlcResult,
      SignalAddressModel address,
      string logHeader
   )
   {
      //保存本工序数据
      if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
      {
         sendPlcResult = ResultTypeEnum.保存数据库失败;
         var batAftWeight = (IBatWeightAfterModel)mainBattery;
         batAftWeight.FirstInjectResult = ResultTypeEnum.保存数据库失败;
      }

      address.WritePlcResult(sendPlcResult, mainBattery.MesOutputStatus, _plc, _parameterConfig, logHeader); //写入PLC结果

      AddDisplayData(mainBattery);
   }

   private async Task SaveInjectTableAsync(IBatMainModel mainBattery, IBatWeightBeforeModel batBefWeight, IBatWeightAfterModel batAftWeight)
   {
      if (mainBattery is IBatInjectStationModel inj) //写入注液量表，不等待
      {
         var injectionData = new InjectionDataModel //记录注液量相关
         {
            Id = mainBattery.Id,
            Barcode = mainBattery.Barcode,
            InjectionTime = inj.InjectElectrolyteTime,
            StationNo = inj.InjectPumpNo,
            NeedleNo = inj.InjectNozzleNo,
            TargetInjectionVolume = batBefWeight.TargetInjectionVolume,
            InjectionValue = batAftWeight.ActualInjectionVolume,
         };
         await _sugarDB.InsertOrUpdateInjectionAsync(injectionData);
      }
   }
}
