namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 静置站
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.静置站, ProcessTypeEnum.静置站], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.静置站), "静置站", "Tank"])]
[AddINotifyPropertyChangedInterface]
public partial class BatTankModel
{
   /// <summary>
   /// 静置站时间
   /// </summary>
   [Languages(["静置站时间", "", "Tank time"])]
   [SugarColumn(ColumnDescription = "静置站时间")]
   [DynamicClass(IgnoreAttributes = [typeof(SplitFieldAttribute)])] //合并类时忽略整个属性
   [OrderMarker]
   public DateTime TankTime { get; set; }

   /// <summary>
   /// 缸号
   /// </summary>
   [Languages(["缸号", "缸号", "Tank index"])]
   [SugarColumn(ColumnDescription = "缸号", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public byte TankIndex { get; set; }

   /// <summary>
   /// 静置循环次数
   /// </summary>
   [Languages(["静置循环次数", "静置循環次數", "Cycle number"])]
   [SugarColumn(ColumnDescription = "静置循环次数")] //UNSIGNED 无符号
   public int CycleNumber { get; set; }

   /// <summary>
   /// 循环时长
   /// </summary>
   [Languages(["循环时长(S)", "循環時长(S)", "Cycle duration(S)"])]
   [SugarColumn(ColumnDescription = "循环时长")]
   public float CycleDuration { get; set; }

   /// <summary>
   /// 功能
   /// </summary>
   [SugarColumn(ColumnDescription = "功能")]
   [Languages(["功能", "", "Func"])]
   public string Func { get; set; } = string.Empty;

   /// <summary>
   /// 设定压力
   /// </summary>
   [SugarColumn(ColumnDescription = "设定压力")]
   [Languages(["设定压力(Kpa)", "", "Set pressure(Kpa)"])]
   public string SetPressure { get; set; } = string.Empty;

   /// <summary>
   /// 实际压力
   /// </summary>
   [SugarColumn(ColumnDescription = "实际压力", Length = 1024)]
   [Languages(["实际压力(Kpa)", "", "Actual pressure(Kpa)"])]
   public string ActualPressure { get; set; } = string.Empty;

   /// <summary>
   /// 设定时长
   /// </summary>
   [SugarColumn(ColumnDescription = "设定时长")]
   [Languages(["设定时长(S)", "", "Set hold pressure duration(S)"])]
   public string SetHoldPressureDuration { get; set; } = string.Empty;

   /// <summary>
   /// 实际保压时长
   /// </summary>
   [SugarColumn(ColumnDescription = "实际保压时长", Length = 1024)]
   [Languages(["实际保压时长(S)", "", "Actual hold pressure duration(S)"])]
   public string ActualHoldPressureDuration { get; set; } = string.Empty;

   ///// <summary>
   ///// 设定到达时长
   ///// </summary>
   //[SugarColumn(ColumnDescription = "设定到达时长(S)", Length = 1024)]
   //[Languages(["设定到达时长(S)", "", "Set Reaching pressure duration(S)"])]
   //public string SetReachingPressureDuration { get; set; } = string.Empty;

   /// <summary>
   /// 实际到达时长
   /// </summary>
   [SugarColumn(ColumnDescription = "实际到达时长(S)", Length = 1024)]
   [Languages(["实际到达时长(S)", "", "Actual Reaching pressure duration(S)"])]
   public string ActualReachingPressureDuration { get; set; } = string.Empty;
   ///// <summary>
   ///// 排队动作时间
   ///// </summary>
   //[SugarColumn(ColumnDescription = "排队动作时长(S)", Length = 1024)]
   //[Languages("排队动作时长(S)")]
   //public string lineUpTime { get; set; } = string.Empty;
}

/// <summary>
/// 静置站
/// </summary>
//[SugarTable($"Tank_{{year}}{{month}}{{day}}")]
//[SplitTable(SplitType.Month)] //按月分表 （自带分表支持 年、季、月、周、日）
//[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.静置站], DeviceCommunicationType = [CommunicationEnum.None])]
//[Languages([nameof(ProcessTypeEnum.静置站), "静置站", "Tank"])]
//[AddINotifyPropertyChangedInterface]
public partial class BatTankModel_Old
{
   private long _trayId;

   /// <summary>
   /// 托盘ID
   /// </summary>
   [Languages(["ID", "ID", "ID"])]
   [SugarColumn(IsPrimaryKey = true, ColumnDescription = "ID")] //设置主键
   [DynamicClass(IsIgnoreProperty = true)] //合并类时忽略整个属性
   public long TrayID
   {
      get { return _trayId; }
      set
      {
         if (value != _trayId)
         {
            _trayId = value;
            TankTime = SnowflakeHelper.GetDateTimeFromId(value);
         }
      }
   }

   /// <summary>
   /// 静置站时间，同时为分表字段
   /// </summary>
   [Languages(["静置站时间", "", "Tank time"])]
   [SugarColumn(ColumnDescription = "静置站时间")]
   [SplitField] //分表字段
   [DynamicClass(IgnoreAttributes = [typeof(SplitFieldAttribute)])] //合并类时忽略整个属性
   public DateTime TankTime { get; set; }

   /// <summary>
   /// 缸号
   /// </summary>
   [Languages(["缸号", "缸号", "Tank index"])]
   [SugarColumn(ColumnDescription = "缸号", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public byte TankIndex { get; set; }

   /// <summary>
   /// 静置循环次数
   /// </summary>
   [Languages(["静置循环次数", "静置循環次數", "Cycle number"])]
   [SugarColumn(ColumnDescription = "静置循环次数")] //UNSIGNED 无符号
   public int CycleNumber { get; set; }

   /// <summary>
   /// 循环时长
   /// </summary>
   [Languages(["循环时间", "循環時間", "Cycle duration"])]
   [SugarColumn(ColumnDescription = "循环时长")]
   public int CycleDuration { get; set; }

   /// <summary>
   /// 功能
   /// </summary>
   [SugarColumn(ColumnDescription = "功能")]
   [Languages(["功能", "", "Func"])]
   public string Func { get; set; } = string.Empty;

   /// <summary>
   /// 设定压力
   /// </summary>
   [SugarColumn(ColumnDescription = "设定压力")]
   [Languages(["设定压力", "", "Set pressure"])]
   public string SetPressure { get; set; } = string.Empty;

   /// <summary>
   /// 设定时长
   /// </summary>
   [SugarColumn(ColumnDescription = "设定时长")]
   [Languages(["设定时长", "", "Set hold pressure duration"])]
   public string SetHoldPressureDuration { get; set; } = string.Empty;

   /// <summary>
   /// 实际压力
   /// </summary>
   [SugarColumn(ColumnDescription = "实际压力", Length = 1024)]
   [Languages(["实际压力", "", "Actual pressure"])]
   public string ActualPressure { get; set; } = string.Empty;

   /// <summary>
   /// 实际保压时长
   /// </summary>
   [SugarColumn(ColumnDescription = "实际保压时长", Length = 1024)]
   [Languages(["实际保压时长", "", "Actual hold pressure duration"])]
   public string ActualHoldPressureDuration { get; set; } = string.Empty;

   /// <summary>
   /// 实际到达时长
   /// </summary>
   [SugarColumn(ColumnDescription = "实际到达时长", Length = 1024)]
   [Languages(["实际到达时长", "", "Actual Reaching pressure duration"])]
   public string ActualReachingPressureDuration { get; set; } = string.Empty;
}
