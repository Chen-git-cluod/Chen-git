namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 前称重
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.前称重], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.前称重), "前稱重", "Before Weight"])]
[AddINotifyPropertyChangedInterface]
public partial class BatWeightBeforeModel
{
   /// <summary>
   /// 前称时间
   /// </summary>
   [Languages(["前称时间", "前稱時間", "Before weight time"])]
   [SugarColumn(ColumnDescription = "前称时间")]
   [OrderMarker]
   public DateTime BeforeWeightTime { get; set; }

   /// <summary>
   /// 前称位置
   /// </summary>
   [Languages(["前称位置", "前稱位置", "Before Weight Index"])]
   [SugarColumn(ColumnDescription = "前称位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public byte BeforeWeightIndex { get; set; }

   /// <summary>
   /// 前称重量范围
   /// </summary>
   [Languages(["前称重量范围", "", "Incoming Weight Range"])]
   [SugarColumn(ColumnDescription = "前称重量范围")]
   public string BeforWeightRange { get; set; } = string.Empty;

   /// <summary>
   /// 前称重量
   /// </summary>
   [Languages(["前称重量", "前稱重量", "Before Weight"])]
   [SugarColumn(ColumnDescription = "前称重量")]
   public double BeforeWeight { get; set; }

   /// <summary>
   /// 首注目标注液量
   /// </summary>
   [Languages(["首注目标量", "首注目標量", "Target injection"])]
   [SugarColumn(ColumnDescription = "首注目标量")]
   public double TargetInjectionVolume { get; set; }

   /// <summary>
   /// 化成失液量(g)
   /// </summary>
   [Languages(["化成失液量(g)", "", "loss of fluid(g)"])]
   [SugarColumn(ColumnDescription = "化成失液量(g)")]
   public double LossOfFluid { get; set; }

   /// <summary>
   /// 前称重结果
   /// </summary>
   [Languages(["前称结果", "前稱結果", "Before weight result"])]
   [SugarColumn(ColumnDescription = "前称重结果")]
   [DynamicClass(Process = ProcessTypeEnum.前称重)]
   [ProcessRatio(nameof(BeforeWeightTime))]
   public ResultTypeEnum BeforeWeightResult { get; set; }
}
