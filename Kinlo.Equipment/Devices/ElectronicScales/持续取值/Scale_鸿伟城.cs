namespace Kinlo.Equipment.Devices.ElectronicScales;

/// <summary>
/// Scale_鸿伟城
/// </summary>
[DeviceConnec([CommunicationEnum.Scale_鸿伟城])]
public class Scale_HWC : CachedWeighingScaleBase
{
   protected override byte[] ZeroCommand => [0x5A, 0x0D, 0x0A];
   protected override int FrameLength => 13;

   public Scale_HWC(DeviceInfoModel info)
      : base(info) { }

   protected override DeviceResult<double> TryParseWeight(byte[] frame, string logHeader)
   {
      var strWeights = Encoding.ASCII.GetString(frame);
      var str = strWeights.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault(x => x.Length == 11); //此处不需要稳定吗（重写时无文档无法确定）？2025 12 17 刘亮

      if (str == null)
      {
         var msg = $"[{strWeights}] 未取到正确字节";
         msg.LogProcess(logHeader);
         return DeviceResult<double>.Failure(DeviceStatus.取值失败, msg);
      }
      string weighStr = str.Substring(0, 8).Replace(" ", "");
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
