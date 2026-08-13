namespace Kinlo.Equipment.Helpers;

public static class DeviceCommExtensions
{
   /// <summary>
   /// 是否是停机
   /// </summary>
   /// <param name="TokenSource"></param>
   /// <returns></returns>
   public static bool IsShutdown(this CancellationTokenSource TokenSource) => TokenSource == null || TokenSource.IsCancellationRequested;

   /// <summary>
   ///
   /// </summary>
   /// <param name="Device">设备实例</param>
   /// <param name="Request">请求报文</param>
   /// <param name="Length">期望读取的数据长度</param>
   /// <param name="RetryCount">重试次数</param>
   /// <param name="Protocol">协议</param>
   /// <param name="Validator">对返回bytes的额外校验，如果为null就不校验</param>
   /// <param name="TokenSource">取消令牌</param>
   /// <param name="LogHeader">日志头</param>
   public record DeviceCommand(
      DeviceBase Device,
      byte[] Request,
      int Length,
      int RetryCount,
      IProtocolHelper? Protocol,
      Func<byte[], bool>? Validator,
      CancellationTokenSource TokenSource,
      string LogHeader
   );

   /// <summary>
   /// 执行完整的设备通信请求（发送并接收）
   /// </summary>
   public static DeviceResult<byte[]> ExecuteRequest(this DeviceCommand record)
   {
      return ExecuteWithRetry(
         record.Device,
         record.LogHeader,
         record.TokenSource,
         record.RetryCount,
         () =>
         {
            var res = record.Device.Connect.WriteAndRead(
               record.Request,
               record.Protocol,
               record.LogHeader,
               record.Length == 0 ? 1024 : record.Length
            );

            if (res.State == CommState.Failed)
               return DeviceResult<byte[]>.Failure(DeviceStatus.Failure);

            if (res.State == CommState.NeedReconnect)
               return DeviceResult<byte[]>.Failure(DeviceStatus.通信失败, res.Message);

            var bytes = res.Data!;
            if (record.Validator != null && !record.Validator(bytes))
            {
               var errMsg = $"取数据长度不对bytes[{BitConverter.ToString(bytes)}]";
               errMsg.LogProcess(record.LogHeader, Log4NetLevelEnum.错误);
               return DeviceResult<byte[]>.Failure(DeviceStatus.报文校验不通过, errMsg);
            }

            return DeviceResult<byte[]>.Success(bytes);
         }
      );
   }

   /// <summary>
   /// 分离式设备通信命令（先发送，等待后接收）
   /// </summary>
   /// <param name="Device">设备实例</param>
   /// <param name="Request">请求报文</param>
   /// <param name="Length">期望读取的数据长度</param>
   /// <param name="RetryCount">重试次数</param>
   /// <param name="Validator">对返回bytes的额外校验，如果为null就不校验</param>
   /// <param name="TokenSource">取消令牌</param>
   /// <param name="LogHeader">日志头</param>
   /// <param name="ReadDelay">写入与读取之间的等待时间（毫秒），默认100ms</param>
   public record DeviceSplitCommand(
      DeviceBase Device,
      byte[] Request,
      int Length,
      int RetryCount,
      Func<byte[], bool>? Validator,
      CancellationTokenSource TokenSource,
      string LogHeader,
      int ReadDelay = 100
   );

   /// <summary>
   /// 执行分离式设备通信请求（先发送，等待后接收）
   /// </summary>
   public static DeviceResult<byte[]> ExecuteSplitRequest(this DeviceSplitCommand record)
   {
      return ExecuteWithRetry(
         record.Device,
         record.LogHeader,
         record.TokenSource,
         record.RetryCount,
         () =>
         {
            // 写入指令
            var writeRes = record.Device.Connect.Write(record.Request, record.LogHeader);
            if (writeRes.State == CommState.Failed)
               return DeviceResult<byte[]>.Failure(DeviceStatus.Failure);

            if (writeRes.State == CommState.NeedReconnect)
               return DeviceResult<byte[]>.Failure(DeviceStatus.通信失败, writeRes.Message);

            // 等待并读取
            Thread.Sleep(record.ReadDelay);
            var readRes = record.Device.Connect.Read(record.Length, record.LogHeader);
            if (readRes.State == CommState.Failed)
               return DeviceResult<byte[]>.Failure(DeviceStatus.Failure);

            if (readRes.State == CommState.NeedReconnect)
               return DeviceResult<byte[]>.Failure(DeviceStatus.通信失败, writeRes.Message);

            var bytes = readRes.Data!;

            // 校验
            if (record.Validator != null && !record.Validator(bytes))
            {
               var errMsg = $"取数据长度不对bytes[{BitConverter.ToString(bytes)}]";
               errMsg.LogProcess(record.LogHeader, Log4NetLevelEnum.错误);
               return DeviceResult<byte[]>.Failure(DeviceStatus.报文校验不通过, errMsg);
            }

            return DeviceResult<byte[]>.Success(bytes);
         }
      );
   }

   /// <summary>
   /// 通用的设备通信重试执行器
   /// </summary>
   /// <param name="device">设备实例（用于重连）</param>
   /// <param name="logHeader">日志头</param>
   /// <param name="tokenSource">取消令牌</param>
   /// <param name="retryCount">重试次数</param>
   /// <param name="executeComm">具体的通信逻辑委托（返回通信结果）</param>
   private static DeviceResult<byte[]> ExecuteWithRetry(
      DeviceBase device,
      string logHeader,
      CancellationTokenSource tokenSource,
      int retryCount,
      Func<DeviceResult<byte[]>> executeComm
   )
   {
      var result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);

      for (int i = 0; i < retryCount; i++)
      {
         if (tokenSource.IsShutdown())
            return DeviceResult<byte[]>.Failure(DeviceStatus.设备已停机);

         try
         {
            // 每次重试前清空缓存
            device.Connect.ClearCache(logHeader);

            // 执行具体的通信逻辑
            var res = executeComm();

            // 如果成功，直接返回
            if (res.Status == DeviceStatus.Success)
               return res;

            // 处理通信失败状态
            if (res.Status == DeviceStatus.Failure)
            {
               result = res;
               Thread.Sleep(300);
               continue;
            }

            // 处理需要重连的状态
            if (res.Status == DeviceStatus.通信失败)
            {
               result = res;
               if (!device.Reconnect(logHeader))
                  return result; // 重连失败，直接终止重试
               continue;
            }

            // 处理报文校验不通过等其他失败情况
            result = res;
         }
         catch (Exception ex)
         {
            var errMsg = $"第[{i + 1}]次[{Helper.GetCurrentMethodName()}]异常：{ex};";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
            result = DeviceResult<byte[]>.Failure(DeviceStatus.异常, errMsg);
         }
      }

      return result;
   }
}
