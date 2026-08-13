using System.Text;
using Kinlo.Equipment.Interfaces;
using Kinlo.Equipment.Models;

namespace Kinlo.Test.Devices;

public sealed class FakePlc : IPLC
{
   public DeviceInfoModel DeviceInfo
   {
      get => throw new NotImplementedException();
      set => throw new NotImplementedException();
   }

   public void Close()
   {
      throw new NotImplementedException();
   }

   public bool Open()
   {
      throw new NotImplementedException();
   }

   public DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class, new()
   {
      throw new NotImplementedException();
   }

   public DeviceResult<List<TClass>> ReadClasses<TClass>(
      SignalAddressModel[] addresses,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class, new()
   {
      throw new NotImplementedException();
   }

   public DeviceResult<TClass> ReadLargeClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class, new()
   {
      throw new NotImplementedException();
   }

   public DeviceResult<List<T>> ReadLargeObjects<T>(SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null)
   {
      throw new NotImplementedException();
   }

   public DeviceResult<List<object>> ReadObjects(SignalAddressModel[] addresses, string logHeader, DeviceOperationOptions? options = null)
   {
      throw new NotImplementedException();
   }

   public DeviceResult<TValue> ReadValue<TValue>(SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null)
   {
      throw new NotImplementedException();
   }

   public DeviceResult<List<TValue>> ReadValues<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      throw new NotImplementedException();
   }

   public void Reconnect(IConnect? connect)
   {
      throw new NotImplementedException();
   }

   public DeviceResult<TClass> Scan<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class, new()
   {
      throw new NotImplementedException();
   }

   public bool WriteClass<TClass>(TClass value, SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null)
      where TClass : class
   {
      throw new NotImplementedException();
   }

   public bool WriteLargeClass<TClass>(TClass value, SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null)
      where TClass : class
   {
      throw new NotImplementedException();
   }

   public bool WriteLargeValues<TValue>(
      List<TValue> values,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      throw new NotImplementedException();
   }

   public bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   )
   {
      throw new NotImplementedException();
   }
}
