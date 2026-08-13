namespace Kinlo.Equipment.Abstractions;

/// <summary>
/// 持续读取接口，如 大部分的称及一些刷卡器
/// </summary>
public interface IContinuousReading
{
   /// <summary>
   /// 在一定时间窗口内超过指定重连就不再重连了
   /// </summary>
   ReconnectInfoModel ReconnectInfo { get; set; }

   /// <summary>
   /// 是否能重连
   /// </summary>
   /// <returns></returns>
   bool CanReconnect();

   /// <summary>
   /// 持续读取
   /// </summary>
   /// <returns></returns>
   Task ContinuousReading();
}
