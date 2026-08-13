namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
///测厚
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.真空打钉], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.真空打钉), "真空打钉", "Vacuum Nail"])]
[AddINotifyPropertyChangedInterface]
public partial class BatVacuumNailModel
{
   /// <summary>
   /// 真空打钉时间
   /// </summary>
   [Languages("真空打钉时间")]
   [SugarColumn(ColumnDescription = "真空打钉时间")]
   [OrderMarker]
   public DateTime VacuumNailTime { get; set; }

   /// <summary>
   /// 设置真空值
   /// </summary>
   [Languages("设置真空值")]
   [SugarColumn(ColumnDescription = "设置真空值")]
   public float SetVaccumValue { get; set; }

   /// <summary>
   /// 设置保压时间
   /// </summary>
   [Languages("设置保压时间")]
   [SugarColumn(ColumnDescription = "设置保压时间")]
   public float KeepPressureTime { get; set; }

   /// <summary>
   /// 保压前真空值
   /// </summary>
   [Languages("保压前真空值")]
   [SugarColumn(ColumnDescription = "保压前真空值")]
   public float BeforeKeepPressureVacuumValue { get; set; }

   /// <summary>
   /// 保压后真空值
   /// </summary>
   [Languages("保压后真空值")]
   [SugarColumn(ColumnDescription = "保压后真空值")]
   public float AfterKeepPressureVacuumValue { get; set; }
}
