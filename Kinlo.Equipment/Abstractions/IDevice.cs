namespace Kinlo.Equipment.Interfaces;

/// <summary>
///
/// </summary>
public interface IDevice
{
   DeviceInfoModel DeviceInfo { get; set; }

   bool Open();
   void Close();
   DeviceResult<TValue> ReadValue<TValue>(SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null);

   DeviceResult<TClass> ReadClass<TClass>(SignalAddressModel address, TClass obj, string logHeader, DeviceOperationOptions? options = null)
      where TClass : class, new();
   bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   );

   bool WriteClass<TClass>(TClass value, SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null)
      where TClass : class;

   /// <summary>
   /// 重连方法，部分USB等没有IConnect设备需重写断线重连方法
   /// </summary>
   // void Reconnect(IConnect? connect);
}
