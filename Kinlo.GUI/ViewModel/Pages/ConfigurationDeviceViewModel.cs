using System.Collections.Specialized;
using HandyControl.Controls;
using Kinlo.GUI.ViewModel.Parts.FileDisplay;
using static Kinlo.Common.GlobalStaticTemporary;

namespace Kinlo.GUI.ViewModel;

[Languages(["设备配置", "Konfigurasi Perangkat", "Device config"], IsScanProperty = false)]
[UIDisplay(true, 7, (ulong)DefaultRoleEnum.工艺 | (ulong)DefaultRoleEnum.设备, isRunEdit: false, "\xe69b")]
public class ConfigurationDeviceViewModel : Screen, IMenu
{
   /// <summary>
   /// 操作设备的ViewModel
   /// </summary>
   public object DeviceHanderVM { get; set; }

   /// <summary>
   /// 设备文档ViewModel
   /// </summary>
   public object DeviceDocumentVM { get; set; }
   public ObservableCollection<string> ServiceItems { get; set; }
   public DeviceClientModel NewDeviceClient { get; set; } = new DeviceClientModel();
   public string Ports { get; set; } = string.Empty;
   public DevicesConfig Devices { get; set; }
   public PLCSignalConfig PLCSignal { get; set; }
   private string _ipPattern = "((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}";
   string _ipPattern2 = "(((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})\\.){3}";
   IContainer _container;
   UsersStatusConfig _usersStatus;
   private AlarmDialogDto _alarmDialog = new AlarmDialogDto(string.Empty, string.Empty); //弹窗内容
   Notification _notification;
   ServiceCore _serviceCore;
   GlobalStaticTemporary _globalTemporary;
   IWindowManager _windowManager;

   public ConfigurationDeviceViewModel(IContainer container, IWindowManager windowManager)
   {
      _container = container;
      _windowManager = windowManager;
      Devices = container.Get<DevicesConfig>();
      PLCSignal = container.Get<PLCSignalConfig>();
      _usersStatus = container.Get<UsersStatusConfig>();
      ServiceItems = new ObservableCollection<string>(PLCSignal.PLCScanSignals.Select(x => x.ServiceName));
      _serviceCore = container.Get<ServiceCore>();
      _globalTemporary = container.Get<GlobalStaticTemporary>();

      // 触发测试页面预加载， Test_UnitViewModel
      _ = Test_UnitViewModel.PreloadAllAsync(_container, Devices.DeviceList);

      //  监听新增/删除 设备，预加载测试页面
      Devices.DeviceList.CollectionChanged += OnDeviceListChanged;
   }

   private void OnDeviceListChanged(object? sender, NotifyCollectionChangedEventArgs e)
   {
      if (e.NewItems != null)
         foreach (DeviceClientModel d in e.NewItems)
            Test_UnitViewModel.PreloadOne(_container, d); //  委托给 Test_UnitViewModel

      if (e.OldItems != null)
         foreach (DeviceClientModel d in e.OldItems)
            Test_UnitViewModel.RemoveCache(d); // ✅委托给 Test_UnitViewModel
   }

   /// <summary>
   /// 打开本软件的说明书位置
   /// </summary>
   public void OpenSelfManual()
   {
      var fileDisplayVM = _container.Get<FileDisplayViewModel>();
      var fileNode = DeviceManualHelper.GetFilePath(null, CommunicationEnum.None);
      fileDisplayVM.LoadDirectory(fileNode);
      DeviceDocumentVM = fileDisplayVM;
   }

   /// <summary>
   /// 选中设备
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   public void ListView_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
   {
      ListView listView = (ListView)sender;
      if (listView.SelectedItems.Count < 1)
         return;

      if (listView.SelectedItems[0] is DeviceClientModel device)
      {
         OpenManual(device);
         HandlerDevice(device);
      }
   }

