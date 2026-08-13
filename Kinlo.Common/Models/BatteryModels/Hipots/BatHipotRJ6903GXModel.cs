namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// RJ6903GX 绝缘测试结果模型
/// 缩写规则：
/// PtoN = Positive to Negative (正对负)
/// PtoC = Positive to Case (正对壳)
/// NtoC = Negative to Case (负对壳)
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.测短路], DeviceCommunicationType = [CommunicationEnum.ShortCircuit_RJ6903GX])]
[Languages([nameof(ProcessTypeEnum.测短路), "测短路", "Hipot tester"])]
[AddINotifyPropertyChangedInterface]
public partial class BatHipotRJ6903GXModel
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
   /// 测短路总结果
   /// </summary>
   [Languages(["测短路总结果", "", "Overall reslut"])]
   [SugarColumn(ColumnDescription = "测短路结果")]
   [DynamicClass(Process = ProcessTypeEnum.测短路)]
   [ProcessRatio(nameof(HipotTime))]
   public ResultTypeEnum HiptionOverallResult { get; set; }

   #region 正负

   /// <summary>
   /// 正负跌落1电压Vd1
   /// </summary>
   [Languages(["正负跌落1", "", "Positive To Negative Vd1"])]
   [SugarColumn(ColumnDescription = "正负跌落1")]
   public int PtoNVd1 { get; set; }

   /// <summary>
   /// 正负跌落2电压Vd2
   /// </summary>
   [Languages(["正负跌落2", "", "Positive To Negative Vd2"])]
   [SugarColumn(ColumnDescription = "正负跌落2")]
   public int PtoNVd2 { get; set; }

   /// <summary>
   /// 正负跌落3电压Vd3
   /// </summary>
   [Languages(["正负跌落3", "", "Positive To Negative Vd3"])]
   [SugarColumn(ColumnDescription = "正负跌落3")]
   public int PtoNVd3 { get; set; }

   /// <summary>
   /// 正负级 实际输出电压VP
   /// </summary>
   [Languages(["正负VP电压", "", "Positive To Negative Vp Voltage"])]
   [SugarColumn(ColumnDescription = "正负VP电压")]
   public int PtoNVpVoltage { get; set; }

   /// <summary>
   /// 正负 TP值
   /// </summary>
   [Languages(["正负TP", "", "Positive To Negative TP"])]
   [SugarColumn(ColumnDescription = "正负TP")]
   public double PtoNTpTime { get; set; }

   /// <summary>
   /// 正负 绝缘测试值
   /// </summary>
   [Languages(["正负绝缘阻值", "", "Positive To Negative Insulation"])]
   [SugarColumn(ColumnDescription = "正负绝缘阻值")]
   public double PtoNInsulation { get; set; }

   /// <summary>
   /// 正负结果
   /// </summary>
   [Languages(["正负结果", "", "Positive to Negative Result"])]
   [SugarColumn(ColumnDescription = "正负结果")]
   public ResultTypeEnum PtoNResult { get; set; }

   #endregion

   #region 正极对壳

   /// <summary>
   /// 正极对壳 跌落1电压Vd1
   /// </summary>
   [Languages(["正壳跌落1", "", "Positive To Case Vd1"])]
   [SugarColumn(ColumnDescription = "正壳跌落1")]
   public int PtoCVd1 { get; set; }

   /// <summary>
   /// 正极对壳 跌落2电压Vd2
   /// </summary>
   [Languages(["正壳跌落2", "", "Positive To Case Vd2"])]
   [SugarColumn(ColumnDescription = "正壳跌落2")]
   public int PtoCVd2 { get; set; }

   /// <summary>
   /// 正极对壳 跌落3电压Vd3
   /// </summary>
   [Languages(["正壳跌落3", "", "Positive To Case Vd3"])]
   [SugarColumn(ColumnDescription = "正壳跌落3")]
   public int PtoCVd3 { get; set; }

   /// <summary>
   /// 正极对壳 实际输出电压VP
   /// </summary>
   [Languages(["正壳VP电压", "", "Positive To Case Vp Voltage"])]
   [SugarColumn(ColumnDescription = "正壳VP电压")]
   public int PtoCVpVoltage { get; set; }

   /// <summary>
   /// 正极对壳 TP值
   /// </summary>
   [Languages(["正壳TP", "", "Positive To Case TP"])]
   [SugarColumn(ColumnDescription = "正壳TP")]
   public double PtoCTpTime { get; set; }

   /// <summary>
   /// 正极对壳 阻值
   /// </summary>
   [Languages(["正壳绝缘阻值", "", "Positive To Case Insulation"])]
   [SugarColumn(ColumnDescription = "正壳绝缘阻值")]
   public double PtoCInsulation { get; set; }

   /// <summary>
   /// 正极对壳结果
   /// </summary>
   [Languages(["正壳结果", "", "Positive to Case Result"])]
   [SugarColumn(ColumnDescription = "正壳结果")]
   public ResultTypeEnum PtoCResult { get; set; }
   #endregion

   #region 负极对壳

   /// <summary>
   /// 负极对壳 跌落1电压Vd1
   /// </summary>
   [Languages(["负壳跌落1", "", "Negative To Case Vd1"])]
   [SugarColumn(ColumnDescription = "负壳跌落1")]
   public int NtoCVd1 { get; set; }

   /// <summary>
   /// 负极对壳 跌落2电压Vd2
   /// </summary>
   [Languages(["负壳跌落2", "", "Negative To Case Vd2"])]
   [SugarColumn(ColumnDescription = "负壳跌落2")]
   public int NtoCVd2 { get; set; }

   /// <summary>
   /// 负极对壳 跌落3电压Vd3
   /// </summary>
   [Languages(["负壳跌落3", "", "Negative To Case Vd3"])]
   [SugarColumn(ColumnDescription = "负壳跌落3")]
   public int NtoCVd3 { get; set; }

   /// <summary>
   /// 负极对壳 实际输出电压VP
   /// </summary>
   [Languages(["负壳VP电压", "", "Negative To Case Vp Voltage"])]
   [SugarColumn(ColumnDescription = "负壳VP电压")]
   public int NtoCVpVoltage { get; set; }

   /// <summary>
   /// 负极对壳 TP值
   /// </summary>
   [Languages(["负壳TP", "", "Negative To Case TP"])]
   [SugarColumn(ColumnDescription = "负壳TP")]
   public double NtoCTpTime { get; set; }

   /// <summary>
   /// 负极对壳 阻值
   /// </summary>
   [Languages(["负壳绝缘阻值", "", "Negative To Case Insulation"])]
   [SugarColumn(ColumnDescription = "负壳绝缘阻值")]
   public double NtoCInsulation { get; set; }

   /// <summary>
   /// 负极对壳结果
   /// </summary>
   [Languages(["负壳结果", "", "Negative to Case Result"])]
   [SugarColumn(ColumnDescription = "负壳结果")]
   public ResultTypeEnum NtoCResult { get; set; }

   #endregion

   [Languages(["测短路波形", "测短路波形", "Curve point"])]
   [SugarColumn(ColumnDescription = "测短路波形", ColumnDataType = "text")]
   public string CurvePoint { get; set; } = string.Empty;
}
