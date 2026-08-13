namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 弃用
/// </summary>
[AddINotifyPropertyChangedInterface]
//[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.回氦], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages(["回氦", "回氦", "Back helium"])]
[Obsolete("弃用")]
public partial class BatBackHeliumNJGXModel2
{
   /// <summary>
   /// 回氦时间
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦时间")]
   [Languages(["回氦时间"])]
   [OrderMarker]
   public DateTime BackHeliumTime { get; set; }

   /// <summary>
   /// 回氦位置
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   [Languages(["回氦位置"])]
   //[Statistics(ProcessTypeEnum.回氦, StatisticsFuncEnum.通道)]
   public ushort HeliumLocation { get; set; }

   /// <summary>
   /// 回氦前真空(kpa)
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦前真空(kpa)")]
   [Languages(["回氦前真空(kpa)"])]
   public float HeliumBeforeVacuum { get; set; }

   /// <summary>
   /// 回氦后真空(kpa)
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦后真空(kpa)")]
   [Languages(["回氦后真空(kpa)"])]
   public float HeliumAfterVacuum { get; set; }

   /// <summary>
   /// 回氦时长(m)
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦时长(ms)")]
   [Languages(["回氦时长(ms)"])]
   public float BackHeliumDuration { get; set; }

   /// <summary>
   /// 箱体真空值
   /// </summary>
   [SugarColumn(ColumnDescription = "箱体真空值")]
   [Languages(["箱体真空值"])]
   public float Box { get; set; }

   /// <summary>
   /// 回氦前真空保压时间
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦前真空保压时间")]
   [Languages(["回氦前真空保压时间"])]
   public float HeliumBeforeVacuumKeepTime1 { get; set; }

   /// <summary>
   /// 回氦后真空保压时间
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦后真空保压时间")]
   [Languages(["回氦后真空保压时间"])]
   public float HeliumBeforeVacuumKeepTime2 { get; set; }

   /// <summary>
   /// 回氦后压力保压时长(s)
   /// </summary>
   [SugarColumn(ColumnDescription = " 回氦后压力保压时长(s)")]
   [Languages(["回氦后压力保压时长(s)"])]
   public float HeliumAfterPressureKeepTime { get; set; }

   /// <summary>
   /// 回氦平压值
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦平压值")]
   [Languages(["回氦平压值"])]
   public float ReturnHeliumFlatPressureValue { get; set; }

   /// <summary>
   /// 回氦结果
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦结果")]
   [Languages(["回氦结果"])]
   [DynamicClass(Process = ProcessTypeEnum.回氦)]
   [ProcessRatio(nameof(BackHeliumTime))]
   public ResultTypeEnum BackHeliumResult { get; set; }
}
