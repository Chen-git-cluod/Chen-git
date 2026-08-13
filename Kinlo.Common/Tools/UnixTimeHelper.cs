namespace Kinlo.Common.Tools;

public static class UnixTimeHelper
{
   /// <summary>
   /// 指定时间转时UnixTime （毫秒 Long时间戳）
   /// </summary>
   /// <param name="dateTime"></param>
   /// <returns></returns>
   public static long ToUnixTimeMilliseconds(this DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

   /// <summary>
   /// 指定时间转时UnixTime （秒 Long时间戳）
   /// </summary>
   /// <param name="dateTime"></param>
   /// <returns></returns>
   public static long ToUnixTimeSeconds(this DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeSeconds();

   /// <summary>
   /// 获取发给Plc同步时间戳
   /// </summary>
   /// <returns></returns>
   public static long GetSyncPlcUnixTimeSeconds() => DateTime.Now.AddHours(8).ToUnixTimeSeconds();

   /// <summary>
   /// UnixTime 转本地时间(秒)
   /// </summary>
   /// <param name="unixSeconds"></param>
   /// <returns></returns>
   public static DateTime ToLocalTimeFromSeconds(this long unixSeconds) => DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime;

   /// <summary>
   /// UnixTime 转本地时间(毫秒)
   /// </summary>
   /// <param name="unixMilliseconds"></param>
   /// <returns></returns>
   public static DateTime ToLocalFromMilliseconds(this long unixMilliseconds) =>
      DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).LocalDateTime;
}
