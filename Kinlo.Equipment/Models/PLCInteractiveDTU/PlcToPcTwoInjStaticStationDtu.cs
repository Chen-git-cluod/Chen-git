namespace Kinlo.Equipment.Models.PLCInteractiveDTU;

/// <summary>
/// 二注静置站
/// </summary>
public class PlcToPcTwoInjStaticStationDtu
{
   public short InjectionStationNo { get; set; } //注液站号
   public short RowNo { get; set; } //行号
   public short ColunmNo { get; set; } //列号
   public short InjectionPumpNo { get; set; } //注液泵号
   public short InjectionNozzleNo { get; set; } //注液嘴号/杯号
   public short InjectionTime { get; set; } //注液时间
   public short CycleTime { get; set; } //循环时间
   public short CycleNumber { get; set; } //循环次数
   public short VcheckResult { get; set; } //测漏结果
   public float LeakVacuum { get; set; } //测漏真空值
   public float InjBeforeVacuum { get; set; } //注液前真空值
   public float InjAfterVacuum { get; set; } //注液后真空值
   public float InjPressure { get; set; } //	注液微正压
   public float FinalPressurizationValue { get; set; } //最后加压值
   public float[] CyclicPressurizationValue { get; set; } = new float[10]; //循环加压值
   public float[] CirculatingVacuumValue { get; set; } = new float[10]; //	循环真空值
   public short[] CyclicPressurizationTime { get; set; } = new short[10]; //循环加压时间
   public short[] CirculatingVacuumTime { get; set; } = new short[10]; //循环真空时间
   public short InjBeforeVacuumTime { get; set; } //注液前抽真空时间
   public short LeakVacuumTime { get; set; } //	测漏保压时间
   public float[] Reserve { get; set; } = new float[10]; //	预留
}
