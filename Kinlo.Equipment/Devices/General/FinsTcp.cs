using System.Formats.Asn1;

namespace Kinlo.Equipment.Devices.General;

[DeviceConnec([CommunicationEnum.FinsTcp])]
public class FinsTcp : DeviceBase
{
   public IProtocolHelper? ProtocolHelper { get; set; }
   private List<byte> _handshake = new List<byte>();

   public FinsTcp(DeviceInfoModel info)
      : base(info)
   {
      #region 握手报文
      _handshake =
      [
         0x46,
         0x49,
         0x4e,
         0x53, //FINS
         0x00,
         0x00,
         0x00,
         0x0c, //发送数据长度：命令码开始的字节数量
         0x00,
         0x00,
         0x00,
         0x00, //命令码
         0x00,
         0x00,
         0x00,
         0x00, //错误代码
         0x00,
         0x00,
         0x00,
         info.BindIP.GetAddressBytes()[3],
      ]; //客户端节点
      #endregion
   }

   public override bool Open() => this.BuildFins(_handshake);

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class
   {
      var result = DeviceResult<TClass>.Failure(DeviceStatus.Failure);
      options ??= new DeviceOperationOptions();
      string errMsg = string.Empty;
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      for (int i = 0; i < options.RetryCount; i++)
      {
         if (IsShutdown)
            return DeviceResult<TClass>.Failure(DeviceStatus.设备已停机);

         try
         {
            obj ??= Activator.CreateInstance<TClass>();
            int classSize = (int)StructToBytes.GetClassSize(obj);
            if (classSize > 1998)
            {
               var msg = $"[{Helper.GetCurrentMethodName()}] 超过Fins最大字节数;";
               msg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               return DeviceResult<TClass>.Failure(DeviceStatus.报文校验不通过, msg);
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

            errMsg = res.Message;
            if (res.State == CommState.Failed)
            {
               result = DeviceResult<TClass>.Failure(DeviceStatus.Failure, errMsg);
               continue;
            }
            else if (res.State == CommState.NeedReconnect)
            {
               result = DeviceResult<TClass>.Failure(DeviceStatus.通信失败, errMsg);
               if (!this.ReconnectFins(_handshake))
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
      var result = DeviceResult<TValue>.Failure(DeviceStatus.Failure);
      options ??= new DeviceOperationOptions();
      string errMsg = string.Empty;
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      for (int i = 0; i < options.RetryCount; i++)
      {
         if (IsShutdown)
            return DeviceResult<TValue>.Failure(DeviceStatus.设备已停机);

         try
         {
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
               return DeviceResult<TValue>.Failure(DeviceStatus.Failure, msg);
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
                  return DeviceResult<TValue>.Failure(DeviceStatus.Failure, msg);
            }
            list[5] = (byte)(count_length >> 8);
            list[6] = (byte)count_length;
            var res = Connect.WriteAndRead(list, ProtocolHelper, logHeader);

            errMsg = res.Message;
            if (res.State == CommState.Failed)
            {
               result = DeviceResult<TValue>.Failure(DeviceStatus.Failure, errMsg);
               continue;
            }
            else if (res.State == CommState.NeedReconnect)
            {
               result = DeviceResult<TValue>.Failure(DeviceStatus.通信失败, errMsg);
               if (!this.ReconnectFins(_handshake))
                  return result;

               continue;
            }
            var bytes = res.Data!;

            if (type.Name == "String")
            {
               for (int k = 3; k < bytes[0]; k = k + 2)
               {
                  byte t = bytes[k - 1];
                  bytes[k - 1] = bytes[k];
                  bytes[k] = t;
               }
               var strValue = (TValue)(object)Encoding.ASCII.GetString(bytes.Skip(2).ToArray());
               return DeviceResult<TValue>.Success(strValue);
            }
            var obj = (TValue)StructToBytes.GetValue(type, bytes, 0, Connect.DeviceInfo.Communication);
            return DeviceResult<TValue>.Success(obj);
         }
         catch (Exception ex)
         {
            errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
            result = DeviceResult<TValue>.Failure(DeviceStatus.异常, errMsg);
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
      options ??= new DeviceOperationOptions();
      string errMsg = string.Empty;
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      for (int i = 0; i < options.RetryCount; i++)
      {
         if (IsShutdown)
            return false;
         try
         {
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
               if (!this.ReconnectFins(_handshake))
                  return false;

               continue;
            }
            return res.State == CommState.Success;
         }
         catch (Exception ex)
         {
            errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         }
      }
      return false;
   }

   public override bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   )
   {
      options ??= new DeviceOperationOptions();
      string errMsg = string.Empty;
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      for (int i = 0; i < options.RetryCount; i++)
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
                  for (int k = 3; k < list2.Count; k = k + 2)
                  {
                     byte t = list2[k - 1];
                     list2[k - 1] = list2[k];
                     list2[k] = t;
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
               if (!this.ReconnectFins(_handshake))
                  return false;

               continue;
            }
            return res.State == CommState.Success;
         }
         catch (Exception ex)
         {
            errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         }
      }
      return false;
   }

   public static int ByteToInt(byte[] bytes) => (bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3];
}
