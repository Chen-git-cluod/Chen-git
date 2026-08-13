namespace Kinlo.Equipment.Models;

public class ReconnectInfoModel
{
   /// <summary>
   /// 最后重连时间
   /// </summary>
   public DateTime? LastReconnectTime { get; set; }

   /// <summary>
   /// 当前重连次数
   /// </summary>
   public int CurrentReconnectCount { get; set; }

   /// <summary>
   /// 统计时间窗口(分钟)
   /// </summary>
   public int TimeWindow { get; set; } = 10;

   /// <summary>
   /// 在一定时间内重连次数上限
   /// </summary>
   public int MaxReconnectCount { get; set; } = 8;
}
