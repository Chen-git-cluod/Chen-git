namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 清洗机出站
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.清洗机出站], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.清洗机出站)])]
[AddINotifyPropertyChangedInterface]
public partial class BatCleaningMachineExitModel
{
   /// <summary>
   /// 清洗时间
   /// </summary>
   [SugarColumn(ColumnDescription = "清洗时间")]
   [Languages(["清洗时间"])]
   [OrderMarker]
   public DateTime CleaningTime { get; set; }

   /// <summary>
   /// 清洗温度1
   /// </summary>
   [Languages(["清洗温度1", "", "Cleaning Water Temp 1"])]
   [SugarColumn(ColumnDescription = "清洗温度1")]
   public double CleaningWaterTemp1 { get; set; }

   /// <summary>
   /// 清洗温度2
   /// </summary>
   [Languages(["清洗温度2", "", "Cleaning Water Temp 2"])]
   [SugarColumn(ColumnDescription = "清洗温度2")]
   public double CleaningWaterTemp2 { get; set; }

   /// <summary>
   /// 清洗温度3
   /// </summary>
   [Languages(["清洗温度3", "", "Cleaning Water Temp 3"])]
   [SugarColumn(ColumnDescription = "清洗温度3")]
   public double CleaningWaterTemp3 { get; set; }

   /// <summary>
   /// 烘烤温度1
   /// </summary>
   [Languages(["烘烤温度1", "", "Drying Temp 1"])]
   [SugarColumn(ColumnDescription = "烘烤温度1")]
   public double DryingTemp1 { get; set; }

   /// <summary>
   /// 烘烤温度2
   /// </summary>
   [Languages(["烘烤温度2", "", "Drying Temp 2"])]
   [SugarColumn(ColumnDescription = "烘烤温度2")]
   public double DryingTemp2 { get; set; }

   /// <summary>
   /// 设定清洗温度1
   /// </summary>
   [Languages(["设定清洗温度1", "", "Set Cleaning Water Temp 1"])]
   [SugarColumn(ColumnDescription = "设定清洗温度1")]
   public double SetCleaningWaterTemp1 { get; set; }

   /// <summary>
   /// 设定清洗温度2
   /// </summary>
   [Languages(["设定清洗温度2", "", "Set Cleaning Water Temp 2"])]
   [SugarColumn(ColumnDescription = "设定清洗温度2")]
   public double SetCleaningWaterTemp2 { get; set; }

   /// <summary>
   /// 设定清洗温度3
   /// </summary>
   [Languages(["设定清洗温度3", "", "Set Cleaning Water Temp 3"])]
   [SugarColumn(ColumnDescription = "设定清洗温度3")]
   public double SetCleaningWaterTemp3 { get; set; }

   /// <summary>
   /// 烘烤温度1
   /// </summary>
   [Languages(["设定烘烤温度1", "", "Set Drying Temp 1"])]
   [SugarColumn(ColumnDescription = "设定烘烤温度1")]
   public double SetDryingTemp1 { get; set; }

   /// <summary>
   /// 设定烘烤温度2
   /// </summary>
   [Languages(["设定烘烤温度2", "", "Set Drying Temp 2"])]
   [SugarColumn(ColumnDescription = "设定烘烤温度2")]
   public double SetDryingTemp2 { get; set; }

   /// <summary>
   /// PH 值
   /// </summary>
   [Languages(["PH值", "", "PH value"])]
   [SugarColumn(ColumnDescription = "PH值")]
   public double PhValue { get; set; }

   /// <summary>
   /// 测PH电压
   /// </summary>
   [Languages(["测PH电压", "", "PH Test voltage"])]
   [SugarColumn(ColumnDescription = "测PH电压")]
   public double PhTestVoltage { get; set; }

   /// <summary>
   /// 测PH温度
   /// </summary>
   [Languages(["测PH温度", "", "PH Test Temp"])]
   [SugarColumn(ColumnDescription = "测PH温度")]
   public double PhTestTemp { get; set; }

   /// <summary>
   ///  PH_ORP值
   /// </summary>
   [Languages(["PH_ORP", "", "PH_ORP"])]
   [SugarColumn(ColumnDescription = "PH_ORP")]
   public double PhORPValule { get; set; }
}