   /// <summary>
   /// 打开设备操作
   /// </summary>
   /// <param name="device"></param>
   private void HandlerDevice(DeviceClientModel device)
   {
      //if (device.Communication is CommunicationEnum.RFID_BIS00EJ or CommunicationEnum.RFID_RD900M)
      //{
      //   DeviceHanderVM = _container.Get<Func<DeviceClientModel, RfidViewModel>>().Invoke(device);
      //   return;
      //}
      //DeviceHanderVM = null;

      if (!Test_UnitViewModel.HasTestSupport(device))
      {
         DeviceHanderVM = null;
         return;
      }

      DeviceHanderVM = Test_UnitViewModel.GetOrCreate(_container, device);
   }

   /// <summary>
   /// 打开仪器手册所在位置
   /// </summary>
   /// <param name="client"></param>
   private void OpenManual(DeviceClientModel client)
   {
      // client.ProcessesType.OpenManualDir();
      var fileDisplayVM = _container.Get<FileDisplayViewModel>();
      var fileNode = DeviceManualHelper.GetFilePath(client.ProcessesType, client.Communication);
      fileDisplayVM.LoadDirectory(fileNode);
      DeviceDocumentVM = fileDisplayVM;
   }

   #region 设备
   Permission _permission = new Permission(false, (ulong)DefaultRoleEnum.工艺 | (ulong)DefaultRoleEnum.设备);

   /// <summary>
   /// 新建设备
   /// </summary>
   public void AddDeviceCMD()
   {
      if (!_globalTemporary.PermissionVerification(_permission, out var msg))
      {
         Growl.Warning(msg);
         return;
      }

      DeviceViewModel _deviceVM = _container.Get<DeviceViewModel>();
      _deviceVM.SetDeviceClient(new DeviceClientModel(), 1);
      _windowManager.ShowDialog(_deviceVM);
   }

   /// <summary>
   /// 启用设备
   /// </summary>
   public void EnableDeviceCMD(ListView listView) => EnableOrDisabledDevice(listView, true);

   /// <summary>
   /// 禁用设备
   /// </summary>
   public void DisabledDeviceCMD(ListView listView) => EnableOrDisabledDevice(listView, false);

