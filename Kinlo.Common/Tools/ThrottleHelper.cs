namespace Kinlo.Common.Tools;

/// <summary>
/// 节流工具类（线程安全）
///
/// 在指定时间间隔内最多执行一次操作。
///
/// 特性：
/// 1. 线程安全
/// 2. 支持异步
/// 3. 支持无参数/泛型参数
/// 4. 节流期间只保留最后一次参数，也就是只会执行最后一次
/// </summary>
public class ThrottleHelper<T>
{
   // 节流时间间隔
   private readonly TimeSpan _interval;

   // 真正执行的方法
   private readonly Func<T, Task> _action;

   // 上一次执行时间
   private DateTime _lastInvokeTime = DateTime.MinValue;

   // 当前是否已经有延迟任务在排队
   private bool _pending;

   // 节流期间保存最后一次参数
   private T? _latestArg;

   // 线程锁
   private readonly object _lock = new();

   /// <summary>
   /// 创建节流器
   /// </summary>
   /// <param name="interval">节流时间间隔</param>
   /// <param name="action">执行方法</param>
   public ThrottleHelper(TimeSpan interval, Func<T, Task> action)
   {
      _interval = interval;
      _action = action ?? throw new ArgumentNullException(nameof(action));
   }

   /// <summary>
   /// 同步 Action 自动包装为异步
   /// </summary>
   public ThrottleHelper(TimeSpan interval, Action<T> action)
      : this(
         interval,
         arg =>
         {
            action(arg);
            return Task.CompletedTask;
         }
      ) { }

   /// <summary>
   /// 节流调用
   /// </summary>
   /// <param name="arg">参数</param>
   public async Task InvokeAsync(T arg)
   {
      Task? taskToRun = null;
      TimeSpan? delay = null;

      lock (_lock)
      {
         var now = DateTime.Now;
         var sinceLast = now - _lastInvokeTime;

         // 保存最新参数
         _latestArg = arg;

         // 已超过节流时间，立即执行
         if (sinceLast >= _interval && !_pending)
         {
            _lastInvokeTime = now;

            taskToRun = _action(arg);
         }
         // 节流期间，只创建一个延迟任务
         else if (!_pending)
         {
            _pending = true;

            delay = _interval - sinceLast;
         }
      }

      // 注意：
      // 不在 lock 内 await
      // 避免阻塞其它线程

      // 立即执行
      if (taskToRun != null)
      {
         await taskToRun;
      }
      // 延迟执行
      else if (delay.HasValue)
      {
         await ScheduleDelayedInvokeAsync(delay.Value);
      }
   }

   /// <summary>
   /// 延迟执行最后一次调用
   /// </summary>
   private async Task ScheduleDelayedInvokeAsync(TimeSpan delay)
   {
      try
      {
         await Task.Delay(delay);

         T? latestArg;

         lock (_lock)
         {
            latestArg = _latestArg;

            // 更新最后执行时间
            _lastInvokeTime = DateTime.Now;

            // 清除排队状态
            _pending = false;
         }

         // 执行最后一次参数
         if (latestArg != null)
         {
            await _action(latestArg);
         }
      }
      finally
      {
         lock (_lock)
         {
            _pending = false;
         }
      }
   }
}

// <summary>
/// 节流工具类 无参数版本
///
/// 在指定时间间隔内最多执行一次操作。
///
/// 特性：
/// 1. 线程安全
/// 2. 支持异步
/// 3. 支持无参数/泛型参数
/// 4. 节流期间只保留最后一次参数，也就是只会执行最后一次
/// </summary>
public class ThrottleHelper : ThrottleHelper<bool>
{
   public ThrottleHelper(TimeSpan interval, Func<Task> action)
      : base(interval, async _ => await action()) { }

   public ThrottleHelper(TimeSpan interval, Action action)
      : base(
         interval,
         _ =>
         {
            action();
            return Task.CompletedTask;
         }
      ) { }

   /// <summary>
   /// 无参数调用
   /// </summary>
   public Task InvokeAsync()
   {
      return base.InvokeAsync(true);
   }
}
