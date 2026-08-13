namespace Kinlo.Equipment.Models.PLCInteractiveDTU;

public class PlcToPcVacuumNailDtu
{
   public PlcToPcVacuumNailDtu()
   {
      var idNumber = 16;
      Id = new long[idNumber];
      BeforeKeepPressureVacuumValue = new float[idNumber];
      AfterKeepPressureVacuumValue = new float[idNumber];
   }

   public PlcToPcVacuumNailDtu(int idNumber)
   {
      Id = new long[idNumber];
      BeforeKeepPressureVacuumValue = new float[idNumber];
      AfterKeepPressureVacuumValue = new float[idNumber];
   }

   public long[] Id { get; set; }

   /// <summary>
   /// 设置真空值
   /// </summary>
   public float SetVaccumValue { get; set; }

   /// <summary>
   /// 设置保压时间
   /// </summary>
   public float KeepPressureTime { get; set; }

   /// <summary>
   /// 保压前真空值
   /// </summary>
   public float[] BeforeKeepPressureVacuumValue { get; set; }

   /// <summary>
   /// 保压后真空值
   /// </summary>
   public float[] AfterKeepPressureVacuumValue { get; set; }

   /// <summary>
   /// PcToPlc结果
   /// </summary>
   public short PCResult { get; set; }
}
