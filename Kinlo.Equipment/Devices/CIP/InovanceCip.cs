namespace Kinlo.Equipment.Devices.CIP;

/// <summary>
/// 汇川PLC
/// </summary>
[DeviceConnec([CommunicationEnum.CipInovance])]
public class InovanceCip : CipBase
{
   public InovanceCip(DeviceInfoModel info)
      : base(info) { }

   public override bool Open() //全部为有连接模式
   {
      string logHeader = DeviceInfo.ToDeviceLogHeader();
      Close();
      for (int i = 0; i < 4; i++)
      {
         var plcConnect = this.BuildCip(CipMode.有连接模式, i + 1, logHeader);
         if (plcConnect == null)
         {
            Close();
            return false;
         }
         if (i == 0)
            ScanConnected = plcConnect;
         else
            Connected.Add(plcConnect);
      }
      return true;
   }

   public override void Close()
   {
      string logHeader = DeviceInfo.ToDeviceLogHeader();
      try
      {
         ScanConnected.Close(logHeader);
         while (Connected.TryTake(out var plcConn))
         {
            plcConn.Close(logHeader);
         }
      }
      catch (Exception ex)
      {
         $"关闭异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }
   }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class => ReadLargeClass(address, obj, logHeader, options);

   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   ) => ReadValueCore<TValue>(Connected, address, logHeader, options);

   public override DeviceResult<List<TValue>> ReadValues<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   ) => ReadValuesCore<TValue>(Connected, address, logHeader, options);

   public override DeviceResult<List<object>> ReadObjects(
      SignalAddressModel[] addresses,
      string logHeader,
      DeviceOperationOptions? options = null
   ) => ReadObjectsCore(Connected, addresses, logHeader, options);

   public override DeviceResult<List<TClass>> ReadClasses<TClass>(
      SignalAddressModel[] addresses,
      string logHeader,
      DeviceOperationOptions? options = null
   ) => ReadClassesCore<TClass>(Connected, addresses, logHeader, options);

   public override bool WriteClass<TClass>(
      TClass value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   ) => WriteClassCore(Connected, value, address, logHeader, options);

   public override bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   ) => WriteValueCore(Connected, value, address, logHeader, options, encoding);
}
