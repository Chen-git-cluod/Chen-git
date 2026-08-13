namespace Kinlo.Common.Tools;

public static class Int16ToAlarmHelper
{
   /// <summary>
   ///
   /// </summary>
   /// <param name="state">1：报警，0：消警</param>
   /// <param name="index"></param>
   /// <param name="oldAlarmCode"></param>
   /// <returns></returns>
   public static bool GetAlarmCode(this bool state, byte index, ref ushort oldAlarmCode)
   {
      if (index > 15)
      {
         string _msg = $"索引超出范围，返回旧报警代码！";
         _msg.LogRun(Log4NetLevelEnum.错误);
         return false;
      }

      if (state)
      {
         ushort _newAlarm = (ushort)(1 << index);
         oldAlarmCode = (ushort)(oldAlarmCode | _newAlarm);
      }
      else
      {
         ushort _newAlarm = (ushort)~(1 << index);
         oldAlarmCode = (ushort)(oldAlarmCode & _newAlarm);
      }
      return true;
   }
}
