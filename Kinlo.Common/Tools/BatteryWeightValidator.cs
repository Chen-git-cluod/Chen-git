namespace Kinlo.Common.Tools;

/// <summary>
/// 电池重量校验
/// </summary>
public static class BatteryWeightValidator
{
   static char joinChar = '~';
   static float rangeDefualt = -0.000001f;

   public static string GetWeightRange(this double weightUpper, double weightLower) => $"{weightUpper}{joinChar}{weightLower}";

   public static string GetBeforWeightRange(this ParameterConfig parameterConfig) =>
      GetWeightRange(parameterConfig.RunParameter.IncomingWeightUpper, parameterConfig.RunParameter.IncomingWeightLower);

   public static string GetFinalWeightRange(this ParameterConfig parameterConfig) =>
      GetWeightRange(parameterConfig.RunParameter.AfterWeightUpper, parameterConfig.RunParameter.AfterWeightLower);

   public static string GetInjectionRange(this ParameterConfig parameterConfig) =>
      $"{parameterConfig.RunParameter.InjectionUpper}{joinChar}{parameterConfig.RunParameter.InjectionStandard}{joinChar}{parameterConfig.RunParameter.InjectionLower}";

   /// <summary>
   /// 写入范围值
   /// </summary>
   /// <param name="mainBattery"></param>
   public static void SetBatteryRange(this IBatMainModel mainBattery, ParameterConfig parameterConfig)
   {
      if (mainBattery is IBatWeightBeforeModel beforeWeight)
         beforeWeight.BeforWeightRange = GetBeforWeightRange(parameterConfig); //前称重范围
      if (mainBattery is IBatWeightAfterModel batAftWeight)
      {
         batAftWeight.InjectionVolumeRange = GetInjectionRange(parameterConfig); //注液范围
         batAftWeight.AfterWeighingRange = GetFinalWeightRange(parameterConfig); //后称重范围
      }
   }

   /// <summary>
   /// 计算手动补液量
   /// </summary>
   /// <param name="battery"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static double GetManualRefillVolume(this IBatMainModel battery, string logHeader)
   {
      if (battery is IBatWeightManualRefillModel manualRefill && battery is IBatWeightAfterModel afterWeight)
      {
         double firstWeight = battery switch
         {
            var b when b is BatWeightAutoRefillModel autoRefill && autoRefill.AutoRefillWeight > 0 => autoRefill.AutoRefillWeight,
            _ => afterWeight.FirstInjectWeight,
         };

         return Math.Round(manualRefill.ManualRefillWeight - firstWeight, 3);
      }
      else
      {
         $"注意：条码：[{battery.Barcode}] 在计算手动补液量时对象无法转换为[手动补液]或[后称重]，无法计算手动补液量！".LogProcess(
            logHeader,
            Log4NetLevelEnum.错误
         );
         return 0;
      }
   }

   /// <summary>
   /// 计算保液量
   /// </summary>
   /// <param name="battery"></param>
   /// <param name="config"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static double GetTotalInjectVolume(this IBatMainModel battery, ParameterConfig config, string logHeader)
   {
      if (battery is IBatScanBeforeModel beforScan && battery is IBatWeightAfterModel afterWeight)
         return GetInjectVolume(afterWeight.FinalAfterWeight, beforScan.NetWeight, config);
      else
      {
         $"注意：条码：[{battery.Barcode}] 在计算保液量时对象无法转换为[前扫码]或[后称重]，无法计算保液量！".LogProcess(
            logHeader,
            Log4NetLevelEnum.错误
         );
         return 0;
      }
   }

   /// <summary>
   /// 计算注液量
   /// </summary>
   /// <param name="postInjectWeight"></param>
   /// <param name="preInjectWeight"></param>
   /// <param name="config"></param>
   /// <returns></returns>
   public static double GetInjectVolume(this double postInjectWeight, double preInjectWeight, ParameterConfig config) =>
      Math.Round(postInjectWeight - preInjectWeight - config.RunParameter.NailWeight, 3);

