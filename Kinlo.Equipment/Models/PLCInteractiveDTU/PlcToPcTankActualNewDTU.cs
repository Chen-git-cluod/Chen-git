namespace Kinlo.Equipment.Model.PLCInteractive;

/// <summary>
/// 静默站参数-最新版
/// </summary>
public class PlcToPcTankActualNewDTU
{
   public static int Length { get; set; } = 32;
   public PlcToPcTankActualNewItemDTU[] Actuals { get; set; }

   public PlcToPcTankActualNewDTU()
   {
      Actuals = new PlcToPcTankActualNewItemDTU[Length];
   }

   public PlcToPcTankActualNewDTU(int length)
   {
      Length = length;
      Actuals = new PlcToPcTankActualNewItemDTU[Length];
   }
}

public class PlcToPcTankActualNewItemDTU
{
   /// <summary>
   /// 实际压力
   /// </summary>
   public float ActualPressure { get; set; }

   /// <summary>
   /// 实际保压时间
   /// </summary>
   public short ActualRetentionTime { get; set; }

   /// <summary>
   /// 实际达到时间
   /// </summary>
   public short ArrivalTime { get; set; }

   /// <summary>
   /// 设定压力
   /// </summary>
   public float SetPressure { get; set; }

   /// <summary>
   /// 设定保压时间
   /// </summary>
   public short SetRetentionTime { get; set; }

   /// <summary>
   /// 功能
   /// </summary>
   public short Func { get; set; }
}

/// <summary>
/// ID相关
/// </summary>
public class TankFloorNew
{
   public static int Length { get; set; } = 3;
   public TankLineNew[] Lines { get; set; }

   public TankFloorNew()
   {
      Lines = Enumerable.Repeat(new TankLineNew(), Length).ToArray();
   }

   public TankFloorNew(int length)
   {
      Length = length;
      Lines = Enumerable.Repeat(new TankLineNew(), Length).ToArray();
   }
}

public class TankLineNew
{
   public static int Length { get; set; } = 12;
   public TankId[] Columns { get; set; }

   public TankLineNew()
   {
      Columns = Enumerable.Repeat(new TankId(), Length).ToArray();
   }

   public TankLineNew(int length)
   {
      Length = length;
      Columns = Enumerable.Repeat(new TankId(), Length).ToArray();
   }
}
