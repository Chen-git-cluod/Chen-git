using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Kinlo.WebApi;

/// <summary>
/// 过滤及动态路由映射
/// </summary>
public class DynamicRouteConvention : IApplicationModelConvention
{
   private readonly RouteFilteringSummary _summary = new();
   private readonly WebApiConfig _config;

   public DynamicRouteConvention(WebApiConfig config) => _config = config;

   /// <summary>
   ///
   /// </summary>
   /// <param name="application"></param>
   public void Apply(ApplicationModel application) //所有 Action 处理完后才会被调用一次
   {
      // 1. 手动触发所有 Controller 下的 Action 过滤逻辑
      foreach (var controller in application.Controllers)
      {
         foreach (var action in controller.Actions)
         {
            // 手动调用当前类实现的 Apply(ActionModel)
            this.SpecificAction(action);
         }
      }
      _summary.Print();
   }

   public void SpecificAction(ActionModel action)
   {
      var methodInfo = action.ActionMethod;
      var key = $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";
      var actionId = $"{action.Controller.ControllerName}.{action.ActionName}";

      var verbAttr = action.Attributes.OfType<HttpMethodAttribute>().FirstOrDefault();
      if (verbAttr == null) //没有 HttpMethodAttribute 特性 ，忽略
      {
         ClearedRoute(action);
         _summary.IgnoreList.Add($"接口 [{actionId}] 缺少Http模式特性(如 [HttpPost])");
         return;
      }

      // 获取配置
      var config = _config.WebApiRoutes.FirstOrDefault(r => r.Key == key);
      if (config == null) //没有配置 ，忽略
      {
         ClearedRoute(action);
         _summary.IgnoreList.Add($"接口 [{actionId}] 缺少配置记录 (Key: {key})");
         return;
      }

      if (!config.IsEnable) //未开启 ，忽略
      {
         ClearedRoute(action);
         _summary.IgnoreList.Add($"接口 [{actionId}] 已在配置中禁用");
         return;
      }

      var selector = new SelectorModel { AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(config.Url)) };
      // 直接透传原有的动词约束（POST/GET 等）
      selector.ActionConstraints.Add(new HttpMethodActionConstraint(verbAttr.HttpMethods));

      action.Selectors.Clear();
      action.Selectors.Add(selector);
      _summary.SuccessList.Add($"接口 [{actionId}] 加载成功 -> {config.Url}");
   }

   private void ClearedRoute(ActionModel action)
   {
      action.Selectors.Clear(); //使该 Action 无法通过任何 URL 访问
      action.ApiExplorer.IsVisible = false; // Swagger (ApiExplorer) 中隐藏}
   }
}

public class RouteFilteringSummary
{
   public List<string> SuccessList { get; } = new();
   public List<string> IgnoreList { get; } = new();

   public void Print()
   {
      if (SuccessList.Count == 0 && IgnoreList.Count == 0)
         return;

      var sb = new StringBuilder();
      sb.AppendLine("=== WebApi 接口加载摘要 ===");
      sb.AppendLine($"[成功加载]: {SuccessList.Count} 个接口");
      sb.AppendLine($"[已忽略/禁用]: {IgnoreList.Count} 个接口");

      if (SuccessList.Count > 0)
      {
         sb.AppendLine("--- 成功详情 ---");
         SuccessList.ForEach(msg => sb.AppendLine(msg));
      }

      if (IgnoreList.Count > 0)
      {
         sb.AppendLine("--- 忽略详情 ---");
         IgnoreList.ForEach(msg => sb.AppendLine(msg));
      }
      sb.ToString().LogRun(LogNet.Enums.Log4NetLevelEnum.信息);
   }
}
