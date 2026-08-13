namespace Kinlo.Equipment.Models;

/// <summary>
/// RJ6903GX 绝缘测试结果模型
/// 缩写规则：
/// PtoN = Positive to Negative (正对负)
/// PtoC = Positive to Case (正对壳)
/// NtoC = Negative to Case (负对壳)
/// </summary>
public class RJ6903GXHipotResultModel
{
   /// <summary>
   /// 测短路总结果
   /// </summary>
   public ResultTypeEnum OverallResult { get; set; }

   /// <summary>
   /// 正极对负极 (PtoN) 测试详情
   /// </summary>
   public HipotChannelResult PositiveToNegative { get; set; } = new HipotChannelResult("正负极");

   /// <summary>
   /// 正极对壳 (PtoC) 测试详情
   /// </summary>
   public HipotChannelResult PositiveToCase { get; set; } = new HipotChannelResult("正极壳");

   /// <summary>
   /// 负极对壳 (NtoC) 测试详情
   /// </summary>
   public HipotChannelResult NegativeToCase { get; set; } = new HipotChannelResult("负极壳");

   /// <summary>
   /// 电压曲线点
   /// </summary>
   public string CurvePoint { get; set; } = string.Empty;
}

/// <summary>
/// 单个测试通道的详细结果
/// </summary>
public class HipotChannelResult
{
   public HipotChannelResult(string name)
   {
      name = name.Trim();
   }

   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// 跌落1电压VD1
   /// </summary>
   public int Vd1 { get; set; }

   /// <summary>
   /// 跌落2电压VD2
   /// </summary>
   public int Vd2 { get; set; }

   /// <summary>
   /// 跌落3电压VD3
   /// </summary>
   public int Vd3 { get; set; }

   /// <summary>
   /// 实际输出电压VP
   /// </summary>
   public int VpVoltage { get; set; }

   /// <summary>
   /// TP时间
   /// </summary>
   public double TpTime { get; set; }

   /// <summary>
   /// 绝缘测试值
   /// </summary>
   public double Insulation { get; set; }

   ///// <summary>
   ///// 开路结果
   ///// </summary>
   //public byte OpenCircuitResult { get; set; }

   ///// <summary>
   ///// 严重短路结果
   ///// </summary>
   //public byte SevereShortResult { get; set; }

   ///// <summary>
   ///// 欠压结果
   ///// </summary>
   //public byte UnderVoltageResult { get; set; }

   ///// <summary>
   ///// 过压结果
   ///// </summary>
   //public byte OverVoltageResult { get; set; }

   ///// <summary>
   ///// 跌落1电压VD1结果
   ///// </summary>
   //public byte Vd1Result { get; set; }

   ///// <summary>
   ///// 跌落2电压VD2结果
   ///// </summary>
   //public byte Vd2Result { get; set; }

   ///// <summary>
   ///// 跌落3电压VD3结果
   ///// </summary>
   //public byte Vd3Result { get; set; }

   ///// <summary>
   ///// Tl结果
   ///// </summary>
   //public byte TlResult { get; set; }

   ///// <summary>
   ///// Th结果
   ///// </summary>
   //public byte ThResult { get; set; }

   ///// <summary>
   ///// 电阻结果
   ///// </summary>
   //public double InsulationResult { get; set; }

   /// <summary>
   /// 当前通道结果
   /// </summary>
   public ResultTypeEnum ChannelResult { get; set; }
}
