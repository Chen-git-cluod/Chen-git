using Kinlo.GUI.Helpers;
using Stylet.Xaml;

namespace Kinlo.GUI;

public class Bootstrapper : Bootstrapper<MainViewModel>
{
   Assembly[]? _assemblys;
   Assembly? _asmGUI;
   Assembly? _asmDevice;
   Assembly? _asmCommon;
   Assembly? _asmMESDocking;
   Assembly? _asmServices;
   Assembly? _asmShareBase;
   Assembly? _asmWebApi;

   /// <summary>
   /// 应用程序启动之后，IoC容器设置之前调用
   /// </summary>
   protected override void OnStart()
   {
      AppDomain
         .CurrentDomain.GetAssemblies()
         .ToList()
         .ForEach(s =>
            s.GetReferencedAssemblies()
               .ToList()
               .ForEach(ss =>
               {
                  Assembly.Load(ss);
               })
         );
      _assemblys = AppDomain.CurrentDomain.GetAssemblies();
      _asmGUI = _assemblys.FirstOrDefault(x => x.FullName.Contains(nameof(Kinlo) + "." + nameof(Kinlo.GUI)));
      _asmDevice = _assemblys.FirstOrDefault(x => x.FullName.Contains(nameof(Kinlo) + "." + nameof(Kinlo.Equipment)));
      _asmCommon = _assemblys.FirstOrDefault(x => x.FullName.Contains(nameof(Kinlo) + "." + nameof(Kinlo.Common)));
      _asmMESDocking = _assemblys.FirstOrDefault(x => x.FullName.Contains(nameof(Kinlo) + "." + nameof(Kinlo.MESDocking)));
      _asmServices = _assemblys.FirstOrDefault(x => x.FullName.Contains(nameof(Kinlo) + "." + nameof(Kinlo.Services)));
      _asmShareBase = _assemblys.FirstOrDefault(x => x.FullName.Contains(nameof(Kinlo) + "." + nameof(Kinlo.SharedBase)));
      _asmWebApi = _assemblys.FirstOrDefault(x => x.FullName.Contains(nameof(Kinlo) + "." + nameof(Kinlo.WebApi)));

      RegisterLogDialogs(); //注册日志弹窗事件
      Log4NetInitializer.LoadBasicLogConfig(); //加载除MES日志外的其它日志配置
   }

   /// <summary>
   /// 绑定类型。
   /// </summary>
   /// <param name="builder"></param>
   protected override void ConfigureIoC(IStyletIoCBuilder builder)
   {
      //builder.Bind<IStyletIoCBuilder>().ToInstance(builder);
      builder.Bind<Assembly>().ToInstance(_asmGUI).WithKey(nameof(GUI));
      // builder.Bind<Assembly>().ToInstance(_asmModel).WithKey("Model");
      builder.Bind<Assembly>().ToInstance(_asmCommon).WithKey(nameof(Common));
      builder.Bind<Assembly>().ToInstance(_asmMESDocking).WithKey(nameof(MESDocking));
      builder.Bind<Assembly>().ToInstance(_asmServices).WithKey(nameof(Services));
      builder.Bind<Assembly>().ToInstance(_asmDevice).WithKey(nameof(Equipment));
      builder.Bind<Assembly>().ToInstance(_asmWebApi).WithKey(nameof(WebApi));
      builder.Bind<SnowflakeHelper>().ToFactory(x => new SnowflakeHelper(1)).InSingletonScope(); //雪花ID生成类注入，机器码及数据ID都为1
      builder.Bind<ServiceCore>().ToSelf().InSingletonScope();
      IocBuilderConfig(builder, _asmCommon); //配置注入容器
      builder.Bind<ISqlSugarDbFactory>().To<SqlSugarClientFactory>().InSingletonScope();
      builder.Bind<DbHelper>().ToSelf().InSingletonScope();
      //  builder.Bind<SugarDbHelper>().ToSelf().InSingletonScope();
      IocBuilderView(builder, _asmGUI); //视图注入容器

      builder.Bind<IBatteryCache>().To<BatteryCache>().InSingletonScope(); //注入缓存

      builder.Bind<HttpClientSingleHelper>().ToSelf().InSingletonScope(); //HttpClent注入
      builder.Bind<FtpService>().ToSelf().InSingletonScope();
      builder.Bind<MesService>().ToSelf().InSingletonScope();
      builder.Bind<MqttHelper>().ToSelf().InSingletonScope(); //Mqtt注入
      builder.Bind<IApiService>().To<ApiService>().InSingletonScope(); //WebApi注入
   }

