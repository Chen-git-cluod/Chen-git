namespace Kinlo.Equipment.Model.PLCInteractive;

public class ToPlcDataDTU
{
   public short PCResult { get; set; }
   public short PLCOldResult { get; set; }
   public float PCData1 { get; set; }
   public float PCData2 { get; set; }
   public byte[] Code { get; set; } = new byte[32];
}


//public class PLCToPCAssemblyDTU
//{
//    /// <summary>
//    /// 雪花ID
//    /// </summary>
//    public long ID { get; set; }
//    public short State { get; set; }
//    public short PLCDataType { get; set; }
//    public float[] PLCData { get; set; } = new float[10];
//    public int 占位 { get; set; }
//    public long RFID { get; set; }
//    public short sResult { get; set; }
//    public byte[] Code { get; set; } = new byte[32];
//}
