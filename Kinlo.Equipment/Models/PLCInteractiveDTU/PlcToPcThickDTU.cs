namespace Kinlo.Equipment.Model.PLCInteractive;

public class PlcToPcThicksDTU
{
   public PlcToPcThickDTU[] Battery { get; set; }

   /// <summary>
   /// 厚度上限
   /// </summary>
   public float CheckUpperLimit { get; set; }

   /// <summary>
   /// 厚度下限
   /// </summary>
   public float CheckLowerLimit { get; set; }

   /// <summary>
   /// 测厚实际压力
   /// </summary>
   public float obligate { get; set; }

   /// <summary>
   /// 测厚压力上限
   /// </summary>
   public float obligateUpperLimit { get; set; }

   /// <summary>
   /// 测厚压力下限
   /// </summary>
   public float obligateLowerLimit { get; set; }

   public PlcToPcThicksDTU(int count)
   {
      Battery = new PlcToPcThickDTU[count];
   }

   public PlcToPcThicksDTU()
   {
      Battery = new PlcToPcThickDTU[6];
   }
}

public class PlcToPcThickDTU
{
   /// <summary>
   /// 测厚,0为上值，1为下值
   /// </summary>
   public float[] CheckValue { get; set; } = new float[3];

   /// <summary>
   /// 测厚结果,,0为上值，1为下值
   /// </summary>
   public short[] Result { get; set; } = new short[3];

   public short LastResult { get; set; }

   public int MyProperty { get; set; }

   public long ID { get; set; }
}
