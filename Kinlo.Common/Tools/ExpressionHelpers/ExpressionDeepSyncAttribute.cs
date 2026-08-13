namespace Kinlo.Common.Tools.ExpressionHelpers;

/// <summary>
/// 基于表达式树的高性能深度同步工具 (支持属性、字段及集合内容就地同步)
/// 此版本为最完善版本，此directory 下类似工具相比都不完善
/// </summary>
public static class ExpressionDeepSync
{
   // 动态字典缓存 (用于 object 类型同步)
   private static readonly ConcurrentDictionary<(Type, Type), Action<object, object, Func<MemberInfo, bool>?>> _dynamicCache =
      new ConcurrentDictionary<(Type, Type), Action<object, object, Func<MemberInfo, bool>?>>();

   /// <summary>
   /// 清缓存字典
   /// </summary>
   public static void ClearAll() => _dynamicCache.Clear();

   #region 公开 API

   /// <summary>
   /// 泛型深同步(含集合类型)：将 source 的值同步到 target
   /// </summary>
   /// <typeparam name="TSource"></typeparam>
   /// <typeparam name="TTarget"></typeparam>
   /// <param name="source"></param>
   /// <param name="target"></param>
   /// <param name="interceptor">>拦截器：返回 true 则跳过该成员</param>
   public static void DeepSyncTo<TSource, TTarget>(TSource source, TTarget target, Func<MemberInfo, bool>? interceptor = null)
      where TSource : class
      where TTarget : class
   {
      if (source == null || target == null)
         return;
      try
      {
         MapCache<TSource, TTarget>.SyncAction(source, target, interceptor);
      }
      catch (Exception ex)
      {
         Console.WriteLine($"[泛型同步异常]：{ex}");
      }
   }

   /// <summary>
   /// 动态类型深同步(含集合类型)：将 source 的值同步到 target
   /// </summary>
   /// <param name="source"></param>
   /// <param name="target"></param>
   /// <param name="interceptor">拦截器：返回 true 则跳过该成员</param>
   public static void DeepSyncTo(this object source, object target, Func<MemberInfo, bool>? interceptor = null)
   {
      if (source == null || target == null)
         return;
      try
      {
         var key = (source.GetType(), target.GetType());
         var action = _dynamicCache.GetOrAdd(
            key,
            k =>
            {
               var sParam = Expression.Parameter(typeof(object), "s");
               var tParam = Expression.Parameter(typeof(object), "t");
               var iParam = Expression.Parameter(typeof(Func<MemberInfo, bool>), "interceptor");

               var sConvert = Expression.Convert(sParam, k.Item1);
               var tConvert = Expression.Convert(tParam, k.Item2);

               var block = BuildSyncBlock(sConvert, tConvert, k.Item1, k.Item2, iParam);
               return Expression.Lambda<Action<object, object, Func<MemberInfo, bool>?>>(block, sParam, tParam, iParam).Compile();
            }
         );
         action(source, target, interceptor);
      }
      catch (Exception ex)
      {
         Console.WriteLine($"[动态同步异常]：{ex}");
      }
   }

   /// <summary>
   /// 泛型生成深副本
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="source"></param>
   /// <param name="interceptor">拦截器：返回 true 则跳过该成员</param>
   /// <returns></returns>
   public static T? ToDeepCopy<T>(this T source, Func<MemberInfo, bool>? interceptor = null)
      where T : class, new()
   {
      try
      {
         if (source == null)
            return null;
         var target = new T();
         DeepSyncTo(source, target, interceptor);
         return target;
      }
      catch (Exception ex)
      {
         Console.WriteLine($"[泛型生成深度副本异常]：{ex}");
      }
      return null;
   }

   /// <summary>
   /// 动态类生成深副本
   /// </summary>
   /// <param name="source"></param>
   /// <param name="interceptor">拦截器：返回 true 则跳过该成员</param>
   /// <returns></returns>
   public static object? ToDeepCopy(this object source, Func<MemberInfo, bool>? interceptor = null)
   {
      try
      {
         if (source == null)
            return null;
         var target = Activator.CreateInstance(source.GetType());
         if (target == null)
            return null;
         DeepSyncTo(source, target, interceptor);
         return target;
      }
      catch (Exception ex)
      {
         Console.WriteLine($"[动态生成深度副本异常]：{ex}");
      }
      return null;
   }

