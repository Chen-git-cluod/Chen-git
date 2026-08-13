using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.Equipment.Models.PLCInteractiveDTU;

public class PlcToPcEzInjectStationDTU
{
   public long ID { get; set; }

   /// <summary>
   /// 注液站号
   /// </summary>
   public short InjectionStationNo { get; set; }

   /// <summary>
   /// 行号
   /// </summary>
   public short RowNo { get; set; }

   /// <summary>
   /// 列号
   /// </summary>
   public short ColunmNo { get; set; }

   /// <summary>
   /// 泵号
   /// </summary>
   public short InjectionPumpNo { get; set; }

   /// <summary>
   /// 注液嘴号
   /// </summary>
   public short InjectionNozzleNo { get; set; }

   /// <summary>
   /// 注液嘴使用次数
   /// </summary>
   public short InjectionNozzleUseNumber { get; set; }

   /// <summary>
   /// 注液开始时间
   /// </summary>
   public long InjectionStardTime { get; set; }

   /// <summary>
   /// 注液结束时间
   /// </summary>
   public long InjectionEndTime { get; set; }

   /// <summary>
   /// 循环时间
   /// </summary>
   public short[] CycleTime { get; set; } = new short[10];

   /// <summary>
   /// 循环次数
   /// </summary>
   public short CycleNumber { get; set; }

   /// <summary>
   /// 打液时长
   /// </summary>
   public short InjectionTime { get; set; }

   /// <summary>
   /// 测漏结果
   /// </summary>
   public short VcheckResult { get; set; }

   /// <summary>
   /// 测漏真空实际值
   /// </summary>
   public float LeakVacuum { get; set; }

   /// <summary>
   /// 测漏真空设定值
   /// </summary>
   public float LeakVacuumSetValue { get; set; }

   /// <summary>
   /// 测漏真空压力泄露值
   /// </summary>
   public float LeakVacuumSpillage { get; set; }

   /// <summary>
   /// 测漏保压实际时间
   /// </summary>
   public short LeakVacuumHoldTime { get; set; }

   /// <summary>
   /// 测漏保压设定时间
   /// </summary>
   public short LeakVacuumHoldTimeSetValue { get; set; }

   /// <summary>
   /// 测漏实际时间
   /// </summary>
   public short LeakVacuumTime { get; set; }

   /// <summary>
   /// 测漏设定时间
   /// </summary>
   public short LeakVacuumTimeSetValue { get; set; }

   /// <summary>
   /// 注液前真空值
   /// </summary>
   public float InjBeforeVacuum { get; set; }

   /// <summary>
   /// 注液后真空值
   /// </summary>
   public float InjAfterVacuum { get; set; }

   /// <summary>
   /// 注液前抽真空时间
   /// </summary>
   public short InjBeforeVacuumTime { get; set; }

   /// <summary>
   /// 注液循环前高压实际值
   /// </summary>
   public float InjPressure { get; set; }

   /// <summary>
   /// 注液循环前高压设定值
   /// </summary>
   public float InjPressureSetValue { get; set; }

   /// <summary>
   /// 注液循环前高压保持实际时间
   /// </summary>
   public short InjPressureHoldTime { get; set; }

   /// <summary>
   ///注液循环前高压保持设定时间
   /// </summary>
   public short InjPressureHoldTimeSetValue { get; set; }

   /// <summary>
   /// 注液循环前高压实际时间
   /// </summary>
   public short InjPressureTime { get; set; }

   /// <summary>
   /// 注液循环前高压设定时间
   /// </summary>
   public short InjPressureTimeSetValue { get; set; }

   /// <summary>
   /// 循环加压实际值
   /// </summary>
   public float[] CyclicPressurizationValue { get; set; } = new float[10];

   /// <summary>
   /// 循环加压设定值
   /// </summary>
   public float[] CyclicPressurizationValueSteValue { get; set; } = new float[10];

   /// <summary>
   /// 循环真空实际值
   /// </summary>
   public float[] CirculatingVacuumValue { get; set; } = new float[10];

   /// <summary>
   /// 循环真空设定值
   /// </summary>
   public float[] CirculatingVacuumValueSetValue { get; set; } = new float[10];

   /// <summary>
   /// 循环真空时间实际值
   /// </summary>
   public short[] CirculatingVacuumTime { get; set; } = new short[10];

   /// <summary>
   /// 循环真空时间设定值
   /// </summary>
   public short[] CirculatingVacuumTimeSetValue { get; set; } = new short[10];

   /// <summary>
   /// 循环加压时间实际值
   /// </summary>
   public short[] CyclicPressurizationTime { get; set; } = new short[10];

   /// <summary>
   /// 循环加压时间设定值
   /// </summary>
   public short[] CyclicPressurizationTimeSetValue { get; set; } = new short[10];

   /// <summary>
   /// 循环加压后大气时间
   /// </summary>
   public short[] CyclicPressurizationAfterStandingTime { get; set; } = new short[10];

   /// <summary>
   /// 循环真空后大气时间
   /// </summary>
   public short[] CirculatingVacuumAfterStangdingTime { get; set; } = new short[10];

   /// <summary>
   /// 循环加压后大气设定时间
   /// </summary>
   public short[] CyclicPressurizationAfterStandingTimeSetValue { get; set; } = new short[10];

   /// <summary>
   /// 循环真空后大气设定时间
   /// </summary>
   public short[] CirculatingVacuumAfterStangdingTimeSetValue { get; set; } = new short[10];

   /// <summary>
   /// 最后加压值
   /// </summary>
   public float FinalPressurizationValue { get; set; }

   /// <summary>
   /// 最后加压设定值
   /// </summary>
   public float FinalPressurizationValueSetValue { get; set; }

   /// <summary>
   /// 最后加压时间
   /// </summary>
   public short FinalPressurizationTime { get; set; }

   /// <summary>
   /// 最后加压设定时间
   /// </summary>
   public short FinalPressurizationTimeSetValue { get; set; }

   /// <summary>
   /// 最后加压通大气时间
   /// </summary>
   public short FinalPressurizationStandingTime { get; set; }

   /// <summary>
   /// 最后加压通大气设定时间
   /// </summary>
   public short FinalPressurizationStandingSetTime { get; set; }

   /// <summary>
   /// 循环真空打开时间设定值
   /// </summary>
   public short CirculatingVacuumOpenTime { get; set; }

   /// <summary>
   /// 循环真空打开时间实际值
   /// </summary>
   public short[] CirculatingVacuumOpenActualTime { get; set; } = new short[10];

   /// <summary>
   /// 循环高压打开时间设定值
   /// </summary>
   public short CyclicPressurizationOpenTime { get; set; }

   /// <summary>
   /// 循环高压打开时间实际值
   /// </summary>
   public short[] CyclicPressurizationOpenActualTime { get; set; } = new short[10];

   /// <summary>
   /// 流液时间设定值
   /// </summary>
   public short InjectionSetTime { get; set; }
}
