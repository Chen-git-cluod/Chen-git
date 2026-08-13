namespace Kinlo.Equipment.Models;

public class DeviceOperationOptions
{
   public DeviceOperationOptions(int retryCount = 3, int timeout = 1000)
   {
      RetryCount = retryCount;
      Timeout = timeout;
   }

   /// <summary>
   /// 重试次数（默认3次）
   /// </summary>
   public int RetryCount { get; set; } = 3;

   /// <summary>
   /// 超时，单位毫秒（正常在设备初始化时已经定义，此处为需另外单独定义）
   /// </summary>
   public int Timeout { get; set; } = 1000;

   /// <summary>
   /// 操作类型
   /// </summary>
   public int OperationType { get; set; }

   /// <summary>
   /// 预留的扩展参数
   /// </summary>
   public Dictionary<string, object> Extensions { get; set; } = new();
}
