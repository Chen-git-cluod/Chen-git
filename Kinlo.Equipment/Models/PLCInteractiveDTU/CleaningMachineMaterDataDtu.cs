namespace Kinlo.Equipment.Models.PLCInteractiveDTU;

/// <summary>
/// 清洗机仪表数据
/// </summary>
public class CleaningMachineMaterDataDtu
{
   /// <summary>
   /// 清洗温度1
   /// </summary>
   public short CleaningWaterTemp1 { get; set; }

   /// <summary>
   /// 清洗温度2
   /// </summary>
   public short CleaningWaterTemp2 { get; set; }

   /// <summary>
   /// 清洗温度3
   /// </summary>
   public short CleaningWaterTemp3 { get; set; }

   /// <summary>
   /// 烘烤温度1
   /// </summary>
   public short DryingTemp1 { get; set; }

   /// <summary>
   /// 烘烤温度2
   /// </summary>
   public short DryingTemp2 { get; set; }

   /// <summary>
   /// 清洗温度1
   /// </summary>
   public short SetCleaningWaterTemp1 { get; set; }

   /// <summary>
   /// 清洗温度2
   /// </summary>
   public short SetCleaningWaterTemp2 { get; set; }

   /// <summary>
   /// 清洗温度3
   /// </summary>
   public short SetCleaningWaterTemp3 { get; set; }

   /// <summary>
   /// 烘烤温度1
   /// </summary>
   public short SetDryingTemp1 { get; set; }

   /// <summary>
   /// 烘烤温度2
   /// </summary>
   public short SetDryingTemp2 { get; set; }
   public short ReserveInt1 { get; set; }
   public short ReserveInt2 { get; set; }
   public short ReserveInt3 { get; set; }
   public short ReserveInt4 { get; set; }

   /// <summary>
   /// HP 值
   /// </summary>
   public float PHValue { get; set; }

   /// <summary>
   /// 测HP电压
   /// </summary>
   public float PHTestVoltage { get; set; }

   /// <summary>
   /// 测HP温度
   /// </summary>
   public float PHTestTemp { get; set; }

   /// <summary>
   /// PH_ORP值
   /// </summary>
   public float PHORPValue { get; set; }
   public float ResesrveReal1 { get; set; }
   public float ResesrveReal2 { get; set; }
   public float ResesrveReal3 { get; set; }
}
