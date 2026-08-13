using Kinlo.SharedBase.Model;

namespace Kinlo.GUI.Models;

/// <summary>
/// 查询筛选条件（UI层）
/// </summary>
[AddINotifyPropertyChangedInterface]
public class ResultQueryFilterModel
{
   /// <summary>
   /// 是否去重
   /// </summary>
   public bool IsNotRepeat { get; set; } = false;

   /// <summary>
   /// 时间筛选
   /// </summary>
   public TimeFilterModel TimeFilter { get; set; }

   /// <summary>
   /// 条码筛选
   /// </summary>
   public BarcodeFilterModel BarcodeFilter { get; set; }

   /// <summary>
   /// 结果筛选
   /// </summary>
   public ResultFilterModel ResultFilter { get; set; }

   public ResultQueryFilterModel(TimeFilterModel timeFilter, ResultFilterModel resultFilter)
   {
      TimeFilter = timeFilter;
      BarcodeFilter = new BarcodeFilterModel();
      ResultFilter = resultFilter;
   }

   /// <summary>
   /// 基础校验
   /// </summary>
   public bool Validate(out string msg)
   {
      StringBuilder sb = new();

      if (TimeFilter.StartTime >= TimeFilter.EndTime)
      {
         sb.Append($"开始时间 {TimeFilter.StartTime} 不能大于或等于结束时间 {TimeFilter.EndTime}；\r\n");
      }

      if (BarcodeFilter.IsFuzzyQuery)
      {
         var barcodes = BarcodeFilter.Barcode.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

         if (barcodes.Length > 1)
         {
            sb.Append("模糊查询条码时，条码个数不能大于1；\r\n");
         }
      }

      msg = sb.ToString();
      return msg.Length == 0;
   }
}

#region Result Filter

/// <summary>
/// 结果筛选（UI层模式）
/// </summary>
[AddINotifyPropertyChangedInterface]
public class ResultFilterModel
{
   public bool ShowAll { get; set; } = true;
   public bool ShowPassed { get; set; }
   public bool ShowFailed { get; set; }

   private bool _showCustom;

   /// <summary>
   /// 是否自定义筛选
   /// </summary>
   public bool ShowCustom
   {
      get => _showCustom;
      set
      {
         if (_showCustom == value)
            return;

         _showCustom = value;

         // 关闭自定义时清空选择
         if (!value)
         {
            foreach (var group in FilterGroups)
            {
               foreach (var item in group.Items)
               {
                  item.IsSelected = false;
               }
            }
         }
      }
   }

   /// <summary>
   /// 自定义筛选组
   /// </summary>
   public List<ProcessFilterGroup> FilterGroups { get; set; } = [];

   /// <summary>
   /// 当前模式（用于转换DTO）
   /// </summary>
   public ResultFilterMode FilterMode
   {
      get
      {
         if (ShowPassed)
            return ResultFilterMode.Passed;
         if (ShowFailed)
            return ResultFilterMode.Failed;
         if (ShowCustom)
            return ResultFilterMode.Custom;
         return ResultFilterMode.All;
      }
   }
}

/// <summary>
/// 筛选分组（按工序/模块）
/// </summary>
public class ProcessFilterGroup
{
   public string GroupName { get; set; } = string.Empty;

   public List<ProcessFilterCondition> Items { get; set; } = [];

   public ProcessFilterGroup(string groupName)
   {
      GroupName = groupName;
   }
}

/// <summary>
/// 筛选条件（单个工序/字段条件）
/// </summary>
[AddINotifyPropertyChangedInterface]
public class ProcessFilterCondition
{
   public ResultFilterModel Parent { get; set; }

   /// <summary>
   /// 字段名
   /// </summary>
   public string PropertyName { get; set; } = string.Empty;

   /// <summary>
   /// UI显示
   /// </summary>
   public string Display { get; set; } = string.Empty;

   /// <summary>
   /// UI显示修饰符（不进入多语言字典体系）
   /// 用于在基础显示文本后追加状态/语义信息，例如：NG / OK / 异常等
   /// </summary>
   public string DisplaySuffix { get; set; } = string.Empty;

   /// <summary>
   /// 结果的区间值，如果是要等于就min及max都是同一个值
   /// </summary>
   public ResultRange Range { get; set; }

   private bool _isSelected;

   public bool IsSelected
   {
      get => _isSelected;
      set
      {
         if (_isSelected == value)
            return;

         _isSelected = value;

         // 选中任意条件 → 自动进入自定义模式
         if (value && !Parent.ShowCustom)
         {
            Parent.ShowCustom = true;
         }
      }
   }

   public ProcessFilterCondition(
      ResultFilterModel parent,
      string propertyName,
      string display,
      ResultRange range,
      string displaySuffix = ""
   )
   {
      Parent = parent;
      PropertyName = propertyName;
      Display = display;
      Range = range;
      DisplaySuffix = displaySuffix;
   }
}

#endregion

#region Time

/// <summary>
/// 时间筛选
/// </summary>
[AddINotifyPropertyChangedInterface]
public class TimeFilterModel
{
   public TimePropertyDisplayMapModel PropertyDisplayMap { get; set; } = new TimePropertyDisplayMapModel();

   public DateTime StartTime { get; set; }

   public DateTime EndTime { get; set; }
}

#endregion

#region Barcode

/// <summary>
/// 条码筛选
/// </summary>
[AddINotifyPropertyChangedInterface]
public class BarcodeFilterModel
{
   public bool IsFuzzyQuery { get; set; }

   private string _barcode = string.Empty;

   public string Barcode
   {
      get => _barcode;
      set
      {
         if (_barcode == value)
            return;

         var lines = new List<string>();

         using var reader = new StringReader(value ?? string.Empty);
         string? line;

         while ((line = reader.ReadLine()) != null)
         {
            if (!string.IsNullOrWhiteSpace(line))
               lines.Add(line.Trim());
         }

         _barcode = string.Join(',', lines);
      }
   }
}

#endregion
