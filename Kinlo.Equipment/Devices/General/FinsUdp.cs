namespace Kinlo.Equipment.Devices.General;

[DeviceConnec([CommunicationEnum.FinsUdpShortConn, CommunicationEnum.FinsUdpLongConn])]
public class FinsUdp : DeviceBase
{
   readonly object _lockObj = new object();
   public IProtocolHelper? ProtocolHelper { get; set; }

   public FinsUdp(DeviceInfoModel info)
      : base(info)
   {
      string[] ip = info.IPCOM.Split('.');
      var bindLast = Connect.DeviceInfo.BindIP.GetAddressBytes()[3];
      ProtocolHelper = new FinsUdpProtocol(Convert.ToByte(ip[3]), bindLast);
   }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass? obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class
   {
      string errMsg = string.Empty;
      options ??= new DeviceOperationOptions();
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      var result = DeviceResult<TClass>.Failure(DeviceStatus.Failure);
      lock (_lockObj)
      {
         for (int i = 0; i < options.RetryCount; i++)
         {
            if (IsShutdown)
               return DeviceResult<TClass>.Failure(DeviceStatus.设备已停机);

            try
            {
               if (DeviceInfo.Communication == CommunicationEnum.FinsUdpShortConn)
                  Connect.Open();

               obj ??= Activator.CreateInstance<TClass>();
               int classSize = (int)StructToBytes.GetClassSize(obj);
               if (classSize > 1998)
               {
                  var msg = $"[{Helper.GetCurrentMethodName()}] 超过Fins最大字节数;";
                  msg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
                  return DeviceResult<TClass>.Failure(DeviceStatus.Failure, msg);
               }
               byte[] bytes = new byte[7];
               bytes[0] = 0x01;
               bytes[1] = 0x82;
               bytes[2] = (byte)(address.Address >> 8);
               bytes[3] = (byte)address.Address;
               bytes[4] = 0;
               bytes[5] = (byte)(classSize >> 8);
               bytes[6] = (byte)classSize;
               var res = Connect.WriteAndRead(bytes, ProtocolHelper, logHeader);

               if (res.State == CommState.Failed)
               {
                  result = DeviceResult<TClass>.Failure(DeviceStatus.Failure);
                  Thread.Sleep(300);
                  continue;
               }
               else if (res.State == CommState.NeedReconnect)
               {
                  result = DeviceResult<TClass>.Failure(DeviceStatus.通信失败);
                  if (!this.Reconnect(logHeader))
                     return result;
                  continue;
               }
               bytes = res.Data!;
               int boolSize = 0;
               StructToBytes.FromBytes(obj, bytes, ref boolSize, 0, Connect.DeviceInfo.Communication);
               return DeviceResult<TClass>.Success(obj);
            }
            catch (Exception ex)
            {
               errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
               errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               result = DeviceResult<TClass>.Failure(DeviceStatus.异常, errMsg);
            }
            finally
            {
               if (DeviceInfo.Communication == CommunicationEnum.FinsUdpShortConn && Connect != null)
                  Connect.Close();
            }
         }
      }
      return result;
   }

   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TValue : default
   {
      string errMsg = string.Empty;
      options ??= new DeviceOperationOptions();
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      var result = DeviceResult<TValue>.Failure(DeviceStatus.Failure);
      lock (_lockObj)
      {
         for (int k = 0; k < options.RetryCount; k++)
         {
            if (IsShutdown)
               return DeviceResult<TValue>.Failure(DeviceStatus.设备已停机);

            try
            {
               if (DeviceInfo.Communication == CommunicationEnum.FinsUdpShortConn)
                  Connect.Open();

               byte[] list = new byte[7];
               list[0] = 0x01;
               list[1] = 0x82;
               list[2] = (byte)(address.Address >> 8);
               list[3] = (byte)address.Address;
               list[4] = 0x00;
               Type type = typeof(TValue);
               if (type.IsArray)
               {
                  var msg = $"[{Helper.GetCurrentMethodName()}] 未实现读取数组数据;";
                  msg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
                  return DeviceResult<TValue>.Failure(DeviceStatus.数据类型不支持读取, msg);
               }
               int count_length = 0;
               switch (type.Name)
               {
                  case "Int32":
                  case "UInt32":
                  case "Single":
                     count_length = 2;
                     break;
                  case "Int64":
                  case "UInt64":
                  case "Double":
                     count_length = 4;
                     break;
                  case "Int16":
                  case "UInt16":
                     count_length = 1;
                     break;
                  case "Byte":
                  case "String":
                     count_length = address.Length / 2;
                     break;
                  case "Boolean":
                     var msg = $"[{Helper.GetCurrentMethodName()}] 未实现读取{type.Name}数据;";
                     msg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
                     return DeviceResult<TValue>.Failure(DeviceStatus.数据类型不支持读取, msg);
               }
               list[5] = (byte)(count_length >> 8);
               list[6] = (byte)count_length;
               var res = Connect.WriteAndRead(list, ProtocolHelper, logHeader);

               if (res.State == CommState.Failed)
               {
                  result = DeviceResult<TValue>.Failure(DeviceStatus.Failure);
                  Thread.Sleep(300);
                  continue;
               }
               else if (res.State == CommState.NeedReconnect)
               {
                  result = DeviceResult<TValue>.Failure(DeviceStatus.通信失败);
                  if (!this.Reconnect(logHeader))
                     return result;
                  continue;
               }
               var bytes = res.Data!;
               if (type.Name == "String")
               {
                  for (int i = 3; i < bytes[0]; i = i + 2)
                  {
                     byte t = bytes[i - 1];
                     bytes[i - 1] = bytes[i];
                     bytes[i] = t;
                  }
                  var strValue = (TValue)(object)Encoding.ASCII.GetString(bytes.Skip(2).Take(bytes[1] - 48).ToArray());
                  return DeviceResult<TValue>.Success(strValue);
               }
               var obj = (TValue)StructToBytes.GetValue(type, bytes, 0, Connect.DeviceInfo.Communication);
               return DeviceResult<TValue>.Success(obj);
            }
            catch (Exception ex)
            {
               errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
               errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               result = DeviceResult<TValue>.Failure(DeviceStatus.异常);
            }
            finally
            {
               if (DeviceInfo.Communication == CommunicationEnum.FinsUdpShortConn && Connect != null)
                  Connect.Close();
            }
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
      string errMsg = string.Empty;
      options ??= new DeviceOperationOptions();
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      lock (_lockObj)
      {
         for (int i = 0; i < options.RetryCount; i++)
         {
            if (IsShutdown)
               return false;
            try
            {
               if (DeviceInfo.Communication == CommunicationEnum.FinsUdpShortConn)
                  Connect.Open();

               if (value == null)
               {
                  throw new Exception("不要传递空对象");
               }
               int classSize = (int)StructToBytes.GetClassSize(value);
               if (classSize > 1998)
               {
                  throw new Exception("超过Fins最大字节数");
               }
               byte[] array = new byte[classSize];
               List<byte> list = new List<byte> { 0x02, 0x82, (byte)(address.Address >> 8), (byte)address.Address, 0 };
               int boolSize = 0;
               StructToBytes.ToBytes(value, array, ref boolSize, 0, Connect.DeviceInfo.Communication);
               int length = classSize / 2;
               list.Add((byte)(length >> 8));
               list.Add((byte)length);
               list.AddRange(array);
               var res = Connect.WriteAndRead(list.ToArray(), ProtocolHelper, logHeader);
               errMsg = res.Message;
               if (res.State == CommState.Failed)
                  continue;
               else if (res.State == CommState.NeedReconnect)
               {
                  if (!this.Reconnect(logHeader))
                     return false;
                  continue;
               }
               return res.State == CommState.Success;
            }
            catch (Exception ex)
            {
               $"异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
               return default;
            }
            finally
            {
               if (DeviceInfo.Communication == CommunicationEnum.FinsUdpShortConn && Connect != null)
                  Connect.Close();
            }
         }
         return false;
      }
   }

   public override bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   )
   {
      string errMsg = string.Empty;
      options ??= new DeviceOperationOptions();
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      lock (_lockObj)
      {
         for (int k = 0; k < options.RetryCount; k++)
         {
            if (IsShutdown)
               return false;
            try
            {
               Type type = value.GetType();
               if (type.IsArray)
               {
                  throw new Exception("请使用Write方法写入数组数据");
               }
               List<byte> list = new List<byte> { 0x02, 0x82, (byte)(address.Address >> 8), (byte)address.Address, (byte)address.Offset };
               byte[] bytes = null;
               switch (type.Name)
               {
                  case "Int32":
                  case "UInt32":
                  case "Single":
                     bytes = new byte[4];
                     break;
                  case "Int64":
                  case "UInt64":
                  case "Double":
                     bytes = new byte[8];
                     break;
                  case "Int16":
                  case "UInt16":
                     bytes = new byte[2];
                     break;
                  case "Byte":
                  case "Boolean":
                     throw new Exception($"没有实现 {type.Name}");
                  case "String":
                     encoding ??= Encoding.ASCII;
                     byte[] code = encoding.GetBytes((string)value);
                     List<byte> list2 = new List<byte> { 49, (byte)(code.Length + 48) };
                     list2.AddRange(code);
                     if (list2.Count % 2 != 0)
                     {
                        list2.Add(0);
                     }
                     for (int i = 3; i < list2.Count; i = i + 2)
                     {
                        byte t = list2[i - 1];
                        list2[i - 1] = list2[i];
                        list2[i] = t;
                     }
                     bytes = list2.ToArray();
                     break;
               }
               int length1 = (bytes.Length / 2);
               list.Add((byte)(length1 >> 8));
               list.Add((byte)length1);
               if (type.Name != "String")
               {
                  StructToBytes.GetBytes(value, bytes, 0, Connect.DeviceInfo.Communication);
               }
               list.AddRange(bytes);
               var res = Connect.WriteAndRead(list.ToArray(), ProtocolHelper, logHeader);
               errMsg = res.Message;
               if (res.State == CommState.Failed)
                  continue;
               else if (res.State == CommState.NeedReconnect)
               {
                  if (!this.Reconnect(logHeader))
                     return false;
                  continue;
               }
               return res.State == CommState.Success;
            }
            catch (Exception ex)
            {
               $"异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
               return default;
            }
            finally
            {
               if (DeviceInfo.Communication == CommunicationEnum.FinsUdpShortConn && Connect != null)
                  Connect.Close();
            }
         }
         return false;
      }
   }
}
