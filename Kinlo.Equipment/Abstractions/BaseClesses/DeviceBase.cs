namespace Kinlo.Equipment.BaseClesses.Base;

public abstract class DeviceBase : IDevice
{
   public DeviceInfoModel DeviceInfo { get; set; }

   /// <summary>
   /// 网络连接
   /// </summary>
   public IConnect? Connect { get; set; }

   protected DeviceBase(DeviceInfoModel info)
   {
      DeviceInfo = info;
      Connect = CreateDeviceHelper.GetIConnect(this);
   }

   public virtual void Close() => Connect?.Close();

   public virtual bool Open() => Connect == null ? false : Connect.Open();

   protected bool IsShutdown => DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.IsCancellationRequested;

   // public virtual void Reconnect(IConnect connect) => CreateDeviceHelper.Reconnect(connect);

   public abstract DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class, new();
   public abstract DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   );
   public abstract bool WriteClass<TClass>(
      TClass value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class;
   public abstract bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   );
}