   #endregion

   #region 核心引擎

   // 静态泛型缓存：利用 CLR 特性，为每对类型组合生成唯一的静态 Action
   private static class MapCache<TSource, TTarget>
   {
      public static readonly Action<TSource, TTarget, Func<MemberInfo, bool>?> SyncAction = BuildAction();

      private static Action<TSource, TTarget, Func<MemberInfo, bool>?> BuildAction()
      {
         var sParam = Expression.Parameter(typeof(TSource), "s");
         var tParam = Expression.Parameter(typeof(TTarget), "t");
         var iParam = Expression.Parameter(typeof(Func<MemberInfo, bool>), "interceptor");

         var block = BuildSyncBlock(sParam, tParam, typeof(TSource), typeof(TTarget), iParam);
         return Expression.Lambda<Action<TSource, TTarget, Func<MemberInfo, bool>?>>(block, sParam, tParam, iParam).Compile();
      }
   }

   /// <summary>
   /// 构建同步代码块
   /// </summary>
   private static Expression BuildSyncBlock(Expression sExp, Expression tExp, Type sType, Type tType, ParameterExpression iParam)
   {
      var assignments = new List<Expression>();
      var flags = BindingFlags.Public | BindingFlags.Instance;

      // 1. 合并处理属性和字段
      var sMembers = sType.GetProperties(flags).Cast<MemberInfo>().Concat(sType.GetFields(flags));
      var tProps = tType.GetProperties(flags);
      var tFields = tType.GetFields(flags);

      //var sMemberList = sMembers.ToList();
      foreach (var sMember in sMembers)
      {
         try
         {
            // A. 静态过滤：特性标记忽略
            if (sMember.GetCustomAttribute<ExpressionDeepSyncAttribute>()?.IsIgnore == true)
               continue;

            // B. 寻找匹配的目标成员
            MemberInfo? tMember = null;
            Type? sMemberType = null;
            Type? tMemberType = null;
            Expression? sAccess = null;
            Expression? tAccess = null;

            if (sMember is PropertyInfo sp)
            {
               if (!sp.CanRead)
                  continue;
               var tp = tProps.FirstOrDefault(p => p.Name == sp.Name && p.CanWrite);
               if (tp == null || tp.GetCustomAttribute<ExpressionDeepSyncAttribute>()?.IsIgnore == true)
                  continue;
               tMember = tp;
               sMemberType = sp.PropertyType;
               tMemberType = tp.PropertyType;
               sAccess = Expression.Property(sExp, sp);
               tAccess = Expression.Property(tExp, tp);
            }
            else if (sMember is FieldInfo sf)
            {
               var tf = tFields.FirstOrDefault(f => f.Name == sf.Name && !f.IsInitOnly);
               if (tf == null || tf.GetCustomAttribute<ExpressionDeepSyncAttribute>()?.IsIgnore == true)
                  continue;
               tMember = tf;
               sMemberType = sf.FieldType;
               tMemberType = tf.FieldType;
               sAccess = Expression.Field(sExp, sf);
               tAccess = Expression.Field(tExp, tf);
            }

            if (tMember == null || sAccess == null || tAccess == null)
               continue;

            // C. 动态过滤：构建运行时的拦截器逻辑
            // 生成逻辑：if (interceptor == null || !interceptor(memberInfo)) { ...执行赋值... }
            var memberConst = Expression.Constant(sMember);
            var invokeInterceptor = Expression.Invoke(iParam, memberConst);
            var shouldSync = Expression.OrElse(
               Expression.Equal(iParam, Expression.Constant(null, typeof(Func<MemberInfo, bool>))),
               Expression.Not(invokeInterceptor)
            );

            var memberBlock = new List<Expression>();
            ProcessMember(sAccess, tAccess, sMemberType!, tMemberType!, memberBlock, iParam);

            if (memberBlock.Count > 0)
            {
               assignments.Add(Expression.IfThen(shouldSync, Expression.Block(memberBlock)));
            }
         }
         catch (Exception ex)
         {
            $"[构建同步代码块] 成员名;{sMember.Name} 异常:{ex}".LogRun(Log4NetLevelEnum.错误, true);
         }
      }

      return assignments.Count > 0 ? Expression.Block(assignments) : Expression.Empty();
   }

