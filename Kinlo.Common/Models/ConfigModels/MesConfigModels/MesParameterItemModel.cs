namespace Kinlo.Common.Models.ConfigModels.MesConfigModels;

[AddINotifyPropertyChangedInterface]
public class MesParameterItemModel
{
   [Languages("Mes编号")]
   public string MesCode { get; set; } = string.Empty;

   [Languages("Mes名称")]
   public string MesName { get; set; } = string.Empty;
   public string Description { get; set; } = string.Empty;
   public string LanguagerKey { get; set; } = string.Empty;

   /// <summary>
   /// 本地属性名
   /// </summary>
   public string LocalPropertyName { get; set; } = string.Empty;

   /// <summary>
   /// 本地属性类型
   /// </summary>
   [JsonIgnore]
   public Type? LocalType { get; set; }

   /// <summary>
   /// 值转换器名
   /// </summary>
   public string ConverterName { get; set; } = string.Empty;

   /// <summary>
   /// 值转换器（上传MES时有些数据需要转换）
   /// </summary>
   [JsonIgnore]
   public IMesValueConverter? ValueConverter { get; set; }

   /// <summary>
   /// 转换器的参数
   /// </summary>
   public string ConverterParam { get; set; } = string.Empty;

   /// <summary>
   /// 推荐的本地候选属性
   /// </summary>
   [JsonIgnore]
   public ObservableRangeCollection<CandidateItem> Candidates { get; set; } = new();

   /// <summary>
   /// 是否选中
   /// </summary>
   [JsonIgnore]
   public bool IsSelected { get; set; }
   public bool IsEnable { get; set; } = true;

   /// <summary>
   /// 是否失效
   /// </summary>
   public bool IsExpired { get; set; }

   /// <summary>
   /// 检查是否有效
   /// </summary>
   /// <returns></returns>
   public bool CheckExpired()
   {
      if (IsExpired)
         return false;
      if (string.IsNullOrEmpty(MesCode) || string.IsNullOrEmpty(LocalPropertyName))
         return false;
      if (!IsEnable)
         return false;
      return true;
   }
}

public class CandidateItem
{
   public CandidateItem(string description, string propertyName)
   {
      Description = description;
      PropertyName = propertyName;
   }

   public string Description { get; set; } = string.Empty;
   public string PropertyName { get; set; } = string.Empty;
}
