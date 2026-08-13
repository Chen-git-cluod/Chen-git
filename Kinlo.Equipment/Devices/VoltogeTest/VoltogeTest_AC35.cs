namespace Kinlo.Equipment.Devices.VoltogeTest;

/// <summary>
/// 艾测电压测试 AC35系列
/// </summary>
[DeviceConnec([CommunicationEnum.VoltogeTest_AC35])]
public class VoltogeTest_AC35 : DeviceBase
{
   //private byte[] _query = Encoding.ASCII.GetBytes(":FETCh?\r\n");//读取一次
   private byte[] _query = Encoding.ASCII.GetBytes(":READ?\r\n"); //触发测试并读取最新测量值

   public VoltogeTest_AC35(DeviceInfoModel info)
      : base(info) { }

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

         var bytes = readResult.Value!;
         var valutStr = Encoding.ASCII.GetString(bytes);
         $"读取数据，长度：{bytes.Length}；bytes：{BitConverter.ToString(bytes)},转换ASCII:{valutStr.Trim('\r', '\n')}".LogProcess(
            logHeader
         );
         var values = valutStr.Split(',');
         if (values.Length != 2)
         {
            var errMsg = $"读取数据 [{valutStr}] 个数小于2个";
            errMsg.LogProcess(logHeader);
            return DeviceResult<TValue>.Failure(DeviceStatus.取值失败, errMsg);
         }

         if (double.TryParse(values[0].Trim(), out var acResistance) && double.TryParse(values[1].Trim(), out var voltoge))
         {
            (double, double) data = (acResistance, voltoge);
            if (data is TValue r)
               return DeviceResult<TValue>.Success(r);
            else
            {
               var errMsg = $"[{Helper.GetCurrentMethodName()}]传入数据类型和实际数据类型不对应;";
               errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误);
               return DeviceResult<TValue>.Failure(DeviceStatus.结果和期望数据类型不同, errMsg);
            }
         }
         else
         {
            $"数据无法正常转换！".LogProcess(logHeader);
            return DeviceResult<TValue>.Failure(DeviceStatus.取值失败, "数据无法正常转换");
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
