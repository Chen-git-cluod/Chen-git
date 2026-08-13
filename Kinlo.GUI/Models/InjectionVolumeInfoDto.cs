namespace Kinlo.GUI.Models;

[AddINotifyPropertyChangedInterface]
public class InjectionVolumeInfoDto
{
   static Color _okColor = Color.FromRgb(45, 184, 77);
   static Color _underInjectColor = Color.FromRgb(255, 165, 0);
   static Color _overInjectColor = Color.FromRgb(255, 0, 0);

   public SolidColorBrush OKBrushe { get; set; } = new SolidColorBrush(_okColor);
   public SolidColorBrush UnderInjectBrushe { get; set; } = new SolidColorBrush(_underInjectColor);
   public SolidColorBrush OverInjectBrushe { get; set; } = new SolidColorBrush(_overInjectColor);

   public int OkInjectCount { get; set; }

   /// <summary>
   /// 注液过少数量
   /// </summary>
   public int UnderInjectCount { get; set; }

   /// <summary>
   /// 注液过多数量
   /// </summary>
   public int OverInjectCount { get; set; }
   public int TotalCount { get; set; }

   public double OkRatio { get; set; }
   public double UnderInjectRatio { get; set; }
   public double OverRatio { get; set; }

   public ObservableCollection<ISeries> InjectSeries { get; set; }

   double _innerRadius,
      _maxRadialColumnWidth;

   public InjectionVolumeInfoDto(double innerRadius, double maxRadialColumnWidth)
   {
      _innerRadius = innerRadius;
      _maxRadialColumnWidth = maxRadialColumnWidth;
      InjectSeries = GetInjectChart(innerRadius, maxRadialColumnWidth);
   }

   public void Reset()
   {
      OkInjectCount = UnderInjectCount = OverInjectCount = TotalCount = 0;

      OkRatio = UnderInjectRatio = OverRatio = 0;

      InjectSeries = GetInjectChart(_innerRadius, _maxRadialColumnWidth);
   }

   private ObservableCollection<ISeries> GetInjectChart(double innerRadius, double maxRadialColumnWidth)
   {
      return new ObservableCollection<ISeries>
      {
         new PieSeries<double>
         {
            Values = new ObservableCollection<double> { 100 },
            //OuterRadiusOffset = outerRadiusOffset,//外半径偏移属性
            // InnerRadius = innerRadius,//内半径属性
            MaxRadialColumnWidth = maxRadialColumnWidth,
            ToolTipLabelFormatter = point => $"注液合格 {point.AsDataLabel}%",
            Fill = new SolidColorPaint(new SKColor(_okColor.R, _okColor.G, _okColor.B)),
         },
         new PieSeries<double>
         {
            Values = new ObservableCollection<double> { 0 },
            DataLabelsSize = 0,
            // OuterRadiusOffset = outerRadiusOffset,//外半径偏移属性
            MaxRadialColumnWidth = maxRadialColumnWidth,
            // InnerRadius = innerRadius,//内半径属性
            ToolTipLabelFormatter = point => $"注液过少 {point.AsDataLabel}%",
            Fill = new SolidColorPaint(new SKColor(_underInjectColor.R, _underInjectColor.G, _underInjectColor.B)),
         },
         new PieSeries<double>
         {
            Values = new ObservableCollection<double> { 0 },
            DataLabelsSize = 0,
            //InnerRadius = innerRadius, //内半径属性
            MaxRadialColumnWidth = maxRadialColumnWidth,
            // OuterRadiusOffset = outerRadiusOffset,//外半径偏移属性
            ToolTipLabelFormatter = point => $"注液过多 {point.AsDataLabel}%",
            Fill = new SolidColorPaint(new SKColor(_overInjectColor.R, _overInjectColor.G, _overInjectColor.B)),
         },
      };
   }
}
