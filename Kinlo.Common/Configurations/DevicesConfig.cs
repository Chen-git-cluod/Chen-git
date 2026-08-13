namespace Kinlo.Common.Configurations;

public class DevicesConfig : ConfigurationBase
{
   private readonly ReaderWriterLockSlim _lock = new();

   [JsonIgnore]
   private List<IDevice> _runDeviceList = new();
   public ObservableCollection<DeviceClientModel> DeviceList { get; set; } = new();

   public DevicesConfig(StyletIoC.IContainer container, bool isStartup)
      : base(container, isStartup) { }

   public override void Load()
   {
      try
      {
         var _dic = FileHelper.LoadToDictionary(this.GetType().Name);
         if (_dic != null && _dic.TryGetValue(nameof(DeviceList), out object value) && value != null)
            DeviceList = JsonSerializer.Deserialize<ObservableCollection<DeviceClientModel>>(value.ToString())!;
      }
      catch (Exception ex)
      {
         $"[初始化DevicesConfig]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   public void AddRunDevice(IDevice device)
   {
      _lock.EnterWriteLock();
      try
      {
         _runDeviceList.Add(device);
      }
      finally
      {
         if (_lock.IsWriteLockHeld)
            _lock.ExitWriteLock();
      }
   }

   public IDevice? GetRunDevice(Func<IDevice, bool> predicate)
   {
      _lock.EnterReadLock();
      try
      {
         return _runDeviceList.FirstOrDefault(predicate);
      }
      finally
      {
         if (_lock.IsReadLockHeld)
            _lock.ExitReadLock();
      }
   }

   public IDevice? GetRunDevice(PLCInteractAddressModel address, int index)
   {
      _lock.EnterReadLock(); //读锁
      try
      {
         return _runDeviceList.FirstOrDefault(x =>
            x.DeviceInfo.ServiceName == address.ServiceName
            && x.DeviceInfo.ProcessesType == address.ProcessesType
            && x.DeviceInfo.Communication == address.DeviceCommunicationType
            && x.DeviceInfo.Index == index
         );
      }
      finally
      {
         if (_lock.IsReadLockHeld)
            _lock.ExitReadLock();
      }
   }

   public List<IDevice> GetRunDevices(Func<IDevice, bool> predicate)
   {
      _lock.EnterReadLock(); //读锁
      try
      {
         return _runDeviceList.Where(predicate).ToList();
      }
      finally
      {
         if (_lock.IsReadLockHeld)
            _lock.ExitReadLock();
      }
   }

   /// <summary>
   /// 释放所有设备
   /// </summary>
   /// <returns></returns>
   public void ClearRunDevice()
   {
      _lock.EnterWriteLock(); //写锁
      try
      {
         //释放所有设备
         foreach (var device in _runDeviceList)
         {
            try
            {
               device?.Close();
               $"释放成功：IP:{device.DeviceInfo.IPCOM}".LogRun(Log4NetLevelEnum.成功);
            }
            catch (Exception ex)
            {
               $"释放IP:{device.DeviceInfo.IPCOM}异常：{ex}".LogRun(Log4NetLevelEnum.错误);
            }
         }
         //异步释放，取消使用
         //var tasks = RunDeviceList.Where(x => x != null).Select(async item =>
         //{
         //    try
         //    {
         //        await Task.Run(() => item.Close());
         //        $"释放成功：IP:{item.DeviceInfo.IPCOM}".LogRun(Log4NetLevelEnum.成功);
         //    }
         //    catch (Exception ex)
         //    {
         //        $"释放IP:{item.DeviceInfo.IPCOM}异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         //    }
         //});
         //await Task.WhenAny(tasks);
         _runDeviceList.Clear();
      }
      finally
      {
         if (_lock.IsWriteLockHeld)
            _lock.ExitWriteLock();
      }
   }
}
