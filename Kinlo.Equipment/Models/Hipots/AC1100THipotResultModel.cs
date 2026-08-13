namespace Kinlo.Equipment.Models;

public class AC1100THipotResultModel
{
   /// <summary>
   /// 测短路总结果
   /// </summary>
   public ResultTypeEnum OverallResult { get; set; }

   /// <summary>
   /// 壳测试是否开路（00表示合格，01表示开路，FF表示不 判定 Ps： 1，当出现error10壳开路时，仪器实际并没有测试，直接报警，这时数据上传时要测试的数据直接上传0。
   /// </summary>
   public ResultTypeEnum CaseResult { get; set; }

   #region 正负极
   /// <summary>
   /// 正负极结果
   /// </summary>
   public ResultTypeEnum PositiveToNegativeResult { get; set; }

   /// <summary>
   /// 正负级 实际输出电压VP
   /// </summary>
   public int PositiveToNegativeVpVoltage { get; set; }

   /// <summary>
   /// 正负极跌落1电压VD1
   /// </summary>
   public int PositiveToNegativeVD1 { get; set; }

   /// <summary>
   /// 正负极跌落2电压VD2
   /// </summary>
   public int PositiveToNegativeVD2 { get; set; }

   /// <summary>
   /// 正负极跌落3电压VD3
   /// </summary>
   public int PositiveToNegativeVD3 { get; set; }

   /// <summary>
   /// 正负极 TP值
   /// </summary>
   public double PositiveToNegativeTP { get; set; }

   /// <summary>
   /// 正负极 绝缘测试值
   /// </summary>
   public double PositiveToNegativeInsulation { get; set; }

   /// <summary>
   /// 电压曲线点
   /// </summary>
   public string CurvePoint { get; set; } = string.Empty;

   #endregion

   #region 正极对壳
   /// <summary>
   /// 正极对壳结果
   /// </summary>
   public ResultTypeEnum PositiveToCaseResult { get; set; }

   /// <summary>
   /// 正极对壳 实际输出电压VP
   /// </summary>
   public int PositiveToCaseVpVoltage { get; set; }

   /// <summary>
   /// 正极对壳 跌落1电压VD1
   /// </summary>
   public int PositiveToCaseVD1 { get; set; }

   /// <summary>
   /// 正极对壳 跌落2电压VD2
   /// </summary>
   public int PositiveToCaseVD2 { get; set; }

   /// <summary>
   /// 正极对壳 跌落3电压VD3
   /// </summary>
   public int PositiveToCaseVD3 { get; set; }

   /// <summary>
   /// 正极对壳 TP值
   /// </summary>
   public double PositiveToCaseTP { get; set; }

   /// <summary>
   /// 正极对壳 阻值
   /// </summary>
   public double PositiveToCaseInsulation { get; set; }

   /// <summary>
   /// 正极对壳 弱导测试阻值
   /// </summary>
   public double PositiveToCaseWeakConduction { get; set; }
   #endregion

   #region 负极对壳
   /// <summary>
   /// 正极对壳结果
   /// </summary>
   public ResultTypeEnum NegativeToCaseResult { get; set; }

   /// <summary>
   /// 正极对壳 实际输出电压VP
   /// </summary>
   public int NegativeToCaseVpVoltage { get; set; }

   /// <summary>
   /// 正极对壳 跌落1电压VD1
   /// </summary>
   public int NegativeToCaseVD1 { get; set; }

   /// <summary>
   /// 正极对壳 跌落2电压VD2
   /// </summary>
   public int NegativeToCaseVD2 { get; set; }

   /// <summary>
   /// 正极对壳 跌落3电压VD3
   /// </summary>
   public int NegativeToCaseVD3 { get; set; }

   /// <summary>
   /// 正极对壳 TP值
   /// </summary>
   public double NegativeToCaseTP { get; set; }

   /// <summary>
   /// 正极对壳 阻值
   /// </summary>
   public double NegativeToCaseInsulation { get; set; }
   #endregion
}
