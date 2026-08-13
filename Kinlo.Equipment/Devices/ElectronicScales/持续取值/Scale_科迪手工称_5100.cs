namespace Kinlo.Equipment.Devices.ElectronicScales;

/// <summary>
/// Scale_科迪手工称_5100
/// </summary>
[DeviceConnec([CommunicationEnum.Scale_科迪手工称_5100])]
public class Scale_KD_5100 : CachedWeighingScaleBase
{
   protected override int FrameLength => 25;

   protected override byte[] ZeroCommand => throw new NotImplementedException("未实现");

   public Scale_KD_5100(DeviceInfoModel info)
      : base(info) { }

   protected override DeviceResult<double> TryParseWeight(byte[] frame, string logHeader)
   {
      var result = DeviceResult<double>.Failure(DeviceStatus.Failure);
      List<byte[]> splitBytes = frame.SplitByteArray([0x0D, 0x0A]);
      int count = 1;
      for (int i = splitBytes.Count - 1; i >= 0; i--)
      {
         count++;
         var byteArray = splitBytes[i];
         if (byteArray.Length >= 23)
         {
            if (byteArray[0] == 0x53 && byteArray[1] == 0x54) //"ST" 0x53 ="S" 0X54="T" ,稳定
            {
               var weighStr = Encoding.ASCII.GetString(byteArray, 7, 10).Replace(" ", "");
               if (double.TryParse(weighStr, out var weight))
               {
                  $"取到稳定值[{weight}]！".LogProcess(logHeader);
                  return DeviceResult<double>.Success(weight);
               }
            }
            else
            {
               var msg = $"第{count}次 [{Encoding.ASCII.GetString(byteArray)}] 未取到稳定值";
               msg.LogProcess(logHeader);
               result = DeviceResult<double>.Failure(DeviceStatus.称重不稳定, msg);
            }
         }
         else
         {
            var msg = $"第{count}次读取字节不合法！字节:[{(byteArray == null ? "null" : BitConverter.ToString(byteArray))}];\r\n";
            msg.LogProcess(logHeader);
            result = DeviceResult<double>.Failure(DeviceStatus.取值失败, msg);
         }

         if (count >= 5)
            break;
      }
      return result;
   }

   public override bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   )
   {
      throw new NotImplementedException("未实现");
   }
}
