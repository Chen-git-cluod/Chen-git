namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 锐捷短路测试
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.测短路], DeviceCommunicationType = [CommunicationEnum.ShortCircuit_RJ6902R])]
[Languages(["测短路", "测短路", "Short circuit test"])]
[AddINotifyPropertyChangedInterface]
public partial class BatShortCircuitRj6902rModel
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
   //[Statistics(ProcessTypeEnum.测短路, StatisticsFuncEnum.通道)]
   public byte ShortCircuitTestRJIndex { get; set; }

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
   /// VP电压(V)
   /// </summary>
   [SugarColumn(ColumnDescription = "VP电压(V)")]
   [Languages(["VP电压", "", ""])]
   public int ShellShortCircuitVoltage { get; set; }

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
   /// 开路结果
   /// </summary>
   [SugarColumn(ColumnDescription = "开路结果")]
   [Languages(["开路结果", "", ""])]
   public int ShellShortCircuitOpenCircuitResult { get; set; }

   /// <summary>
   /// 严重短路结果
   /// </summary>
   [SugarColumn(ColumnDescription = "严重短路结果")]
   [Languages(["严重短路结果", "", ""])]
   public int ShellShortCircuitVoltageResult { get; set; }

   /// <summary>
   /// 欠压结果
   /// </summary>
   [SugarColumn(ColumnDescription = "欠压结果")]
   [Languages(["欠压结果", "", ""])]
   public int ShellShortCircuitDischargeOneResult { get; set; }

   /// <summary>
   /// 过压结果
   /// </summary>
   [SugarColumn(ColumnDescription = "过压结果")]
   [Languages(["过压结果", "", ""])]
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
   /// 短路测试结果
   /// </summary>
   [SugarColumn(ColumnDescription = "短路测试结果")]
   [DynamicClass(Process = ProcessTypeEnum.测短路)]
   [Languages(["短路测试结果", "", ""])]
   [ProcessRatio(nameof(ShortCircuitTestRJTime))]
   public ResultTypeEnum ShortCircuitResult { get; set; }
}
