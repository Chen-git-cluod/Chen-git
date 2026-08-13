namespace Kinlo.Equipment.Devices;

internal static class CipExtensions
{
   /// <summary>
   /// 从连接池获取 PLC 连接的超时时间（毫秒）
   /// </summary>
   public const int ConnectionPoolTakeTimeoutMs = 5000;
   public const int RetryIntervalMs = 100;

   /// <summary>
   /// 从连接池获取一个 PLC 连接并执行一次完整的 CIP 请求。
   /// <para>
   /// 此方法负责连接池资源管理，确保无论执行成功、失败还是发生异常，
   /// 获取到的 PLC 连接都会归还到连接池，避免连接泄露。
   /// </para>
   /// </summary>
   /// <param name="cipClients"></param>
   /// <param name="request"></param>
   /// <param name="retryCount"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static DeviceResult<byte[]> CipExecuteRequest(
      this BlockingCollection<CipClient> cipClients,
      byte[] request,
      int retryCount,
      string logHeader,
      CancellationTokenSource tokenSource
   )
   {
      var result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);
      if (!cipClients.TryTake(out var plc, ConnectionPoolTakeTimeoutMs))
      {
         result = DeviceResult<byte[]>.Failure(
            $"从连接池获取PLC连接超时（{ConnectionPoolTakeTimeoutMs}ms），当前连接池数量：{cipClients.Count}"
         );
         result.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return result;
      }
      try
      {
         return CipExecuteWithRetry(request, retryCount, logHeader, tokenSource, ref plc);
      }
      catch (Exception ex)
      {
         result = DeviceResult<byte[]>.Failure(DeviceStatus.异常, $"PLC执行操作异常：{ex}", ex);
         result.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return result;
      }
      finally
      {
         cipClients.Add(plc);
      }
   }

   /// <summary>
   /// 从连接池获取一个 PLC 连接并执行写入请求。
   /// 由于写入报文的生成依赖 PLC 连接实例，因此在方法内部构建请求报文。
   /// </summary>
   /// <para>
   /// 此方法负责连接池资源管理，确保无论执行成功、失败还是发生异常，
   /// 获取到的 PLC 连接都会归还到连接池，避免连接泄露。
   /// </para>
   /// <param name="cipClients"></param>
   /// <param name="deviceInfo"></param>
   /// <param name="lable"></param>
   /// <param name="value"></param>
   /// <param name="retryCount"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static DeviceResult<byte[]> CipExecuteRequest(
      this BlockingCollection<CipClient> cipClients,
      DeviceInfoModel deviceInfo,
      string lable,
      object value,
      int retryCount,
      string logHeader
   )
   {
      var result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);
      if (!cipClients.TryTake(out var plc, ConnectionPoolTakeTimeoutMs))
      {
         result = DeviceResult<byte[]>.Failure(
            $"从连接池获取PLC连接超时（{ConnectionPoolTakeTimeoutMs}ms），当前连接池数量：{cipClients.Count}"
         );
         result.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return result;
      }
      try
      {
         var request = ParseCipLabel.LableWriteClassRequest(lable, value, deviceInfo.Communication, plc, logHeader);
         return CipExecuteWithRetry(request, retryCount, logHeader, deviceInfo.TaskToken, ref plc);
      }
      catch (Exception ex)
      {
         result = DeviceResult<byte[]>.Failure(DeviceStatus.异常, $"PLC执行操作异常：{ex}", ex);
         result.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return result;
      }
      finally
      {
         cipClients.Add(plc);
      }
   }

   /// <summary>
   /// 使用指定的 PLC 连接执行 CIP 请求，并在通讯失败时自动重试。
   /// <para>
   /// 此方法仅负责通讯及重试逻辑，不负责连接池资源管理。
   /// </para>
   /// </summary>
   /// <param name="plc"></param>
   /// <param name="request"></param>
   /// <param name="retryCount"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static DeviceResult<byte[]> CipExecuteWithRetry(
      this byte[] request,
      int retryCount,
      string logHeader,
      CancellationTokenSource tokenSource,
      ref CipClient plc
   )
   {
      var result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);
      for (int i = 0; i < retryCount; i++)
      {
         if (tokenSource.IsShutdown())
            return DeviceResult<byte[]>.Failure(DeviceStatus.设备已停机);
         try
         {
            var res = plc.Conn.WriteAndRead(request, plc.Protocol, logHeader);
            if (res.State == CommState.Success)
            {
               plc.OnWatchdogFed();
               return DeviceResult<byte[]>.Success(res.Data!);
            }

            result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure, res.Message, res.Exception);

            if (res.State == CommState.NeedReconnect && !plc.RepairCip())
               return DeviceResult<byte[]>.Failure(res.Message);
         }
         catch (Exception ex)
         {
            result = DeviceResult<byte[]>.Failure(DeviceStatus.异常, $"第{i + 1}次 PLC执行操作异常：{ex}", ex);
            result.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误);
         }
         Thread.Sleep(RetryIntervalMs);
      }
      return result;
   }
}
