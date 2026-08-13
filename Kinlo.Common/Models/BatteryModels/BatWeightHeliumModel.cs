namespace Kinlo.Common.Models.BatteryModels;

[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.回氦称重], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.回氦称重), "", "Helium weight"])]
[AddINotifyPropertyChangedInterface]
public partial class BatWeightHeliumModel
{
   /// <summary>
   /// 回氦称重时间
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦称重时间")]
   [Languages(["回氦称重时间", "回氦稱重時間", "weight time"])]
   [OrderMarker]
   public DateTime HeliumWeightTime { get; set; }

   /// <summary>
   /// 回氦称位置
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦称位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   [Languages(["回氦称位置", "回氦稱位置", "weight index"])]
   public byte HeliumWeightIndex { get; set; }

   /// <summary>
   /// 回氦称重
   /// </summary>
   [SugarColumn(ColumnDescription = "回氦称重")]
   [Languages(["回氦称重", "回氦称重", "weight"])]
   public double HeliumWeight { get; set; }

   /// <summary>
   /// 来料重量范围
   /// </summary>
   [Languages(["来料重量范围", "", "Incoming Weight Range"])]
   [SugarColumn(ColumnDescription = "来料重量范围")]
   public string IncomingWeightRange { get; set; } = string.Empty;

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
   /// 回氦失液量(g)
   /// </summary>
   [Languages(["回氦失液量(g)", "", "helium loss of fluid(g)"])]
   [SugarColumn(ColumnDescription = "回氦失液量(g)")]
   public double HeliumLossOfFluid { get; set; }

   /// <summary>
   /// 回氦称重结果
   /// </summary>
   [Languages(["回氦称重结果", "回氦称重结果", "Weight result"])]
   [SugarColumn(ColumnDescription = "回氦称重结果")]
   [DynamicClass(Process = ProcessTypeEnum.回氦称重)]
   [ProcessRatio(nameof(HeliumWeightTime))]
   public ResultTypeEnum HeliumWeightResult { get; set; }
}
