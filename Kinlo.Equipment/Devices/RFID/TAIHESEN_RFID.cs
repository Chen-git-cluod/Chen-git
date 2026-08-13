namespace Kinlo.Equipment.Devices.RFID;

[DeviceConnec([CommunicationEnum.RFID_TAIHESEN, CommunicationEnum.RFID_TAIHESEN_RJ45])]
public class TAIHESEN_RFID : DeviceBase
{
   private byte[] _cmd = new byte[] { 0xAA, 0x00, 0x22, 0x00, 0x00, 0x22, 0xBB };

   public TAIHESEN_RFID(DeviceInfoModel info)
      : base(info) 
    {
        _cmd = info.Communication switch
        {
            CommunicationEnum.RFID_TAIHESEN => [0xAA, 0x00, 0x22, 0x00, 0x00, 0x22, 0xBB],
            CommunicationEnum.RFID_TAIHESEN_RJ45 => [0x02, 0x00, 0x04, 0x11, 0x02, 0x05, 0x02, 0x10, 0x03]
        };
    }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class
   {
      throw new NotImplementedException();
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
      string msg = string.Empty;
      try
      {
         var readRecord = new DeviceSplitCommand(this, _cmd, 64, 1, bs => bs.Length > 12, DeviceInfo.TaskToken, logHeader, 200);
         var result = DeviceResult<TValue>.Failure(DeviceStatus.Failure);
         while (_stopwatch.ElapsedMilliseconds <= 8000)
         {
            var readResult = readRecord.ExecuteSplitRequest();
            if (readResult.Status != DeviceStatus.Success)
            {
               result = DeviceResult<TValue>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);
            }
            else
            {
               var bytes = readResult.Value!;
               $"[TAIHESEN_RFID]byte：{BitConverter.ToString(bytes)}".LogProcess(logHeader, Log4NetLevelEnum.信息);

               var code = string.Join("", bytes[8..^4].Select(x => x.ToString("X2")));
               if (!string.IsNullOrEmpty(code))
               {
                  if (code is TValue tv)
                     return DeviceResult<TValue>.Success(tv);
                  else
                  {
                     msg = $"[{Helper.GetCurrentMethodName()}]传入数据类型和实际数据类型不对应;";
                     msg.LogProcess(logHeader, Log4NetLevelEnum.错误);
                     return DeviceResult<TValue>.Failure(DeviceStatus.结果和期望数据类型不同, msg);
                  }
               }
            }
            Thread.Sleep(30);
         }
         msg = result.ErrorMessage ?? $"[{Helper.GetCurrentMethodName()}]读取失败;";
         msg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return result;
      }
      catch (Exception ex)
      {
         msg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
         msg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return DeviceResult<TValue>.Failure(DeviceStatus.异常, msg, ex);
      }
      finally
      {
         _stopwatch.Stop();
      }
   }

   public override bool WriteClass<TClass>(
      TClass value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      throw new NotImplementedException();
   }

   public override bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   )
   {
      throw new NotImplementedException();
   }
}
