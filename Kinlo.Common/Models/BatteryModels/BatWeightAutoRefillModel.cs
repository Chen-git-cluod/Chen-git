namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 自动回流补液
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.回流补液], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.回流补液), "", "Rework weight"])]
[AddINotifyPropertyChangedInterface]
public partial class BatWeightAutoRefillModel
{
   /// <summary>
   /// 回流称位置
   /// </summary>
   [SugarColumn(ColumnDescription = "回流称位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   [Languages(["回流称位置", "回流称位置", "Auto refill index"])]
   public byte AutoRefillWeightIndex { get; set; }

   /// <summary>
   /// 回流称重时间
   /// </summary>
   [SugarColumn(ColumnDescription = "回流称重时间")]
   [Languages(["回流称重时间", "回流称重时间", "Auto refill time"])]
   [OrderMarker]
   public DateTime AutoRefillTime { get; set; }

   /// <summary>
   /// 回流称重量
   /// </summary>
   [SugarColumn(ColumnDescription = "回流称重")]
   [Languages(["回流称重", "回流称重", "Auto refill weight"])]
   public double AutoRefillWeight { get; set; }

   /// <summary>
   /// 回流补液量
   /// </summary>
   [SugarColumn(ColumnDescription = "回流补液量")]
   [Languages(["回流补液量", "回流补液量", "auto refill volume"])]
   public double AutoRefillVolume { get; set; }

   /// <summary>
   /// 回流补液结果
   /// </summary>
   [Languages(["回流补液结果", "回流补液结果", "Auto refill results"])]
   [SugarColumn(ColumnDescription = "回流补液结果")]
   [DynamicClass(ParentResult = nameof(BatWeightAfterModel.FinalInjectResult), Rule = ProductResultRuleEnum.由最后子结果决定父结果)]
   [ProcessRatio(nameof(AutoRefillTime), preResultName: nameof(BatTestLeakModel.LeakResult))]
   [ProcessRatioDetail(nameof(ResultTypeEnum.注液量偏多), ResultTypeEnum.注液量偏多)]
   [ProcessRatioDetail(nameof(ResultTypeEnum.注液量偏少), ResultTypeEnum.注液量偏少)]
   public ResultTypeEnum AutoRefillResult { get; set; }
}
