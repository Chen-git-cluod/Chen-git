namespace Kinlo.Common.Tools;

/// <summary>
///
/// </summary>
public static class FastCopy
{
   static ConcurrentDictionary<string, object> copiers = new ConcurrentDictionary<string, object>();

   /// <summary>
   /// 复制两个对象同名属性值
   /// </summary>
   /// <typeparam name="S"></typeparam>
   /// <typeparam name="T"></typeparam>
   /// <param name="source">源对象</param>
   /// <param name="target">目标对象</param>
   /// <param name="copyNull">源对象属性值为null时，是否将值复制给目标对象</param>
   public static void Copy<S, T>(S source, T target, bool copyNull = true)
   {
      string name = string.Format("{0}_{1}_{2}", typeof(S), typeof(T), copyNull);

      object targetCopier;
      if (!copiers.TryGetValue(name, out targetCopier))
      {
         Action<S, T> copier = CreateCopier<S, T>(copyNull);
         copiers.TryAdd(name, copier);
         targetCopier = copier;
      }

      Action<S, T> action = (Action<S, T>)targetCopier;
      action(source, target);
   }

   /// <summary>
   /// 为指定的两种类型编译生成属性复制委托
   /// </summary>
   /// <typeparam name="S"></typeparam>
   /// <typeparam name="T"></typeparam>
   /// <param name="copyNull">源对象属性值为null时，是否将值复制给目标对象</param>
   /// <returns></returns>
   private static Action<S, T> CreateCopier<S, T>(bool copyNull)
   {
      ParameterExpression source = Expression.Parameter(typeof(S));
      ParameterExpression target = Expression.Parameter(typeof(T));
      var sourceProps = typeof(S).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead).ToList();
      var targetProps = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite).ToList();

      // 查找可进行赋值的属性
      var copyProps = targetProps.Where(tProp =>
         sourceProps
            .Where(sProp =>
               sProp.Name == tProp.Name // 名称一致 且
               && (
                  sProp.PropertyType == tProp.PropertyType // 属性类型一致 或
                  || sProp.PropertyType.IsAssignableFrom(tProp.PropertyType) // 源属性类型 为 目标属性类型 的 子类；eg：object target = string source;   或
                  || (
                     tProp.PropertyType.IsValueType
                     && sProp.PropertyType.IsValueType
                     && // 属性为值类型且基础类型一致，但目标属性为可空类型 eg：int? num = int num;
                     (
                        (
                           tProp.PropertyType.GenericTypeArguments.Length > 0
                              ? tProp.PropertyType.GenericTypeArguments[0]
                              : tProp.PropertyType
                        ) == sProp.PropertyType
                     )
                  )
               )
            )
            .Count() > 0
      );

      List<Expression> expressionList = new List<Expression>();
      foreach (var prop in copyProps)
      {
         if (prop.PropertyType.IsValueType) // 属性为值类型
         {
            PropertyInfo sProp = typeof(S).GetProperty(prop.Name);
            PropertyInfo tProp = typeof(T).GetProperty(prop.Name);
            if (sProp.PropertyType == tProp.PropertyType) // 属性类型一致 eg：int num = int num;    或   int? num = int? num;
            {
               var assign = Expression.Assign(Expression.Property(target, prop.Name), Expression.Property(source, prop.Name));
               expressionList.Add(assign);
            }
            else if (sProp.PropertyType.GenericTypeArguments.Length <= 0 && tProp.PropertyType.GenericTypeArguments.Length > 0) // 属性类型不一致且目标属性类型为可空类型 eg：int? num = int num;
            {
               var convert = Expression.Convert(Expression.Property(source, prop.Name), tProp.PropertyType);
               var cvAssign = Expression.Assign(Expression.Property(target, prop.Name), convert);
               expressionList.Add(cvAssign);
            }
         }
         else // 属性为引用类型
         {
            var assign = Expression.Assign(Expression.Property(target, prop.Name), Expression.Property(source, prop.Name)); // 编译生成属性赋值语句   target.{PropertyName} = source.{PropertyName};
            var sourcePropIsNull = Expression.Equal(Expression.Constant(null, prop.PropertyType), Expression.Property(source, prop.Name)); // 判断源属性值是否为Null；编译生成  source.{PropertyName} == null
            var setNull = Expression.IsTrue(Expression.Constant(copyNull)); // 判断是否复制Null值 编译生成  copyNull == True
            var setNullTest = Expression.IfThen(setNull, assign);
            var condition = Expression.IfThenElse(sourcePropIsNull, setNullTest, assign);

            /**
             * 编译生成
             * if(source.{PropertyName} == null)
             * {
             *   if(setNull)
             *   {
             *     target.{PropertyName} = source.{PropertyName};
             *   }
             * }
             * else
             * {
             *   target.{PropertyName} = source.{PropertyName};
             * }
             */
            expressionList.Add(condition);
         }
      }
      var block = Expression.Block(expressionList.ToArray());
      Expression<Action<S, T>> lambda = Expression.Lambda<Action<S, T>>(block, source, target);
      return lambda.Compile();
   }
}
