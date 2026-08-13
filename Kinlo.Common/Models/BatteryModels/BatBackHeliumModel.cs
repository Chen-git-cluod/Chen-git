namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 回氦
/// </summary>
[AddINotifyPropertyChangedInterface]
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.回氦], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages(["回氦", "回氦", "Back helium"])]
public partial class BatBackHeliumModel
{
   /// <summary>
   /// 回氦时间
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦时间")]
   [Languages(["回氦时间"])]
   [OrderMarker]
   public DateTime BackHeliumTime { get; set; }

   /// <summary>
   /// 回氦前第1段真空度设置
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦前第1段真空度设置")]
   [Languages(["回氦前第1段真空度设置"])]
   public float HeliumBeforeVacuumSet1 { get; set; }

   /// <summary>
   /// 回氦前第2段真空度设置
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦前第2段真空度设置")]
   [Languages(["回氦前第2段真空度设置"])]
   public float HeliumBeforeVacuumSet2 { get; set; }

   /// <summary>
   /// 回氦后真空度设置
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦后真空度设置")]
   [Languages(["回氦后真空度设置"])]
   public float HeliumAfterVacuumSet { get; set; }

   /// <summary>
   /// 回氦前真空上限设置
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦前真空上限设置")]
   [Languages(["回氦前真空上限设置"])]
   public float HeliumBeforeVacuumULSet { get; set; }

   /// <summary>
   /// 回氮前真空下限设置
   /// </summary>
   [SugarColumn(ColumnDescription = "回氮前真空下限设置")]
   [Languages(["回氮前真空下限设置"])]
   public float HeliumBeforeVacuumLLSet { get; set; }

   /// <summary>
   /// 回氦后真空上限设置
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦后真空上限设置")]
   [Languages(["回氦后真空上限设置"])]
   public float HeliumAfterVacuumULSet { get; set; }

   /// <summary>
   /// 回氦后真空下限设置
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦后真空下限设置")]
   [Languages(["回氦后真空下限设置"])]
   public float HeliumAfterVacuumLLSet { get; set; }

   /// <summary>
   /// 回氮前第1段保压时间
   /// </summary>
   [SugarColumn(ColumnDescription = "回氮前第1段保压时间")]
   [Languages(["回氮前第1段保压时间"])]
   public short HeliumBeforeKeepTime1 { get; set; }

   /// <summary>
   /// 回氮前第2段保压时间
   /// </summary>
   [SugarColumn(ColumnDescription = "回氮前第2段保压时间")]
   [Languages(["回氮前第2段保压时间"])]
   public short HeliumBeforeKeepTime2 { get; set; }

   /// <summary>
   /// 回氦后保压时间
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦后保压时间")]
   [Languages(["回氦后保压时间"])]
   public short HeliumAfterKeepTime { get; set; }

   /// <summary>
   /// 回氦抽真空段数设置
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦抽真空段数设置")]
   [Languages(["回氦抽真空段数设置"])]
   public short VacuumNumberSet { get; set; }

   /// <summary>
   /// 回氮前第1段真空度实际值
   /// </summary>
   [SugarColumn(ColumnDescription = "回氮前第1段真空度")]
   [Languages(["回氮前第1段真空度"])]
   public float HeliumBeforeVacuumActual1 { get; set; }

   /// <summary>
   /// 回氦前第2段真空度实际值
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦前第2段真空度")]
   [Languages(["回氦前第2段真空度"])]
   public float HeliumBeforeVacuumActual2 { get; set; }

   /// <summary>
   /// 回氦后真空度
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦后真空度")]
   [Languages(["回氦后真空度"])]
   public float HeliumAfterVacuumActual { get; set; }

   /// <summary>
   /// 回氦总时长
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦总时长")]
   [Languages(["回氦总时长"])]
   public short HeliumTime { get; set; }

   /// <summary>
   /// 回氦站号
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦站号", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   [Languages(["回氦站号"])]
   //[Statistics(ProcessTypeEnum.回氦, StatisticsFuncEnum.通道)]
   public byte HeliumStationNo { get; set; }

   /// <summary>
   /// 回氦位置
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   [Languages(["回氦位置"])]
   public byte HeliumPosition { get; set; }

   /// <summary>
   /// 回氦结果
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦结果")]
   [Languages(["回氦结果"])]
   [DynamicClass(Process = ProcessTypeEnum.回氦)]
   [ProcessRatio(nameof(BackHeliumTime))]
   public ResultTypeEnum BackHeliumResult { get; set; }
}
