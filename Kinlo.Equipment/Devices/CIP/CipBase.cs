namespace Kinlo.Equipment.Devices.CIP;

public abstract class CipBase : IPLC
{
   #region Properties
   /// <summary>
   /// 扫描线程专用连接
   /// </summary>
   protected CipClient? ScanConnected = null;

   /// <summary>
   /// PLC队列（无连接）
   /// </summary>
   protected readonly BlockingCollection<CipClient> Unconnected = new();

   /// <summary>
   /// PLC队列（有连接）
   /// </summary>
   protected readonly BlockingCollection<CipClient> Connected = new();
   public DeviceInfoModel DeviceInfo { get; set; }
   #endregion
   public CipBase(DeviceInfoModel info) => DeviceInfo = info;

   public abstract bool Open();
   public abstract void Close();

   public virtual DeviceResult<TClass> Scan<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class, new()
   {
      try
      {
         logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
         options ??= new DeviceOperationOptions();
         obj ??= Activator.CreateInstance<TClass>();

         var lableBytes = ParseCipLabel.LabelReadRequest(address.Lable, address.Length);
         var readRes = CipExtensions.CipExecuteWithRetry(
            lableBytes,
            options.RetryCount,
            logHeader,
            DeviceInfo.TaskToken,
            ref ScanConnected
         );

         if (readRes.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readRes.Status, readRes.ErrorMessage!, readRes.Exception);

         var bytes = readRes.Value!;
         int boolSize = 0;
         StructToBytes.FromBytes(obj, bytes.Skip(4).ToArray(), ref boolSize, 0, DeviceInfo.Communication);
         return DeviceResult<TClass>.Success(obj);
      }
      catch (Exception ex)
      {
         var res = DeviceResult<TClass>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         res.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return res;
      }
   }

   public virtual DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class, new()
   {
      try
      {
         logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
         options ??= new DeviceOperationOptions();
         obj ??= Activator.CreateInstance<TClass>();

         var lableBytes = ParseCipLabel.LabelReadRequest(address.Lable, address.Length);
         var readRes = Unconnected.CipExecuteRequest(lableBytes, options.RetryCount, logHeader, DeviceInfo.TaskToken);

         if (readRes.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readRes.Status, readRes.ErrorMessage!, readRes.Exception);

         var bytes = readRes.Value!;
         int boolSize = 0;
         StructToBytes.FromBytes(obj, bytes.Skip(4).ToArray(), ref boolSize, 0, DeviceInfo.Communication);
         return DeviceResult<TClass>.Success(obj);
      }
      catch (Exception ex)
      {
         var res = DeviceResult<TClass>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         res.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return res;
      }
   }

