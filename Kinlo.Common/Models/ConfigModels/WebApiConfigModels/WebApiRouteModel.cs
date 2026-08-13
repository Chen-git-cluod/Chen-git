namespace Kinlo.Common.Models.ConfigModels.WebApiConfigModels;

[AddINotifyPropertyChangedInterface]
[Languages(IsScanProperty = true)]
public class WebApiRouteModel
{
   public string Key { get; init; } = string.Empty;

   [Languages("接口名")]
   public string InterfaceName { get; set; } = string.Empty;

   [Languages("接口路由")]
   public string Url { get; set; } = string.Empty;

   /// <summary>
   /// 是否启用
   /// </summary>
   public bool IsEnable { get; set; } = true;
}
