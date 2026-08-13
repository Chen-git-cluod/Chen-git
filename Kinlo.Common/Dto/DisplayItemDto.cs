namespace Kinlo.Common.Dto;

public class DisplayItemDto
{
   public int Index { get; set; }
   public string Name { get; set; } = string.Empty;
   public object? Content { get; set; }

   /// <summary>
   /// UI展示数据控件
   /// </summary>
   public FrameworkElement? DataDisplayControl { get; set; }

   /// 描述
   /// </summary>
   public string Description { get; set; } = string.Empty;
}
