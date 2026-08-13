using System.Diagnostics.Metrics;

namespace Kinlo.Equipment.Model.PLCInteractive;

public class PlcAlarmDut
{
   public PlcAlarmDut()
   {
      Alarm = new bool[96];
      Warning = new bool[96];
      Tip = new bool[96];
   }

 
   public bool[] Alarm { get; set; }
   public bool[] Warning { get; set; }

    public bool[] Tip { get; set; }
}

//public class PlcAlarmOther
//{
//    public AlarmArray[] Door { get; set; } = new AlarmArray[4];
//    public AlarmArray[] Estop { get; set; } = new AlarmArray[4];
//    public bool[] Instrument { get; set; } = new bool[104];
//    public bool[] Communication { get; set; } = new bool[104];
//}
public class DoorAlarm
{
   public AlarmArray[] Doors { get; set; } = new AlarmArray[4];
}

public class EstopAlarm
{
   public AlarmArray[] Estops { get; set; } = new AlarmArray[4];
}

public class AlarmArray
{
   public bool[] Alarms { get; set; } = new bool[24];
}

public class InstrumentAlarm
{
   public bool[] Alarms { get; set; } = new bool[104];
}
