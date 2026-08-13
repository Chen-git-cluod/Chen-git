namespace Kinlo.SharedBase.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public class DynamicClassAttribute : Attribute
{
   /// <summary>
   /// 是否忽略属性
   /// </summary>
   public bool IsIgnoreProperty { get; set; } = false;

   /// <summary>
   /// 忽略某些特性
   /// </summary>
   public Type[] IgnoreAttributes { get; set; } = [];

   /// <summary>
   /// 判断最终状态时是否忽略该属性
   /// </summary>
   public bool IsIgnoreForFinalStatus { get; set; }

   /// <summary>
   /// 判断结果时，些结果所属工位（如果当前结果NG时，NG结果会关联些工位）
   /// </summary>
   public ProcessTypeEnum Process { get; set; }

   /// <summary>
   /// 父结果关联
   /// 如果有父结果关联，就是子结果，父结果一般是ResultTypeEnum类型
   /// 子结果不直接影响最终判断
   /// 一般一个父结果有多个子结果，按一定规则影响父结果，由父结果影响最终结果
   /// </summary>
   public string ParentResult { get; set; } = string.Empty;

   /// <summary>
   /// 生产中父子结果规则
   /// </summary>
   public ProductResultRuleEnum Rule { get; set; }
}

/// <summary>
/// 父子结果规则枚举
/// </summary>
public enum ProductResultRuleEnum
{
   由最后子结果决定父结果 = 0,

   //暂时未来使用
   任一子结果为OK即为OK = 1,

   //暂时未来使用
   任一子结果为NG即为NG = 2,
}
