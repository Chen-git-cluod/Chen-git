namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 密封钉
/// </summary>
[BatteryDisplay(
   DisplayProcesses = [ProcessTypeEnum.打钉检测, ProcessTypeEnum.回流打钉检测],
   DeviceCommunicationType = [CommunicationEnum.None]
)]
[Languages([nameof(ProcessTypeEnum.打钉检测), "打钉检测", "Nail check"])]
[Languages([nameof(ProcessTypeEnum.回流打钉检测), "回流打钉检测", "Rework nail check"])]
[AddINotifyPropertyChangedInterface]
public partial class BatNailModel
{
   /// <summary>
   /// 打钉时间
   /// </summary>
   [SugarColumn(ColumnDescription = "打钉时间")]
   [Languages("打钉时间")]
   [OrderMarker]
   public DateTime NailTime { get; set; }

   /// <summary>
   /// 打钉通道
   /// </summary>
   [SugarColumn(ColumnDescription = "打钉通道", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   [Languages(["打钉通道", "打钉通道", "Nail index"])]
   // [Statistics(ProcessTypeEnum.打钉检测, StatisticsFuncEnum.通道)]
   public byte NailIndex { get; set; }

   /// <summary>
   /// 打钉高度
   /// </summary>
   [SugarColumn(ColumnDescription = "打钉高度")]
   [Languages("打钉高度")]
   public float NailHeight { get; set; }

   /// <summary>
   ///打钉结果
   /// </summary>
   [SugarColumn(ColumnDescription = "打钉结果")]
   [Languages("打钉结果")]
   [DynamicClass(Process = ProcessTypeEnum.打钉检测)]
   [ProcessRatio(nameof(NailTime))]
   public ResultTypeEnum NailResult { get; set; }
}
