namespace Kinlo.Equipment.Devices.ElectronicScales;

/// <summary>
/// 鑫精诚电子称
/// </summary>
[DeviceConnec([CommunicationEnum.Scale_XJC_C103])]
public class Scale_XJC_C103 : DeviceBase
{
   byte[] _readRequest = [0x01, 0x03, 0X00, 0x00, 0x00, 0x05, 0x85, 0xC9];
   byte[] _zeroClear = [0x01, 0x06, 0X21, 0x98, 0x00, 0x01, 0xC3, 0xD9];
   IProtocolHelper ProtocolHelper;

   public Scale_XJC_C103(DeviceInfoModel info)
      : base(info)
   {
      ProtocolHelper = new Modbus_RTU_Protocol();
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
   /// <param name="count"></param>
   /// <returns>-1为未取到值</returns>
   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TValue : default
   {
      var result = DeviceResult<TValue>.Failure(DeviceStatus.Failure);
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions() { RetryCount = 3 };
      for (int k = 0; k < options.RetryCount; k++) //最多测3组。每组取3次，任一组OK直接返回
      {
         if (IsShutdown)
            return DeviceResult<TValue>.Failure(DeviceStatus.设备已停机);

         byte[] bytes = [];
         DeviceStatus[] isStableStatuses = [DeviceStatus.Failure, DeviceStatus.Failure, DeviceStatus.Failure];
         StringBuilder msg = new StringBuilder();
         for (int i = 0; i < isStableStatuses.Length; i++)
         {
            Connect.Close();
            Connect.Open();
            var res = Connect.WriteAndRead(_readRequest, ProtocolHelper, logHeader, readLength: 20);
            Connect.Close();
            if (res.State != CommState.Success)
            {
               Thread.Sleep(300);
               break;
            }

            bytes = res.Data!;
            if (bytes != null && bytes.Count() >= 10)
            {
               if ((bytes[9] & 1) == 1) //1为稳定
               {
                  isStableStatuses[i] = DeviceStatus.Success;
                  msg.Append($"第{i + 1}次取到稳定值！字节:[{BitConverter.ToString(bytes)}];   ");
               }
               else
               {
                  isStableStatuses[i] = DeviceStatus.称重不稳定;
                  msg.Append($"第{i + 1}次称重不稳定！字节:[{BitConverter.ToString(bytes)}];   ");
               }
            }
            else
            {
               isStableStatuses[i] = DeviceStatus.取值失败;
               msg.Append($"第{i + 1}次读取字节不合法！字节:[{(bytes == null ? "null" : BitConverter.ToString(bytes))}];   ");
            }
            Thread.Sleep(100);
         }
         if (isStableStatuses.All(x => x == DeviceStatus.Success))
         {
            var stableValue = BitConverter.ToSingle([bytes[3], bytes[2], bytes[1], bytes[0]]);

            $"第[{k + 1}]组取得稳定值[{stableValue}]；\r\n详情：{msg}".LogProcess(logHeader, Log4NetLevelEnum.成功);
            if (stableValue is TValue tv)
            {
               return DeviceResult<TValue>.Success(tv);
            }
            else
            {
               var err = $"[{DeviceInfo.ProcessesType}]传入数据类型和实际类型不对应";
               err.LogProcess(logHeader, Log4NetLevelEnum.错误);
               return DeviceResult<TValue>.Failure(DeviceStatus.结果和期望数据类型不同, err);
            }
         }
         else if (isStableStatuses.All(x => x == DeviceStatus.取值失败))
         {
            string err = $"第[{k + 1}]组取值失败!\r\n详情：{msg}";
            err.LogProcess(logHeader, Log4NetLevelEnum.警告);
            result = DeviceResult<TValue>.Failure(DeviceStatus.取值失败, err);
         }
         else
         {
            string err = $"第[{k + 1}]组未取得稳定值!\r\n详情：{msg}";
            err.LogProcess(logHeader, Log4NetLevelEnum.警告);
            result = DeviceResult<TValue>.Failure(DeviceStatus.称重不稳定, err);
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
         if (IsShutdown)
            return false;

         try
         {
            Connect.Close();
            Connect.Open();
            var rss = Connect.Write(_zeroClear, logHeader);
            Connect.Close();
         }
         catch (Exception ex)
         {
            if (DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
               break;
            $"清零异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
         }
         Thread.Sleep(200);
      }
      return false;
   }
}
