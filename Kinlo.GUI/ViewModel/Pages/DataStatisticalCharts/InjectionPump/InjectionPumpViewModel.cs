using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[Languages(["注液泵信息", "注液泵信息", "Inject pump info"], IsScanProperty = false)]
[UIDisplay(true)]
public class InjectionPumpViewModel : Screen
{
   private IContainer _container;
   private ISqlSugarDbFactory _dbFactory;
   public DateTime StartTime { get; set; }
   public DateTime EndTime { get; set; }
   public int DisyplayMax { get; set; } = 500;
   public List<int> Pumps { get; set; }
   public int SelectPumpIndex { get; set; } = 0;

   public ObservableCollection<InjectPumpModel> InjectPumps { get; set; }

   public InjectionPumpViewModel(IContainer container)
   {
      _container = container;
      var now = DateTime.Now;
      StartTime = now.AddDays(-1);
      EndTime = now;
      Pumps = Enumerable.Range(1, 16).ToList();
      _dbFactory = _container.Get<ISqlSugarDbFactory>();
      InjectPumps = new();
   }

   protected override void OnViewLoaded()
   {
      base.OnViewLoaded();
      if (Pumps.Count > 0 && this.View is InjectionPumpView view)
         view.pumpsCbox.SelectedItems.Add(Pumps[0]);
   }

   /// <summary>
   /// 选择曲线
   /// </summary>
   /// <param name="seriesOption"></param>
   public void SelectCurveCmd(ChartSeriesOption seriesOption)
   {
      if (seriesOption.InjectChart.SeriesOptions.Count(x => x.IsChecked) == 0)
      {
         Growl.Warning("最少需保留一项！");
         seriesOption.IsChecked = !seriesOption.IsChecked;
         return;
      }
      CreateLine(seriesOption.InjectChart.Datas, seriesOption.InjectChart);
   }

   /// <summary>
   /// 查询
   /// </summary>
   /// <param name="pumpsCbox"></param>
   /// <returns></returns>
   public async Task QueryAsync(CheckComboBox pumpsCbox)
   {
      try
      {
         if (DisyplayMax > 5000)
         {
            Growl.Warning("显示数量请不要大于5000！");
            return;
         }
         var pumps = pumpsCbox.SelectedItems.OfType<int>().ToList();
         if (pumps.Count == 0)
         {
            Growl.Warning("请选择泵号！");
            return;
         }

         var data = await GetInjectFromDbAsync(pumps);
         if (data == null)
         {
            Growl.Warning("该条件下无数据！");
            return;
         }
         var group = data.GroupBy(x => x.StationNo).OrderBy(x => x.Key).Select(y => y.Select(z => z).ToList()).ToList();

         StringBuilder stringBuilder = new StringBuilder();

         foreach (var item in group)
         {
            var displayData = new List<InjectionDataModel>();
            if (item.Count == 0)
               continue;

            if (item.Count > DisyplayMax)
            {
               stringBuilder.AppendLine($"[{item[0].StationNo}]号泵数量超出显示数量[{DisyplayMax}]，将裁剪数据！");
               displayData = item[^DisyplayMax..]; //直接获取最后 DisyplayMax 个元素}
            }
            else
            {
               displayData = item;
            }
            CreateSeries(displayData);
         }
         SelectPumpIndex = 0;
         if (stringBuilder.Length > 0)
         {
            Growl.Warning(stringBuilder.ToString());
         }
      }
      catch (Exception)
      {
         throw;
      }
   }

   /// <summary>
   /// 从数据库取数据
   /// </summary>
   /// <returns></returns>
   private async Task<List<InjectionDataModel>?> GetInjectFromDbAsync(IList<int> pumps) =>
      await _dbFactory.UsingDbAsync(
         DatabaseRole.LocalDb1,
         async db =>
         {
            try
            {
               var monthCount = StartTime.GetMonthCount(EndTime);
               List<ISugarQueryable<InjectionDataModel>> methods = new List<ISugarQueryable<InjectionDataModel>>();
               for (int i = 0; i < monthCount; i++)
               {
                  var tableName = db.SplitHelper<InjectionDataModel>().GetTableName(EndTime.AddMonths(-i)); //根据时间获取表名
                  if (!db.DbMaintenance.IsAnyTable(tableName, false))
                  {
                     $"[查询注液泵数据]无此表[{tableName}]".LogRun(Log4NetLevelEnum.信息);
                     continue;
                  }

                  var method = db.Queryable<InjectionDataModel>()
                     .AS(tableName)
                     .Where(x => x.InjectionTime >= StartTime && x.InjectionTime < EndTime && pumps.Contains(x.StationNo));
                  methods.Add(method);
               }
               if (methods.Count < 1)
                  return null;
               // 合并所有 Query
               var unionQuery = db.UnionAll(methods);
               var result = await unionQuery.ToListAsync();
               return result;
            }
            catch (Exception ex)
            {
               $"[查询注液泵数据]异常：{ex}".LogRun(Log4NetLevelEnum.警告, true);
               return null;
            }
         }
      );

