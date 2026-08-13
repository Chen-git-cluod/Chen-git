using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.Common.Models.ConfigModels;

/// <summary>
/// 工序统计显示对象
/// </summary>
public class ProcessRatioItem
{
   /// <summary>
   /// 工序名称
   /// </summary>
   public string Process { get; set; } = string.Empty;

   public Visibility Visibility { get; set; } = Visibility.Visible;

   /// <summary>
   /// OK数量
   /// </summary>
   public int OkTotal { get; set; }

   /// <summary>
   /// NG数量
   /// </summary>
   public int NgTotal { get; set; }

   /// <summary>
   /// 总数量
   /// </summary>
   public int TotalCount { get; set; }

   /// <summary>
   /// OK占比
   /// </summary>
   public double OkRatio { get; set; }

   /// <summary>
   /// NG占比
   /// </summary>
   public double NgRatio { get; set; }

   /// <summary>
   /// UI绑定细分NG
   /// </summary>
   public ObservableCollection<RatioDetailItem> NgDetails { get; } = [];

   /// <summary>
   /// 运行时快速查找字典
   /// Key=ResultTypeEnum
   /// Value=细分统计项
   /// </summary>
   public Dictionary<ResultTypeEnum, RatioDetailItem> NgDetailMap { get; } = [];

   /// <summary>
   /// 重置统计
   /// </summary>
   public void Reset()
   {
      OkTotal = 0;
      NgTotal = 0;
      TotalCount = 0;

      foreach (var item in NgDetails)
      {
         item.Count = 0;
         item.Ratio = 0;
      }
   }
}

/// <summary>
/// 细分NG统计项
/// </summary>
public class RatioDetailItem
{
   /// <summary>
   /// 显示名称
   /// </summary>
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// 对应结果
   /// </summary>
   public ResultTypeEnum Result { get; set; }

   /// <summary>
   /// 数量
   /// </summary>
   public int Count { get; set; }

   /// <summary>
   /// 百分比
   /// </summary>
   public double Ratio { get; set; }
}

/// <summary>
/// 工序统计规则
/// </summary>
public class ProcessRatioRule
{
   /// <summary>
   /// 工序名称
   /// </summary>
   public string Process { get; }

   /// <summary>
   /// 时间字段
   /// </summary>
   public string TimeName { get; }

   /// <summary>
   /// 结果字段
   /// </summary>
   public string ResultName { get; }

   /// <summary>
   /// 前置结果字段
   /// </summary>
   public string? PreResultName { get; }

   /// <summary>
   /// 统计结果对象
   /// </summary>
   public ProcessRatioItem ProcessRatio { get; }

   public ProcessRatioRule(string process, string resultName, string timeName, ProcessRatioItem processRatio, string? preResultName = null)
   {
      Process = process;
      ResultName = resultName;
      TimeName = timeName;
      PreResultName = preResultName;
      ProcessRatio = processRatio;
   }
}
