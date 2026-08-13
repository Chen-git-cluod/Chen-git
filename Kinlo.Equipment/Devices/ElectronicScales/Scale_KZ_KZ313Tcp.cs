using static Kinlo.Equipment.Devices.ElectronicScales.Scale_KZ_KZ313Rtu;

namespace Kinlo.Equipment.Devices.ElectronicScales;

/// <summary>
/// 可竹电子称（KZ313）
/// </summary>
[DeviceConnec([CommunicationEnum.Scale_KZ313_TCP])]
public class Scale_KZ_KZ313Tcp : DeviceBase
{
   byte[] _readRequest = [0x01, 0x03, 0X00, 0x00, 0x00, 0x03, 0x05, 0xCB];
   byte[] _zeroClear = [0x01, 0x06, 0X00, 0x32, 0x00, 0x01, 0xE9, 0xC5];
   IProtocolHelper ProtocolHelper;

   public Scale_KZ_KZ313Tcp(DeviceInfoModel info)
      : base(info)
   {
      ProtocolHelper = new Modbus_TCP_Protocol(1);
      _readRequest = [0x03, 0X00, 0x00, 0x00, 0x03];
      _zeroClear = [0x06, 0X00, 0x32, 0x00, 0x01];
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
   /// <param name="count"></param>
   /// <returns>-1为未取到值</returns>
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
            Connect.Close();
            Connect.Open();
            var r = Connect.Write(ProtocolHelper.Serialize(_zeroClear), logHeader);
            Connect.Close();
            return r.State == CommState.Success;
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
}