   /// <summary>
   /// 在Stylet创建了IoC容器之后调用的,在此处配置服务等
   /// </summary>
   protected override void Configure()
   {
      try
      {
         var mesService = this.Container.Get<MesService>();
         var infos = mesService.RegisterMesInterface();
         var mesInterfaceNames = infos?.InterfaceDescriptions?.Select(x => x.interfaceName).ToList() ?? new List<string>();
         var webConfig = this.Container.Get<WebApiConfig>();
         webConfig.ScanWebRouteFunc = ScanRouteHelper.ScanAssemblyUpAction;
         webConfig.ScanAssemblyUpActionAsync().GetAwaiter().GetResult();
         var webInterfaceNames = webConfig?.WebApiRoutes?.Select(x => x.InterfaceName).ToList() ?? new List<string>();
         Log4NetInitializer.LoadFullLogConfig(mesInterfaceNames, webInterfaceNames); //加载完整日志配置
         this.Container.Get<MesInterfaceParameterConfig>().LoadMesInterfaceInfo(infos); //加载MES接口显示信息
         this.Container.Get<LogHistoryViewModel>(); //日志显示
         _externalAssembies.Add(_asmCommon);
         _externalAssembies.Add(_asmGUI);
         _externalAssembies.Add(_asmDevice);
         _externalAssembies.Add(_asmShareBase);
         _externalAssembies.Add(_asmMESDocking);
         _externalAssembies.Add(_asmWebApi);
         this.Container.Get<OtherParameterConfig>().InitLanguage(_externalAssembies); //加载语言
         var _roleConfig = this.Container.Get<RoleConfig>();

         _roleConfig.AddMenus(_menuTypes); //加载菜单
      }
      catch (Exception ex)
      {
         MessageBox.Show($"配置时异常：{ex}");
      }
   }

   /// <summary>
   /// 在启动根ViewModel之后调用的,可以从这里启动类似于显示对话框的版本检查之类的操作
   /// </summary>
   protected override void OnLaunch()
   {
      //var _displayDataCollection = this.Container.Get<DisplayDataCollection>();
      //_displayDataCollection.Init();

      ////初始化数据库
      //this.Container.Get<SugarDB>().InitData();
   }

   /// <summary>
   /// 对Application.Exit调用
   /// </summary>
   /// <param name="e"></param>
   protected override void OnExit(ExitEventArgs e) { }

   /// <summary>
   /// 对Application.DispatcherUnhandledException调用
   /// </summary>
   /// <param name="e"></param>
   protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
   {
      try
      {
         $"OnUnhandledException:{e}".LogSetting();
         e.Handled = true;
      }
      catch { }
   }

   /// <summary>
   /// 注册日志中的弹窗事件
   /// </summary>
   public static void RegisterLogDialogs()
   {
      try
      {
         Log4NetHelper.PromptAction += new Action<string, Log4NetLevelEnum>(
            (msg, leve) =>
            {
               switch (leve)
               {
                  case Log4NetLevelEnum.错误:
                     HandyControl.Controls.Growl.Error(msg);
                     break;
                  case Log4NetLevelEnum.警告:
                     HandyControl.Controls.Growl.Warning(msg);
                     break;
                  case Log4NetLevelEnum.信息:
                     HandyControl.Controls.Growl.Info(msg);
                     break;
                  case Log4NetLevelEnum.成功:
                     HandyControl.Controls.Growl.Success(msg);
                     break;
                  default:
                     break;
               }
            }
         );
      }
      catch (Exception ex)
      {
         MessageBox.Show($"注册日志弹窗事件异常：{ex}");
      }
   }

