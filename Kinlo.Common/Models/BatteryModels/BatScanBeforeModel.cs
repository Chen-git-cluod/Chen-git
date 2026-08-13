namespace Kinlo.Common.Models.BatteryModels;

[AddINotifyPropertyChangedInterface]
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.前扫码], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages(["前扫码", "前掃碼", "Before Scan"])]
public partial class BatScanBeforeModel
{
   /// <summary>
   /// 前扫码时间
   /// </summary>
   [SugarColumn(ColumnDescription = "前扫码时间")]
   [Languages(["前扫码时间", "前掃碼时间", "Before scan time"])]
   [OrderMarker]
   public DateTime BeforeScanTime { get; set; }

   /// <summary>
   /// 前扫码位置
   /// </summary>
   [Languages(["前扫码位置", "前掃碼位置", "Before scan index"])]
   [SugarColumn(ColumnDescription = "前扫码位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   //[Statistics(ProcessTypeEnum.前扫码, StatisticsFuncEnum.通道)]
   public byte BeforeScanIndex { get; set; }

   /// <summary>
   /// 电池干重(g)
   /// </summary>
   [Languages(["电池干重(g)", "電池幹重(g)", "Net weight(g)"])]
   [SugarColumn(ColumnDescription = "电池干重(g)")]
   public double NetWeight { get; set; }

   /// <summary>
   /// 前工序重量(g),前工序出站重量
   /// </summary>
   [Languages(["前工序重量(g)", "", "Pre process Weight(g)"])]
   [SugarColumn(ColumnDescription = "前工序重量(g)")]
   public double PreProcessWeight { get; set; }

   /// <summary>
   ///前扫码结果
   /// </summary>
   [Languages(["前扫码结果", "前掃碼結果", "Before Scan Result"])]
   [SugarColumn(ColumnDescription = "前扫码结果")]
   [DynamicClass(Process = ProcessTypeEnum.前扫码)]
   [ProcessRatio(nameof(BeforeScanTime))]
   public ResultTypeEnum BeforeScanResult { get; set; }
}
