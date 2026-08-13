using System.Windows.Input;
using HandyControl.Controls;
using Kinlo.GUI.Helpers;
using Kinlo.GUI.ViewModel.Parts.FileDisplay;
using Kinlo.Services.PeriodicTasks;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(true)]
public class MainViewModel : Screen
{
   public EntityPropertyVisibleViewModel EntityPropertyVisibleVM { get; set; }
   public HeaderLayoutViewModel NonClientArea { get; set; }

   /// <summary>
   /// webapi服务
   /// </summary>
   public WebServerTrayViewModel WebServerTrayVM { get; set; }

   /// <summary>
   /// 菜单集合
   /// </summary>
   public ObservableCollection<ControlInfoModel> Menus { get; set; } = new();

   /// <summary>
   /// 所选菜单
   /// </summary>
   public ControlInfoModel SelectedMenu { get; set; }

   /// <summary>
   /// 主显示
   /// </summary>
   public object ViewModelContent { get; set; }

   /// <summary>
   /// 用户状态
   /// </summary>
   public UsersStatusConfig UsersStatus { get; set; }
   public ParameterConfig Parameter { get; set; }
   public GlobalStaticTemporary Temporary { get; set; }
   private OtherParameterConfig _otherParameterConfig;

   /// <summary>
   /// 版本号
   /// </summary>
   public string Version { get; } = $"V {Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version}";
   public bool IsDark { get; set; }
   public PlcStatusConfig PlcStatusCFG { get; set; }
   public IContainer _container;
   IWindowManager _windowManager;

