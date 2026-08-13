namespace Kinlo.Common.Tools;

/// <summary>
/// 更新补液量
/// </summary>
public static class UpdateRefillVolumeHelper
{
   public record RefillVolumeResult(bool state, ResultTypeEnum result, string errMsg);

   /// <summary>
   /// 更新手动补液
   /// </summary>
   /// <param name="battery"></param>
   /// <param name="replenishWeight"></param>
   /// <param name="parameterConfig"></param>
   /// <param name="logHeader"></param>
   /// <param name="isUpdateAfterWeightResult"></param>
   /// <returns></returns>
   public static RefillVolumeResult UpdateManualRefill(
      this IBatMainModel battery,
      double replenishWeight,
      ParameterConfig parameterConfig,
      string logHeader
   )
   {
      if (battery is not IBatWeightManualRefillModel batteryReplenishWeight)
         return new RefillVolumeResult(false, ResultTypeEnum._, "未找到补液称工序，需配置补液工序！");
      if (battery is not IBatWeightAfterModel batAftWeight)
         return new RefillVolumeResult(false, ResultTypeEnum._, "未找到后称工序，请先配置后称工序！");
      if (battery is not IBatScanBeforeModel batteryBeforeScan)
         return new RefillVolumeResult(false, ResultTypeEnum._, "未找到前扫码工序，请先配置前扫码工序！");

      if (parameterConfig.FunctionEnable.IsEnableCurrentRange) //是否使用实时范围值
      {
         batAftWeight.InjectionVolumeRange = parameterConfig.GetInjectionRange(); //注液范围
         batAftWeight.AfterWeighingRange = parameterConfig.GetFinalWeightRange(); //最终重量范围
      }

      batteryReplenishWeight.ManualRefillTime = DateTime.Now;
      batAftWeight.FinalAfterWeight = batteryReplenishWeight.ManualRefillWeight = replenishWeight;
      batteryReplenishWeight.ManualRefillVolume = battery.GetManualRefillVolume(logHeader);
      batAftWeight.TotalInjectionVolume = battery.GetTotalInjectVolume(parameterConfig, logHeader);
      batAftWeight.TotalInjectionVolumeDeviation = battery.GetTotalInjectionVolumeDeviation(parameterConfig, logHeader);
      batteryReplenishWeight.ManualRefillResult = battery.GetTotalInjectionVolumeResult(parameterConfig, logHeader);
      batAftWeight.FinalWeighingResult = battery.FinalWeightRangeCheck(parameterConfig, logHeader);

      return new RefillVolumeResult(true, (ResultTypeEnum)batteryReplenishWeight.ManualRefillResult, string.Empty);
   }
}