   /// <summary>
   /// 计算保液量偏差
   /// </summary>
   /// <param name="battery"></param>
   /// <param name="config"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static double GetTotalInjectionVolumeDeviation(this IBatMainModel battery, ParameterConfig config, string logHeader)
   {
      double weight = 0;
      string range = string.Empty;
      if (battery is IBatWeightAfterModel afterWeight)
      {
         float standard = -999;
         if (!string.IsNullOrEmpty(afterWeight.InjectionVolumeRange))
         {
            var rangeArr = afterWeight.InjectionVolumeRange.Split(joinChar);
            if (rangeArr.Length >= 3)
            {
               float.TryParse(rangeArr[1], out standard);
            }
         }
         if (standard == -999)
         {
            standard = config.RunParameter.InjectionStandard;
            $"在计算保液量偏差时注液范围未跟踪，使用实时设置！{GetInjectionRange(config)}".LogProcess(logHeader, Log4NetLevelEnum.警告);
         }
         return Math.Round(afterWeight.TotalInjectionVolume - standard, 3);
      }
      else
      {
         $"注意：条码：[{battery.Barcode}] 在计算保液量偏差时对象无法转换为[后称重]，无法计算保液量偏差！".LogProcess(
            logHeader,
            Log4NetLevelEnum.错误
         );
         return 0;
      }
   }

   /// <summary>
   /// 计算保液量结果
   /// </summary>
   /// <param name="battery"></param>
   /// <param name="config"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static ResultTypeEnum GetTotalInjectionVolumeResult(this IBatMainModel battery, ParameterConfig config, string logHeader)
   {
      if (battery is IBatWeightAfterModel afterWeight)
         return afterWeight.TotalInjectionVolume.InjectInjectCheck(afterWeight.InjectionVolumeRange, config, logHeader);
      else
      {
         $"注意：条码：[{battery.Barcode}] 在计算保液量结果时对象无法转换为[后称重]，无法计算保液量结果！".LogProcess(
            logHeader,
            Log4NetLevelEnum.错误
         );
         return ResultTypeEnum.异常;
      }
   }

   /// <summary>
   /// 注液量检测
   /// </summary>
   /// <param name="injectVolume"></param>
   /// <param name="parameterConfig"></param>
   /// <returns></returns>
   public static ResultTypeEnum InjectInjectCheck(this double injectVolume, string range, ParameterConfig parameterConfig, string logHeader)
   {
      if (!parameterConfig.FunctionEnable.IsEnableInjectionCheck)
      {
         $"请注意!!!未启用注液范围检查==>>强制合格".LogProcess(logHeader, Log4NetLevelEnum.警告);
         return ResultTypeEnum.OK;
      }

      float upper = rangeDefualt,
         lower = rangeDefualt;
      if (!string.IsNullOrEmpty(range))
      {
         var rangeArr = range.Split(joinChar);
         if (rangeArr.Length >= 3)
         {
            if (float.TryParse(rangeArr[0], out var upperVal))
               upper = upperVal;
            if (float.TryParse(rangeArr[2], out var lowerVal))
               lower = lowerVal;
         }
      }
      if (upper == rangeDefualt || lower == rangeDefualt)
      {
         upper = parameterConfig.RunParameter.InjectionUpper;
         lower = parameterConfig.RunParameter.InjectionLower;
         $"注液范围未跟踪，使用实时设置！{GetInjectionRange(parameterConfig)}".LogProcess(logHeader, Log4NetLevelEnum.警告);
      }
      return injectVolume switch
      {
         var inj when inj >= lower && inj <= upper => ResultTypeEnum.OK,
         var inj when inj < lower => ResultTypeEnum.注液量偏少,
         var inj when inj > upper => ResultTypeEnum.注液量偏多,
         _ => ResultTypeEnum.NG,
      };
   }

