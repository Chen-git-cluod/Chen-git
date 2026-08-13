using Kinlo.Common.Configurations;

namespace Kinlo.Services.Helper;

public static class InspectionHelper
{
   /// <summary>
   /// 扫码点检方法
   /// </summary>
   /// <param name="process"></param>
   /// <param name="inspectionConfig"></param>
   /// <param name="plcValue"></param>
   /// <param name="scanCodeResults"></param>
   /// <returns></returns>
   public static (string barcode, ResultTypeEnum result)[] ScanCodeInspection(
      this ProcessTypeEnum process,
      InspectionConfig inspectionConfig,
      short plcValue,
      params ScanBarcodeResultDto[] scanCodeResults
   )
   {
      (string barcode, ResultTypeEnum result)[] results = Enumerable.Repeat(("ERROR", ResultTypeEnum.NG), scanCodeResults.Length).ToArray();
      if (process is not (ProcessTypeEnum.前扫码 or ProcessTypeEnum.后扫码))
      {
         Growl.Warning("只能前扫码或后扫码点检");
         return results;
      }

      var inspection =
         process == ProcessTypeEnum.前扫码 ? inspectionConfig.BeforeScanBarcodeParameter : inspectionConfig.AfterScanBarcodeParameter;
      results = Enumerable.Repeat(("ERROR", ResultTypeEnum.NG), inspection.Lanes.Count).ToArray();

      for (int i = 0; i < results.Length; i++)
      {
         if (scanCodeResults.Length <= i)
         {
            Growl.Warning($"实际条码少于设置扫码个数！");
            break;
         }

         var scanCode = scanCodeResults[i];
         results[i].barcode = scanCode.Code;
         int laneIndex = plcValue + i; //通道

         var laneData = inspection.Lanes.FirstOrDefault(x => x.Index == laneIndex);
         if (laneData == null)
         {
            Growl.Warning($"未找到[{laneIndex}]通道！");
            continue;
         }
         laneData.UpdateTime = DateTime.Now;
         laneData.IsSuccess = false;
         if (scanCode.ScanStatus == ResultTypeEnum.OK)
         {
            laneData.CurrentValue = scanCode.Code;
            if (laneData.TragetValue == scanCode.Code)
            {
               results[i].result = ResultTypeEnum.OK;
               laneData.IsSuccess = true;
            }
         }
         else
         {
            laneData.CurrentValue = "扫码失败";
         }
      }

      _ = inspectionConfig.OnShowInspectionWindow(process);
      return results;
   }

   /// <summary>
   /// 范围值点检方法
   /// </summary>
   /// <param name="process"></param>
   /// <param name="inspectionConfig"></param>
   /// <param name="plcValue"></param>
   /// <param name="scanCodeResults"></param>
   /// <returns></returns>
   public static ResultTypeEnum RangeInspection(
      this ProcessTypeEnum process,
      int index,
      InspectionConfig inspectionConfig,
      bool isSuccess,
      double value
   )
   {
      if (process is not (ProcessTypeEnum.前称重 or ProcessTypeEnum.后称重 or ProcessTypeEnum.测电压))
      {
         Growl.Warning("只能前称重、后称重或测电压点检");
         return ResultTypeEnum.NG;
      }
      var inspection = process switch
      {
         ProcessTypeEnum.前称重 => inspectionConfig.BeforeWeighParameter,
         ProcessTypeEnum.后称重 => inspectionConfig.AfterWeighParameter,
         _ => inspectionConfig.TestVoltageParameter,
      };
      var laneData = inspection.Lanes.FirstOrDefault(x => index == x.Index);
      if (laneData == null)
      {
         Growl.Warning($"未找到[{index}]通道！");
         return ResultTypeEnum.NG;
      }
      laneData.UpdateTime = DateTime.Now;
      laneData.IsSuccess = false;
      laneData.CurrentValue = value.ToString();
      ResultTypeEnum checkResult = isSuccess switch
      {
         true => value.CheckWeight(laneData.Lower, laneData.Upper),
         _ => ResultTypeEnum.NG,
      };
      if (checkResult == ResultTypeEnum.OK)
      {
         laneData.IsSuccess = true;
      }

      _ = inspectionConfig.OnShowInspectionWindow(process);
      return checkResult;
   }
}
