namespace Kinlo.Equipment.Models;

public class Ac3200HipotResultModel
{
   /// <summary>
   /// 脉冲结果
   /// </summary>
   public string HipotPulseResult { get; set; } = string.Empty;

   /// <summary>
   /// VP电压(V)
   /// </summary>
   public int HipotVpVoltage { get; set; }

   /// <summary>
   /// 跌落1(V)
   /// </summary>
   public int HipotFallOne { get; set; }

   /// <summary>
   /// 跌落2(V)
   /// </summary>
   public int HipotFallTwo { get; set; }

   /// <summary>
   /// 跌落3(V)
   /// </summary>
   public int HipotFallThree { get; set; }

   /// <summary>
   /// 脉冲项 TP 值 TP(ms)
   /// </summary>
   public float HipotPulseTp { get; set; }

   /// <summary>
   /// 绝缘电阻的测试结果
   /// </summary>
   public string ResistanceTestResult { get; set; } = string.Empty;

   /// <summary>
   /// 阻值
   /// </summary>
   public float InsulationTestValue { get; set; }

   /// <summary>
   /// 电容的测试结果
   /// </summary>
   public string CapacitorsResult { get; set; } = string.Empty;

   /// <summary>
   /// 电容值
   /// </summary>
   public float Capacitors { get; set; }

   /// <summary>
   /// 测短路结果
   /// </summary>
   public ResultTypeEnum HipotResult { get; set; }

   /// <summary>
   /// 电压曲线点
   /// </summary>
   public string CurvePoint { get; set; } = string.Empty;
}
