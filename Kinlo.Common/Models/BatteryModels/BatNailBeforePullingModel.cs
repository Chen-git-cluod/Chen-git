namespace Kinlo.Common.Models.BatteryModels;

[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.拔钉前检测], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.拔钉前检测), "拔钉前检测", "Before Pulling Nail"])]
[AddINotifyPropertyChangedInterface]
public partial class BatNailBeforePullingModel
{
   /// <summary>
   /// 拔钉前时间
   /// </summary>
   [SugarColumn(ColumnDescription = "拔钉前时间")]
   [Languages("拔钉前时间")]
   [OrderMarker]
   public DateTime BeforePullingNailTime { get; set; }

   /// <summary>
   /// 前工序化成钉高度
   /// </summary>
   [SugarColumn(ColumnDescription = "前工序化成钉高度")]
   [Languages("前工序化成钉高度")]
   public float PreProcessNailHeight { get; set; }

   /// <summary>
   /// 拔钉前高度
   /// </summary>
   [SugarColumn(ColumnDescription = "拔钉前高度")]
   [Languages("拔钉前高度")]
   public float BeforePullingNailHeight { get; set; }

   /// <summary>
   /// 胶钉高度差
   /// </summary>
   [SugarColumn(ColumnDescription = "胶钉高度差")]
   [Languages("胶钉高度差")]
   public float NailHeightDifference { get; set; }

   /// <summary>
   ///拔钉前结果
   /// </summary>
   [SugarColumn(ColumnDescription = "拔钉前结果")]
   [Languages("拔钉前结果")]
   [DynamicClass(Process = ProcessTypeEnum.拔钉前检测)]
   [ProcessRatio(nameof(BeforePullingNailTime))]
   public ResultTypeEnum BeforePullingNailResult { get; set; }
}
