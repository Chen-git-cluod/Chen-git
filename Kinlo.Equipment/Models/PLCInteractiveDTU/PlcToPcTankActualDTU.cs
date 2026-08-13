namespace Kinlo.Equipment.Model.PLCInteractive;

public class PlcToPcTankActualDTU
{
   public PlcToPcTankActualItemDTU[] Actuals { get; set; }

   public PlcToPcTankActualDTU()
   {
      Actuals = new PlcToPcTankActualItemDTU[32];
   }
}

public class PlcToPcTankActualItemDTU
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
}

public class TankFloors
{
   public TankFloor Floors { get; set; }
}

public class TankFloor
{
   public TankLine[] Lines { get; set; }

   public TankFloor()
   {
      Lines = Enumerable.Repeat(new TankLine(), 4).ToArray();
   }
}

public class TankLine
{
   public TankId[] Columns { get; set; }

   public TankLine()
   {
      Columns = Enumerable.Repeat(new TankId(), 16).ToArray();
   }
}

public class TankId
{
   public long Id { get; set; }
}
