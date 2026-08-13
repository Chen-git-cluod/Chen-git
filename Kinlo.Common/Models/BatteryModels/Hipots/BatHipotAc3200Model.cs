namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
///测短路 (AC3200)
/// </summary>
[BatteryDisplay(
   DisplayProcesses = [ProcessTypeEnum.测短路],
   DeviceCommunicationType = [
      CommunicationEnum.ShortCircuit_AC3200,
      CommunicationEnum.ShortCircuit_Ainuo_ANBTS7201, //临时使用
   ]
)]
[Languages([nameof(ProcessTypeEnum.测短路), "测短路", "Hipot tester"])]
[AddINotifyPropertyChangedInterface]
public partial class BatHipotAc3200Model
{
   /// <summary>
   /// 测短路时间
   /// </summary>
   [Languages(["测短路时间", "测短路时间", "Hipot time"])]
   [SugarColumn(ColumnDescription = "测短路时间")]
   [OrderMarker]
   public DateTime HipotTime { get; set; }

   /// <summary>
   /// 测短路通道
   /// </summary>
   [Languages(["测短路通道", "测短路通道", "Hipot index"])]
   [SugarColumn(ColumnDescription = "测短路通道", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   // [Statistics(ProcessTypeEnum.测短路, StatisticsFuncEnum.通道)]
   public byte HipotIndex { get; set; }

   /// <summary>
   /// 脉冲结果
   /// </summary>
   [SugarColumn(ColumnDescription = "脉冲结果")]
   [Languages(["脉冲结果", "", "Pulse"])]
   public string HipotPulseResult { get; set; } = string.Empty;

   /// <summary>
   /// VP电压(V)
   /// </summary>
   [SugarColumn(ColumnDescription = "VP电压(V)")]
   [Languages(["VP电压(V)", "", ""])]
   public int HipotVpVoltage { get; set; }

   /// <summary>
   /// 跌落1(V)
   /// </summary>
   [SugarColumn(ColumnDescription = "跌落1(V)")]
   [Languages(["跌落1(V)", "", ""])]
   public int HipotFallOne { get; set; }

   /// <summary>
   /// 跌落2(V)
   /// </summary>
   [SugarColumn(ColumnDescription = "跌落2(V)")]
   [Languages(["跌落2(V)", "", ""])]
   public int HipotFallTwo { get; set; }

   /// <summary>
   /// 跌落3(V)
   /// </summary>
   [SugarColumn(ColumnDescription = "跌落3(V)")]
   [Languages(["跌落3(V)", "", ""])]
   public int HipotFallThree { get; set; }

   /// <summary>
   /// 脉冲项 TP 值 TP(ms)
   /// </summary>
   [SugarColumn(ColumnDescription = "TP(ms)")]
   [Languages(["TP(ms)", "", ""])]
   public float HipotPulseTp { get; set; }

   /// <summary>
   /// 绝缘电阻的测试结果
   /// </summary>
   [SugarColumn(ColumnDescription = "电阻测试结果")]
   [Languages(["电阻测试结果", "", "Resistance test result"])]
   public string ResistanceTestResult { get; set; } = string.Empty;

   /// <summary>
   /// 阻值
   /// </summary>
   [Languages(["阻值", "阻值", "Hiption value"])]
   [SugarColumn(ColumnDescription = "阻值")]
   public float InsulationTestValue { get; set; }

   /// <summary>
   /// 电容的测试结果
   /// </summary>
   [Languages(["电容的测试结果", "电容的测试结果", "Capacitors result"])]
   [SugarColumn(ColumnDescription = "电容的测试结果")]
   public string CapacitorsResult { get; set; } = string.Empty;

   /// <summary>
   /// 电容值
   /// </summary>
   [Languages(["电容值", "电容值", "Capacitors"])]
   [SugarColumn(ColumnDescription = "电容值")]
   public float Capacitors { get; set; }

   /// <summary>
   /// 电压波形
   /// </summary>
   [Languages(["电压波形", "电压波形", "Curve"])]
   [SugarColumn(ColumnDescription = "电压波形", ColumnDataType = "text")]
   public string CurvePoint { get; set; } = string.Empty;

   /// <summary>
   /// 测短路结果
   /// </summary>
   [Languages(["测短路结果", "", "Hiption reslut"])]
   [SugarColumn(ColumnDescription = "测短路结果")]
   [DynamicClass(Process = ProcessTypeEnum.测短路)]
   [ProcessRatio(nameof(HipotTime))]
   public ResultTypeEnum HipotResult { get; set; }
}
