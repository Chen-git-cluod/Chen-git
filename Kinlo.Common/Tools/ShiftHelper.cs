namespace Kinlo.Common.Tools;

public static class ShiftHelper
{
   /// <summary>
   /// 根据时间返回班次
   /// </summary>
   /// <param name="t"></param>
   /// <param name="parameterConfig"></param>
   /// <returns></returns>
   public static ShiftType GetShiftByTime(this DateTime t, ParameterConfig parameterConfig)
   {
      var dayShif = parameterConfig.DeviceParameter.DayShift;
      var nightShift = parameterConfig.DeviceParameter.NightShift;

      TimeSpan currentTimeSpan = t.TimeOfDay;
      return (currentTimeSpan, dayShif, nightShift) switch
      {
         var e when e.currentTimeSpan >= e.dayShif && e.currentTimeSpan < e.nightShift => ShiftType.白班,
         _ => ShiftType.夜班,
      };
   }

   /// <summary>
   /// 根据时间返回班次详情
   /// </summary>
   /// <param name="t"></param>
   /// <param name="parameterConfig"></param>
   /// <returns></returns>
   public static ShiftInfo GetShiftInfoByTime(this DateTime time, ParameterConfig parameterConfig)
   {
      var shift = time.GetShiftByTime(parameterConfig);

      var dayShift = parameterConfig.DeviceParameter.DayShift;
      var nightShift = parameterConfig.DeviceParameter.NightShift;

      if (shift == ShiftType.白班)
         return new ShiftInfo(shift, time.Date + dayShift, time.Date + nightShift);

      TimeSpan currentTimeSpan = time.TimeOfDay;

      if (currentTimeSpan < dayShift)
         return new ShiftInfo(shift, time.Date.AddDays(-1) + nightShift, time.Date + dayShift);

      return new ShiftInfo(shift, time.Date + nightShift, time.Date.AddDays(1) + dayShift);
   }

   /// <summary>
   /// 判断时间是否在当前班次
   /// </summary>
   /// <param name="lastExportTime"></param>
   /// <param name="parameterConfig"></param>
   /// <returns></returns>
   public static bool IsTimeInShift(this DateTime lastExportTime, DateTime currentTime, ParameterConfig parameterConfig)
   {
      var dayShifTime = parameterConfig.DeviceParameter.DayShift;
      var nightShiftTime = parameterConfig.DeviceParameter.NightShift;

      var currentDate = currentTime.Date;
      DateTime start = DateTime.Now;
      DateTime end = DateTime.Now;
      var shift = GetShiftByTime(currentTime, parameterConfig);

      if (shift == ShiftType.白班)
      {
         start = currentDate + dayShifTime;
         end = currentDate + nightShiftTime;
      }
      else
      {
         if (currentTime.TimeOfDay >= nightShiftTime)
         {
            start = currentDate + nightShiftTime;
            end = currentDate.AddDays(1) + dayShifTime;
         }
         else
         {
            start = currentDate.AddDays(-1) + nightShiftTime;
            end = currentDate + dayShifTime;
         }
      }
      return lastExportTime >= start && lastExportTime < end;
   }

   public record ShiftInfo(ShiftType shift, DateTime startTime, DateTime endTime);

   /// <summary>
   /// 根据时间获取上一班的时间范围及班次
   /// </summary>
   /// <param name="time"></param>
   /// <param name="parameterConfig"></param>
   /// <returns></returns>
   public static ShiftInfo GetPreShiftInfoByTime(this DateTime time, ParameterConfig parameterConfig)
   {
      var dayShifTime = parameterConfig.DeviceParameter.DayShift;
      var nightShiftTime = parameterConfig.DeviceParameter.NightShift;

      var currentShift = GetShiftByTime(time, parameterConfig);

      if (currentShift == ShiftType.白班)
      {
         var date = time.Date;
         return new ShiftInfo(ShiftType.夜班, date.AddDays(-1) + nightShiftTime, date + dayShifTime);
      }
      else
      {
         var date = time.TimeOfDay >= nightShiftTime ? time.Date : time.AddDays(-1).Date;
         return new ShiftInfo(ShiftType.白班, date + dayShifTime, date + nightShiftTime);
      }
   }
}
