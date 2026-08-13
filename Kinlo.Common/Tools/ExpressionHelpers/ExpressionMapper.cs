namespace Kinlo.Common.Tools;

public static class ExpressionMapper
{
   /// <summary>
   /// 字典缓存，保存的是委托，委托内部是转换的动作
   /// </summary>
   private static ConcurrentDictionary<int, object> _GetDic = new ConcurrentDictionary<int, object>();
   private static ConcurrentDictionary<int, object> _SetDic = new ConcurrentDictionary<int, object>();
   private static ConcurrentDictionary<int, object> _CreateDic = new ConcurrentDictionary<int, object>();

   /// <summary>
   ///
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <returns></returns>
   public static object ExpCreateInstance(this Type t)
   {
      try
      {
         int _key = $"createkey_{t.FullName}".GetHashCode();
         if (!_CreateDic.ContainsKey(_key))
         {
            var _newExpression = Expression.New(t);
            var _lambda = Expression.Lambda<Func<object>>(_newExpression);
            var _func = _lambda.Compile();
            _CreateDic[_key] = _func;
         }
         return ((Func<object>)_CreateDic[_key]).Invoke();
      }
      catch (Exception ex)
      {
         $"[ExpCreateInstance]异常:{ex}".LogRun(Log4NetLevelEnum.错误);
         return default;
      }
   }

   static object _getBarcodeFunc = null;

   /// <summary>
   /// 此方法为动态类（其它类不可用）取条码特意优化，其它属性请用下面【GetPropertyValue】通用方法或用自带反射方法,速度会稍慢
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="t"></param>
   /// <returns></returns>
   public static string GetDynamicBarcode<T>(this T t)
   {
      if (_getBarcodeFunc == null)
      {
         string _name = nameof(BatMainModel.Barcode);
         Type _type = t.GetType();
         PropertyInfo _propertyInfo = _type.GetProperty(_name);
         if (_propertyInfo == null)
         {
            $"[GetPropertyValue] 该类型没有名为[{_name}]的属性".LogRun(Log4NetLevelEnum.错误);
            return default;
         }
         var _paramT = Expression.Parameter(typeof(T));
         //转成真实类型，防止Dynamic类型转换成object
         var _unaryRealType = Expression.Convert(_paramT, _type);
         var _memberExpression = Expression.Property(_unaryRealType, _propertyInfo);
         //防止Dynamic类型不兼容,转成object
         //var _unaryObjectType = Expression.Convert(_memberExpression, typeof(object));
         // Expression<Func<T, object>> _lambda = Expression.Lambda<Func<T, object>>(_unaryObjectType, _paramT);
         Expression<Func<T, string>> _lambda = Expression.Lambda<Func<T, string>>(_memberExpression, _paramT);
         var _func = _lambda.Compile();
         _getBarcodeFunc = _func;
      }
      return ((Func<T, string>)_getBarcodeFunc)(t);
   }

   static object _getIDFunc = null;

   /// <summary>
   /// 此方法为动态类（其它类不可用）取ID特意优化，其它属性请用下面【GetPropertyValue】通用方法或用自带反射方法,速度会稍慢
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="t"></param>
   /// <returns></returns>
   public static long GetDynamicID<T>(this T t)
   {
      if (_getIDFunc == null)
      {
         string _name = nameof(BatMainModel.Id);
         Type _type = t.GetType();
         PropertyInfo _propertyInfo = _type.GetProperty(_name);
         if (_propertyInfo == null)
         {
            $"[GetPropertyValue] 该类型没有名为[{_name}]的属性".LogRun(Log4NetLevelEnum.错误);
            return default;
         }
         var _paramT = Expression.Parameter(typeof(T));
         //转成真实类型，防止Dynamic类型转换成object
         var _unaryRealType = Expression.Convert(_paramT, _type);
         var _memberExpression = Expression.Property(_unaryRealType, _propertyInfo);
         //防止Dynamic类型不兼容,转成object
         //var _unaryObjectType = Expression.Convert(_memberExpression, typeof(object));
         // Expression<Func<T, object>> _lambda = Expression.Lambda<Func<T, object>>(_unaryObjectType, _paramT);
         Expression<Func<T, long>> _lambda = Expression.Lambda<Func<T, long>>(_memberExpression, _paramT);
         var _func = _lambda.Compile();
         _getIDFunc = _func;
      }
      return ((Func<T, long>)_getIDFunc)(t);
   }

