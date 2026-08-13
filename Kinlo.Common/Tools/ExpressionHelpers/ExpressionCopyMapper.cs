namespace Kinlo.Common.Tools;

/// <summary>
/// 表达式高效率复制（注意如果两个不同类型的类，相同属性名类type应一样，不复制除string外引用类型），如何复杂的请用FastCopy类， Expression动态拼接+泛型缓存
/// </summary>
/// <typeparam name="TIn"></typeparam>
/// <typeparam name="TOut"></typeparam>
public class ExpressionCopyMapper<TIn, TOut> //Mapper`2
{
   private static Func<TIn, TOut> _func = null;

   static ExpressionCopyMapper()
   {
      try
      {
         ParameterExpression parameterExpression = Expression.Parameter(typeof(TIn), "p");
         List<MemberBinding> memberBindingList = new List<MemberBinding>();
         foreach (var item in typeof(TOut).GetProperties())
         {
            if (!item.CanRead || !item.CanWrite)
               continue;
            if (!item.PropertyType.IsValueType && item.PropertyType != typeof(string) && item.PropertyType != typeof(System.Type)) //不复制非值类型
               continue;
            MemberExpression property = Expression.Property(parameterExpression, typeof(TIn).GetProperty(item.Name));
            MemberBinding memberBinding = Expression.Bind(item, property);
            memberBindingList.Add(memberBinding);
         }
         foreach (var item in typeof(TOut).GetFields())
         {
            if (!item.FieldType.IsValueType && item.FieldType != typeof(string) && item.FieldType != typeof(System.Type)) //不复制非值类型
               continue;
            MemberExpression property = Expression.Field(parameterExpression, typeof(TIn).GetField(item.Name));
            MemberBinding memberBinding = Expression.Bind(item, property);
            memberBindingList.Add(memberBinding);
         }
         MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
         Expression<Func<TIn, TOut>> lambda = Expression.Lambda<Func<TIn, TOut>>(
            memberInitExpression,
            new ParameterExpression[] { parameterExpression }
         );
         _func = lambda.Compile(); //拼装是一次性的
      }
      catch (Exception ex)
      {
         $"[表达式复制]生成表达式出现异常:{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   public static TOut? Trans(TIn t)
   {
      try
      {
         return _func(t);
      }
      catch (Exception ex)
      {
         $"[表达式复制]运行表达式出现异常:{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
      return default;
   }
}