   /// <summary>
   /// 配置注入容器
   /// </summary>
   /// <param name="assemblies"></param>
   public static void IocBuilderConfig(IStyletIoCBuilder builder, Assembly assemblie)
   {
      try
      {
         "[配置注入容器]开始！".LogRun();

         if (assemblie == null)
         {
            "[配置注入容器] 配置为空！".LogRun(Log4NetLevelEnum.警告);
            return;
         }
         builder.Bind<bool>().ToInstance(true);
         var configs = assemblie
            .GetTypes()
            .Where(x => x.IsClass && x.Name != nameof(ConfigurationBase) && typeof(ConfigurationBase).IsAssignableFrom(x))
            .ToList();
         foreach (var item in configs)
         {
            builder.Bind(item).ToSelf().InSingletonScope();
         }
         "[配置注入容器]完成！".LogRun(Log4NetLevelEnum.成功);
      }
      catch (Exception ex)
      {
         $"[配置注入容器]异常：{ex}！".LogRun(Log4NetLevelEnum.错误);
      }
   }

   static List<Type> _menuTypes = new List<Type>();
   static List<Assembly> _externalAssembies = new List<Assembly>();

   /// <summary>
   /// 视图注入容器
   /// </summary>
   public static void IocBuilderView(IStyletIoCBuilder builder, Assembly assemblie)
   {
      try
      {
         "[视图注入容器]开始！".LogRun();

         if (assemblie == null)
         {
            "[视图注入容器] 视图为空！".LogRun(Log4NetLevelEnum.警告);
            return;
         }
         foreach (var viewModelType in assemblie.GetTypes())
         {
            GetViewModel(viewModelType, builder);
         }

         #region 加载dll
         string _dllPath = @"Expand_DLL";
         if (Directory.Exists(_dllPath))
            TraverseDirectory(_dllPath, builder);
         else
            Directory.CreateDirectory(_dllPath);
         #endregion
         "[视图注入容器]完成！".LogRun(Log4NetLevelEnum.成功);
      }
      catch (Exception ex)
      {
         $"[视图注入容器]异常：{ex}！".LogRun(Log4NetLevelEnum.错误);
      }
   }

   static void TraverseDirectory(string path, IStyletIoCBuilder builder)
   {
      // 获取文件
      string[] files = Directory.GetFiles(path);
      foreach (string file in files)
      {
         if (file.ToLower().EndsWith(".dll"))
         {
            Assembly assembly = Assembly.LoadFrom(file);
            _externalAssembies.Add(assembly);
            foreach (var item in assembly.GetTypes())
            {
               GetViewModel(item, builder);
            }
         }
      }

      // 获取子文件夹并递归遍历
      string[] directories = Directory.GetDirectories(path);
      foreach (string directory in directories)
      {
         TraverseDirectory(directory, builder); // 递归调用
      }
   }

   static void GetViewModel(Type viewModelType, IStyletIoCBuilder builder)
   {
      try
      {
         if (
            viewModelType.IsClass
            && viewModelType.FullName != null
            && (viewModelType.FullName.EndsWith("ViewModel") || viewModelType.FullName.EndsWith("View"))
         )
         {
            if (typeof(IMenu).IsAssignableFrom(viewModelType))
            {
               _menuTypes.Add(viewModelType);
            }

            if (viewModelType == typeof(RfidViewModel))
            {
               builder
                  .Bind<Func<DeviceClientModel, RfidViewModel>>()
                  .ToFactory<Func<DeviceClientModel, RfidViewModel>>(conttainer => (device) => new RfidViewModel(conttainer, device));
            }
            var _meumAttritube = viewModelType.GetCustomAttribute<UIDisplayAttribute>();
            if (_meumAttritube != null && _meumAttritube.IsSingleton)
            {
               builder.Bind(viewModelType).ToSelf().InSingletonScope();
            }
            else
            {
               builder.Bind(viewModelType).ToSelf();
            }
         }
      }
      catch (Exception ex)
      {
         $"[单个视图注入容器]异常：{ex}！".LogRun(Log4NetLevelEnum.错误);
      }
   }
}
