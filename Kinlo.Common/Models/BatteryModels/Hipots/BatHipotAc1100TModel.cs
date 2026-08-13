namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
///测短路 (AC3200)
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.测短路], DeviceCommunicationType = [CommunicationEnum.ShortCircuit_AC1100T])]
[Languages([nameof(ProcessTypeEnum.测短路), "测短路", "Hipot tester"])]
[AddINotifyPropertyChangedInterface]
public partial class BatHipotAc1100TModel
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

   /// <summary>
   /// 壳测试是否开路（00表示合格，01表示开路，FF表示不 判定 Ps： 1，当出现error10壳开路时，仪器实际并没有测试，直接报警，这时数据上传时要测试的数据直接上传0。
   /// </summary>
   [Languages(["壳测试是否开路", "", "Case Result"])]
   [SugarColumn(ColumnDescription = "壳测试是否开路")]
   public ResultTypeEnum HiptionCaseResult { get; set; }

   #region 正负极
   /// <summary>
   /// 正负极结果
   /// </summary>
   [Languages(["正负极结果", "", "Positive to Negative Result"])]
   [SugarColumn(ColumnDescription = "正负极结果")]
   public ResultTypeEnum PositiveToNegativeResult { get; set; }

   /// <summary>
   /// 正负级 实际输出电压VP
   /// </summary>
   [Languages(["正负极VP电压", "", "Positive To Negative Vp Voltage"])]
   [SugarColumn(ColumnDescription = "正负极VP电压")]
   public int PositiveToNegativeVpVoltage { get; set; }

   /// <summary>
   /// 正负极跌落1电压VD1
   /// </summary>
   [Languages(["正负极跌落1", "", "Positive To Negative VD1"])]
   [SugarColumn(ColumnDescription = "正负极跌落1")]
   public int PositiveToNegativeVD1 { get; set; }

   /// <summary>
   /// 正负极跌落2电压VD2
   /// </summary>
   [Languages(["正负极跌落2", "", "Positive To Negative VD2"])]
   [SugarColumn(ColumnDescription = "正负极跌落2")]
   public int PositiveToNegativeVD2 { get; set; }

   /// <summary>
   /// 正负极跌落3电压VD3
   /// </summary>
   [Languages(["正负极跌落3", "", "Positive To Negative VD3"])]
   [SugarColumn(ColumnDescription = "正负极跌落3")]
   public int PositiveToNegativeVD3 { get; set; }

   /// <summary>
   /// 正负极 TP值
   /// </summary>
   [Languages(["正负极TP", "", "Positive To Negative TP"])]
   [SugarColumn(ColumnDescription = "正负极TP")]
   public double PositiveToNegativeTP { get; set; }

   /// <summary>
   /// 正负极 绝缘测试值
   /// </summary>
   [Languages(["正负极绝缘阻值", "", "Positive To Negative Insulation"])]
   [SugarColumn(ColumnDescription = "正负极绝缘阻值")]
   public double PositiveToNegativeInsulation { get; set; }

   ///// <summary>
   ///// 电压波形
   ///// </summary>
   //[Languages(["电压波形", "电压波形", "Curve"])]
   //[SugarColumn(ColumnDescription = "电压波形", ColumnDataType = "text")]
   //public string CurvePoint { get; set; } = string.Empty;

   #endregion

   #region 正极对壳
   /// <summary>
   /// 正极对壳结果
   /// </summary>
   [Languages(["正极壳结果", "", "Positive to Case Result"])]
   [SugarColumn(ColumnDescription = "正极壳结果")]
   public ResultTypeEnum PositiveToCaseResult { get; set; }

   /// <summary>
   /// 正极对壳 实际输出电压VP
   /// </summary>
   [Languages(["正极壳VP电压", "", "Positive To Case Vp Voltage"])]
   [SugarColumn(ColumnDescription = "正极壳VP电压")]
   public int PositiveToCaseVpVoltage { get; set; }

   /// <summary>
   /// 正极对壳 跌落1电压VD1
   /// </summary>
   [Languages(["正极壳跌落1", "", "Positive To Case VD1"])]
   [SugarColumn(ColumnDescription = "正极壳跌落1")]
   public int PositiveToCaseVD1 { get; set; }

   /// <summary>
   /// 正极对壳 跌落2电压VD2
   /// </summary>
   [Languages(["正极壳跌落2", "", "Positive To Case VD2"])]
   [SugarColumn(ColumnDescription = "正极壳跌落2")]
   public int PositiveToCaseVD2 { get; set; }

   /// <summary>
   /// 正极对壳 跌落3电压VD3
   /// </summary>
   [Languages(["正极壳跌落3", "", "Positive To Case VD3"])]
   [SugarColumn(ColumnDescription = "正极壳跌落3")]
   public int PositiveToCaseVD3 { get; set; }

   /// <summary>
   /// 正极对壳 TP值
   /// </summary>
   [Languages(["正极壳TP", "", "Positive To Case TP"])]
   [SugarColumn(ColumnDescription = "正极壳TP")]
   public double PositiveToCaseTP { get; set; }

   /// <summary>
   /// 正极对壳 阻值
   /// </summary>
   [Languages(["正极壳绝缘阻值", "", "Positive To Case Insulation"])]
   [SugarColumn(ColumnDescription = "正极壳绝缘阻值")]
   public double PositiveToCaseInsulation { get; set; }

   /// <summary>
   /// 正极对壳 弱导测试阻值
   /// </summary>
   [Languages(["正极壳弱导测试阻值", "", "Positive To Case Weak Conduction"])]
   [SugarColumn(ColumnDescription = "正极壳弱导测试阻值")]
   public double PositiveToCaseWeakConduction { get; set; }
   #endregion

   #region 负极对壳
   /// <summary>
   /// 负极对壳结果
   /// </summary>
   [Languages(["负极壳结果", "", "Negative to Case Result"])]
   [SugarColumn(ColumnDescription = "负极壳结果")]
   public ResultTypeEnum NegativeToCaseResult { get; set; }

   /// <summary>
   /// 负极对壳 实际输出电压VP
   /// </summary>
   [Languages(["负极壳VP电压", "", "Negative To Case Vp Voltage"])]
   [SugarColumn(ColumnDescription = "负极壳VP电压")]
   public int NegativeToCaseVpVoltage { get; set; }

   /// <summary>
   /// 负极对壳 跌落1电压VD1
   /// </summary>
   [Languages(["负极壳跌落1", "", "Negative To Case VD1"])]
   [SugarColumn(ColumnDescription = "负极壳跌落1")]
   public int NegativeToCaseVD1 { get; set; }

   /// <summary>
   /// 负极对壳 跌落2电压VD2
   /// </summary>
   [Languages(["负极壳跌落2", "", "Negative To Case VD2"])]
   [SugarColumn(ColumnDescription = "负极壳跌落2")]
   public int NegativeToCaseVD2 { get; set; }

   /// <summary>
   /// 负极对壳 跌落3电压VD3
   /// </summary>
   [Languages(["负极壳跌落3", "", "Negative To Case VD3"])]
   [SugarColumn(ColumnDescription = "负极壳跌落3")]
   public int NegativeToCaseVD3 { get; set; }

   /// <summary>
   /// 负极对壳 TP值
   /// </summary>
   [Languages(["负极壳TP", "", "Negative To Case TP"])]
   [SugarColumn(ColumnDescription = "负极壳TP")]
   public double NegativeToCaseTP { get; set; }

   /// <summary>
   /// 负极对壳 阻值
   /// </summary>
   [Languages(["负极壳绝缘阻值", "", "Negative To Case Insulation"])]
   [SugarColumn(ColumnDescription = "负极壳绝缘阻值")]
   public double NegativeToCaseInsulation { get; set; }

   #endregion
}
