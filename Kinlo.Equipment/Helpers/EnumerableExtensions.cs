using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.Equipment.Helpers;

/// <summary>
///  LINQ 相关的扩展方法,
/// </summary>
public static class EnumerableExtensions
{
   /// <summary>
   /// FirstOrDefault 的增强版,
   /// 如果Enum或值类型集合就应该使用此版本，
   /// 值类型的Default类型和实际有没找开会冲突
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="source"></param>
   /// <param name="predicate"></param>
   /// <param name="result"></param>
   /// <returns></returns>
   public static bool TryFindFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T result)
   {
      foreach (var item in source)
      {
         if (predicate(item))
         {
            result = item;
            return true;
         }
      }
      result = default!;
      return false;
   }
}
