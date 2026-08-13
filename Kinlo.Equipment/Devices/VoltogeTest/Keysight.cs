namespace Kinlo.Equipment.Devices.VoltogeTest;

/// <summary>
/// 电压测试  Keysight
/// </summary>
[DeviceConnec([CommunicationEnum.VoltogeTest_Keysight])]
public class Keysight : DeviceBase
{
   private byte[] _query = Encoding.ASCII.GetBytes("READ?\r\n"); //查询最新测量值

   public Keysight(DeviceInfoModel info)
      : base(info) { }

   public override bool Open()
   {
      return base.Open();
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

   /// <summary>
   /// 读取电压
   /// </summary>
   /// <typeparam name="TValue"></typeparam>
   /// <param name="address"></param>
   /// <param name="length"></param>
   /// <param name="count"></param>
   /// <returns></returns>
   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TValue : default
   {
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions() { RetryCount = 3 };

      try
      {
         //读取结果
         var readRecord = new DeviceSplitCommand(
            this,
            _query,
            1024,
            options.RetryCount,
            bs => bs != null && bs.Any(),
            DeviceInfo.TaskToken,
            logHeader,
            100
         );
         var readResult = readRecord.ExecuteSplitRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TValue>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);

         var bytes = readResult.Value;
         var valueStr = Encoding.ASCII.GetString(bytes);
         $"读取数据成功，长度：{bytes.Length}；\r\nbytes：[{BitConverter.ToString(bytes)}]\r\n转换ASCII:[{valueStr}]".LogProcess(
            logHeader,
            Log4NetLevelEnum.信息
         );
         var listByte = bytes.SplitByteArray([0x0D, 0x0A]);
         var byteValue = listByte.Where(x => x.Count() == 15).LastOrDefault();
         if (byteValue == null)
         {
            var msg = $"报文不合法，长度错误！";
            msg.LogProcess(logHeader, Log4NetLevelEnum.错误);
            return DeviceResult<TValue>.Failure(DeviceStatus.报文校验不通过, msg);
         }
         valueStr = Encoding.ASCII.GetString(byteValue);
         if (!float.TryParse(valueStr, out float value))
         {
            return DeviceResult<TValue>.Failure(DeviceStatus.取值失败);
         }

         float f = (float)Math.Round(value, 6);
         if (f is TValue tv)
         {
            $"取值成功[{tv}]！".LogProcess(logHeader, Log4NetLevelEnum.成功);
            return DeviceResult<TValue>.Success(tv);
         }
         else
         {
            var errMsg = $"[{Helper.GetCurrentMethodName()}]传入数据类型和实际数据类型不对应;";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误);
            return DeviceResult<TValue>.Failure(DeviceStatus.结果和期望数据类型不同, errMsg);
         }
      }
      catch (Exception ex)
      {
         var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
         errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return DeviceResult<TValue>.Failure(DeviceStatus.异常, errMsg);
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
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions() { RetryCount = 3 };
      string errMsg = string.Empty;
      for (int i = 0; i < options.RetryCount; i++)
      {
         if (IsShutdown)
            return false;
         try
         {
            var res = Connect.Write((byte[])value, logHeader);
            if (res.State == CommState.Failed)
            {
               Thread.Sleep(300);
               continue;
            }
            else if (res.State == CommState.NeedReconnect)
            {
               if (!this.Reconnect(logHeader))
                  return false;
               continue;
            }
            return true;
         }
         catch (Exception ex)
         {
            errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         }
      }
      return false;
   }
}
