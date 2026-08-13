using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.Common.Tools;

public static class DelegateExtensions
{
   public static Func<OtherParameterConfig> GetLanguage;

   public static void SafeInvoke(this Action action)
   {
      action?.Invoke();
   }

   public static void SafeInvoke<T>(this Action<T> action, T arg)
   {
      action?.Invoke(arg);
   }

   /// <summary>
   ///
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <typeparam name="TResult"></typeparam>
   /// <param name="func"></param>
   /// <param name="defaultValue"></param>
   /// <returns></returns>
   public static Func<T, TResult> WithExceptionHandling<T, TResult>(this Func<T, TResult> func, TResult defaultValue)
   {
      return (arg) =>
      {
         try
         {
            return func(arg);
         }
         catch (Exception ex)
         {
            Console.WriteLine($"异常捕获: {ex.Message}");
            return defaultValue;
         }
      };
   }
}
