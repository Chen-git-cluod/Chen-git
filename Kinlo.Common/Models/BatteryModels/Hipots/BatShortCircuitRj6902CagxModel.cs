namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 锐捷短路测试
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.测短路], DeviceCommunicationType = [CommunicationEnum.ShortCircuit_RJ6902CAGX])]
[Languages(["测短路", "测短路", "Short circuit test"])]
[AddINotifyPropertyChangedInterface]
public partial class BatShortCircuitRj6902CagxModel
{
   /// <summary>
   /// 短路时间
   /// </summary>
   [SugarColumn(ColumnDescription = "短路时间")]
   [Languages(["短路时间", "短路时间", "ShortCircuit time"])]
   [OrderMarker]
   public DateTime ShortCircuitTestRJTime { get; set; }

   /// <summary>
   /// 短路测试位置
   /// </summary>
   [SugarColumn(ColumnDescription = "短路测试位置", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   [Languages(["短路测试位置", "短路测试位置", "ShortCircuit index"])]
   // [Statistics(ProcessTypeEnum.测短路, StatisticsFuncEnum.通道)]
   public byte ShortCircuitTestRJIndex { get; set; }

   /// <summary>
   /// 升压时间(ms)
   /// </summary>
   [SugarColumn(ColumnDescription = "升压时间(ms)")]
   [Languages(["升压时间", "升压时间", "boost time"])]
   public double BoostTime { get; set; }

   /// <summary>
   /// 电阻测试值(MΩ)
   /// </summary>
   [SugarColumn(ColumnDescription = "电阻测试值(MΩ)")]
   [Languages(["电阻测试值", "", ""])]
   public float ResistanceTestValue { get; set; }

   /// <summary>
   /// 跌落1(V)
   /// </summary>
   [SugarColumn(ColumnDescription = "跌落1(V)")]
   [Languages(["跌落1", "", ""])]
   public int FallOne { get; set; }

   /// <summary>
   /// 跌落2(V)
   /// </summary>
   [SugarColumn(ColumnDescription = "跌落2(V)")]
   [Languages(["跌落2", "", ""])]
   public int FallTwo { get; set; }

   /// <summary>
   /// 跌落3(V)
   /// </summary>
   [SugarColumn(ColumnDescription = "跌落3(V)")]
   [Languages(["跌落3", "", ""])]
   public int FallThree { get; set; }

   /// <summary>
   /// VP电压(V)
   /// </summary>
   [SugarColumn(ColumnDescription = "VP电压(V)")]
   [Languages(["VP电压", "", ""])]
   public int ShellShortCircuitVoltage { get; set; }

   /// <summary>
   /// 电容数据
   /// </summary>
   [SugarColumn(ColumnDescription = "电容数据")]
   [Languages(["电容数据", "", ""])]
   public float ShellShortCircuitCapacitance { get; set; }

   /// <summary>
   /// 开路结果
   /// </summary>
   [SugarColumn(ColumnDescription = "开路结果")]
   [Languages(["开路结果", "", ""])]
   public int ShellShortCircuitOpenCircuitResult { get; set; }

   /// <summary>
   /// VP结果
   /// </summary>
   [SugarColumn(ColumnDescription = "VP结果")]
   [Languages(["VP结果", "", ""])]
   public int ShellShortCircuitVoltageResult { get; set; }

   /// <summary>
   /// 放电1结果
   /// </summary>
   [SugarColumn(ColumnDescription = "放电1结果")]
   [Languages(["放电1结果", "", ""])]
   public int ShellShortCircuitDischargeOneResult { get; set; }

   /// <summary>
   /// 放电2结果
   /// </summary>
   [SugarColumn(ColumnDescription = "放电2结果")]
   [Languages(["放电2结果", "", ""])]
   public int ShellShortCircuitDischargeTwoResult { get; set; }

   /// <summary>
   /// 跌落1结果
   /// </summary>
   [SugarColumn(ColumnDescription = "跌落1结果")]
   [Languages(["跌落1结果", "", ""])]
   public int ShellShortCircuitFallOneResult { get; set; }

   /// <summary>
   /// 跌落2结果
   /// </summary>
   [SugarColumn(ColumnDescription = "跌落2结果")]
   [Languages(["跌落2结果", "", ""])]
   public int ShellShortCircuitFallTwoResult { get; set; }

   /// <summary>
   /// 跌落3结果
   /// </summary>
   [SugarColumn(ColumnDescription = "跌落3结果")]
   [Languages(["跌落3结果", "", ""])]
   public int ShellShortCircuitFallThreeResult { get; set; }

   /// <summary>
   /// TL结果
   /// </summary>
   [SugarColumn(ColumnDescription = "TL结果")]
   [Languages(["TL结果", "", ""])]
   public int ShellShortCircuitTLResult { get; set; }

   /// <summary>
   /// TH结果
   /// </summary>
   [SugarColumn(ColumnDescription = "TH结果")]
   [Languages(["TH结果", "", ""])]
   public int ShellShortCircuitTHResult { get; set; }

   /// <summary>
   /// 电阻测试结果
   /// </summary>
   [SugarColumn(ColumnDescription = "电阻测试结果")]
   [Languages(["电阻测试结果", "", ""])]
   public int ShellShortCircuitResistanceTestResult { get; set; }

   /// <summary>
   /// 电容测试结果
   /// </summary>
   [SugarColumn(ColumnDescription = "电容测试结果")]
   [Languages(["电容测试结果", "", ""])]
   public int ShellShortCircuitCapacitanceResult { get; set; }

   /// <summary>
   /// 短路测试结果
   /// </summary>
   [SugarColumn(ColumnDescription = "短路测试结果")]
   [Languages(["短路测试结果", "", ""])]
   [DynamicClass(Process = ProcessTypeEnum.测短路)]
   [ProcessRatio(nameof(ShortCircuitTestRJTime))]
   public ResultTypeEnum ShortCircuitResult { get; set; }
}
