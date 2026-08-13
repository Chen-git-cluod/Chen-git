namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
///测厚
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.测厚], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.测厚), "测厚", "Thick test"])]
[AddINotifyPropertyChangedInterface]
public partial class BatThickModel
{
   /// <summary>
   /// 测厚时间
   /// </summary>
   [Languages(["测厚时间", "测厚时间", "Thick test time"])]
   [SugarColumn(ColumnDescription = "测厚时间")]
   [OrderMarker]
   public DateTime ThickTime { get; set; }

   /// <summary>
   /// 测厚位置
   /// </summary>
   [Languages(["测厚位置", "测厚位置", "Test thick index"])]
   [SugarColumn(ColumnDescription = "测厚位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public byte ThickIndex { get; set; }

   /// <summary>
   /// 上厚度
   /// </summary>
   [Languages(["上厚度", "上厚度", "Top thickness"])]
   [SugarColumn(ColumnDescription = "上厚度")]
   public float TopThickness { get; set; }

   /// <summary>
   /// 下厚度
   /// </summary>
   [Languages(["下厚度", "下厚度", "Bottom thickness"])]
   [SugarColumn(ColumnDescription = "下厚度")]
   public float BottomThickness { get; set; }

   /// <summary>
   /// 上结果
   /// </summary>
   [Languages("上结果")]
   [SugarColumn(ColumnDescription = "上结果")]
   public string TopThicknessResult { get; set; } = string.Empty;

   /// <summary>
   /// 下结果
   /// </summary>
   [Languages("下结果")]
   [SugarColumn(ColumnDescription = "下结果")]
   public string BottomThicknessResult { get; set; } = string.Empty;

   /// <summary>
   /// 厚度范围
   /// </summary>
   [Languages(["厚度范围", "厚度范围", "thickness range"])]
   [SugarColumn(ColumnDescription = "厚度范围")]
   public string ThicknessRange { get; set; } = string.Empty;

   /// <summary>
   /// 测厚压力
   /// </summary>
   [Languages(["测厚压力", "测厚压力", "thickness pressure"])]
   [SugarColumn(ColumnDescription = "测厚压力")]
   public float ThicknessPressure { get; set; }

   /// <summary>
   /// 测厚压力范围
   /// </summary>
   [Languages(["测厚压力范围", "测厚压力范围", "thickness pressure range"])]
   [SugarColumn(ColumnDescription = "测厚压力范围")]
   public string ThicknessPressureRange { get; set; } = string.Empty;

   /// <summary>
   /// 测厚结果
   /// </summary>
   [Languages(["测厚结果", "", "Test thick reslut"])]
   [SugarColumn(ColumnDescription = "测厚结果")]
   [DynamicClass(Process = ProcessTypeEnum.测厚)]
   [ProcessRatio(nameof(ThickTime))]
   public ResultTypeEnum TestThickResult { get; set; }
}
