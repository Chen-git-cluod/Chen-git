namespace Kinlo.Equipment.Devices.RFID;

/// <summary>
/// RFID扫码
/// </summary>
[DeviceConnec([CommunicationEnum.RFID_RD900M])]
public class RD900M : DeviceBase
{
   private byte[] _cmd = new byte[] { 0x04, 0x00, 0x01, 0xDB, 0x4B };

   public RD900M(DeviceInfoModel info)
      : base(info) { }

   public override bool Open()
   {
      if (base.Open())
      {
         return true;
      }
      return false;
   }

   Stopwatch _stopwatch = new Stopwatch();

   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TValue : default
   {
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      _stopwatch.Restart();

      try
      {
         Connect.Close();
         Connect.Open();
         var readRecord = new DeviceSplitCommand(this, _cmd, 64, 1, bs => bs.Length > 8, DeviceInfo.TaskToken, logHeader, 100);
         var result = DeviceResult<TValue>.Failure(DeviceStatus.Failure);
         while (_stopwatch.ElapsedMilliseconds <= 5000)
         {
            var readResult = readRecord.ExecuteSplitRequest();
            if (readResult.Status == DeviceStatus.Success)
            {
               var bytes = readResult.Value!;
               $"[RD900M]byte：{BitConverter.ToString(bytes)}".LogProcess(logHeader, Log4NetLevelEnum.信息);

               var code = string.Join("", bytes.Skip(6).Take(bytes.Length - 8).Select(x => x.ToString("X2")));
               if (!string.IsNullOrEmpty(code))
                  return DeviceResult<TValue>.Success((TValue)(object)code);
               else
                  result = DeviceResult<TValue>.Failure(DeviceStatus.报文校验不通过);
            }
            else
            {
               result = DeviceResult<TValue>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);
            }
            Thread.Sleep(30);
         }
         var msg = result.ErrorMessage ?? $"[{Helper.GetCurrentMethodName()}]读取失败;";
         msg.LogProcess(logHeader, Log4NetLevelEnum.错误);
         return result;
      }
      catch (Exception ex)
      {
         var msg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
         msg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return DeviceResult<TValue>.Failure(DeviceStatus.异常, msg, ex);
      }
      finally
      {
         Connect.Close();
         _stopwatch.Stop();
      }
   }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class
   {
      throw new NotImplementedException("未实现");
   }

   public override bool WriteClass<TClass>(
      TClass value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      throw new NotImplementedException("未实现");
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
