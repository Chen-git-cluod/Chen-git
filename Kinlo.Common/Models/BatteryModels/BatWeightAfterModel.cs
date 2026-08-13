namespace Kinlo.Common.Models.BatteryModels;

[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.后称重], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.后称重), "", "After weight"])]
[AddINotifyPropertyChangedInterface]
public partial class BatWeightAfterModel
{
   /// <summary>
   /// 后称时间
   /// </summary>
   [SugarColumn(ColumnDescription = "后称时间")]
   [Languages(["后称时间", "後稱時間", "After weight time"])]
   [OrderMarker]
   public DateTime AfterWeightTime { get; set; }

   /// <summary>
   /// 后称位置
   /// </summary>
   [SugarColumn(ColumnDescription = "后称位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   [Languages(["后称位置", "後稱位置", "After weight index"])]
   public byte AfterWeightIndex { get; set; }

   /// <summary>
   /// 首注重量
   /// </summary>
   [SugarColumn(ColumnDescription = "首注重量")]
   [Languages(["首注重量", "首注重量", "first inject weight"])]
   public double FirstInjectWeight { get; set; }

   /// <summary>
   /// 最终重量范围
   /// </summary>
   [SugarColumn(ColumnDescription = "最终重量范围")]
   [Languages(["最终重量范围", "最终重量範圍", "final weighing range"])]
   public string AfterWeighingRange { get; set; } = string.Empty;

   /// <summary>
   /// 首注实注量
   /// </summary>
   [SugarColumn(ColumnDescription = "首注实注量")]
   [Languages(["首注实注量", "首注實注量", "Actual injection"])]
   public double ActualInjectionVolume { get; set; }

   /// <summary>
   /// 首注偏差
   /// </summary>
   [SugarColumn(ColumnDescription = "首注偏差")]
   [Languages(["首注偏差", "首注偏差", "Target injection deviation"])]
   public double TargetInjectionVolumeDeviation { get; set; }

   /// <summary>
   /// 首注结果
   /// </summary>
   [Languages(["首注结果", "首注结果", "first inject results"])]
   [SugarColumn(ColumnDescription = "首注结果")]
   [DynamicClass(ParentResult = nameof(FinalInjectResult), Rule = ProductResultRuleEnum.由最后子结果决定父结果)]
   [ProcessRatio(nameof(AfterWeightTime), preResultName: nameof(BatTestLeakModel.LeakResult), DisplayName = "首注结果")]
   [ProcessRatioDetail(nameof(ResultTypeEnum.注液量偏多), ResultTypeEnum.注液量偏多)]
   [ProcessRatioDetail(nameof(ResultTypeEnum.注液量偏少), ResultTypeEnum.注液量偏少)]
   public ResultTypeEnum FirstInjectResult { get; set; }

   /// <summary>
   /// 最终重量
   /// </summary>
   [SugarColumn(ColumnDescription = "最终重量")]
   [Languages(["最终重量", "最终重量", "final weight"])]
   public double FinalAfterWeight { get; set; }

   /// <summary>
   /// 保液量范围
   /// </summary>
   [SugarColumn(ColumnDescription = "保液量范围")]
   [Languages(["保液量范围", "保液量范围", "Injection volume range"])]
   public string InjectionVolumeRange { get; set; } = "0-100-200";

   /// <summary>
   /// 保液量
   /// </summary>
   [SugarColumn(ColumnDescription = "保液量")]
   [Languages(["保液量", "保液量", "Total injection"])]
   public double TotalInjectionVolume { get; set; }

   /// <summary>
   /// 保液量偏差
   /// </summary>
   [SugarColumn(ColumnDescription = "保液量偏差")]
   [Languages(["保液量偏差", "保液量偏差", "Total injection deviation"])]
   public double TotalInjectionVolumeDeviation { get; set; }

   /// <summary>
   /// 最终注液结果
   /// </summary>
   [Languages(["最终注液结果", "最终注液结果", "final inject results"])]
   [SugarColumn(ColumnDescription = "最终注液结果")]
   [DynamicClass(Process = ProcessTypeEnum.注液)]
   [ProcessRatio(nameof(AfterWeightTime), preResultName: nameof(BatTestLeakModel.LeakResult), DisplayName = "最终注液结果")]
   [ProcessRatioDetail(nameof(ResultTypeEnum.注液量偏多), ResultTypeEnum.注液量偏多)]
   [ProcessRatioDetail(nameof(ResultTypeEnum.注液量偏少), ResultTypeEnum.注液量偏少)]
   public ResultTypeEnum FinalInjectResult { get; set; }

   /// <summary>
   /// 最终重量结果（首注、自动补液、手动补液）
   /// </summary>
   [Languages(["最终重量结果", "最终重量结果", "final weight results"])]
   [SugarColumn(ColumnDescription = "最终重量结果")]
   [DynamicClass(Process = ProcessTypeEnum.注液后称重)]
   [ProcessRatio(nameof(AfterWeightTime), preResultName: nameof(BatTestLeakModel.LeakResult))]
   [ProcessRatioDetail(nameof(ResultTypeEnum.注液量偏多), ResultTypeEnum.注液量偏多)]
   [ProcessRatioDetail(nameof(ResultTypeEnum.注液量偏少), ResultTypeEnum.注液量偏少)]
   public ResultTypeEnum FinalWeighingResult { get; set; }
}