   private void CreateSeries(List<InjectionDataModel> datas)
   {
      var pumpIndex = datas[0].StationNo;

      var pump = InjectPumps.FirstOrDefault(x => x.PumpNo == pumpIndex);
      if (pump == null)
      {
         pump = new InjectPumpModel();
         pump.PumpNo = pumpIndex;
         InjectPumps.Add(pump);
      }

      if (pump.Charts.Count < 2)
      {
         pump.Charts.Clear();
         pump.Charts.Add(new InjectChartModel(0) { Id = 1 });
         pump.Charts.Add(new InjectChartModel(1) { Id = 2 });
      }

      foreach (var item in pump.Charts)
      {
         CreateLine(datas, item);
      }
   }

   SkiaSharp.SKColor _red = new SkiaSharp.SKColor(229, 57, 53);
   SkiaSharp.SKColor _gree = new SkiaSharp.SKColor(0x00, 0xEC, 0x00);

   /// <summary>
   /// 曲线颜色
   /// </summary>
   private readonly Dictionary<ChartType, SKColor> ChartColor = new()
   {
      { ChartType.温度, new(0x00, 0x6C, 0xBE) }, //深蓝
      { ChartType.注液量, new(0xE6, 0x7E, 0x22) }, //橙色
      { ChartType.目标注液量, new(0x27, 0xAE, 0x60) }, //绿色
      { ChartType.温度补偿, new(0xE7, 0x4C, 0x3C) }, //红色
      { ChartType.工艺补偿, new(0x8E, 0x44, 0xAD) }, //紫色
   };

   /// <summary>
   /// 曲线数据映射
   /// </summary>
   private readonly Dictionary<ChartType, Func<InjectionDataModel, int, Coordinate>> ChartMap = new()
   {
      { ChartType.温度, (model, index) => new Coordinate(index, model.Temperature) },
      { ChartType.注液量, (model, index) => new Coordinate(index, model.InjectionValue) },
      { ChartType.目标注液量, (model, index) => new Coordinate(index, model.TargetInjectionVolume) },
      { ChartType.温度补偿, (model, index) => new Coordinate(index, model.TempComp) },
      { ChartType.工艺补偿, (model, index) => new Coordinate(index, model.ProcessComp) },
   };

   /// <summary>
   /// 获取Chart Tip
   /// </summary>
   /// <param name="type"></param>
   /// <param name="injectionData"></param>
   /// <returns></returns>
   private string GetChartTip(ChartType type, InjectionDataModel? injectionData)
   {
      if (injectionData == null)
         return "";
      return type switch
      {
         ChartType.温度 =>
            $"目标注液：{injectionData.TargetInjectionVolume}，实际注液：{injectionData.InjectionValue}，补偿模式：{injectionData.CompMode}，温度补偿：{injectionData.TempComp}，工艺补偿：{injectionData.ProcessComp}\r\n时间:{injectionData.InjectionTime:yy-MM-dd_HH:mm:ss}",
         ChartType.注液量 or ChartType.目标注液量 =>
            $"温度：{injectionData.Temperature}，补偿模式：{injectionData.CompMode}，温度补偿：{injectionData.TempComp}，工艺补偿：{injectionData.ProcessComp}\r\n时间:{injectionData.InjectionTime:yy-MM-dd_HH:mm:ss}",
         ChartType.温度补偿 or ChartType.工艺补偿 =>
            $"温度：{injectionData.Temperature}，补偿模式：{injectionData.CompMode}，目标注液：{injectionData.TargetInjectionVolume}，实际注液：{injectionData.InjectionValue}\r\n时间:{injectionData.InjectionTime:yy-MM-dd_HH:mm:ss}",
      };
   }

   private void CreateLine(List<InjectionDataModel> datas, InjectChartModel injectChart)
   {
      injectChart.Datas = datas;
      injectChart.ChatSeries.Clear();
      List<Axis> xAxes = new List<Axis>();
      List<Axis> yAxes = new List<Axis>();
      int yAt = 0; //对应Y轴索引
      foreach (var item in injectChart.SeriesOptions)
      {
         if (!item.IsChecked)
            continue;

         var injtionVol = new LineSeries<InjectionDataModel>
         {
            Values = datas,
            Mapping = ChartMap[item.Type],
            Stroke = new SolidColorPaint(ChartColor[item.Type], 1),
            GeometryFill = new SolidColorPaint(ChartColor[item.Type]),
            GeometryStroke = new SolidColorPaint(ChartColor[item.Type], 3),
            LineSmoothness = 0, //LineSmoothness 属性 0 是直线，大于0是最曲线
            GeometrySize = 3,
            MiniatureShapeSize = 6,
            Fill = null,
            Name = item.Type.ToString(),
            XToolTipLabelFormatter = (chartPoint) => $"注液时间：{chartPoint.Model.InjectionTime:yyyy/MM/dd_HH:mm:ss}",
            // XToolTipLabelFormatter = (chartPoint) => GetChartTip(item.Type, chartPoint.Model),
            ScalesYAt = yAt,
         };
         injectChart.ChatSeries.Add(injtionVol);
         var axis = GetAxis(datas, item.Type);

         if (xAxes.Count == 0)
            xAxes.Add(axis.x);
         yAxes.Add(axis.y);
         yAt++;
      }
      injectChart.XAxes = xAxes.ToArray();
      injectChart.YAxes = yAxes.ToArray();
   }

