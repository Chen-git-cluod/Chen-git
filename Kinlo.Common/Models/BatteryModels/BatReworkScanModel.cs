namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 回流扫码
/// </summary>
[AddINotifyPropertyChangedInterface]
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.回流扫码], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages(["回流扫码", "Rework Scan", "Rework Scan"])]
public partial class BatReworkScanModel
{
   /// <summary>
   /// 回流扫码时间
   /// </summary>
   [Languages(["回流扫码时间", "回流扫码时间", "Rework scan time"])]
   [SugarColumn(ColumnDescription = "回流扫码时间")]
   [OrderMarker]
   public DateTime ReworkScanTime { get; set; }

   /// <summary>
   /// 回流扫码位置
   /// </summary>
   [Languages(["回流扫码位置", "回流扫码位置", "Rework scan index"])]
   [SugarColumn(ColumnDescription = "回流扫码位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   // [Statistics(ProcessTypeEnum.回流扫码, StatisticsFuncEnum.通道)]
   public byte ReworkScanIndex { get; set; }

   /// <summary>
   /// 回流扫码结果
   /// </summary>
   [Languages(["回流扫码结果", "", "Rework scan reslut"])]
   [SugarColumn(ColumnDescription = "回流扫码结果")]
   [DynamicClass(Process = ProcessTypeEnum.回流扫码)]
   [ProcessRatio(nameof(ReworkScanTime))]
   public ResultTypeEnum ReworkScanResult { get; set; }
}
