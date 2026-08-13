namespace Kinlo.Common.Models.BatteryModels;

[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.全压钉], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.全压钉), "全压钉", "After nail"])]
[AddINotifyPropertyChangedInterface]
public partial class BatNailAfterModel
{
   /// <summary>
   /// 全压钉时间
   /// </summary>
   [SugarColumn(ColumnDescription = "全压钉时间")]
   [Languages("全压钉时间")]
   [OrderMarker]
   public DateTime AfterNailTime { get; set; }

   /// <summary>
   /// 全压钉高度
   /// </summary>
   [SugarColumn(ColumnDescription = "全压钉高度")]
   [Languages("全压钉高度")]
   public float AfterNailHeight { get; set; }

   /// <summary>
   /// 全压钉结果
   /// </summary>
   [SugarColumn(ColumnDescription = "全压钉结果")]
   [Languages("全压钉结果")]
   [DynamicClass(Process = ProcessTypeEnum.全压钉)]
   [ProcessRatio(nameof(AfterNailTime))]
   public ResultTypeEnum AfterNailResult { get; set; }
}