   /// <summary>
   /// 来料称重范围检测
   /// </summary>
   /// <param name="weight"></param>
   /// <param name="barcode"></param>
   /// <param name="parameterConfig"></param>
   /// <returns></returns>
   public static ResultTypeEnum IncomingWeightRangeCheck(
      this double weight,
      string range,
      ParameterConfig parameterConfig,
      string logHeader
   )
   {
      if (!parameterConfig.FunctionEnable.IsEnableIncomingWeightControl)
      {
         $"请注意!!!未启用前称重量管控==>>强制合格".LogProcess(logHeader, Log4NetLevelEnum.警告);
         return ResultTypeEnum.OK;
      }

      float upper = rangeDefualt,
         lower = rangeDefualt;
      if (!string.IsNullOrEmpty(range))
      {
         var rangeArr = range.Split(joinChar);
         if (rangeArr.Length >= 2)
         {
            if (float.TryParse(rangeArr[0], out var upperVal))
               upper = upperVal;
            if (float.TryParse(rangeArr[1], out var lowerVal))
               lower = lowerVal;
         }
      }

      if (upper == rangeDefualt || lower == rangeDefualt)
      {
         upper = parameterConfig.RunParameter.IncomingWeightUpper;
         lower = parameterConfig.RunParameter.IncomingWeightLower;
         $"前称范围未跟踪，使用实时设置！{GetInjectionRange(parameterConfig)}".LogProcess(logHeader, Log4NetLevelEnum.警告);
      }
      return weight.CheckWeight(lower, upper);
   }

   /// <summary>
   /// 最终重量范围检测
   /// </summary>
   /// <param name="battery"></param>
   /// <param name="parameterConfig"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static ResultTypeEnum FinalWeightRangeCheck(this IBatMainModel battery, ParameterConfig parameterConfig, string logHeader)
   {
      if (!parameterConfig.FunctionEnable.IsEnableAfterWeightCheck)
      {
         $"请注意!!!未启后称重量管控==>>强制合格".LogProcess(logHeader, Log4NetLevelEnum.警告);
         return ResultTypeEnum.OK;
      }

      double weight = 0;
      string range = string.Empty;
      if (battery is IBatWeightAfterModel afterWeight)
      {
         weight = afterWeight.FinalAfterWeight;
         range = afterWeight.AfterWeighingRange;
      }
      else
      {
         $"注意：条码：[{battery.Barcode}] 在计算最终重量结果时对象无法转换为[后称重]，无法计算最终重量结果！".LogProcess(
            logHeader,
            Log4NetLevelEnum.错误
         );
         return ResultTypeEnum.异常;
      }

      float upper = rangeDefualt,
         lower = rangeDefualt;
      if (!string.IsNullOrEmpty(range))
      {
         var rangeArr = range.Split(joinChar);
         if (rangeArr.Length >= 2)
         {
            if (float.TryParse(rangeArr[0], out var upperVal))
               upper = upperVal;
            if (float.TryParse(rangeArr[1], out var lowerVal))
               lower = lowerVal;
         }
      }
      if (upper == rangeDefualt || lower == rangeDefualt)
      {
         upper = parameterConfig.RunParameter.AfterWeightUpper;
         lower = parameterConfig.RunParameter.AfterWeightLower;
         $"后称范围未跟踪，使用实时设置！{GetInjectionRange(parameterConfig)}".LogProcess(logHeader, Log4NetLevelEnum.警告);
      }
      return weight.CheckWeight(lower, upper);
   }

   public static ResultTypeEnum CheckWeight(this double weight, double lower, double upper)
   {
      return weight switch
      {
         var k when k <= upper && k >= lower => ResultTypeEnum.OK,
         var k when k > upper => ResultTypeEnum.称重偏重,
         var k when k < lower => ResultTypeEnum.称重偏轻,
         _ => ResultTypeEnum.NG,
      };
   }
}
