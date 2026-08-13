namespace Kinlo.Equipment.Model.PLCInteractive;

public class PlcToPcLeakLineDTU
{
   static int _columnCount = 12;
   public PlcToPcLeakItemDTU[] Columns { get; set; }

   public PlcToPcLeakLineDTU(int columnCount)
   {
      _columnCount = columnCount;
      Columns = Init();
   }

   public PlcToPcLeakLineDTU() => Columns = Init();

   private PlcToPcLeakItemDTU[] Init()
   {
      var columns = new PlcToPcLeakItemDTU[_columnCount];
      for (int i = 0; i < _columnCount; i++)
      {
         columns[i] = new PlcToPcLeakItemDTU();
      }
      return columns;
   }
}

public class PlcToPcLeakItemDTU
{
   /// <summary>
   /// 测漏后压力
   /// </summary>
   public float AfterVacuum { get; set; }

   /// <summary>
   /// 测漏前压力
   /// </summary>
   public float BeforVacuum { get; set; }

   /// <summary>
   /// 设定压力
   /// </summary>
   public float SetVacuum { get; set; }

   /// <summary>
   /// 实际测漏压力
   /// </summary>
   public float LeakVacuum { get; set; }

   /// <summary>
   /// 设定漏率
   /// </summary>
   public float SetLeak { get; set; }

   /// <summary>
   /// 测漏实际时间
   /// </summary>
   public short ActTime { get; set; }

   /// <summary>
   /// 测漏保压时间
   /// </summary>
   public short KeepTime { get; set; }

   /// <summary>
   /// 测漏设定时间
   /// </summary>
   public short SetTime { get; set; }

   /// <summary>
   /// 测漏结果
   /// </summary>
   public short VcheckResult { get; set; }
   public int MyProperty { get; set; }
   public long ID { get; set; }
}