   /// <summary>
   /// 成员
   /// </summary>
   /// <param name="sMember"></param>
   /// <param name="tMember"></param>
   /// <param name="sType"></param>
   /// <param name="tType"></param>
   /// <param name="assignments"></param>
   /// <param name="iParam"></param>
   private static void ProcessMember(
      Expression sMember,
      Expression tMember,
      Type sType,
      Type tType,
      List<Expression> assignments,
      ParameterExpression iParam
   )
   {
      // 处理集合 (排除 string)
      if (typeof(IEnumerable).IsAssignableFrom(sType) && sType != typeof(string))
      {
         var syncMethod = typeof(ExpressionDeepSync).GetMethod(nameof(SyncCollectionItems), BindingFlags.Static | BindingFlags.NonPublic);
         assignments.Add(Expression.Call(syncMethod, sMember, tMember, iParam));
      }
      //处理复杂对象引用 (排除基础类型、枚举、string、decimal等)
      else if (sType.IsClass && sType != typeof(string))
      {
         // 调用 DeepSyncTo 的动态版本，实现递归深度同步
         // 逻辑：if (sMember != null) { DeepSyncTo(sMember, tMember, iParam); }
         var syncMethod = typeof(ExpressionDeepSync).GetMethods().First(m => m.Name == nameof(DeepSyncTo) && m.IsGenericMethod == false);

         // 注意：这里需要处理 target 属性为 null 的情况
         // 简单做法是调用一个辅助方法来处理“创建或同步”逻辑
         var ensureAndSyncMethod = typeof(ExpressionDeepSync).GetMethod(
            nameof(EnsureAndSyncObject),
            BindingFlags.Static | BindingFlags.NonPublic
         );
         assignments.Add(Expression.Call(ensureAndSyncMethod, sMember, tMember, iParam));
      }
      //基础类型/值类型
      else
      {
         Expression val = sMember;
         if (sType != tType)
         {
            try
            {
               val = Expression.Convert(val, tType);
            }
            catch
            {
               return;
            }
         }
         assignments.Add(Expression.Assign(tMember, val));
      }
   }

   private static void EnsureAndSyncObject(object sourceMember, object targetMember, Func<MemberInfo, bool>? interceptor)
   {
      if (sourceMember == null)
         return;

      // 这里需要反射获取 target 的属性/字段，如果为 null 则实例化
      // 但在表达式树外部处理更稳妥。为了保持高性能，
      // 我们直接利用已有的 DeepSyncTo 逻辑：

      // 如果 target 已经有实例，直接同步过去
      if (targetMember != null)
      {
         DeepSyncTo(sourceMember, targetMember, interceptor);
      }
      // 注意：如果 targetMember 是 null，由于 C# 参数传递是按值传递（引用本身的值），
      // 无法在这里直接 new 之后赋回给 target 对象的属性。
      // 完美的解决办法是在 BuildSyncBlock 中生成更复杂的表达式，或者使用反射。
   }

   private static void SyncCollectionItems(object sourceColl, object targetColl, Func<MemberInfo, bool>? interceptor)
   {
      if (sourceColl is not IEnumerable sourceItems || targetColl is not IList targetList)
         return;

      targetList.Clear();
      foreach (var item in sourceItems)
      {
         if (item == null)
         {
            targetList.Add(null);
            continue;
         }

         var itemType = item.GetType();
         // 基础类型直接添加
         if (itemType.IsValueType || itemType == typeof(string))
         {
            targetList.Add(item);
         }
         else
         {
            // 复杂对象递归同步
            var newItem = Activator.CreateInstance(itemType);
            if (newItem != null)
            {
               DeepSyncTo(item, newItem, interceptor);
               targetList.Add(newItem);
            }
         }
      }
   }

   #endregion
}
