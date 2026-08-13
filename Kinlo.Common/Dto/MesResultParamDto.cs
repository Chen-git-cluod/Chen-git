namespace Kinlo.Common.Dto;

[AddINotifyPropertyChangedInterface]
public class MesResultParamDto
{
   public int Index { get; set; }
   public bool IsSelected { get; set; }
   public string Processes { get; set; } = string.Empty;

   /// <summary>
   /// 单工序原始类
   /// </summary>
   [JsonIgnore]
   public Type? OriginalClass { get; set; }

   /// <summary>
   /// 原始工序的属性，用于数据MES 参数上传
   /// </summary>
   public ObservableCollection<DisplayPropertyBindingDto> OriginalClassProperties { get; set; } = new();
}
