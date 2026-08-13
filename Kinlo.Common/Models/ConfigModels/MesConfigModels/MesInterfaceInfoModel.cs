namespace Kinlo.Common.Models.ConfigModels.MesConfigModels;

[AddINotifyPropertyChangedInterface]
public class MesInterfaceInfoModel
{
   /// <summary>
   /// 接口名
   /// </summary>
   public string InterfaceName { get; set; } = string.Empty;

   /// <summary>
   /// 如果有多个服务器可以选择
   /// </summary>
   public byte BserUrlIndex { get; set; } = 1;
   public string Url { get; set; } = string.Empty;

   /// <summary>
   /// 获取报文时的参数类型
   /// </summary>
   [JsonIgnore]
   public Type? ParameterType { get; set; }

   /// <summary>
   /// 是否启用
   /// </summary>
   public bool IsEnable { get; set; }

   /// <summary>
   /// 频率(秒)
   /// </summary>
   public int PollingIntervalSec { get; set; } = 60;
}
