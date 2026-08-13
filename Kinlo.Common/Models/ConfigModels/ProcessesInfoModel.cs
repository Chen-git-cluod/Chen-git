namespace Kinlo.Common.Models.ConfigModels;

public class ProcessesInfoModel
{
   /// <summary>
   /// 排序
   /// </summary>
   public int Index { get; set; }

   public ProcessTypeEnum ProcessesType { get; set; }

   /// <summary>
   /// 指定工序的类
   /// </summary>
   [JsonIgnore]
   public Type DataType { get; set; }

   /// <summary>
   /// 描述
   /// </summary>
   [JsonIgnore]
   public string Description { get; set; } = string.Empty;

   /// <summary>
   /// 工序属性
   /// </summary>
   public ObservableCollection<DisplayPropertyBindingDto> PropertyBindings { get; set; } = new();

   /// <summary>
   /// 此字段用来标识初始化时是否是多余的，要删除的
   /// </summary>
   [JsonIgnore]
   public bool IsDelete = true;
}
