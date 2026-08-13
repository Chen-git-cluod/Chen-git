namespace Kinlo.Equipment.Helpers;

/// <summary>
/// 重连策略
/// </summary>
public static class ReconnectPolicy
{
   /// <summary>
   /// 判断当前是否允许重连
   /// </summary>
   /// <param name="info">重连状态信息</param>
   /// <returns>true=允许重连，false=禁止重连</returns>
   public static bool CanReconnect(this ReconnectInfoModel info)
   {
      // 当前时间
      var now = DateTime.Now;

      // 第一次重连（从未连接过）
      if (info.LastReconnectTime == null)
      {
         info.LastReconnectTime = now;
         info.CurrentReconnectCount = 1;
         return true;
      }

      // 距离上次重连的时间差
      var diff = now - info.LastReconnectTime.Value;

      // 超过时间窗口：重置计数
      if (diff.TotalMinutes > info.TimeWindow)
      {
         info.LastReconnectTime = now;
         info.CurrentReconnectCount = 1;
         return true;
      }

      // 没超过时间窗口：检查次数上限
      if (info.CurrentReconnectCount >= info.MaxReconnectCount)
      {
         return false;
      }

      // 次数 +1
      info.CurrentReconnectCount++;
      info.LastReconnectTime = now;

      return true;
   }
}
