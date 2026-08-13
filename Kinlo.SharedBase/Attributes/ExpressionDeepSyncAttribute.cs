namespace Kinlo.SharedBase.Attributes;

/// <summary>
/// 深度同步特性：用于标记忽略特定属性或字段
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class ExpressionDeepSyncAttribute : Attribute
{
   public bool IsIgnore { get; set; }

   public ExpressionDeepSyncAttribute(bool isIgnore = true) => IsIgnore = isIgnore;
}