   private readonly Dictionary<ChartType, Func<InjectionDataModel, double>> keyValuePairs = new()
   {
      { ChartType.温度, (x) => x.Temperature },
      { ChartType.注液量, (x) => x.InjectionValue },
      { ChartType.目标注液量, (x) => x.TargetInjectionVolume },
      { ChartType.温度补偿, (x) => x.TempComp },
      { ChartType.工艺补偿, (x) => x.ProcessComp },
   };

   private (Axis x, Axis y) GetAxis(List<InjectionDataModel> injectionDatas, ChartType chartType)
   {
      var tatoalCount = injectionDatas.Count;
      double xMin = 0;
      double xMax = tatoalCount + 2;
      var yMin = injectionDatas.Min(keyValuePairs[chartType]) - 10;
      var yMax = injectionDatas.Max(keyValuePairs[chartType]) + 10;
      double xStep = Math.Round((xMax - xMin) / 30, 0);
      double yStep = Math.Round((yMax - yMin) / 8, 0);

      return (
         CreateAxis(xMin, xMax, xStep, 0, SKColors.DarkSlateGray, new SolidColorPaint { Color = SKColors.LightGray, StrokeThickness = 1 }),
         CreateAxis(yMin, yMax, yStep, 30, ChartColor[chartType], new SolidColorPaint { Color = SKColors.LightGray, StrokeThickness = 1 })
      );
   }

   private Axis CreateAxis(double min, double max, double step, double labelRotation, SKColor color, SolidColorPaint paint) =>
      new Axis
      {
         LabelsRotation = labelRotation,
         MinLimit = min,
         MaxLimit = max,
         ForceStepToMin = true,
         MinStep = step,
         Padding = new LiveChartsCore.Drawing.Padding(0),
         LabelsPaint = new SolidColorPaint { Color = color },
         SeparatorsPaint = paint,
         // SeparatorsPaint = new SolidColorPaint { Color = SkiaSharp.SKColors.LightGray, StrokeThickness = strokeThick },
      };
}

#region Models
/// <summary>
/// 每个泵的Model
/// </summary>
[AddINotifyPropertyChangedInterface]
public class InjectPumpModel
{
   public int PumpNo { get; set; }

   public ObservableCollection<InjectChartModel> Charts { get; set; } = new();
}

/// <summary>
/// 图表
/// </summary>
[AddINotifyPropertyChangedInterface]
public class InjectChartModel
{
   public int Id { get; set; }
   public List<InjectionDataModel> Datas { get; set; }
   public ObservableCollection<ChartSeriesOption> SeriesOptions { get; set; }

   //温度
   public Axis[] XAxes { get; set; } = [];
   public Axis[] YAxes { get; set; } = [];
   public ObservableCollection<ISeries> ChatSeries { get; set; } = new();

   public InjectChartModel(int index)
   {
      SeriesOptions = new ObservableCollection<ChartSeriesOption>();

      foreach (var item in Enum.GetValues<ChartType>())
      {
         var isChecked = (index, item) switch
         {
            var p when p.index == 0 && (p.item is ChartType.温度 or ChartType.温度补偿 or ChartType.工艺补偿) => true,
            var p when p.index == 1 && (p.item is ChartType.注液量 or ChartType.目标注液量) => true,
            _ => false,
         };

         SeriesOptions.Add(new ChartSeriesOption(isChecked, item, this));
      }
   }
}

/// <summary>
/// 曲线开关
/// </summary>
[AddINotifyPropertyChangedInterface]
public class ChartSeriesOption
{
   public bool IsChecked { get; set; } = true;
   public ChartType Type { get; set; }

   /// <summary>
   /// 引用父类
   /// </summary>
   public InjectChartModel InjectChart { get; set; }

   public ChartSeriesOption(bool isChecked, ChartType type, InjectChartModel injectChart)
   {
      IsChecked = isChecked;
      Type = type;
      InjectChart = injectChart;
   }
}

public enum ChartType
{
   温度,
   注液量,
   目标注液量,
   温度补偿,
   工艺补偿,
}
#endregion
