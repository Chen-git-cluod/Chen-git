using System.IO;
using Kinlo.SharedBase.Model;

namespace Kinlo.Common.Dto;

/// <summary>
/// 查询电池时的筛选条件
/// </summary>
public class QueryFilterDto
{
   /// <summary>
   /// 是否去重
   /// </summary>
   public bool IsNotRepeat { get; set; } = false;

   /// <summary>
   /// 时间筛选
   /// </summary>
   public TimeFilterDto TimeFilter { get; set; }

   /// <summary>
   /// 条码筛选
   /// </summary>
   public BarcodeFilterDto? BarcodeFilter { get; set; }

   /// <summary>
   /// 自定义时筛选结果
   /// </summary>
   public ResultFilterDto ResultFilter { get; set; }

   public QueryFilterDto(TimeFilterDto timeFilter, ResultFilterDto resultFilter)
   {
      TimeFilter = timeFilter;
      ResultFilter = resultFilter;
   }
}

/// <summary>
/// 条码筛选
/// </summary>
public class BarcodeFilterDto
{
   /// <summary>
   /// 查询条码
   /// </summary>
   public HashSet<string> Barcodes { get; set; } = [];

   /// <summary>
   /// 模糊查询条码
   /// </summary>
   public bool IsFuzzyQuery { get; set; }
}

/// <summary>
/// 时间筛选
/// </summary>
public class TimeFilterDto
{
   /// <summary>
   /// 是否按ID查询 ，一般只有进站是按ID查询 ，其它都按实时时间查询
   /// </summary>
   public bool IsQueryById { get; set; }
   public string PropertyName { get; set; } = string.Empty;

   /// <summary>
   /// 开始时间
   /// </summary>
   public DateTime StartTime { get; set; }

   /// <summary>
   /// 结束时间
   /// </summary>
   public DateTime EndTime { get; set; }
}

/// <summary>
/// 查询数库时的结果筛选条件
/// </summary>
public class ResultFilterDto
{
   /// <summary>
   /// 结果筛选模式
   /// </summary>
   public ResultFilterMode FilterMode { get; set; }

   /// <summary>
   /// 自定义时筛选结果
   /// </summary>
   public List<ProcessResultFilterItemDto> ResultFilters { get; set; } = [];

   public ResultFilterDto(ResultFilterMode filterMode)
   {
      FilterMode = filterMode;
   }
}

public class ProcessResultFilterItemDto
{
   public string PropertyName { get; set; } = string.Empty;

   /// <summary>
   /// 当前结果筛选状态， 空集合为不限制，
   /// 不要有区域重叠，后续直接生成sql，
   /// 所以要先处理区域重叠
   /// </summary>
   public List<ResultRange> ResultRanges { get; set; } = [];
}

public enum ResultFilterMode
{
   All, //所有
   Passed, //只看成功
   Failed, //只看失败
   Custom, //自定义
}

/// <summary>
/// 属性名和UI的映射
/// </summary>
public class TimePropertyDisplayMapModel
{
   /// <summary>
   /// 是否按ID查询 ，一般只有进站是按ID查询 ，其它都按实时时间查询
   /// </summary>
   public bool IsQueryById { get; set; }

   /// <summary>
   /// UI显示
   /// </summary>
   public string Display { get; set; } = string.Empty;
   public string PropertyName { get; set; } = string.Empty;
}