   public MainViewModel(IContainer container, IWindowManager windowManager)
   {
      _container = container;
      _windowManager = windowManager;
      UsersStatus = _container.Get<UsersStatusConfig>();
      _otherParameterConfig = container.Get<OtherParameterConfig>();
      Temporary = _container.Get<GlobalStaticTemporary>();
      EntityPropertyVisibleVM = container.Get<EntityPropertyVisibleViewModel>();
      Parameter = _container.Get<ParameterConfig>();
      Menus = _container.Get<RoleConfig>().Menus;
      NonClientArea = _container.Get<HeaderLayoutViewModel>();
      PlcStatusCFG = container.Get<PlcStatusConfig>();
      WebServerTrayVM = container.Get<WebServerTrayViewModel>();
      NonClientArea.HandlerWindow = HandlerView;
      LiveCharts.Configure(config => config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')));
      UsersStatus.BackHome += UsersStatus_BackHome;
      DelegateExtensions.GetLanguage = () => _otherParameterConfig;

      SelectedIndex = -1;
      SelectedIndex = 0;

      _ = InitAsync(); //初始化

      //for (int i = 0; i < 5; i++)
      //   ChartExporter.ExportToPng(
      //      $"20.5,19.5,65,36,48,46.3,{i * 10 + 20}",
      //      $"D:\\test{i}.png",
      //      $"【测短路波形】 电池条码：JSLDKFJJLJKLEWJRERJLWKE{i + 1} 通道：{i + 1}"
      //   );
   }

   private async Task InitAsync()
   {
      try
      {
         await UIThreadHelper.InvokeOnUiThreadAsync(() => Temporary.IsLoadFun = false);

         //启用键勾子
         KeyboardHook.Hook.Start();
         //注册点检窗口弹出
         _container.Get<InspectionConfig>().ShowInspectionWindow += ShowInspectionWindow;
         //开启设备ping
         _container.Get<ConfigurationDeviceViewModel>().DevicePing();

         //配置更新后同步资源
         await SyncResourcesAfterConfigUpdate();
         //创建外设文档文件夹
         Enum.GetValues<DeviceManualType>().ToList().ForEach(type => type.CreateManualDir());
         //启动周期任务
         var periodic = new PeriodicTasksHelper(_container);
         periodic.Start();
         //绑定web服务开关
         var webServer = _container.Get<Kinlo.WebApi.IApiService>();
         webServer.RunStatusFunc = WebServerTrayVM.SetRunStatusAsync;
         //启动web服务
         if (_container.Get<WebApiConfig>().AutoStartWebApi)
            await webServer.StartApiAsync();

         "[程序初始化]成功！".LogRun(Log4NetLevelEnum.成功);
      }
      catch (Exception ex)
      {
         $"程序初始化异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
      finally
      {
         await UIThreadHelper.InvokeOnUiThreadAsync(() => Temporary.IsLoadFun = true);
      }
   }

   /// <summary>
   /// 配置更新后同步资源
   /// </summary>
   /// <returns></returns>
   public async Task SyncResourcesAfterConfigUpdate()
   {
      "[同步资源]开始！".LogRun(Log4NetLevelEnum.成功);

      //初始化界面显示数据
      var collection = _container.Get<DisplayDataCollection>();
      await collection.InitAsync();
      var processRatioDisplay = _container.Get<ProcessRatioDisplay>();
      processRatioDisplay.Init(collection.ProcessesDatas);

      //初始化数据库
      if (await _container.Get<DbHelper>().Initializer(DatabaseRole.LocalDb1))
      {
         var appGlabalCfg = _container.Get<AppGlobalConfig>();
         //先检查一下班次时间
         await PeriodicTasksHelper.UpdateShiftAsync(DateTime.Now, Parameter, appGlabalCfg);
         await _container.Get<IBatteryCache>().LoadCache();
      }
      "[同步资源]完成！".LogRun(Log4NetLevelEnum.成功);
   }

   /// <summary>
   /// 返回主页方法
   /// </summary>
   private void UsersStatus_BackHome() => SelectedIndex = 0;

   public KinloControls.SlidingContentControl.SlideDirection Direction { get; set; } =
      KinloControls.SlidingContentControl.SlideDirection.Down;
   private int _selectedIndex;

   /// <summary>
   /// 菜单切换
   /// </summary>
   public int SelectedIndex
   {
      get => _selectedIndex;
      set
      {
         try
         {
            if (value == -1)
            {
               _selectedIndex = value;
               return;
            }
            if (_selectedIndex == value)
               return;

            //非主页，且程序未加载完成，不可点击
            if (value != 0 && !Temporary.IsLoadFun)
            {
               Growl.Warning("程序正在加载；请稍候......");
               return;
            }

            #region 取消鼠标焦点,将鼠标焦点移动到下一个可聚焦元素。
            var focusedElement = Keyboard.FocusedElement as UIElement;
            focusedElement?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            #endregion

            Direction =
               _selectedIndex > value
                  ? KinloControls.SlidingContentControl.SlideDirection.Up
                  : KinloControls.SlidingContentControl.SlideDirection.Down;

            SelectedMenu = Menus[value];
            if (ViewModelContent != null && !((IMenu)ViewModelContent).Unload())
            {
               return;
            }
            var viewModel = _container.Get(SelectedMenu.Type);
            ViewModelContent = viewModel;
            Task.Run(() =>
            {
               if (ViewModelContent != null)
                  ((IMenu)ViewModelContent).Load();
            });
            _selectedIndex = value;
         }
         catch (Exception ex)
         {
            $"切换菜单异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
         }
      }
   }

   private void HandlerView(int i)
   {
      MainView mainView = this.View as MainView;
      if (mainView == null)
      {
         $"未取到MainWindow".LogRun();
         return;
      }
      switch (i)
      {
         case 1:
            mainView.WindowState = WindowState.Minimized;
            break;
         case 2:
            mainView.WindowState = mainView.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            break;
         case 3:
            mainView.Close();
            break;
      }
   }

   #region 打开各窗口

   GasConcentrationViewModel? _gasConcentrationVM;

   /// <summary>
   /// 打开气体浓度窗口
   /// </summary>
   public void GasConcentrationShowCmd()
   {
      try
      {
         PlcStatusCFG.IsTest = !PlcStatusCFG.IsTest;
         if (_gasConcentrationVM != null && _gasConcentrationVM.View != null)
         {
            ((GasConcentrationView)_gasConcentrationVM.View).Activate();
            return;
         }

         _gasConcentrationVM ??= _container.Get<GasConcentrationViewModel>();
         _windowManager.ShowWindow(_gasConcentrationVM);
      }
      catch (Exception ex)
      {
         $"打开气体浓度窗口异常：{ex}".LogRun(Log4NetLevelEnum.警告, true);
      }
   }

   MaterialLoadViewModel? _materialLoadVM;

   /// <summary>
   /// 打开上料窗口
   /// </summary>
   public void MaterialLoadCMD()
   {
      try
      {
         _materialLoadVM = _materialLoadVM ??= _container.Get<MaterialLoadViewModel>();
         _windowManager.ShowDialog(_materialLoadVM);
      }
      catch (Exception ed) { }
   }

   WorkOrderViewModel? _workOrderVM;

   /// <summary>
   /// 打开工单窗口
   /// </summary>
   public void GetWorkOrderCMD()
   {
      try
      {
         _workOrderVM = _workOrderVM ??= _container.Get<WorkOrderViewModel>();
         _windowManager.ShowDialog(_workOrderVM);
      }
      catch (Exception ed) { }
   }

   ManualRefillViewModel? _updateReplenishVolumeVM;

   /// <summary>
   /// 打开更新补液结果窗口
   /// </summary>
   public void UpdateReplenishVolumeCMD()
   {
      try
      {
         if (_updateReplenishVolumeVM != null && _updateReplenishVolumeVM.View != null)
         {
            ((ManualRefillView)_updateReplenishVolumeVM.View).Activate();
            return;
         }
         _updateReplenishVolumeVM ??= _container.Get<ManualRefillViewModel>();
         _windowManager.ShowWindow(_updateReplenishVolumeVM);
         _updateReplenishVolumeVM.RegistrationHook(true);
      }
      catch (Exception ex)
      {
         $"打开更新补液结果窗口异常：{ex}".LogRun(Log4NetLevelEnum.警告, true);
      }
   }

   InspectionViewModel? _inspectionVM;

   /// <summary>
   /// 打开点检窗口
   /// </summary>
   public void InspectionCmd()
   {
      try
      {
         if (_inspectionVM != null && _inspectionVM.View != null && _inspectionVM.View is InspectionView window)
         {
            window.Activate();
            window.Topmost = true; //强制置顶
            window.Topmost = false; // 立即取消，避免长期置顶干扰用户
            return;
         }
         _inspectionVM ??= _container.Get<InspectionViewModel>();
         _windowManager.ShowWindow(_inspectionVM);
      }
      catch (Exception ex)
      {
         $"打开点检窗口异常：{ex}".LogRun(Log4NetLevelEnum.警告, true);
      }
   }

   /// <summary>
   /// 打开点检窗口并指定选中工序
   /// </summary>
   /// <param name="process"></param>
   public void ShowInspectionWindow(ProcessTypeEnum process)
   {
      InspectionCmd();
      if (_inspectionVM == null)
         return;
      _inspectionVM.SelectItem(process);
   }

   public void TestCMD()
   {
      //var _sugarDB = _container.Get<SugarDB>();
      //var _v = await _sugarDB.GetMainBattereyByBarcode("Scan7736519777766");
      //var _v2 = await _sugarDB.GetProcessByBarcode<BatNailModel>("Scan7176853022536");
      ObservableCollection<double> Doubles = new ObservableCollection<double>();
      Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
      {
         { "0", 200.0 },
         { "1", 170.0 },
         { "2", 185.0 },
         { "3", Doubles },
      };

      DataRelayStatic.CPK.LoadData(keyValuePairs);

      _ = Task.Run(async () =>
      {
         await UIThreadHelper.InvokeOnUiThreadAsync(async () =>
         {
            Random random = new Random();
            while (true)
            {
               if (Doubles.Count > 59)
                  Doubles.Remove(Doubles[0]);
               var MyProperty = random.Next(17500, 19500) / 100.0;
               Doubles.Add(MyProperty);
               await Task.Delay(1000);
            }
         });
      });
   }
   #endregion
}
