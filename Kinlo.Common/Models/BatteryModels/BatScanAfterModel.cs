namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 后扫码
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.后扫码], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.后扫码), "", "After scan"])]
[AddINotifyPropertyChangedInterface]
public partial class BatScanAfterModel
{
   /// <summary>
   /// 后扫码时间
   /// </summary>
   [Languages(["后扫码时间", "后扫码时间", "After scan time"])]
   [SugarColumn(ColumnDescription = "后扫码时间")]
   [OrderMarker]
   public DateTime AfterScanTime { get; set; }

   /// <summary>
   /// 后扫码位置
   /// </summary>
   [Languages(["后扫码位置", "后扫码位置", "After scan index"])]
   [SugarColumn(ColumnDescription = "后扫码位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   //[Statistics(ProcessTypeEnum.后扫码, StatisticsFuncEnum.通道)]
   public byte AfterIndex { get; set; }

   /// <summary>
   /// 后扫码结果
   /// </summary>
   [Languages(["后扫码结果", "", "After scan reslut"])]
   [SugarColumn(ColumnDescription = "后扫码结果")]
   [DynamicClass(Process = ProcessTypeEnum.后扫码)]
   [ProcessRatio(nameof(AfterScanTime))]
   public ResultTypeEnum AfterScanResult { get; set; }
}
