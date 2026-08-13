namespace Kinlo.Common.Tools;

/// <summary>
/// 表达式高效率赋值， Expression动态拼接+字典缓存， 相对ExpressionAssignmentMapper1稍慢,但可以不用显式声明类型
/// </summary>
public static class ExpressionAssignmentMapper2
{
   /// <summary>
   /// 清缓存字典
   /// </summary>
   public static void ClearAll() => _cacheDic.Clear();

   /// <summary>
   /// 字典缓存，保存委托，委托内部是转换的动作
   /// </summary>
   private static ConcurrentDictionary<string, object> _cacheDic = new ConcurrentDictionary<string, object>();

   /// <summary>
   /// 表达式赋值
   /// </summary>
   /// <typeparam name="TSource"></typeparam>
   /// <typeparam name="TTarget"></typeparam>
   /// <param name="source"></param>
   /// <param name="target"></param>
   public static void EntityAssign<TSource, TTarget>(this TSource source, TTarget target)
   {
      try
      {
         string key = $"funckey_{source}_{target}";
         if (!_cacheDic.ContainsKey(key))
         {
            Type _sourceType = source.GetType();
            Type _targetType = target.GetType();
            ParameterExpression _parameterExpressionSource = Expression.Parameter(typeof(TSource), "tSource");
            ParameterExpression _parameterExpressionTarget = Expression.Parameter(typeof(TTarget), "tTarget");
            List<BinaryExpression> _binarys = new List<BinaryExpression>();

            foreach (var item in _sourceType.GetProperties())
            {
               if (!item.CanRead || !item.CanWrite)
                  continue;
               var _attribute = item.GetCustomAttribute<BatteryDisplayAttribute>();
               if (_attribute != null && _attribute.IsIgnore)
                  continue;
               if (!item.PropertyType.IsValueType && item.PropertyType != typeof(string)) //不赋值非值类型
                  //  if (item.Name == "Index" || (!item.PropertyType.IsValueType && item.PropertyType != typeof(string)))//不复制非值类型
                  continue;

               MemberExpression _expressionSource = Expression.Property(
                  Expression.Convert(_parameterExpressionSource, _sourceType),
                  _sourceType.GetProperty(item.Name)
               );
               var _targetPropertyInfo = _targetType.GetProperty(item.Name);
               if (_targetPropertyInfo != null)
               {
                  MemberExpression _expressionTarget = Expression.Property(
                     Expression.Convert(_parameterExpressionTarget, _targetType),
                     _targetPropertyInfo
                  );
                  BinaryExpression binary = null;
                  if (item.PropertyType.IsEnum)
                  {
                     var _unaryExpression = Expression.Convert(_expressionSource, typeof(int));
                     _unaryExpression = Expression.Convert(_unaryExpression, _targetPropertyInfo.PropertyType);
                     binary = Expression.Assign(_expressionTarget, _unaryExpression);
                  }
                  else
                  {
                     binary = Expression.Assign(_expressionTarget, _expressionSource);
                  }
                  _binarys.Add(binary);
               }
            }
            foreach (var item in _sourceType.GetFields())
            {
               if (!item.FieldType.IsValueType && item.FieldType != typeof(string)) //不复制非值类型
                  continue;

               MemberExpression _expressionRead = Expression.Field(
                  Expression.Convert(_parameterExpressionSource, _sourceType),
                  _sourceType.GetField(item.Name)
               );
               var _targetFieldInfo = _targetType.GetField(item.Name);
               if (_targetFieldInfo != null)
               {
                  MemberExpression _expressionWrite = Expression.Field(
                     Expression.Convert(_parameterExpressionTarget, _targetType),
                     _targetFieldInfo
                  );
                  BinaryExpression binary = null;
                  if (item.FieldType.IsEnum)
                  {
                     var _unaryExpression = Expression.Convert(_expressionRead, typeof(int));
                     _unaryExpression = Expression.Convert(_unaryExpression, _targetFieldInfo.FieldType);
                     binary = Expression.Assign(_expressionWrite, _unaryExpression);
                  }
                  else
                  {
                     binary = Expression.Assign(_expressionWrite, _expressionRead);
                  }
                  _binarys.Add(binary);
               }
            }
            BlockExpression block = Expression.Block(_binarys);
            var _lambda = Expression.Lambda<Action<TSource, TTarget>>(
               block,
               new ParameterExpression[] { _parameterExpressionSource, _parameterExpressionTarget }
            );

            var _action = _lambda.Compile();
            _cacheDic[key] = _action;
         }

         ((Action<TSource, TTarget>)_cacheDic[key]).Invoke(source, target);
      }
      catch (Exception ex)
      {
         $"[表达式赋值2]出现异常:{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }
}
