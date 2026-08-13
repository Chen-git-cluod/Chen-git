namespace Kinlo.Equipment.Model.PLCInteractive;

public class PlcAlarmArraryDut
{
   public PlcAlarmArraryDut(int count)
   {
      PLCAlarm = new PlcAlarmDut[count];
   }

   public PlcAlarmDut[] PLCAlarm { get; set; }
}
