namespace Kinlo.Common.Models.BatteryModels;

[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.预压钉], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.预压钉), "预压钉", "Prepreing nail"])]
[AddINotifyPropertyChangedInterface]
public partial class BatNailPrepressModel
{
   /// <summary>
   /// 预压钉时间
   /// </summary>
   [SugarColumn(ColumnDescription = "预压钉时间")]
   [Languages("预压钉时间")]
   [OrderMarker]
   public DateTime PrepressNailTime { get; set; }

   /// <summary>
   /// 预压钉高度
   /// </summary>
   [SugarColumn(ColumnDescription = "预压钉高度")]
   [Languages("预压钉高度")]
   public float PrepressNailHeight { get; set; }

   /// <summary>
   ///预压钉结果
   /// </summary>
   [SugarColumn(ColumnDescription = "预压钉结果")]
   [Languages("预压钉结果")]
   [DynamicClass(Process = ProcessTypeEnum.预压钉)]
   [ProcessRatio(nameof(PrepressNailTime))]
   public ResultTypeEnum PrepressNailResult { get; set; }
}
