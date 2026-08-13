namespace Kinlo.Equipment.Devices.ElectronicScales;

/// <summary>
/// Scale_Sartorius赛多利斯
/// </summary>
[DeviceConnec([CommunicationEnum.Scale_Sartorius赛多利斯])]
public class Scale_Sartorius : CachedWeighingScaleBase
{
   protected override byte[] ZeroCommand => [0x1B, 0x56];
   protected override int FrameLength => 48;

   public Scale_Sartorius(DeviceInfoModel info)
      : base(info) { }

   protected override DeviceResult<double> TryParseWeight(byte[] frame, string logHeader)
   {
      StringBuilder msg = new StringBuilder();

      List<byte[]> splitBytes = frame.SplitByteArray([0x0D, 0x0A]);
      splitBytes = splitBytes.Where(x => x.Count() == 14).ToList();
      if (splitBytes.Count < 3)
      {
         var err = $"[{BitConverter.ToString(frame)}] 未取到正确字节";
         err.LogProcess(logHeader);
         return DeviceResult<double>.Failure(DeviceStatus.取值失败, err);
      }

      int count = 1;
      double weight = 0;
      for (int i = splitBytes.Count - 3; i < splitBytes.Count; i++) //取最后三次，如果三次都稳定就视为稳定
      {
         count++;
         var byteArray = splitBytes[i];
         if (byteArray.Length == 14) //有单位则是稳定
         {
            if (byteArray[13] == 0x20 && byteArray[12] == 0x20 && byteArray[11] == 0x20)
            {
               msg.Append($"第{count}次称重不稳定！字节:[{BitConverter.ToString(byteArray)}];\r\n");
               msg.ToString().LogProcess(logHeader, Log4NetLevelEnum.警告);
               return DeviceResult<double>.Failure(DeviceStatus.称重不稳定, msg.ToString());
            }
            string weighStr = Encoding.ASCII.GetString(byteArray, 0, 10);
            if (!double.TryParse(weighStr.Replace(" ", ""), out weight))
            {
               msg.Append($"第{count}转换值失败[{weight}]！字节:[{BitConverter.ToString(byteArray)}];\r\n");
               msg.ToString().LogProcess(logHeader, Log4NetLevelEnum.警告);
               return DeviceResult<double>.Failure(DeviceStatus.取值失败, msg.ToString());
            }
            else
            {
               msg.Append($"第{count}次取到稳定值[{weight}]！字节:[{BitConverter.ToString(byteArray)}];\r\n");
            }
         }
         else
         {
            msg.Append($"第{count}次读取字节不合法！字节:[{(byteArray == null ? "null" : BitConverter.ToString(byteArray))}];\r\n");
            msg.ToString().LogProcess(logHeader, Log4NetLevelEnum.警告);
            return DeviceResult<double>.Failure(DeviceStatus.取值失败, msg.ToString());
         }
      }
      msg.ToString().LogProcess(logHeader, Log4NetLevelEnum.成功);
      return DeviceResult<double>.Success(weight);
   }
}
