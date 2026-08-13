namespace Kinlo.Common.Tools.ExpressionHelpers;

/// <summary>
///
/// </summary>
public static class PathHelper
{
   /// <summary>
   /// 获取完整路径
   /// 调用方式
   /// 注意：这里不能直接传 abc.MyProperty...，要传 lambda 表达式
   ///  string fullPath = PathHelper.GetPath((ABC abc) => abc.MyProperty.MyProperty.MyProperty);
   /// 输出: abc.MyProperty.MyProperty.MyProperty
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <typeparam name="TProperty"></typeparam>
   /// <param name="expression"></param>
   /// <returns></returns>
   public static string GetFullPath<T, TProperty>(this Expression<Func<T, TProperty>> expression)
   {
      string path = "";
      Expression current = expression.Body;

      while (current != null)
      {
         if (current is MemberExpression member)
         {
            // 如果是属性访问，把名字加到前面
            path = string.IsNullOrEmpty(path) ? member.Member.Name : $"{member.Member.Name}.{path}";
            current = member.Expression;
         }
         //取消了最外层变量名，变量名无意义
         //else if (current is ParameterExpression parameter)
         //{
         //   // 如果是参数（最外层的 abc），把名字加到最前面
         //   path = string.IsNullOrEmpty(path) ? parameter.Name : $"{parameter.Name}.{path}";
         //   current = null;
         //}
         else
         {
            break;
         }
      }
      return path;
   }
}
