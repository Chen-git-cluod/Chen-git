using System.Reflection;
using Kinlo.Common.Models.ConfigModels.WebApiConfigModels;
using Microsoft.AspNetCore.Mvc.Routing;
using static Kinlo.Common.Configurations.WebApiConfig;

namespace Kinlo.WebApi;

public static class ScanRouteHelper
{
   public static List<WebApiRouteModel> ScanAssemblyUpAction(this ScanRouteArgs args)
   {
      if (args.assemblies == null || args.assemblies.Length == 0)
         new List<WebApiRouteModel>();

      // 临时字典：Key 使用 "Verb:Route" 确保全局（跨程序集）路由唯一性；在忽略大小写的情况下，进行基于“二进制编码值”的快速比较
      var dic = new Dictionary<string, WebApiRouteModel>(StringComparer.OrdinalIgnoreCase);

      foreach (var assembly in args.assemblies)
      {
         // 过滤出所有的 Controller 类
         var controllerTypes = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Controller"));

         foreach (var type in controllerTypes)
         {
            // 获取类级别的路由定义 [Route]
            var classRoute = type.GetCustomAttribute<RouteAttribute>()?.Template;

            // 获取所有公共实例方法
            foreach (var methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
               // 获取 HttpMethod 特性
               var httpMethodAttr = methodInfo.GetCustomAttribute<HttpMethodAttribute>();
               if (httpMethodAttr == null)
                  continue;

               // 提取动词
               string verb = httpMethodAttr.HttpMethods.FirstOrDefault() ?? "GET";

               string key = $"{type.FullName}.{methodInfo.Name}";

               (string url, bool isEnable) config;
               if (args.webApiRoutes.TryGetValue(key, out var oldConfig))
               {
                  config = (oldConfig.Url, oldConfig.IsEnable);
               }
               else
               {
                  // 动态合成默认路由
                  string generatedUrl = BuildDefaultRoute(classRoute, httpMethodAttr.Template, type, methodInfo);
                  config = (generatedUrl, true);
               }

               string routeUniqueId = $"{verb}:{config.url.ToLower().Trim('/')}";

               if (dic.TryGetValue(routeUniqueId, out var existModel))
               {
                  $"[路由冲突] 程序集: {assembly.GetName().Name}，方法: {key}，路由: {config.url} 已存在于 {existModel.Key}。".LogRun(
                     LogNet.Enums.Log4NetLevelEnum.警告
                  );
                  continue;
               }

               var languageAttr = methodInfo.GetCustomAttribute<LanguagesAttribute>();
               string interfaceName = languageAttr?.Languages.FirstOrDefault() ?? methodInfo.Name;

               dic.Add(
                  routeUniqueId,
                  new WebApiRouteModel
                  {
                     Key = key,
                     InterfaceName = interfaceName,
                     IsEnable = config.isEnable,
                     Url = config.url,
                  }
               );
            }
         }
      }
      return dic.Values.ToList();
      //// 扫描完成后，一次性更新 UI，避免多次刷新闪烁
      //await UIThreadHelper.InvokeOnUiThreadAsync(() =>
      //{
      //    WebApiRoutes.ReplaceRange(dic.Values);
      //});
   }

   // 辅助方法：模拟路由合并
   private static string BuildDefaultRoute(string? classTpl, string? methodTpl, Type type, MethodInfo method)
   {
      // 如果方法特性 [HttpPost("save")] 已经写了完整或部分路径，优先使用
      if (!string.IsNullOrWhiteSpace(methodTpl))
      {
         // 这里简化处理，实际可能需要处理 / 开头的绝对路径
         if (methodTpl.StartsWith("/"))
            return methodTpl;
      }

      string ctrlName = type.Name.Replace("Controller", "");

      // 如果类上有 [Route]
      if (!string.IsNullOrWhiteSpace(classTpl))
      {
         string combined = $"{classTpl}/{methodTpl}".TrimEnd('/');
         return combined
            .Replace("[controller]", ctrlName, StringComparison.OrdinalIgnoreCase)
            .Replace("[action]", method.Name, StringComparison.OrdinalIgnoreCase);
      }

      // 彻底没有特性路径时的兜底策略
      return $"/api/{ctrlName}/{method.Name}";
   }
}
