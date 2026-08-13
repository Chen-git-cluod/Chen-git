namespace Kinlo.Common.Dto;

[AddINotifyPropertyChangedInterface]
public class DisplayPropertyBindingDto
{
   /// <summary>
   /// 排序
   /// </summary>
   public int Index { get; set; }

   public string Description { get; set; } = string.Empty;

   /// <summary>
   /// 绑定或Key
   /// </summary>
   public string BindingPaht { get; set; } = string.Empty;

   [JsonIgnore]
   public Type? PropertyType { get; set; }

   /// <summary>
   /// 是否显示
   /// </summary>
   public bool IsVisible { get; set; } = true;

   /// <summary>
   /// 是否导出
   /// </summary>
   public bool IsExport { get; set; } = true;

   /// <summary>
   /// 是否选中
   /// </summary>
   [JsonIgnore]
   public bool IsSelected { get; set; } = false;
}
