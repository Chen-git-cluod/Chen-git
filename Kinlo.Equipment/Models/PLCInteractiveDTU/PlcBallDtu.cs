using Kinlo.Equipment.Model.PLCInteractive;

namespace Kinlo.Equipment.Models.PLCInteractiveDTU;

public class PlcBallDtu
{
   private static int _count = 10;

   public PlcBallDtu()
   {
      Init();
   }

   public PlcBallDtu(int count)
   {
      Init();
   }

   private void Init()
   {
      Actuals = new PlcBallItemDtu[_count];
      for (int i = 0; i < _count; i++)
      {
         Actuals[i] = new PlcBallItemDtu();
      }
   }

   public PlcBallItemDtu[] Actuals { get; set; }
}

public class PlcBallItemDtu
{
   /// <summary>
   /// 打钢珠偏移值
   /// </summary>
   public float deviant1 { get; set; }

   /// <summary>
   /// 打钢珠高度偏移值
   /// </summary>
   public float deviant2 { get; set; }

   /// <summary>
   /// 打钢珠偏移结果
   /// </summary>
   public short Results1 { get; set; }

   /// <summary>
   /// 打钢珠高度偏移结果
   /// </summary>
   public short Results2 { get; set; }
}