   private void EnableOrDisabledDevice(ListView listView, bool status)
   {
      if (!_globalTemporary.PermissionVerification(_permission, out var msg))
      {
         Growl.Warning(msg);
         return;
      }
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      List<string> _names = new List<string>();
      foreach (var item in listView.SelectedItems)
      {
         var _device = item as DeviceClientModel;
         _names.Add($"[工序：{_device.ProcessesType},序号：{_device.Index}]");
         _device.IsEnable = status;
      }
      Devices.Save(_usersStatus.LocalLoggedinUser.Account, $"{(status ? "启用" : "禁用")}设备 {string.Join(',', _names)} ;");
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   public void DeviceCMD_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => EditDeviceCMD(sender as ListView);

   /// <summary>
   /// 编辑
   /// </summary>
   /// <param name="listView"></param>
   public void EditDeviceCMD(ListView listView)
   {
      if (!_globalTemporary.PermissionVerification(_permission, out var msg))
      {
         Growl.Warning(msg);
         return;
      }
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      DeviceViewModel _deviceVM = _container.Get<DeviceViewModel>();
      _deviceVM.SetDeviceClient(listView.SelectedItems[0] as DeviceClientModel, 2);
      _windowManager.ShowDialog(_deviceVM);
   }

   /// <summary>
   /// 删除
   /// </summary>
   /// <param name="listView"></param>
   public void DeleteDeviceCMD(ListView listView)
   {
      if (!_globalTemporary.PermissionVerification(_permission, out var msg))
      {
         Growl.Warning(msg);
         return;
      }
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      List<string> _names = new List<string>();
      foreach (var item in listView.SelectedItems)
      {
         var _device = item as DeviceClientModel;
         _names.Add($"[工序：{_device.ProcessesType},序号：{_device.Index}]");
      }
      if (
         HandyControl.Controls.MessageBox.Show($"确定要删除 {string.Join(',', _names)} 吗?", "删除警告！", MessageBoxButton.OKCancel)
         != MessageBoxResult.OK
      )
         return;

      for (int i = listView.SelectedItems.Count - 1; i > -1; i--)
      {
         var _device = listView.SelectedItems[i] as DeviceClientModel;
         var _deleteDevice = Devices.DeviceList.FirstOrDefault(x => x.ProcessesType == _device.ProcessesType && x.Index == _device.Index);
         Devices.DeviceList.Remove(_deleteDevice);
      }
      Devices.Save(_usersStatus.LocalLoggedinUser.Account, $"删除设备 {string.Join(',', _names, false)} ;");
      Growl.Success("删除成功！");
   }
   #endregion

   #region Ping
   StringBuilder _stringBuilder = new StringBuilder();

   public void DevicePing()
   {
      Task.Run(async () =>
      {
         try
         {
            using Ping ping = new Ping();
            while (!GlobalStaticTemporary.GlobalToken.IsCancellationRequested)
            {
               if (Devices?.DeviceList.Count != 0)
               {
                  _stringBuilder.Clear();
                  for (int i = 0; i < Devices.DeviceList.Count; i++)
                  {
                     var model = Devices.DeviceList[i];
                     if (!model.IsEnable || model.ConnectType is ConnectTypeEnum.SerialPort or ConnectTypeEnum.None)
                     {
                        model.IsOnline = 99;
                        continue;
                     }
                     try
                     {
                        var pingReply = ping.Send(model.IPCOM, 500);
                        model.PingTime = pingReply.RoundtripTime;
                        model.ICMPResult = pingReply.Status;
                        if (pingReply.Status == IPStatus.Success)
                        {
                           model.IsOnline = 1;
                           model.PingNGCount = 0;
                        }
                        else
                        {
                           model.IsOnline = 2;
                           model.PingNGCount++;
                        }
                     }
                     catch (Exception ex)
                     {
                        model.PingTime = -1;
                        model.IsOnline = 2;
                        model.ICMPResult = IPStatus.Unknown;
                        model.PingNGCount++;
                        string _msg =
                           $"[设备在线检测]发生异常:[{model.ProcessesType}]工序：[{model.ProcessesType}] 序号:[{model.Index}] IP:[{model.IPCOM}] 端口号:[{model.Port}] 异常信息：{ex}";
                        _msg.LogRun(Log4NetLevelEnum.警告);
                     }
                     finally
                     {
                        if (model.PingNGCount > 2)
                        {
                           _globalTemporary.RequestStop(); //停机
                           string _msg =
                              $"[设备在线检测]工序：[{model.ProcessesType}] 序号:[{model.Index}] IP:[{model.IPCOM}] 端口:[{model.Port}]设备通信无法连接,请检查网线！！！";
                           _stringBuilder.AppendLine(_msg);
                        }
                        if (model.PingNGCount > 10000)
                           model.PingNGCount = 9000;
                     }
                  }

                  if (_stringBuilder.Length > 0)
                  {
                     _stringBuilder.ToString().LogRun(Log4NetLevelEnum.错误);
                     _ = UIThreadHelper.InvokeOnUiThreadAsync(() =>
                     {
                        _alarmDialog.Message = _stringBuilder.ToString();
                        if (_notification == null || !_notification.IsVisible)
                           _notification = Notification.Show(
                              new AlarmNotification(_alarmDialog),
                              HandyControl.Data.ShowAnimation.Fade,
                              true
                           );
                     });
                  }
                  else
                  {
                     _ = UIThreadHelper.InvokeOnUiThreadAsync(() =>
                     {
                        _notification?.Close();
                        _notification = null;
                     });
                  }
               }

               await Task.Delay(3000, GlobalStaticTemporary.GlobalToken);
            }
         }
         catch { }
         finally
         {
            _notification?.Close();
            _notification = null;
         }
      });
   }
   #endregion


   public void Load() { }

   public bool Unload() => true;
}
