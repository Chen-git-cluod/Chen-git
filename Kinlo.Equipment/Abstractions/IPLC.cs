namespace Kinlo.Equipment.Interfaces;

public interface IPLC : IDevice
{
   /// <summary>
   ///  单地址（标签）读取值类型list
   /// </summary>
   /// <typeparam name="TValue"></typeparam>
   /// <param name="address"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   DeviceResult<List<TValue>> ReadValues<TValue>(SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null);

   /// <summary>
   /// 多地址（标签）读取类list
   /// </summary>
   /// <typeparam name="TClass"></typeparam>
   /// <param name="addInfo"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   DeviceResult<List<TClass>> ReadClasses<TClass>(SignalAddressModel[] addresses, string logHeader, DeviceOperationOptions? options = null)
      where TClass : class, new();

   /// <summary>
   /// 多地址（标签）读取object类型list
   /// </summary>
   /// <param name="addInfo"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   DeviceResult<List<object>> ReadObjects(SignalAddressModel[] addresses, string logHeader, DeviceOperationOptions? options = null);

   /// <summary>
   /// 读取一个超大类，通常用于PLC等设备的标签读取
   /// </summary>
   /// <typeparam name="TClass"></typeparam>
   /// <param name="address"></param>
   /// <param name="obj"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   DeviceResult<TClass> ReadLargeClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class, new();

   /// <summary>
   /// 读取超大类型数据list，通常用于PLC等设备的标签读取
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="address"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   DeviceResult<List<T>> ReadLargeObjects<T>(SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null);

   /// <summary>
   ///  PLC专用扫描方法
   /// </summary>
   /// <typeparam name="TClass"></typeparam>
   /// <param name="address"></param>
   /// <param name="obj"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   DeviceResult<TClass> Scan<TClass>(SignalAddressModel address, TClass obj, string logHeader, DeviceOperationOptions? options = null)
      where TClass : class, new();

   /// <summary>
   /// 写入一个超大类，通常用于PLC等设备的标签读取
   /// </summary>
   /// <typeparam name="TClass"></typeparam>
   /// <param name="value"></param>
   /// <param name="address"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   bool WriteLargeClass<TClass>(TClass value, SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null)
      where TClass : class;

   /// <summary>
   /// 写入值类型（含string）List
   /// </summary>
   /// <typeparam name="TClass"></typeparam>
   /// <param name="value"></param>
   /// <param name="address"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   bool WriteLargeValues<TValue>(List<TValue> values, SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null);
}
