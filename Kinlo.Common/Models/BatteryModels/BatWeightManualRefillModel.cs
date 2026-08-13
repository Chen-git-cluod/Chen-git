namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 手工补液
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.手动补液], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.手动补液), "", "Manual refill weight"])]
[AddINotifyPropertyChangedInterface]
public partial class BatWeightManualRefillModel
{
   /// <summary>
   ///
   /// </summary>
   [SugarColumn(ColumnDescription = "手动补液时间")]
   [Languages(["手动补液时间", "手动补液时间", "Manual refill Time"])]
   [OrderMarker]
   public DateTime ManualRefillTime { get; set; }

   /// <summary>
   /// 手动补液称重
   /// </summary>
   [SugarColumn(ColumnDescription = "手动补液称重")]
   [Languages(["手动补液称重", "手动补液称重", "Manual refill weight"])]
   public double ManualRefillWeight { get; set; }

   /// <summary>
   /// 手动补液量
   /// </summary>
   [SugarColumn(ColumnDescription = "手动补液量")]
   [Languages(["补液量", "补液量", "Manual refill volume"])]
   public double ManualRefillVolume { get; set; }

   /// <summary>
   /// 手动补液结果
   /// </summary>
   [Languages(["手动补液结果", "手动补液结果", "Manual refill results"])]
   [SugarColumn(ColumnDescription = "手动补液结果")]
   [DynamicClass(ParentResult = nameof(BatWeightAfterModel.FinalInjectResult), Rule = ProductResultRuleEnum.由最后子结果决定父结果)]
   [ProcessRatio(nameof(ManualRefillTime), preResultName: nameof(BatTestLeakModel.LeakResult))]
   [ProcessRatioDetail(nameof(ResultTypeEnum.注液量偏多), ResultTypeEnum.注液量偏多)]
   [ProcessRatioDetail(nameof(ResultTypeEnum.注液量偏少), ResultTypeEnum.注液量偏少)]
   public ResultTypeEnum ManualRefillResult { get; set; }
}
