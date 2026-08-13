using Kinlo.SharedBase.Model;

namespace Kinlo.GUI.Helpers;

public static class Common
{
   public static SolidColorPaint TextPaint { get; set; } =
      new SolidColorPaint() { Color = SKColors.DarkSlateGray, SKTypeface = SKFontManager.Default.MatchCharacter('汉') };

   /// <summary>
   /// UI筛选条件转数据查询筛选条件
   /// 结果筛选部分 已合并区间及去重，在sql时不需要再处理
   /// </summary>
   public static QueryFilterDto ToQueryFilter(this ResultQueryFilterModel filter)
   {
      //  时间条件
      var timeFilterDto = new TimeFilterDto
      {
         StartTime = filter.TimeFilter.StartTime,
         EndTime = filter.TimeFilter.EndTime,
         IsQueryById = filter.TimeFilter.PropertyDisplayMap.IsQueryById,
         PropertyName = filter.TimeFilter.PropertyDisplayMap.PropertyName,
      };

      // 结果条件
      var resultFilterDto = new ResultFilterDto(filter.ResultFilter.FilterMode);

      if (filter.ResultFilter.FilterMode == ResultFilterMode.Custom)
      {
         var filterItems = filter.ResultFilter.FilterGroups.ToProcessResultFilterItems();

         if (filterItems.Count > 0)
            resultFilterDto.ResultFilters.AddRange(filterItems);
      }

      var queryFilterDto = new QueryFilterDto(timeFilterDto, resultFilterDto) { IsNotRepeat = filter.IsNotRepeat };

      // 条码条件
      if (!string.IsNullOrWhiteSpace(filter.BarcodeFilter.Barcode))
      {
         queryFilterDto.BarcodeFilter = new BarcodeFilterDto
         {
            IsFuzzyQuery = filter.BarcodeFilter.IsFuzzyQuery,
            Barcodes = filter
               .BarcodeFilter.Barcode.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               .Select(x => x.Trim())
               .ToHashSet(),
         };
      }

      return queryFilterDto;
   }

   /// <summary>
   /// 结果筛选转换
   /// </summary>
   /// <param name="filterGroups"></param>
   /// <returns></returns>
   private static List<ProcessResultFilterItemDto> ToProcessResultFilterItems(this IEnumerable<ProcessFilterGroup> filterGroups)
   {
      if (filterGroups == null)
         return [];

      var selected = filterGroups.SelectMany(x => x.Items.Where(y => y.IsSelected)).ToArray();

      if (selected.Length == 0)
         return [];

      return selected
         .GroupBy(x => x.PropertyName)
         .Select(g => new ProcessResultFilterItemDto { PropertyName = g.Key, ResultRanges = g.MergeAreas() })
         .ToList();
   }

   private static List<ResultRange> MergeAreas(this IGrouping<string, ProcessFilterCondition> groups)
   {
      var ranges = groups.Select(x => x.Range).ToArray();

      if (ranges.Length <= 1)
         return ranges.Length == 0 ? [] : [ranges[0]];

      return MergeAreas(ranges);
   }

   /// <summary>
   /// 合并整数区间（相邻区间也会合并）
   /// </summary>
   /// <param name="areas">待合并区间</param>
   /// <returns>合并后的区间</returns>
   private static List<ResultRange> MergeAreas(ResultRange[] areas)
   {
      if (areas.Length == 0)
         return [];

      var ordered = areas.OrderBy(x => x.Min).ThenBy(x => x.Max).ToArray();

      var result = new List<ResultRange>();
      var current = ordered[0];

      for (int i = 1; i < ordered.Length; i++)
      {
         var next = ordered[i];

         if (current.CanMerge(next))
         {
            current = current.Merge(next);
         }
         else
         {
            result.Add(current);
            current = next;
         }
      }

      result.Add(current);
      return result;
   }
}
