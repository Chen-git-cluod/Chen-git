namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 测电压 爱测AC35
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.测电压], DeviceCommunicationType = [CommunicationEnum.VoltogeTest_AC35])]
[Languages(["测电压", "测电压", "Voltage test"])]
[AddINotifyPropertyChangedInterface]
public partial class BatVoltageTestAc35Model
{
   /// <summary>
   /// 测电压时间
   /// </summary>
   [Languages(["测电压时间", "测电压时间", "Voltage test time"])]
   [SugarColumn(ColumnDescription = "测电压时间")]
   [OrderMarker]
   public DateTime TestVoltageTime { get; set; }

   /// <summary>
   /// 测电压位置
   /// </summary>
   [Languages(["测电压位置", "测电压位置", "Voltage test index"])]
   [SugarColumn(ColumnDescription = "测电压位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public byte TestVoltageIndex { get; set; }

   /// <summary>
   /// 交流内阻(Ω)
   /// </summary>
   [Languages(["交流内阻(Ω)", "交流内阻(Ω)", "Ac Resistance(Ω)"])]
   [SugarColumn(ColumnDescription = "交流内阻(Ω)")]
   public double AcResistance { get; set; }

   /// <summary>
   /// 电压(V)
   /// </summary>
   [Languages(["电压(V)", "电压(V)", "Voltage(V)"])]
   [SugarColumn(ColumnDescription = "电压(V)")]
   public double TestVoltageValue { get; set; }

   /// <summary>
   /// 电压范围
   /// </summary>
   [Languages(["电压范围(V)", "电压范围(V)", "Voltage range(V)"])]
   [SugarColumn(ColumnDescription = "电压范围(V)")]
   public string VoltageRange { get; set; } = string.Empty;

   /// <summary>
   /// 标准偏差
   /// </summary>
   [Languages(["标准偏差", "标准偏差", "Standard deviation"])]
   [SugarColumn(ColumnDescription = "标准偏差")]
   public string VoltageStandardDeviation { get; set; } = string.Empty;

   /// <summary>
   /// 测电压结果
   /// </summary>
   [Languages(["测电压结果", "测电压结果", "Voltage test reslut"])]
   [SugarColumn(ColumnDescription = "测电压结果")]
   [DynamicClass(Process = ProcessTypeEnum.测电压)]
   [ProcessRatio(nameof(TestVoltageTime))]
   public ResultTypeEnum VoltageTestResult { get; set; }
}
