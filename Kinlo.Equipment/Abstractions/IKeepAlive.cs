namespace Kinlo.Equipment;

/// <summary>
/// 保活接口
/// </summary>
public interface IDeviceKeepAlive
{
   /// <summary>
   /// 心跳请求参数
   /// </summary>
   /// <param name="Command"></param>
   /// <param name="ExpectedLength">设备返回byte长度</param>
   /// <param name="TimeoutMs"></param>
   public record KeepAliveRequest(byte[] Command, int ExpectedLength, int TimeoutMs = 1000);

   /// <summary>
   /// 心跳
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="request"></param>
   /// <param name="cancellationToken"></param>
   /// <returns></returns>
   DeviceResult<T> Heartbeat<T>(KeepAliveRequest request, CancellationToken cancellationToken);
}