   public virtual DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   ) => ReadValueCore<TValue>(Unconnected, address, logHeader, options);

   public DeviceResult<TValue> ReadValueCore<TValue>(
      BlockingCollection<CipClient> connQueue,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      try
      {
         logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
         options ??= new DeviceOperationOptions();

         var lableBytes = ParseCipLabel.LabelReadRequest(address.Lable, address.Length);
         var readRes = connQueue.CipExecuteRequest(lableBytes, options.RetryCount, logHeader, DeviceInfo.TaskToken);
         if (readRes.Status != DeviceStatus.Success)
            return DeviceResult<TValue>.Failure(readRes.Status, readRes.ErrorMessage!, readRes.Exception);

         Type type = typeof(TValue);
         var bytes = readRes.Value!;
         var result = bytes[0] switch
         {
            0xD0 => new Func<TValue>(() =>
            {
               var rsBytes = bytes.Skip(4).Take(bytes[2]).ToArray();
               object str = Encoding.ASCII.GetString(rsBytes);
               return (TValue)str;
            })(),
            _ => (TValue)StructToBytes.GetValue(type, bytes.Skip(2).ToArray(), 0, DeviceInfo.Communication),
         };
         return DeviceResult<TValue>.Success(result);
      }
      catch (Exception ex)
      {
         var res = DeviceResult<TValue>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         res.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return res;
      }
   }

   public virtual DeviceResult<List<TValue>> ReadValues<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   ) => ReadValuesCore<TValue>(Unconnected, address, logHeader, options);

   public DeviceResult<List<TValue>> ReadValuesCore<TValue>(
      BlockingCollection<CipClient> connQueue,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      try
      {
         logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
         options ??= new DeviceOperationOptions();

         var lableBytes = ParseCipLabel.LabelReadRequest(address.Lable, address.Length);
         var readRes = connQueue.CipExecuteRequest(lableBytes, options.RetryCount, logHeader, DeviceInfo.TaskToken);
         if (readRes.Status != DeviceStatus.Success)
            return DeviceResult<List<TValue>>.Failure(readRes.Status, readRes.ErrorMessage!, readRes.Exception);

         List<TValue> obj = new List<TValue>();
         var bytes = readRes.Value!;
         int start = 2;

         var name = typeof(TValue).Name;
         while (start < bytes.Length)
         {
            switch (name)
            {
               case "String":
                  int strLen = bytes[start] + (bytes[start + 1] << 8);
                  string strValue = Encoding.ASCII.GetString(bytes.Skip(start + 2).Take(strLen).ToArray());
                  obj.Add((TValue)(object)strValue);
                  start = start + strLen + 2;
                  break;
               default:
                  var info = CIPDataInfoHelper.CIPDataTypeInfos.First(x => x.PropertyName == name);
                  obj.Add((TValue)StructToBytes.GetValue(info.DataType, bytes.Skip(start).ToArray(), 0, DeviceInfo.Communication));
                  start += info.Length;
                  break;
            }
         }
         return DeviceResult<List<TValue>>.Success(obj);
      }
      catch (Exception ex)
      {
         var res = DeviceResult<List<TValue>>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         res.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return res;
      }
   }

   public virtual DeviceResult<List<object>> ReadObjects(
      SignalAddressModel[] addresses,
      string logHeader,
      DeviceOperationOptions? options = null
   ) => ReadObjectsCore(Unconnected, addresses, logHeader, options);

   public DeviceResult<List<object>> ReadObjectsCore(
      BlockingCollection<CipClient> connQueue,
      SignalAddressModel[] addresses,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      try
      {
         logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, addresses);
         options ??= new DeviceOperationOptions();

         var lableBytes = ParseCipLabel.MultipleLableReadRequest(addresses);
         var readRes = connQueue.CipExecuteRequest(lableBytes, options.RetryCount, logHeader, DeviceInfo.TaskToken);
         if (readRes.Status != DeviceStatus.Success)
            return DeviceResult<List<object>>.Failure(readRes.Status, readRes.ErrorMessage!, readRes.Exception);

         List<object> objects = new List<object>();
         var bytes = readRes.Value!;
         int label_num = bytes[0] + (bytes[1] << 8);
         int stat = (label_num * 2 + 2);

         while (stat < bytes.Length)
         {
            if (bytes[stat] == 0xCC)
            {
               stat += 4;
            }
            stat += 2;
            switch (bytes[stat - 2])
            {
               case 0xD0:
                  int strLen = bytes[stat] + (bytes[stat + 1] << 8);
                  string strValue = Encoding.ASCII.GetString(bytes.Skip(stat + 2).Take(strLen).ToArray());
                  objects.Add(strValue);
                  stat = stat + strLen + 2;
                  break;
               default:
                  var info = CIPDataInfoHelper.CIPDataTypeInfos.First(x => x.PropertyByre == bytes[stat - 2]);
                  if (info == null)
                     objects.Add(null);
                  objects.Add(StructToBytes.GetValue(info.DataType, bytes.Skip(stat).ToArray(), 0, DeviceInfo.Communication));
                  stat += info.Length;
                  break;
            }
         }
         return DeviceResult<List<object>>.Success(objects);
      }
      catch (Exception ex)
      {
         var res = DeviceResult<List<object>>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         res.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return res;
      }
   }

   public virtual DeviceResult<List<TClass>> ReadClasses<TClass>(
      SignalAddressModel[] addresses,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class, new() => ReadClassesCore<TClass>(Unconnected, addresses, logHeader, options);

   public DeviceResult<List<TClass>> ReadClassesCore<TClass>(
      BlockingCollection<CipClient> connQueue,
      SignalAddressModel[] addresses,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class, new()
   {
      try
      {
         logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, addresses);
         options ??= new DeviceOperationOptions();

         var lableBytes = ParseCipLabel.MultipleLableReadRequest(addresses);
         var readRes = connQueue.CipExecuteRequest(lableBytes, options.RetryCount, logHeader, DeviceInfo.TaskToken);
         if (readRes.Status != DeviceStatus.Success)
            return DeviceResult<List<TClass>>.Failure(readRes.Status, readRes.ErrorMessage!, readRes.Exception);

         var bytes = readRes.Value!;
         int label_num = bytes[0] + (bytes[1] << 8);
         int stat = (label_num * 2 + 2);
         int size = 0;
         List<TClass> datas = new List<TClass>();
         while (stat < bytes.Length)
         {
            if (bytes[stat] == 0xCC)
            {
               stat += 8;
            }
            var obj = Activator.CreateInstance<TClass>();
            double sizt_count = StructToBytes.FromBytes(obj, bytes.Skip(stat).ToArray(), ref size, 0, DeviceInfo.Communication);
            datas.Add(obj);
            stat += (int)sizt_count;
         }
         return DeviceResult<List<TClass>>.Success(datas);
      }
      catch (Exception ex)
      {
         var res = DeviceResult<List<TClass>>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         res.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return res;
      }
   }

   #region Class3-有连接

   /// <summary>
   /// CIP协议为有连接读取超大类（最大支持 1996 byte）
   /// </summary>
   /// <typeparam name="TClass"></typeparam>
   /// <param name="address"></param>
   /// <param name="obj"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public virtual DeviceResult<TClass> ReadLargeClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class, new()
   {
      try
      {
         logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
         options ??= new DeviceOperationOptions();

         var lableBytes = ParseCipLabel.LabelReadRequest(address.Lable, address.Length);
         var readRes = Connected.CipExecuteRequest(lableBytes, options.RetryCount, logHeader, DeviceInfo.TaskToken);
         if (readRes.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readRes.Status, readRes.ErrorMessage!, readRes.Exception);

         var bytes = readRes.Value!;
         int boolSize = 0;
         StructToBytes.FromBytes(obj, bytes.Skip(4).ToArray(), ref boolSize, 0, DeviceInfo.Communication);
         return DeviceResult<TClass>.Success(obj);
      }
      catch (Exception ex)
      {
         var res = DeviceResult<TClass>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         res.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return res;
      }
   }

   /// <summary>
   /// 有连接读取超大数据（最大支持 1996 byte）
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="address"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public virtual DeviceResult<List<T>> ReadLargeObjects<T>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      try
      {
         Type type = typeof(T);
         if (type.Name == "String")
         {
            return DeviceResult<List<T>>.Failure("协议不支持字符串数组!!!");
         }

         logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
         options ??= new DeviceOperationOptions();

         var lableBytes = ParseCipLabel.LabelReadRequest(address.Lable, address.Length);
         var readRes = Connected.CipExecuteRequest(lableBytes, options.RetryCount, logHeader, DeviceInfo.TaskToken);
         if (readRes.Status != DeviceStatus.Success)
            return DeviceResult<List<T>>.Failure(readRes.Status, readRes.ErrorMessage!, readRes.Exception);

         var bytes = readRes.Value!;
         int size = 0;
         List<T> datas = new List<T>();
         int stat = 4;
         if (type.BaseType.Name == "ValueType")
         {
            stat = 2;
            if (bytes[0] == 0xc1)
            {
               var bs = bytes.Skip(2).ToArray();
               foreach (var item in bs)
               {
                  for (int i = 0; i < 8; i++)
                  {
                     var b = (item >> i) & 1;
                     bool bb = b == 1;
                     if (bb is T b3)
                        datas.Add(b3);
                  }
               }
            }
            else
            {
               while (stat < bytes.Length)
               {
                  var info = CIPDataInfoHelper.CIPDataTypeInfos.First(x => x.DataType == type);
                  datas.Add((T)StructToBytes.GetValue(type, bytes.Skip(stat).ToArray(), 0, DeviceInfo.Communication));
                  stat += info.Length;
               }
            }
         }
         else
         {
            while (stat < bytes.Length)
            {
               var obj = Activator.CreateInstance<T>();
               double sizt_count = StructToBytes.FromBytes(obj, bytes.Skip(stat).ToArray(), ref size, 0, DeviceInfo.Communication);
               datas.Add(obj);
               stat += (int)sizt_count;
            }
         }
         return DeviceResult<List<T>>.Success(datas);
      }
      catch (Exception ex)
      {
         var res = DeviceResult<List<T>>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         res.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return res;
      }
   }

   public virtual bool WriteLargeClass<TClass>(
      TClass value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class => WriteClassCore(Connected, value, address, logHeader, options);
   #endregion
   public virtual bool WriteClass<TClass>(
      TClass value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class => WriteClassCore(Unconnected, value, address, logHeader, options);

   public bool WriteClassCore<TClass>(
      BlockingCollection<CipClient> connQueue,
      TClass value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class
   {
      try
      {
         logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
         options ??= new DeviceOperationOptions();

         var readRes = connQueue.CipExecuteRequest(DeviceInfo, address.Lable, value, options.RetryCount, logHeader);
         return readRes.Status == DeviceStatus.Success;
      }
      catch (Exception ex)
      {
         var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
         errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return false;
      }
   }

   public virtual bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   ) => WriteValueCore(Unconnected, value, address, logHeader, options, encoding);

   public bool WriteValueCore(
      BlockingCollection<CipClient> connQueue,
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   )
   {
      try
      {
         logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
         options ??= new DeviceOperationOptions();

         var lableBytes = ParseCipLabel.LableWriteValueRequest(address.Lable, value, DeviceInfo.Communication, encoding);
         var readRes = connQueue.CipExecuteRequest(lableBytes, options.RetryCount, logHeader, DeviceInfo.TaskToken);
         return readRes.Status == DeviceStatus.Success;
      }
      catch (Exception ex)
      {
         var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
         errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return false;
      }
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
}
