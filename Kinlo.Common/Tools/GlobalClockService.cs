namespace Kinlo.Common.Tools;

/// <summary>
///  线程安全的全局时钟服务（单例）
/// - 支持时、分、秒 /订阅/取消
/// </summary>
public sealed class GlobalClockService
{
   private static readonly Lazy<GlobalClockService> _instance = new(() => new GlobalClockService());
   public static GlobalClockService Instance => _instance.Value;

   /// <summary>
   /// 当前执行中任务....
   /// </summary>
   public static readonly ConcurrentDictionary<long, string> ActiveTasks = new();

   // 所有状态变更必须在此锁内进行
   private readonly object _syncRoot = new();

   private Task? _workerTask;
   private CancellationTokenSource _cts = new();

   // 使用 backing field 实现线程安全的事件
   private Action<DateTime>? _onSecond;
   private Action<DateTime>? _onMinute;
   private Action<DateTime>? _onHour;

   /// <summary>
   /// 每秒一次
   /// </summary>
   public event Action<DateTime> OnSecond
   {
      add
      {
         lock (_syncRoot)
            _onSecond += value;
      }
      remove
      {
         lock (_syncRoot)
            _onSecond -= value;
      }
   }

   /// <summary>
   /// 每分钟执行一次
   /// </summary>
   public event Action<DateTime> OnMinute
   {
      add
      {
         lock (_syncRoot)
            _onMinute += value;
      }
      remove
      {
         lock (_syncRoot)
            _onMinute -= value;
      }
   }

   /// <summary>
   /// 每小时一次
   /// </summary>
   public event Action<DateTime> OnHour
   {
      add
      {
         lock (_syncRoot)
            _onHour += value;
      }
      remove
      {
         lock (_syncRoot)
            _onHour -= value;
      }
   }

   private GlobalClockService() { }

   /// <summary>
   /// 任务追踪器，自动跟踪其生命周期。如果执行长时间操作任务需加入任务追踪，防止退出程序时中断未完成任务
   /// </summary>
   /// <param name="key"></param>
   /// <param name="taskName"></param>
   /// <param name="work"></param>
   /// <returns></returns>
   public static async Task ExecuteWithTracking(long key, string taskName, Func<Task> work)
   {
      ActiveTasks[key] = taskName;
      try
      {
         await work().ConfigureAwait(false);
      }
      finally
      {
         ActiveTasks.TryRemove(key, out _);
      }
   }

   /// <summary>
   /// 线程安全启动
   /// </summary>
   public void Start()
   {
      lock (_syncRoot)
      {
         // 已在运行 直接退出
         if (_workerTask?.IsCompleted == false)
            return;

         // 清理旧资源
         _cts.Cancel();
         _cts.Dispose();

         // 重建
         _cts = new CancellationTokenSource();
         _workerTask = RunAsync(_cts.Token);
      }
   }

   /// <summary>
   /// 线程安全停止
   /// </summary>
   public void Stop()
   {
      lock (_syncRoot)
      {
         _cts.Cancel();
         _cts.Dispose();
      }
   }

   private async Task RunAsync(CancellationToken ct)
   {
      var now = DateTime.Now;
      var _lastSecondKey = TruncateToSecond(now);
      var _lastMinuteKey = TruncateToMinute(now);
      var _lastHourKey = TruncateToHour(now);

      while (!ct.IsCancellationRequested)
      {
         now = DateTime.Now;
         var wakeUpTime = TruncateToSecond(now).AddSeconds(1);

         var delay = wakeUpTime - now;
         if (delay.TotalMilliseconds < 1)
            delay = TimeSpan.FromMilliseconds(1); // 防御极端情况
         try
         {
            await Task.Delay(delay, ct);
         }
         catch (OperationCanceledException) when (ct.IsCancellationRequested)
         {
            break;
         }

         // 注意：广播时不能持有锁！否则可能死锁
         // 所以先“快照”当前委托引用（在锁外调用）
         Action<DateTime>? secondHandler,
            minuteHandler,
            hourHandler;
         lock (_syncRoot)
         {
            secondHandler = _onSecond;
            minuteHandler = _onMinute;
            hourHandler = _onHour;
         }

         now = DateTime.Now;
         var secondStart = TruncateToSecond(now);
         if (secondStart != _lastSecondKey)
         {
            FireAndForget(secondHandler, now);
            _lastSecondKey = secondStart;

            var minuteStart = TruncateToMinute(now);
            if (minuteStart != _lastMinuteKey)
            {
               FireAndForget(minuteHandler, now);
               _lastMinuteKey = minuteStart;

               var hourStart = TruncateToHour(now);
               if (hourStart != _lastHourKey)
               {
                  FireAndForget(hourHandler, now);
                  _lastHourKey = hourStart;
               }
            }
         }
      }
   }

   private static void FireAndForget(Action<DateTime>? handler, DateTime time)
   {
      if (handler == null)
         return;

      foreach (var sub in handler.GetInvocationList().Cast<Action<DateTime>>())
      {
         _ = Task.Run(() =>
         {
            try
            {
               sub(time);
            }
            catch (Exception ex)
            {
               $"[GlobalClock] 异常: {ex}".LogRun();
            }
         });
      }
   }

   private static DateTime TruncateToSecond(DateTime dt) => new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);

   private static DateTime TruncateToMinute(DateTime dt) => new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0);

   private static DateTime TruncateToHour(DateTime dt) => new(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0);
}
