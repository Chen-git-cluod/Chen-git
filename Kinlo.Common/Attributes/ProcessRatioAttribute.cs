using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.Common.Attributes;

/// <summary>
/// 实时统计使用的特性
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class ProcessRatioAttribute : Attribute
{
   // 时间字段
   public string TimeName { get; }

   /// <summary>
   /// 前置结果字段（可选）,有些结果计算时城其它结果辅助，如果注液，测漏NG的不计算注液
   /// </summary>
   public string? PreResultName { get; }

   /// <summary>
   /// 显示名 ，正常不用标志，程序初始化时指定，但部分一个工序多个结果时就要手动指定
   /// </summary>
   public string DisplayName { get; set; } = string.Empty;

   public ProcessRatioAttribute(string timeName, string? preResultName = null)
   {
      TimeName = timeName;
      PreResultName = preResultName;
   }
}

/// <summary>
///  实时统计使用的特性。用于细分NG，比如注液过多、过少分开统计
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class ProcessRatioDetailAttribute : Attribute
{
   public string Name { get; }

   public ResultTypeEnum Result { get; }

   public ProcessRatioDetailAttribute(string name, ResultTypeEnum result)
   {
      Name = name;
      Result = result;
   }
}
