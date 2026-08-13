namespace Kinlo.Equipment.Devices.ElectronicScales;

/// <summary>
/// 可竹电子称（KZ313）
/// </summary>
[DeviceConnec([CommunicationEnum.Scale_KZ313_RTU])]
public class Scale_KZ_KZ313Rtu : DeviceBase
{
   byte[] _readRequest = [0x01, 0x03, 0X00, 0x00, 0x00, 0x03, 0x05, 0xCB];
   byte[] _zeroClear = [0x01, 0x06, 0X00, 0x32, 0x00, 0x01, 0xE9, 0xC5];
   IProtocolHelper ProtocolHelper;

   public Scale_KZ_KZ313Rtu(DeviceInfoModel info)
      : base(info)
   {
      ProtocolHelper = new Modbus_RTU_Protocol();
      _readRequest = [0x01, 0x03, 0X00, 0x00, 0x00, 0x03, 0x05, 0xCB];
      _zeroClear = [0x01, 0x06, 0X00, 0x32, 0x00, 0x01, 0xE9, 0xC5];
   }

   public override bool Open()
   {
      if (base.Open())
      {
         return true;
      }
      return false;
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
   ///
   /// </summary>
   /// <typeparam name="TValue"></typeparam>
   /// <param name="address"></param>
   /// <param name="logHeader"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TValue : default
   {
      if (IsShutdown)
         return DeviceResult<TValue>.Failure(DeviceStatus.设备已停机);

      return ReadWeight<TValue>(Connect, _readRequest, ProtocolHelper, this, logHeader, options);
   }

   public static DeviceResult<TValue> ReadWeight<TValue>(
      IConnect connect,
      byte[] request,
      IProtocolHelper protocol,
      IDevice device,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      var result = DeviceResult<TValue>.Failure(DeviceStatus.Failure);
      options ??= new DeviceOperationOptions() { RetryCount = 3 };

      for (int k = 0; k < options.RetryCount; k++)
      {
         try
         {
            List<StableStatus> stableStatuses = new List<StableStatus>();
            for (int i = 0; i < 2; i++) //每组取n次，n次须全部稳定
            {
               connect.Close();
               connect.Open();
               var res = connect.WriteAndRead(request, protocol, logHeader, readLength: 20);
               connect.Close();
               var stableState = IsStable(res.Data!, i + 1);
               stableStatuses.Add(stableState);
               Thread.Sleep(100);
            }
            result = CalculateResult<TValue>(stableStatuses, k + 1, logHeader);
            if (result.Status == DeviceStatus.Success)
               return result;
         }
         catch (Exception ex)
         {
            var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
            result = DeviceResult<TValue>.Failure(DeviceStatus.异常, errMsg, ex);
         }
      }
      return result;
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

   /// <summary>
   /// 清零
   /// </summary>
   /// <param name="value"></param>
   /// <param name="address"></param>
   /// <param name="offset"></param>
   /// <param name="count"></param>
   /// <returns></returns>
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
      for (int i = 0; i < options.RetryCount; i++)
      {
         try
         {
            if (IsShutdown)
               return false;
            var rss = Connect.Write(_zeroClear, logHeader);
            Thread.Sleep(100);
            Connect.ClearCache(logHeader);
            return rss.State == CommState.Success;
         }
         catch (Exception ex)
         {
            if (IsShutdown)
               return false;
            $"清零异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
         }
         Thread.Sleep(200);
      }
      return false;
   }

   /// <summary>
   /// 稳定状态Record
   /// </summary>
   /// <param name="status"></param>
   /// <param name="bytes"></param>
   /// <param name="msg"></param>
   public record StableStatus(DeviceStatus status, byte[] bytes, string msg);

   /// <summary>
   /// 是否稳定
   /// </summary>
   /// <param name="bytes"></param>
   /// <param name="index"></param>
   /// <returns></returns>
   public static StableStatus IsStable(byte[] bytes, int index)
   {
      string msg = string.Empty;
      if (bytes != null && bytes.Count() >= 6)
      {
         if (((bytes[4] >> 1) & 1) == 0) //是否稳定
         {
            msg = $"第{index}次取到稳定值！字节:[{BitConverter.ToString(bytes)}];   ";
            return new StableStatus(DeviceStatus.Success, bytes, msg);
         }
         else
         {
            msg = $"第{index}次称重不稳定！字节:[{BitConverter.ToString(bytes)}];   ";
            return new StableStatus(DeviceStatus.称重不稳定, bytes, msg);
         }
      }
      else
      {
         msg = $"第{index}次读取字节不合法！字节:[{(bytes == null ? "null" : BitConverter.ToString(bytes))}];   ";
         return new StableStatus(DeviceStatus.取值失败, bytes, msg);
      }
   }

   /// <summary>
   /// 计算重量
   /// </summary>
   /// <typeparam name="TValue"></typeparam>
   /// <param name="isStableStatuses"></param>
   /// <param name="bytes"></param>
   /// <param name="index"></param>
   /// <param name="msg"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static DeviceResult<TValue> CalculateResult<TValue>(List<StableStatus> stableStatuses, int index, string logHeader)
   {
      string msg = string.Join(';', stableStatuses.Select(x => x.msg));

      if (stableStatuses.All(x => x.status == DeviceStatus.Success))
      {
         var bytes = stableStatuses[^1].bytes;
         var stableValue = (int)((bytes[2] << 24) | (bytes[3] << 16) | (bytes[0] << 8) | (bytes[1])); //重量 BitConverter.ToInt32([_bytes[1], _bytes[0], _bytes[3], _bytes[2]]);

         int decimalPlaces = (int)(bytes[5] >> 5); //小数有几位
         var scaledResult = (double)(stableValue / Math.Pow(10, decimalPlaces));

         $"第[{index}]组取得稳定值[{scaledResult}]；\r\n详情：{msg}".LogProcess(logHeader, Log4NetLevelEnum.成功);

         if (scaledResult is TValue tv)
         {
            return DeviceResult<TValue>.Success(tv);
         }
         else
         {
            var err = $"传入数据类型和实际类型不对应";
            err.LogProcess(logHeader, Log4NetLevelEnum.错误);
            return DeviceResult<TValue>.Failure(DeviceStatus.结果和期望数据类型不同, err);
         }
      }
      else if (stableStatuses.All(x => x.status == DeviceStatus.取值失败))
      {
         string err = $"第[{index + 1}]组取值失败!\r\n详情：{msg}";
         err.LogProcess(logHeader, Log4NetLevelEnum.警告);
         return DeviceResult<TValue>.Failure(DeviceStatus.取值失败, err);
      }
      else
      {
         string err = $"第[{index + 1}]组未取得稳定值!\r\n详情：{msg}";
         err.LogProcess(logHeader, Log4NetLevelEnum.警告);
         return DeviceResult<TValue>.Failure(DeviceStatus.称重不稳定, err);
      }
   }
}
