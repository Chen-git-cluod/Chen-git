namespace Kinlo.GUI.Models;

[AddINotifyPropertyChangedInterface]
public class StatisticsModel
{
   public ProcessTypeEnum Process { get; set; }

   /// <summary>
   /// 用作显示的名称
   /// </summary>
   public string Display { get; set; } = string.Empty;

   /// <summary>
   /// 结果对应字段，用来查询数据库时的字段名
   /// </summary>
   public string ResultField { get; set; } = string.Empty;

   /// <summary>
   /// 位置对应字段，用来查询数据库时的字段名
   /// </summary>
   public ObservableCollection<FieldInfoModel> Fields { get; set; } = new();
}

[AddINotifyPropertyChangedInterface]
public class FieldInfoModel
{
   /// <summary>
   /// 用作显示的名称
   /// </summary>
   public string Display { get; set; } = string.Empty;

   /// <summary>
   /// 数据库中对应的字段名
   /// </summary>
   public string FieldName { get; set; } = string.Empty;
}

[AddINotifyPropertyChangedInterface]
public class StatisticsDataItemModel
{
   public string Name { get; set; } = string.Empty;
   public int Row { get; set; }
   public int Column { get; set; }
   public int OK { get; set; }
   public int NG { get; set; }
   private int _total;

   public int Total
   {
      get { return _total; }
      set
      {
         if (_total != value)
         {
            _total = value;
            OKProportion = (float)Math.Round((value == 0 ? 1 : OK / (value * 1.0f)), 2);
            NGProportion = (float)Math.Round((value == 0 ? 0 : NG / (value * 0.1f)), 2);
         }
      }
   }

   public float OKProportion { get; set; } = 1f;
   public float NGProportion { get; set; }
   public QeryType Type { get; set; }
   public List<InjectDisplay> Datas { get; set; } = new();
}

public class InjectDisplay
{
   /// <summary>
   /// 行号
   /// </summary>
   public byte LineIndex { get; set; }

   /// <summary>
   /// 列号
   /// </summary>
   public byte ColumnIndex { get; set; }
   public string Barcode { get; set; } = string.Empty;

   /// <summary>
   /// 首注结果
   /// </summary>
   public ResultTypeEnum FirstInjectResult { get; set; }

   /// <summary>
   /// 测漏结果
   /// </summary>
   public ResultTypeEnum LeakResult { get; set; }

   /// <summary>
   /// 托盘号
   /// </summary>
   public string TrayCode { get; set; } = string.Empty;

   /// <summary>
   /// 套杯号
   /// </summary>
   public string CupCode { get; set; } = string.Empty;

   /// <summary>
   /// 首注目标注液量
   /// </summary>
   public double TargetInjectionVolume { get; set; }

   /// <summary>
   /// 首注实注量
   /// </summary>
   public double ActualInjectionVolume { get; set; }

   /// <summary>
   /// 首注偏差
   /// </summary>
   public double TargetInjectionVolumeDeviation { get; set; }
}