   /// <summary>
   /// 根据属性名获取属性值
   /// </summary>
   /// <typeparam name="T">对象类型</typeparam>
   /// <param name="t">对象</param>
   /// <param name="name">属性名</param>
   /// <returns>属性的值</returns>
   public static object GetPropertyValue<T>(this T t, string name)
      where T : class
   {
      try
      {
         int _key = $"funckey_{t}_{name}".GetHashCode();
         if (!_GetDic.ContainsKey(_key))
         {
            Type _type = t.GetType();
            PropertyInfo _propertyInfo = _type.GetProperty(name);
            if (_propertyInfo == null)
            {
               $"[GetPropertyValue] 该类型没有名为[{name}]的属性".LogRun(Log4NetLevelEnum.错误);
               return default;
            }

            var _paramT = Expression.Parameter(typeof(T));
            //转成真实类型，防止Dynamic类型转换成object
            var _unaryRealType = Expression.Convert(_paramT, _type);
            var _memberExpression = Expression.Property(_unaryRealType, _propertyInfo);
            //防止Dynamic类型不兼容,转成object
            var _unaryObjectType = Expression.Convert(_memberExpression, typeof(object));
            Expression<Func<T, object>> _lambda = Expression.Lambda<Func<T, object>>(_unaryObjectType, _paramT);
            var _func = _lambda.Compile();
            _GetDic[_key] = _func;
         }
         return ((Func<T, object>)_GetDic[_key])(t);
      }
      catch (Exception ex)
      {
         $"[GetPropertyValue]异常:{ex}".LogRun(Log4NetLevelEnum.错误);
         return default;
      }
   }

   /// <summary>
   /// 根据属性名称设置属性的值
   /// </summary>
   /// <typeparam name="T">对象类型</typeparam>
   /// <param name="t">对象</param>
   /// <param name="name">属性名</param>
   /// <param name="value">属性的值</param>
   public static void SetPropertyValue<T>(this T t, string name, object value)
   {
      try
      {
         int _key = $"actionkey_{t}_{name}".GetHashCode();
         if (!_SetDic.ContainsKey(_key))
         {
            Type _type = t.GetType();
            PropertyInfo _propertyInfo = _type.GetProperty(name);
            if (_propertyInfo == null)
            {
               $"[SetPropertyValue] [{_type.FullName}]该类型没有名为[{name}]的属性".LogRun(Log4NetLevelEnum.错误);
               return;
            }
            if (!_propertyInfo.CanWrite)
            {
               $"[SetPropertyValue] [{_type.FullName}]该类型属性[{name}]不可写入，无法赋值；".LogRun(Log4NetLevelEnum.错误);
               return;
            }
            var _paramT = Expression.Parameter(typeof(T));
            //转成真实类型，防止Dynamic类型转换成object
            UnaryExpression _unaryRealType = Expression.Convert(_paramT, _type);
            var _paramObject = Expression.Parameter(typeof(object));
            var _bodyVal = Expression.Convert(_paramObject, _propertyInfo.PropertyType);

            var _member = Expression.Property(_unaryRealType, _propertyInfo);
            var _binary = Expression.Assign(_member, _bodyVal);
            var _lambda = Expression.Lambda<Action<T, object>>(_binary, _paramT, _paramObject);
            var _action = _lambda.Compile();
            _SetDic[_key] = _action;
         }
         ((Action<T, object>)_SetDic[_key])(t, value);
      }
      catch (Exception ex)
      {
         $"[SetPropertyValue]异常:{ex}".LogRun(Log4NetLevelEnum.错误);
      }
   }

   /// <summary>
   /// 根据属性名称设置属性的值（Set方法）
   /// </summary>
   /// <typeparam name="T">对象类型</typeparam>
   /// <param name="t">对象</param>
   /// <param name="name">属性名</param>
   /// <param name="value">属性的值</param>
   public static void SetPropertyValue2<T>(this T t, string name, object value)
   {
      try
      {
         int _key = $"actionsetkey_{t}_{name}".GetHashCode();
         if (!_SetDic.ContainsKey(_key))
         {
            Type _type = t.GetType();
            PropertyInfo _propertyInfo = _type.GetProperty(name);
            if (_propertyInfo == null)
            {
               $"[SetPropertyValue] 该类型没有名为[{name}]的属性".LogRun(Log4NetLevelEnum.错误);
               return;
            }
            var _paramT = Expression.Parameter(typeof(T));
            //转成真实类型，防止Dynamic类型转换成object
            UnaryExpression _unaryRealType = Expression.Convert(_paramT, _type);

            var _param_Object = Expression.Parameter(typeof(object));
            var _bodyVal = Expression.Convert(_param_Object, _propertyInfo.PropertyType);

            //获取设置属性的值的方法
            var _setMethod = _propertyInfo.GetSetMethod(true);

            //如果只是只读,则setMethod==null
            if (_setMethod == null)
            {
               $"[SetPropertyValue] 该类型属性[{name}]为只读类型，无法赋值；".LogRun(Log4NetLevelEnum.错误);
               return;
            }
            var _body = Expression.Call(_unaryRealType, _setMethod, _bodyVal);
            var _action = Expression.Lambda<Action<T, object>>(_body, _paramT, _param_Object).Compile();
            _SetDic[_key] = _action;
         }
         ((Action<T, object>)_SetDic[_key])(t, value);
      }
      catch (Exception ex)
      {
         $"[SetPropertyValue]异常:{ex}".LogRun(Log4NetLevelEnum.错误);
      }
   }
}
