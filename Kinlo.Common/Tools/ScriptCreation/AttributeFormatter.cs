namespace Kinlo.Common.Tools.ScriptCreation;

/// <summary>
/// 特性生成
/// </summary>
public static class AttributeFormatter
{
   static readonly HashSet<string> _ignoredAttributes = new()
   {
      "System.Runtime.CompilerServices.NullableContextAttribute",
      "System.Runtime.CompilerServices.NullableAttribute",
      "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
   };

   public static List<string> GetAttributeToStringList(this ICustomAttributeProvider member)
   {
      if (member == null)
         throw new ArgumentNullException(nameof(member));

      var result = new List<string>();

      var attributes = member switch
      {
         Type t => CustomAttributeData.GetCustomAttributes(t),
         MemberInfo mi => CustomAttributeData.GetCustomAttributes(mi),
         _ => throw new NotSupportedException($"Unsupported member type: {member.GetType()}"),
      };

      foreach (var attr in attributes)
      {
         if (_ignoredAttributes.Contains(attr.AttributeType.FullName!))
            continue;
         if (attr.AttributeType == typeof(DefaultMemberAttribute))
            continue;

         var sb = new StringBuilder();
         sb.Append('[');
         sb.Append(attr.AttributeType.Name.Replace("Attribute", ""));
         sb.Append('(');

         var args = new List<string>();

         // 构造参数
         args.AddRange(attr.ConstructorArguments.Select(FormatTypedArgument));

         // 命名参数
         if (attr.NamedArguments is { Count: > 0 })
         {
            args.AddRange(attr.NamedArguments.Select(na => $"{na.MemberName} = {FormatTypedArgument(na.TypedValue)}"));
         }

         sb.Append(string.Join(", ", args));
         sb.Append(")]");

         result.Add(sb.ToString());
      }

      return result;
   }

   private static string FormatTypedArgument(CustomAttributeTypedArgument arg)
   {
      if (arg.ArgumentType.IsArray && arg.Value is IList<CustomAttributeTypedArgument> array)
      {
         var items = array.Select(FormatTypedArgument);
         return $"[{string.Join(", ", items)}]";
      }

      return FormatSingleValue(arg.Value, arg.ArgumentType);
   }

   private static string FormatSingleValue(object? value, Type type)
   {
      if (value == null)
         return "null";

      if (type == typeof(string))
         return $"\"{value}\"";

      if (type.IsEnum)
      {
         var enumValue = Enum.ToObject(type, value);
         return $"{type.Name}.{enumValue}";
      }

      if (type == typeof(bool))
         return value.ToString()!.ToLowerInvariant();

      if (type == typeof(char))
         return $"'{value}'";

      if (type == typeof(Type) && value is Type t)
         return $"typeof({t.FullName})";

      return value.ToString()!;
   }
}
