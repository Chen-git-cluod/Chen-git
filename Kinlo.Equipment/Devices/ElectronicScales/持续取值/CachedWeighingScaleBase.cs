namespace Kinlo.Equipment.Devices.ElectronicScales;

/// <summary>
/// 持续读取 称 的值（不建议使用）
/// </summary>
public abstract class CachedWeighingScaleBase : DeviceBase, IContinuousReading
{
   /// <summary>
   /// 单帧长度（用于判断是否可以尝试解析）
   /// </summary>
   protected abstract int FrameLength { get; }

   /// <summary>
   /// 清零命令
   /// </summary>
   protected abstract byte[] ZeroCommand { get; }
   public ReconnectInfoModel ReconnectInfo { get; set; } = new ReconnectInfoModel();

   private readonly List<byte> _cacheByte = new List<byte>();
   private readonly object _lock = new object();

   public CachedWeighingScaleBase(DeviceInfoModel info)
      : base(info) { }

   public override bool Open()
   {
      Close();
      if (base.Open())
      {
         _ = ContinuousReading();
         return true;
      }
      return false;
   }

   /// <summary>
   /// 解析一帧数据为重量
   /// </summary>
   protected abstract DeviceResult<double> TryParseWeight(byte[] frame, string logHeader);

   private Task ContinuousReading()
   {
      return Task.Run(() =>
      {
         var logHeader = DeviceInfo.ToDeviceLogHeader();
         Thread.Sleep(200);
         while (!IsShutdown)
         {
            try
            {
               lock (_lock)
               {
                  if (_cacheByte.Count >= FrameLength * 20) //限制缓存以保证数据的实时性
                  {
                     _cacheByte.RemoveRange(0, _cacheByte.Count - FrameLength * 5);
                  }
               }
               var res = Connect.Read(512, logHeader);
               if (res.State == CommState.Failed)
               {
                  Thread.Sleep(300);
                  continue;
               }
               else if (res.State == CommState.NeedReconnect)
               {
                  if (CanReconnect())
                     this.Reconnect(logHeader);
                  else
                     $"在{ReconnectInfo.TimeWindow}分钟内重连次数超{ReconnectInfo.MaxReconnectCount}次上限，不重连！".LogProcess(logHeader);
                  break; //一定要退出，重连打开时会重新开始一个任务
               }
               var raw = res.Data!;
               if (raw != null && raw.Length > 0)
               {
                  lock (_lock)
                     _cacheByte.AddRange(raw);
               }

               Thread.Sleep(20);
            }
            catch (Exception ex)
            {
               $"[{DeviceInfo.Communication}]读取原始数据异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
               Thread.Sleep(300);
            }
         }
      });
   }

   int _sampleCount = 2; //重量采样数量

   /// <summary>
   /// 读取重量
   /// </summary>
   /// <typeparam name="TValue"></typeparam>
   /// <param name="address"></param>
   /// <param name="logHeader"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TValue : default
   {
      //Thread.Sleep(200); //延时200ms取称重量
      var result = DeviceResult<TValue>.Failure("默认");
      List<double> weighSamples = new(); //采样重量集合
      Stopwatch weightStopwatch = new Stopwatch();
      weightStopwatch.Restart();

      while (weighSamples.Count < _sampleCount && weightStopwatch.ElapsedMilliseconds < 9000 && !DeviceInfo.TaskToken.IsShutdown()) //对比重量是否一致
      {
         var weightRes = ReadWeight(logHeader);
         if (weightRes.Status != DeviceStatus.Success)
         {
            weighSamples.Clear();
            result = DeviceResult<TValue>.Failure(weightRes.Status, weightRes.ErrorMessage, weightRes.Exception);
            Thread.Sleep(300);
            continue;
         }
         weighSamples.Add(weightRes.Value);
         if (weighSamples.Count == _sampleCount)
         {
            string weighStr = $"[{string.Join(',', weighSamples.Select(x => x))}]";
            $"取对比重量：{weighStr}".LogProcess(logHeader);
            var max = weighSamples.Max(x => x);
            var min = weighSamples.Min(x => x);
            if (max - min <= 0.2) //接受误差0.2g
            {
               $"通过重量比对，取得正确重量：[{weighSamples[^1]}]".LogProcess(logHeader);
               if (weighSamples[^1] is TValue value)
               {
                  return DeviceResult<TValue>.Success(value);
               }
               else
               {
                  var msg = $"[{DeviceInfo.ProcessesType}]传入数据类型非double！";
                  msg.LogProcess(logHeader, Log4NetLevelEnum.错误);
                  return DeviceResult<TValue>.Failure(DeviceStatus.结果和期望数据类型不同, msg);
               }
            }
            else
            {
               var msg = $"未通过重量比对：{weighStr}";
               msg.LogProcess(logHeader);
               result = DeviceResult<TValue>.Failure(DeviceStatus.称重不稳定, msg);
               weighSamples.RemoveAt(0);
            }
         }
         Thread.Sleep(100);
      }
      weightStopwatch.Stop();
      return result;
   }

   /// <summary>
   /// 取称重量
   /// </summary>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   private DeviceResult<double> ReadWeight(string logHeader)
   {
      try
      {
         string msg = string.Empty;
         byte[]? bytes = null;
         for (int i = 0; i < 5; i++)
         {
            lock (_lock)
            {
               if (_cacheByte.Count >= FrameLength * 2)
               {
                  bytes = _cacheByte.ToArray();
                  _cacheByte.Clear();
                  break;
               }
            }
            Thread.Sleep(100); // 如果没拿到足够的数据，休眠100ms再试
         }

         if (bytes == null || !bytes.Any())
         {
            msg = $"[{DeviceInfo.ProcessesType}]称重取值 IP:{DeviceInfo.IPCOM}，端口：{DeviceInfo.Port},未取到数据!";
            msg.LogProcess(logHeader);
            return DeviceResult<double>.Failure(DeviceStatus.取值失败, msg);
         }
         if (bytes.Length < FrameLength)
         {
            msg =
               $"[{DeviceInfo.ProcessesType}]称重取值 IP:{DeviceInfo.IPCOM}，端口：{DeviceInfo.Port},取值byte[{BitConverter.ToString(bytes)}]长度小于{FrameLength}!";
            msg.LogProcess(logHeader);
            return DeviceResult<double>.Failure(DeviceStatus.报文校验不通过, msg);
         }

         return TryParseWeight(bytes, logHeader);
      }
      catch (Exception ex)
      {
         var msg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
         msg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return DeviceResult<double>.Failure(DeviceStatus.异常, msg, ex);
      }
   }

   /// <summary>
   /// 清零
   /// </summary>
   /// <param name="value"></param>
   /// <param name="address"></param>
   /// <param name="logHeader"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public override bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   )
   {
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions() { RetryCount = 3 };
      for (int i = 0; i < options.RetryCount; i++)
      {
         try
         {
            var res = Connect.Write(ZeroCommand, logHeader);
            if (res.State == CommState.Failed)
            {
               Thread.Sleep(300);
               continue;
            }
            else if (res.State == CommState.NeedReconnect)
            {
               this.Reconnect(logHeader);
               continue;
            }
            return true;
         }
         catch (Exception ex)
         {
            if (DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
               break;
            $"[清零]异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
            Thread.Sleep(300);
         }
      }
      return false;
   }

   public override bool WriteClass<TClass>(
      TClass value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      throw new NotImplementedException();
   }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class
   {
      throw new NotImplementedException();
   }

   Task IContinuousReading.ContinuousReading()
   {
      return ContinuousReading();
   }

   public bool CanReconnect() => ReconnectInfo.CanReconnect();
}
