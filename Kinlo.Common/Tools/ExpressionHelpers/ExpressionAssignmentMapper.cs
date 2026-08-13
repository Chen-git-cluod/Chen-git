namespace Kinlo.Common.Tools;

/// <summary>
/// 表达式高效率赋值， Expression动态拼接+泛型缓存，此方法最快
/// </summary>
public class ExpressionAssignmentMapper<TSource, TTarget>
{
   private static Action<TSource, TTarget> _action = null;

   static ExpressionAssignmentMapper()
   {
      try
      {
         ParameterExpression parameterExpressionSource = Expression.Parameter(typeof(TSource), "tSource");
         ParameterExpression parameterExpressionTarget = Expression.Parameter(typeof(TTarget), "tTarget");
         List<BinaryExpression> binarys = new List<BinaryExpression>();

         foreach (var item in typeof(TSource).GetProperties())
         {
            if (!item.CanRead || !item.CanWrite)
               continue;
            if (!item.PropertyType.IsValueType && item.PropertyType != typeof(string)) //不复制非值类型
               continue;

            MemberExpression _expressionSource = Expression.Property(parameterExpressionSource, typeof(TSource).GetProperty(item.Name));
            var _targetPropertyInfo = typeof(TTarget).GetProperty(item.Name);
            if (_targetPropertyInfo != null)
            {
               MemberExpression expressionTarget = Expression.Property(parameterExpressionTarget, typeof(TTarget).GetProperty(item.Name));
               BinaryExpression binary = null;
               if (item.PropertyType.IsEnum)
               {
                  var _unaryExpression = Expression.Convert(_expressionSource, typeof(int));
                  _unaryExpression = Expression.Convert(_unaryExpression, _targetPropertyInfo.PropertyType);
                  binary = Expression.Assign(expressionTarget, _unaryExpression);
               }
               else
               {
                  binary = Expression.Assign(expressionTarget, _expressionSource);
               }
               binarys.Add(binary);
            }
         }
         foreach (var item in typeof(TSource).GetFields())
         {
            if (!item.FieldType.IsValueType && item.FieldType != typeof(string)) //不复制非值类型
               continue;

            MemberExpression expressionSource = Expression.Field(parameterExpressionSource, typeof(TSource).GetField(item.Name));

            var targetFieldInfo = typeof(TTarget).GetField(item.Name);
            if (targetFieldInfo != null)
            {
               MemberExpression _expressionTarget = Expression.Field(parameterExpressionTarget, typeof(TTarget).GetField(item.Name));
               BinaryExpression binary = null;
               if (item.FieldType.IsEnum)
               {
                  var unaryExpression = Expression.Convert(expressionSource, typeof(int));
                  unaryExpression = Expression.Convert(unaryExpression, targetFieldInfo.FieldType);
                  binary = Expression.Assign(_expressionTarget, unaryExpression);
               }
               else
               {
                  binary = Expression.Assign(_expressionTarget, expressionSource);
               }
               binarys.Add(binary);
            }
         }
         BlockExpression block = Expression.Block(binarys);
         var lambda = Expression.Lambda<Action<TSource, TTarget>>(
            block,
            new ParameterExpression[] { parameterExpressionSource, parameterExpressionTarget }
         );

         _action = lambda.Compile();
      }
      catch (Exception ex)
      {
         $"[表达式赋值1]生成表达式出现异常:{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   public static void Trans(TSource source, TTarget target)
   {
      try
      {
         if (_action != null)
         {
            _action(source, target);
         }
         else
         {
            $"[表达式赋值1]表达式为空！,不会运算！".LogRun(Log4NetLevelEnum.错误, true);
         }
      }
      catch (Exception ex)
      {
         $"[表达式赋值1]运行表达式出现异常:{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }
}
