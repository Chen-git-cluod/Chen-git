namespace Kinlo.Equipment.Models;

public class RJ6091AResultStruct
{
   /// <summary>
   /// 电压
   /// </summary>
   public float Voltage { get; set; }

   /// <summary>
   /// 电阻
   /// </summary>
   public float Resistance { get; set; }

   /// <summary>
   /// 电流
   /// </summary>
   public float ElectricCurrent { get; set; }
   public byte Result { get; set; }

   /// <summary>
   /// 电阻上限
   /// </summary>
   public float ResistanceUpper { get; set; }

   /// <summary>
   /// 电阻下限
   /// </summary>
   public float ResistanceLower { get; set; }

   /// <summary>
   /// 测试时间（S）
   /// </summary>
   public float TestTime { get; set; }
}
