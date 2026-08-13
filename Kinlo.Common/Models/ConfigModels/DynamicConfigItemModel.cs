namespace Kinlo.Common.Models.ConfigModels;

[AddINotifyPropertyChangedInterface]
public class DynamicConfigItemModel
{
   /// <summary>
   /// 索引
   /// </summary>
   public int Index { get; set; }

   /// <summary>
   /// 绑定类名
   /// </summary>
   public string ClassName { get; set; } = string.Empty;

   /// <summary>
   /// 类型
   /// </summary>
   [JsonIgnore]
   public Type ClassType { get; set; }

   ///// <summary>
   ///// 描述
   ///// </summary>
   // public string Description { get; set; } = string.Empty;
   /// <summary>
   /// 绑定的属性
   /// </summary>
   public ObservableCollection<DisplayPropertyBindingDto> PropertyBindings { get; set; } = new();

   /// <summary>
   /// 是否启用
   /// </summary>
   public bool IsEnable { get; set; } = false;

   /// <summary>
   /// 任务类型
   /// </summary>
   public ProcessTypeEnum ProcessesType { get; set; }
}
