using System.Windows.Threading;

namespace Kinlo.Common.Tools;

/// <summary>
/// UI 调度器工具类
/// </summary>
public static class UIThreadHelper
{
   private static Dispatcher _dispatcher;
   public static Dispatcher Dispatcher => _dispatcher ??= Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

   public static void Initialize() => _dispatcher = Application.Current?.Dispatcher ?? throw new InvalidOperationException();

   public static bool IsUIThread => Dispatcher.CheckAccess();

   /// <summary>
   /// 异步执行 UI 操作（推荐默认使用）
   /// </summary>
   /// <param name="action"></param>
   /// <returns></returns>
   public static Task InvokeOnUiThreadAsync(Action action)
   {
      if (action == null)
         return Task.CompletedTask;

      if (Dispatcher.CheckAccess())
      {
         action();
         return Task.CompletedTask;
      }

      return Dispatcher.InvokeAsync(action).Task;
   }

   /// <summary>
   /// 异步执行异步操作（推荐）
   /// </summary>
   public static async Task InvokeOnUiThreadAsync(Func<Task> action)
   {
      if (action == null)
         return;

      if (Dispatcher.CheckAccess())
         await action();
      else
         await Dispatcher.InvokeAsync(async () => await action()).Task;
   }

   /// <summary>
   /// 异步执行异步操作（推荐）
   /// </summary>
   public static async Task<T?> InvokeOnUiThreadAsync<T>(Func<Task<T>> action)
   {
      if (action == null)
         return default;

      if (Dispatcher.CheckAccess())
         return await action();
      else
      {
         return await Dispatcher.InvokeAsync(async () => await action()).Task.Unwrap();
      }
   }

   /// <summary>
   /// 同步步执行异步操作
   /// </summary>
   public static T? InvokeOnUiThread<T>(Func<T> action)
   {
      if (action == null)
         return default;

      if (Dispatcher.CheckAccess())
         return action();
      else
      {
         return Dispatcher.Invoke(action);
      }
   }

   /// <summary>
   /// 同步执行
   /// </summary>
   public static void InvokeOnUiThread(Action action)
   {
      if (Dispatcher.CheckAccess())
         action();
      else
         Dispatcher.Invoke(action);
   }
}
