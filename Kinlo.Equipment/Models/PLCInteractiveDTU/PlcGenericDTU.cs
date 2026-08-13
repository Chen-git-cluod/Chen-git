namespace Kinlo.Equipment.Model.PLCInteractive;

public class PlcGenericArrayDTU
{
   public PlcGenericArrayDTU()
   {
      Batterys = new PlcGeneric2DTU[8];
   }

   public PlcGeneric2DTU[] Batterys { get; set; }

   public PlcGenericArrayDTU(int length)
   {
      Batterys = new PlcGeneric2DTU[length];
      //for (int i = 0; i < length; i++)
      //    Batterys[i] = new PlcGeneric2DTU();
   }
}

public class PlcGeneric2DTU
{
   /// <summary>
   /// 雪花ID
   /// </summary>
   public long ID { get; set; }

   /// <summary>
   /// 类型
   /// </summary>
   public short PLCDataType { get; set; }

   /// <summary>
   /// 占位
   /// </summary>
   public short MyProperty { get; set; }

   /// <summary>
   /// PLC写入的数据
   /// </summary>
   public float PLCData { get; set; }
}
