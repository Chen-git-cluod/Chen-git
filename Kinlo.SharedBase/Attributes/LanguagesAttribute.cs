namespace Kinlo.SharedBase.Attributes;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public class LanguagesAttribute : Attribute
{
   /// <summary>
   /// 一般情况为0：简中，1：繁中,2:英语,其它另外定义
   /// </summary>
   public string[] Languages { get; set; } = [];

   /// <summary>
   /// 指示是否需要扫描内部的属性多语言（在类标记此特性时）
   /// </summary>
   public bool IsScanProperty { get; set; } = true;

   /// <summary>
   /// 指示是否需要扫描内部的方法多语言（在类标记此特性时）
   /// </summary>
   public bool IsScanMethod { get; set; } = false;

   public LanguagesAttribute() { }

   public LanguagesAttribute(string zh_cn)
   {
      Languages = [zh_cn];
   }

   public LanguagesAttribute(params string[] langs)
   {
      Languages = langs;
   }
}
