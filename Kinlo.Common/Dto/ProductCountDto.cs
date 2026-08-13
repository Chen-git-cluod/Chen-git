namespace Kinlo.Common.Dto;

[AddINotifyPropertyChangedInterface]
public class ProductCountDto
{
   /// <summary>
   /// 进站数量
   /// </summary>
   [JsonIgnore]
   public int InputCount { get; set; }

   /// <summary>
   /// 出站数量
   /// </summary>
   [JsonIgnore]
   public int OutputCount { get; set; }

   /// <summary>
   /// 缓存数量
   /// </summary>
   [JsonIgnore]
   public int CacheCount { get; set; }
}

/// <summary>
/// 班次切换详情
/// </summary>
[AddINotifyPropertyChangedInterface]
public class ShiftSwitchInfoModel
{
   /// <summary>
   /// 班次
   /// </summary>
   [JsonIgnore]
   public ShiftType Shift { get; set; }

   /// <summary>
   /// 上次清零时间，转班时会自动切换时间
   /// </summary>
   public DateTime LastResetTime { get; set; } = DateTime.Now.AddDays(-1);

   /// <summary>
   /// 自动切换清零时间
   /// </summary>
   [JsonIgnore]
   public bool IsAutoTimeSwitch { get; set; }
}

public enum ShiftType
{
   白班,
   夜班,
}
