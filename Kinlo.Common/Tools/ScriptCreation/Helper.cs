using System.ComponentModel;

namespace Kinlo.Common.Tools.ScriptCreation;

internal static class Helper
{
   /// <summary>
   /// 动态程序集
   /// </summary>
   public static string AssemblyName { get; set; } = "AzCustomAssembly";

   // <summary>
   /// 命名空间
   /// </summary>
   public static string NamespaceName { get; set; } = "AzNamespace";

   /// <summary>
   /// 每次编译生成一个新的命名空间,一次编译只调用一次(注意，只在编译时调用)
   /// </summary>
   /// <returns></returns>
   internal static string GetNamespaceName()
   {
      NamespaceName = $"{NamespaceName}{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
      return NamespaceName;
   }

   /// <summary>
   /// 类名
   /// </summary>
   public static string ClassName { get; set; } = "Battery";
   public static string Usings { get; set; } =
      "using System;\r\nusing System.ComponentModel;\r\nusing System.Runtime.CompilerServices;\r\n";
   public static string ViewModelBase { get; set; } =
      @"       public event PropertyChangedEventHandler? PropertyChanged;
         protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
         {
             if (this.PropertyChanged != null)
                 this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
         }";

   /// <summary>
   ///
   /// </summary>
   /// <param name="propertyInfo"></param>
   /// <param name="entity"></param>
   /// <returns></returns>
   public static (string propertyTypeStr, string valueStr) CreatePropertyInfo(this PropertyInfo propertyInfo, object entity)
   {
      var _value = propertyInfo.GetValue(entity, null);

      string _propertyTypeStr = string.Empty;
      string _valuesStr = string.Empty;
      bool _IsNull = false;
      var _propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
      if (_propertyType == null)
      {
         _propertyType = propertyInfo.PropertyType;
         _IsNull = false;
      }
      else
      {
         _IsNull = true;
      }
      var _propertyTypeName = _propertyType.Name.ToString();

      if (typeof(IEnumerable).IsAssignableFrom(_propertyType) && _propertyType != typeof(string)) //数组 及 List<T>等
      {
         if (_propertyType.IsGenericType)
         {
            Type? elementType = _propertyType.GetGenericArguments()[0]; //  获取数组的元素类型
            var genericType = _propertyType.GetGenericTypeDefinition();
            if (genericType == typeof(ObservableCollection<>))
            {
               _propertyTypeStr = _IsNull
                  ? "ObservableCollection<" + elementType.Name + ">?"
                  : "ObservableCollection<" + elementType.Name + ">";
               _valuesStr = _value == null ? string.Empty : $"= new()";
            }
            else
            {
               throw new Exception("未实现"); //其它待实现//其它待实现
            }
         }
         else if (_propertyType.IsArray)
         {
            Type? elementType = _propertyType.GetElementType(); //  获取数组的元素类型
            _propertyTypeStr = _IsNull ? $"{elementType.Name}[]?" : $"{elementType.Name}[]";
            _valuesStr = _value == null ? string.Empty : $"= []";
         }
         else
         {
            throw new Exception("未实现"); //其它待实现
         }
      }
      else if (_propertyType == typeof(Int64))
      {
         _propertyTypeStr = _IsNull ? "long?" : "long";
         _valuesStr = _value == null ? string.Empty : $"= {_value}";
      }
      else if (_propertyType == typeof(Int32))
      {
         _propertyTypeStr = _IsNull ? "int?" : "int";
         _valuesStr = _value == null ? string.Empty : $"= {_value}";
      }
      else if (_propertyType == typeof(Int16))
      {
         _propertyTypeStr = _IsNull ? "short?" : "short";
         _valuesStr = _value == null ? string.Empty : $"= {_value}";
      }
      else if (_propertyType == typeof(String) || _propertyType == typeof(string))
      {
         _propertyTypeStr = _IsNull ? "string?" : "string";
         _valuesStr = _value == null ? string.Empty : $"= \"{_value}\"";
      }
      else if (_propertyType == typeof(float))
      {
         _propertyTypeStr = _IsNull ? "float?" : "float";
         _valuesStr = _value == null ? string.Empty : $"= {_value}";
      }
      else if (_propertyType == typeof(double) || _propertyType == typeof(Double))
      {
         _propertyTypeStr = _IsNull ? "double?" : "double";
         _valuesStr = _value == null ? string.Empty : $"= {_value}";
      }
      else if (_propertyType == typeof(Boolean))
      {
         _propertyTypeStr = _IsNull ? "bool?" : "bool";
         _valuesStr = $"= {(_value == null ? "false" : _value.ToString().ToLower())}";
      }
      else if (_propertyType == typeof(Byte) || _propertyType == typeof(byte))
      {
         _propertyTypeStr = _IsNull ? "byte?" : "byte";
         _valuesStr = _value == null ? string.Empty : $"= {_value}";
      }
      else if (_propertyType == typeof(DateTime))
      {
         _propertyTypeStr = _IsNull ? _propertyTypeName + "?" : _propertyTypeName;
         //DateTime  不使用默认值
         // _valuesStr = DateTime.TryParse(_value.ToString(),out DateTime result) ? $"= Convert.ToDateTime(\"{_value}\")" : "";
      }
      else if (typeof(Enum).IsAssignableFrom(_propertyType))
      {
         _propertyTypeStr = _IsNull ? _propertyTypeName + "?" : _propertyTypeName;
         _valuesStr = $"= {_propertyTypeName}.{_value}";
      }
      else
      {
         _propertyTypeStr = _IsNull ? _propertyTypeName + "?" : _propertyTypeName;
         _valuesStr = _value == null ? string.Empty : $"= {_value}";
      }
      return (_propertyTypeStr, _valuesStr);
   }

   /// <summary>
   /// 枚举转代码
   /// </summary>
   /// <param name="enumType"></param>
   /// <returns></returns>
   public static string EnumToString(this Type enumType)
   {
      string _enumName = enumType.Name;
      var values = Enum.GetValues(enumType);

      StringBuilder _valueCode = new StringBuilder();
      foreach (var v in values)
      {
         int i = (int)v;
         _valueCode.Append($"                {v}={i},\r\n");
      }

      string _code =
         $@"         public enum {_enumName}
            {{
{_valueCode}
            }}";

      return _code;
   }
}
