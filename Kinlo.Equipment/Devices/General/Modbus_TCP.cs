using System.Formats.Asn1;
using System.Windows.Interop;

namespace Kinlo.Equipment.Devices.General;

[DeviceConnec([
   CommunicationEnum.Modbus_TCP_DCBA,
   CommunicationEnum.Modbus_TCP_CDAB,
   CommunicationEnum.Modbus_TCP_ABCD,
   CommunicationEnum.Modbus_TCP_BADC,
])]
public class Modbus_TCP : DeviceBase
{
   IProtocolHelper _protocolHelper;

   public Modbus_TCP(DeviceInfoModel info)
      : base(info)
   {
      _protocolHelper = new Modbus_TCP_Protocol(1);
   }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class
   {
      options ??= new DeviceOperationOptions();
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      var result = DeviceResult<TClass>.Failure(DeviceStatus.Failure);
      for (int i = 0; i < options.RetryCount; i++)
      {
         if (IsShutdown)
            return DeviceResult<TClass>.Failure(DeviceStatus.设备已停机);

         try
         {
            if (obj == null)
            {
               obj = Activator.CreateInstance<TClass>();
            }
            int classSize = (int)StructToBytes.GetClassSize(obj);
            if (classSize > 252)
            {
               var msg = $"[{Helper.GetCurrentMethodName()}] 超过Modbus最大字节数;";
               msg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               return DeviceResult<TClass>.Failure(DeviceStatus.数据类型不支持读取, msg);
            }
            byte[] bytes = new byte[5];
            bytes[0] = 0x03;
            bytes[1] = (byte)(address.Address >> 8);
            bytes[2] = (byte)address.Address;
            bytes[3] = (byte)(classSize >> 8);
            bytes[4] = (byte)classSize;
            var res = Connect.WriteAndRead(bytes, _protocolHelper, logHeader);
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
            StructToBytes.FromBytes(obj, bytes, ref boolSize, 0, DeviceInfo.Communication);
            return DeviceResult<TClass>.Success(obj);
         }
         catch (Exception ex)
         {
            var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
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
      options ??= new DeviceOperationOptions();
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      var result = DeviceResult<TValue>.Failure(DeviceStatus.Failure);
      for (int i = 0; i < options.RetryCount; i++)
      {
         if (IsShutdown)
            return DeviceResult<TValue>.Failure(DeviceStatus.设备已停机);

         try
         {
            logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
            byte[] bytes = new byte[5];
            bytes[0] = 0x03;
            bytes[1] = (byte)(address.Address >> 8);
            bytes[2] = (byte)address.Address;
            bytes[3] = 0x00;
            Type type = typeof(TValue);
            switch (type.Name)
            {
               case "Int32":
               case "UInt32":
               case "Single":
                  bytes[4] = 2;
                  break;
               case "Double":
                  bytes[4] = 4;
                  break;
               case "Int16":
               case "UInt16":
                  bytes[4] = 1;
                  break;
               case "Boolean":
               case "Byte":
               case "Int64":
               case "UInt64":
                  var msg = $"[{Helper.GetCurrentMethodName()}] 未实现读取{type.Name}数据;";
                  msg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
                  return DeviceResult<TValue>.Failure(DeviceStatus.数据类型不支持读取, msg);
            }
            var res = Connect.WriteAndRead(bytes, _protocolHelper, logHeader);

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
            bytes = res.Data!;
            var obj = (TValue)StructToBytes.GetValue(type, bytes, 0, DeviceInfo.Communication);
            return DeviceResult<TValue>.Success(obj);
         }
         catch (Exception ex)
         {
            var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
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
      string errMsg = string.Empty;
      options ??= new DeviceOperationOptions();
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
            if (classSize > 252)
            {
               throw new Exception("超过Modbus最大字节数");
            }
            byte[] array = new byte[classSize];
            List<byte> list = new List<byte>() { 0x10 };
            list.Add((byte)(address.Address >> 8));
            list.Add((byte)address.Address);
            int boolSize = 0;
            StructToBytes.ToBytes(value, array, ref boolSize, 0, DeviceInfo.Communication);
            list.Add((byte)((classSize / 2) >> 8));
            list.Add((byte)(classSize / 2));
            list.Add((byte)classSize);
            list.AddRange(array);
            var res = Connect.WriteAndRead(list.ToArray(), _protocolHelper, logHeader);

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
            var bytes = res.Data!;
            return bytes != null && bytes.Length > 8;
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
      string errMsg = string.Empty;
      options ??= new DeviceOperationOptions();
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
            List<byte> list = new List<byte>() { 0x10 };
            list.Add((byte)(address.Address >> 8));
            list.Add((byte)address.Address);
            byte[] bytes = null;
            switch (type.Name)
            {
               case "Int32":
               case "UInt32":
               case "Single":
                  bytes = new byte[4];
                  list.Add(0);
                  list.Add(2);
                  list.Add(4);
                  break;
               case "Double":
                  bytes = new byte[8];
                  list.Add(0);
                  list.Add(4);
                  list.Add(8);
                  break;
               case "Int16":
               case "UInt16":
                  list[0] = 0x06;
                  bytes = new byte[2];
                  break;
               case "Boolean":
               case "Byte":
               case "Int64":
               case "UInt64":
                  throw new Exception($"没有实现 {type.Name}");
               case "String":
                  encoding ??= Encoding.ASCII;
                  List<byte> list2 = new List<byte>(encoding.GetBytes((string)value));
                  if (list2.Count % 2 != 0)
                  {
                     list2.Add(0);
                  }
                  list.Add(0);
                  list.Add((byte)(list2.Count / 2));
                  list.Add((byte)list2.Count);
                  if (DeviceInfo.Communication == CommunicationEnum.Modbus_TCP_BADC) { }
                  bytes = list2.ToArray();
                  break;
            }
            if (type.Name != "String")
            {
               StructToBytes.GetBytes(value, bytes, 0, DeviceInfo.Communication);
            }
            list.AddRange(bytes);
            var res = Connect.WriteAndRead(list.ToArray(), _protocolHelper, logHeader);

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
            bytes = res.Data!;
            return bytes != null && bytes.Length > 8;
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
