namespace Kinlo.Equipment.Devices.ElectronicScales;

/// <summary>
/// 普瑞逊TC06电子称
/// </summary>
[DeviceConnec([CommunicationEnum.Scale_Pris_TC06])]
public class Scale_Pris_TC06 : CachedWeighingScaleBase
{
   public Scale_Pris_TC06(DeviceInfoModel info)
      : base(info) { }

   protected override int FrameLength => 20;

   protected override byte[] ZeroCommand => Encoding.ASCII.GetBytes("DT\r\n");

   protected override DeviceResult<double> TryParseWeight(byte[] frame, string logHeader)
   {
      var strWeights = Encoding.ASCII.GetString(frame);
      var weightArray = strWeights
         .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
         .Where(x => x.Length == 18 && !x.Contains("WT,") && (x.Contains("ST,GS") || x.Contains("ST,NT")))
         .ToArray();

      if (!weightArray.Any())
      {
         var msg = $"[{strWeights}] 未取到正确字节";
         msg.LogProcess(logHeader);
         return DeviceResult<double>.Failure(DeviceStatus.取值失败, msg);
      }

      var str = weightArray.LastOrDefault(x => x.Substring(0, 2).ToUpper() == "ST");
      if (str == null)
      {
         var msg = $"[{strWeights}] 未取到稳定值";
         msg.LogProcess(logHeader);
         return DeviceResult<double>.Failure(DeviceStatus.称重不稳定, msg);
      }
      string weighStr = str.Substring(6, 8).Replace(" ", "");
      if (double.TryParse(weighStr, out var weight))
      {
         $"取到稳定值[{weight}]！".LogProcess(logHeader);
         return DeviceResult<double>.Success(weight);
      }
      else
      {
         var msg = $"称稳定，但值[{weighStr}]未能正常转换!";
         msg.LogProcess(logHeader);
         return DeviceResult<double>.Failure(DeviceStatus.取值失败, msg);
      }
   }
}
