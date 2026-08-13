namespace Kinlo.Common.Models.ConfigModels.MesConfigModels;

/// <summary>
/// 参数修改通知MES模型
/// </summary>
[AddINotifyPropertyChangedInterface]
public class ParamChangNotifyModel
{
   public int Index { get; set; }

   [Languages("Mes编号")]
   public string MesCode { get; set; } = string.Empty;

   [Languages("Mes名称")]
   public string MesName { get; set; } = string.Empty;
   public string Description { get; set; } = string.Empty;
   public string LanguagerKey { get; set; } = string.Empty;

   /// <summary>
   /// 本地属性完整路径
   /// </summary>
   public string PtyFullName { get; set; } = string.Empty;

   /// <summary>
   /// 是否选中
   /// </summary>
   [JsonIgnore]
   public bool IsSelected { get; set; }
   public bool IsEnable { get; set; } = true;
}
