using LiveChartsCore.SkiaSharpView.Painting;

namespace Kinlo.GUI.View
{
   /// <summary>
   /// ProcessesStatisticsView.xaml 的交互逻辑
   /// </summary>
   public partial class PercentChartView : UserControl
   {
      /// <summary>
      /// 百分比合格率
      /// </summary>
      LineSeries<double> _percentColumn;
      ColumnSeries<double> _okColumn;
      ColumnSeries<double> _ngColumn;
      Axis _yAxisPercent; //百分比Y轴
      Axis _yAxisCount; //数量Y轴
      Axis _xAxis;
      public ISeries[] Series { get; set; }
      public Axis[] YAxes { get; set; }
      public Axis[] XAxes { get; set; }

      public PercentChartView()
      {
         InitializeComponent();
         this.DataContext = this;
         LiveCharts.Configure(config => config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')));

         #region 初始化
         _percentColumn = new LineSeries<double>()
         {
            Values = [6, 3, 5, 7, 3, 4, 6, 3],
            //MaxBarWidth = 40,
            //IgnoresBarPosition = true,
            ScalesYAt = 1,
            Stroke = new SolidColorPaint(SKColor.Parse("#006CBE"), 1),
            GeometryFill = new SolidColorPaint(SKColor.Parse("#006CBE")),
            GeometryStroke = new SolidColorPaint(SKColor.Parse("#006CBE"), 3),
            LineSmoothness = 0, //LineSmoothness 属性 0 是直线，大于0是最曲线
            GeometrySize = 3,
            // MiniatureShapeSize = 16,
            Fill = null,

            Name = "合格率",
         };
         _okColumn = new ColumnSeries<double>()
         {
            Values = [2, 4, 8, 9, 5, 2, 4, 7],
            Stroke = new SolidColorPaint(SKColor.Parse("#4BC0C0"), 1),
            Fill = new SolidColorPaint(SKColor.Parse("#A5DFDF")),
            MaxBarWidth = 40,
            IgnoresBarPosition = true,
            Name = "OK",
         };
         _ngColumn = new ColumnSeries<double>()
         {
            Values = [3, 3, 5, 6, 3, 4, 5, 3],
            Stroke = new SolidColorPaint(SKColor.Parse("#FF6384"), 1),
            Fill = new SolidColorPaint(SKColor.Parse("#FFB1C1")),
            MaxBarWidth = 20,
            IgnoresBarPosition = true,
            Name = "ng",
         };
         _yAxisCount = new Axis //数量Y轴
         {
            Name = "数量",
            NameTextSize = 15,
            MinLimit = 0,
            //  ForceStepToMin = true,
            Position = LiveChartsCore.Measure.AxisPosition.End,
            SeparatorsPaint = null,
            //SeparatorsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint
            //{
            //    Color = SkiaSharp.SKColors.LightGray,
            //    StrokeThickness = 1
            //},
         };
         _xAxis = new Axis //X轴
         {
            Name = "工序序号",
            //  NamePaint = new SolidColorPaint(SKColor.Parse("#006CBE")),
            TextSize = 13,
            NameTextSize = 15,
            LabelsRotation = -30,
            SeparatorsPaint = new SolidColorPaint { Color = SKColors.LightGray, StrokeThickness = 1 },
         };
         _yAxisPercent = new Axis //百分比Y轴
         {
            Name = "百分比",
            NameTextSize = 15,
            // LabelsRotation = 45,
            MinLimit = 0,
            MaxLimit = 105,
            ForceStepToMin = true,
            MinStep = 10,
            SeparatorsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint { Color = SKColors.LightGray, StrokeThickness = 1 },
         };
         Series = [_okColumn, _ngColumn, _percentColumn];
         YAxes = [_yAxisCount, _yAxisPercent];
         XAxes = [_xAxis];
         #endregion
      }

      /// <summary>
      /// 更新数据
      /// </summary>
      /// <param name="oks"></param>
      /// <param name="ngs"></param>
      public void UpdateChartData(double[] oks, double[] ngs, string[] lables, string processName)
      {
         _okColumn.Values = oks;
         _ngColumn.Values = ngs;
         _percentColumn.Values = oks.Select(
               (x, i) =>
               {
                  if (i >= ngs.Length)
                     return 0;
                  var total = x + ngs[i];
                  if (total <= 0)
                     return 100;
                  return Math.Round(x / total * 100, 2);
               }
            )
            .ToArray();
         //var _max = Math.Max(oks.Max(), ngs.Max());

         //// 计算每个刻度的值（向上取整）,最后为正好10步
         //_max = Math.Ceiling(_max / 10.0) * 10;
         //_yAxisCount.MaxLimit = _max;
         //_yAxisCount.MinStep = _max / 10;
         _xAxis.Name = processName;
         _xAxis.Labels = lables;
      }
   }
}
