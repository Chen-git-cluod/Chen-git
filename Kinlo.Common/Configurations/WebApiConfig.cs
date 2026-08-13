using Kinlo.Common.Models.ConfigModels.WebApiConfigModels;

namespace Kinlo.Common.Configurations;

[Languages(IsScanProperty = true)]
public class WebApiConfig : ConfigurationBase
{
   /// <summary>
   /// 监听IP
   /// </summary>
   [Languages("监听IP")]
   public string ListeningIP { get; set; } = "*";

   /// <summary>
   /// 监听端口
   /// </summary>
   [Languages("监听端口")]
   public int Port { get; set; } = 6070;

   /// <summary>
   /// 自动启动
   /// </summary>
   [Languages("自动启动")]
   public bool AutoStartWebApi { get; set; } = true;

   /// <summary>
   /// 是否启用 Swagger 接口文档
   /// </summary>
   [Languages("启用接口测试面板（Swagger）")]
   public bool IsEnableSwagger { get; set; } = true;
   public ObservableRangeCollection<WebApiRouteModel> WebApiRoutes { get; set; } = new();

   [JsonConstructor]
   public WebApiConfig(IContainer container, bool isStartup)
      : base(container, isStartup) { }

   public override void Load()
   {
      try
      {
         var json = FileHelper.LoadToString(this.GetType().Name);
         MapJsonProperties(json);
      }
      catch (Exception ex)
      {
         $"加载配置文件 {this.GetType().Name} 异常: {ex.Message}".LogRun(LogNet.Enums.Log4NetLevelEnum.错误);
      }
   }

   public record ScanRouteArgs(Dictionary<string, WebApiRouteModel> webApiRoutes, params Assembly[] assemblies);

   public Func<ScanRouteArgs, IEnumerable<WebApiRouteModel>>? ScanWebRouteFunc;

   public async Task ScanAssemblyUpActionAsync()
   {
      var assembly = _container.Get<Assembly>("WebApi");
      if (assembly == null)
         return;
      // 预处理旧配置：以 Key (类名.方法名) 为锚点，用于状态找回
      var oldRoutesDict = WebApiRoutes.ToDictionary(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase);
      var list = ScanWebRouteFunc?.Invoke(new ScanRouteArgs(oldRoutesDict, assembly));
      if (list != null)
      {
         // 扫描完成后，一次性更新 UI，避免多次刷新闪烁
         await UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            // WebApiRoutes.ReplaceRange(dic.Values);
            WebApiRoutes.ReplaceRange(list);
         });
      }
   }

   public static void Copy(WebApiConfig resource, WebApiConfig target)
   {
      List<WebApiRouteModel> list = new();
      foreach (var route in resource.WebApiRoutes)
      {
         WebApiRouteModel webRoute = new();
         ExpressionAssignmentMapper<WebApiRouteModel, WebApiRouteModel>.Trans(route, webRoute);
         list.Add(webRoute);
      }

      UIThreadHelper.InvokeOnUiThread(() =>
      {
         target.ListeningIP = resource.ListeningIP;
         target.Port = resource.Port;
         target.AutoStartWebApi = resource.AutoStartWebApi;
         target.IsEnableSwagger = resource.IsEnableSwagger;

         target.WebApiRoutes.Clear();
         target.WebApiRoutes.AddRange(list);
      });
   }
}
