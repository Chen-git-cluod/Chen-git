using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[Languages(["注液CPK图表", "注液CPK图表", "Inject Chart"], IsScanProperty = false)]
[UIDisplay(true)]
public class InjectCPKChartViewModel : Screen
{
   public ObservableCollection<InjectionCpkDto> InjectionCpks { get; set; } = new();
   public List<BatchDirection> BatchDirections { get; set; } = Enum.GetValues<BatchDirection>().ToList();
   public BatchDirection SelectBatchDirection { get; set; } = BatchDirection.从后往前;
   public List<string> GroupTypes { get; set; } = ["注液针", "注液泵"];

   private string _selectedGroupType = "注液泵";

   public string SelectedGroupType
   {
      get { return _selectedGroupType; }
      set
      {
         if (_selectedGroupType != value)
         {
            _selectedGroupType = value;

            if (QueryData == null || QueryData.Count == 0)
               return;
            _ = Task.Run(async () => await GroupingAsync());
         }
      }
   }

   /// <summary>
   /// 开始时间
   /// </summary>
   public DateTime StartTime { get; set; }

   /// <summary>
   /// 结束时间
   /// </summary>
   public DateTime EndTime { get; set; }

   /// <summary>
   /// 上限
   /// </summary>
   public double UpperLimit { get; set; }

   /// <summary>
   /// 标准值
   /// </summary>
   public double StandardValue { get; set; }

   /// <summary>
   /// 下限
   /// </summary>
   public double LowerLimit { get; set; }

   /// <summary>
   /// 计算时的有效范围上限
   /// </summary>
   public double EffectiveUpper { get; set; }

   /// <summary>
   /// 计算时的有效范围下限
   /// </summary>
   public double EffectiveLower { get; set; }

   /// <summary>
   /// 批次
   /// </summary>
   public int BathNo { get; set; } = 1;

   /// <summary>
   /// 批次个数
   /// </summary>
   public int BathCount { get; set; } = 200;

   /// <summary>
   /// 查询出来的总数
   /// </summary>
   public int TotalCount { get; set; } = 0;

   public bool IsQueryFinish { get; set; } = true;

   private ParameterConfig _parameterConfig;
   private DbHelper _sugarDB;

   public InjectCPKChartViewModel(IContainer container)
   {
      _parameterConfig = container.Get<ParameterConfig>();
      StartTime = DateTime.Now.AddHours(-3);
      EndTime = DateTime.Now;

      _sugarDB = container.Get<DbHelper>();
      GetLocalConfigCMD();
      #region 加测试数据

      // InjectionDataModel injectionData = new InjectionDataModel();

      //injectionData.Id = container.Get<SnowflakeHelper>().NextId();
      //injectionData.Id = 41028145142304768;
      //injectionData.Temperature = 964.325;
      //injectionData.InjectionValue = 86.23;
      //_ = _sugarDB.InsertOrUpdateInjectionAsync(injectionData, true);

      //Task.Run(async () =>
      //{
      //    Random random = new Random();
      //    for (int k = 0; k < 500; k++)
      //    {
      //        for (int i = 1; i < 31; i++)
      //        {
      //            InjectionDataModel injectionData = new InjectionDataModel();

      //            injectionData.NeedleNo = (byte)i;
      //            injectionData.StationNo = (byte)(i % 4);
      //            injectionData.InjectionValue = random.Next(188000, 189000) / 100d;
      //            injectionData.Id = container.Get<SnowflakeHelper>().NextId();
      //            injectionData.Temperature = random.Next(2000, 3000) / 100d;
      //            injectionData.InjectionTime = DateTime.Now;
      //            await _sugarDB.InsertOrUpdateInjectionAsync(injectionData, true);
      //            // await _sugarDB.InsertOrUpdateInjectionAsync(injectionData, false);
      //        }
      //    }
      //});
      #endregion
   }

   public void GetLocalConfigCMD()
   {
      UpperLimit = _parameterConfig.RunParameter.InjectionUpper;
      LowerLimit = _parameterConfig.RunParameter.InjectionLower;
      StandardValue = _parameterConfig.RunParameter.InjectionStandard;

      EffectiveUpper = UpperLimit + 10;
      EffectiveLower = LowerLimit - 10;
   }

   private List<InjectionDataModel> QueryData = new();

   public void QueryCMD()
   {
      IsQueryFinish = false;
      int count = 0;
      _ = Task.Run(async () =>
      {
         try
         {
            QueryData = await _sugarDB.GetInjectionDataAsync(StartTime, EndTime);
            if (QueryData == null || QueryData.Count == 0)
            {
               count = 0;
               Growl.Warning("此时间段无数据！");
               return;
            }
            count = QueryData.Count;
            await GroupingAsync();
            await ApplyAsync();
         }
         catch (Exception ex)
         {
            count = 0;
            Growl.Warning($"[查询注液CPK]异常：{ex}");
         }
         finally
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(() =>
            {
               IsQueryFinish = true;
               TotalCount = count;
            });
         }
      });
   }

   public async Task ApplyCmd()
   {
      await ApplyAsync();
   }

   public async Task PreviousBatchCmd()
   {
      if (BathNo <= 1)
      {
         Growl.Warning("已是第一批！");
         return;
      }
      await UIThreadHelper.InvokeOnUiThreadAsync(() => --BathNo);
      await ApplyAsync();
   }

   public async Task NextBatchCmd()
   {
      var tatalCount = InjectionCpks.Max(x => x.ValidCount);
      if (BathNo * BathCount > tatalCount)
      {
         Growl.Warning("已是最后一批！");
         return;
      }

      await UIThreadHelper.InvokeOnUiThreadAsync(() => ++BathNo);
      await ApplyAsync();
   }

   public async Task FirstBatchCmd()
   {
      if (BathNo <= 1)
      {
         Growl.Warning("已是第一批！");
         return;
      }
      await UIThreadHelper.InvokeOnUiThreadAsync(() => BathNo = 1);
      await ApplyAsync();
   }

   public async Task LastBatchCmd()
   {
      var tatalCount = InjectionCpks.Max(x => x.ValidCount);
      if (BathNo * BathCount > tatalCount)
      {
         Growl.Warning("已是最后一批！");
         return;
      }
      var bathNo = Math.Ceiling(tatalCount * 1.0 / BathCount);

      await UIThreadHelper.InvokeOnUiThreadAsync(() => BathNo = (int)bathNo);
      await ApplyAsync();
   }

   private async Task GroupingAsync()
   {
      var injectGroup = SelectedGroupType switch
      {
         "注液针" => QueryData.Where(x => x.NeedleNo > 0).GroupBy(x => x.NeedleNo).ToList(),
         _ => QueryData.Where(x => x.StationNo > 0).GroupBy(x => x.StationNo).ToList(),
      };
      if (injectGroup.Count == 0)
      {
         Growl.Warning($"{QueryData.Count}条数据中没有符合条件数据！");
         return;
      }

      ObservableCollection<InjectionCpkDto> injectionCpks = new ObservableCollection<InjectionCpkDto>();
      foreach (var group in injectGroup)
      {
         var d = group.Select(y => y).ToArray();
         var validDatas = d.Where(x => x.InjectionValue >= EffectiveLower && x.InjectionValue < EffectiveUpper).ToArray();

         injectionCpks.Add(
            new InjectionCpkDto
            {
               SelectedGroupType = SelectedGroupType,
               InjectionNeedleNo = group.Key,
               ValidDatas = validDatas,
               ValidCount = validDatas.Length,
               InjectionDatas = d,
               TotalCount = d.Length,
            }
         );
      }

      await UIThreadHelper.InvokeOnUiThreadAsync(() => InjectionCpks = injectionCpks);
   }

   private async Task ApplyAsync()
   {
      await UIThreadHelper.InvokeOnUiThreadAsync(() =>
      {
         if (BathNo < 1)
            BathNo = 1;
         IsQueryFinish = false;
      });
      try
      {
         if (InjectionCpks == null || InjectionCpks.Count == 0)
         {
            return;
         }

         if (BathCount < 3)
         {
            Growl.Warning($"批次数量{BathCount}少于最低2个！");
            return;
         }
         foreach (var injCpk in InjectionCpks)
         {
            if (injCpk.ValidCount < 2)
            {
               Growl.Warning($"[{injCpk.InjectionNeedleNo}号{injCpk.SelectedGroupType}] 有效数据必须至少包含两个值");
               continue;
            }

            var skipCount = (BathNo - 1) * BathCount;

            InjectionDataModel[] datas = [];
            int count = 0;
            int start = 0;
            if (SelectBatchDirection == BatchDirection.从前往后)
            {
               datas = injCpk.ValidDatas.Skip(skipCount).Take(BathCount).ToArray();
               count = datas.Length;
               start = skipCount + 1;
            }
            else
            {
               datas = injCpk.ValidDatas.SkipLast(skipCount).TakeLast(BathCount).ToArray();
               count = datas.Length;
               start = injCpk.ValidCount - skipCount + 1;
               if (start < 1)
                  start = 1;
            }

            var axes = CreateAxis();
            var info = CalculateCpk(datas.Select(x => x.InjectionValue).ToArray(), LowerLimit, UpperLimit);
            var series = CreateSeries(datas);

            await UIThreadHelper.InvokeOnUiThreadAsync(() =>
            {
               injCpk.Count = count;
               injCpk.Start = start;
               injCpk.Average = info.Average;
               injCpk.Cpk = info.Cpk;
               injCpk.StdDev = info.StdDev;
               injCpk.YAxes = axes.y;
               injCpk.XAxes = axes.x;
               injCpk.ChatSeries = series;
            });
         }
      }
      catch (Exception ex)
      {
         Growl.Warning($"[查询注液CPK]异常：{ex}");
      }
      finally
      {
         await UIThreadHelper.InvokeOnUiThreadAsync(() => IsQueryFinish = true);
      }
   }

   SkiaSharp.SKColor _red = new SkiaSharp.SKColor(229, 57, 53);
   SkiaSharp.SKColor _gree = new SkiaSharp.SKColor(0x00, 0xEC, 0x00);

   private ObservableCollection<ISeries> CreateSeries(InjectionDataModel[] datas)
   {
      var lineSeriesUpper = new LineSeries<double>
      {
         Values = Enumerable.Repeat(UpperLimit, datas.Count()).ToArray(),
         Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(_red, 1),
         GeometryStroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(_red, 1),
         LineSmoothness = 0,
         GeometrySize = 0,
         Fill = null,
         Name = "上限",
      };
      var lineSeriesStd = new LineSeries<double>
      {
         Values = Enumerable.Repeat(StandardValue, datas.Count()).ToArray(),
         Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(_gree, 1),
         GeometryStroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(_gree, 1),
         LineSmoothness = 0,
         GeometrySize = 0,
         Fill = null,
         Name = "标准值",
      };
      var lineSeriesLower = new LineSeries<double>
      {
         Values = Enumerable.Repeat(LowerLimit, datas.Count()).ToArray(),
         Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(_red, 1),
         GeometryStroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(_red, 1),
         LineSmoothness = 0,
         GeometrySize = 0,
         Fill = null,
         Name = "下限",
      };

      var injtionVol = new LineSeries<InjectionDataModel>
      {
         Values = datas,
         Mapping = (model, index) => new Coordinate(index, model.InjectionValue),
         Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SKColor.Parse("#006CBE"), 1),
         GeometryFill = new SolidColorPaint(SKColor.Parse("#006CBE")),
         GeometryStroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SKColor.Parse("#006CBE"), 3),
         LineSmoothness = 0, //LineSmoothness 属性 0 是直线，大于0是最曲线
         GeometrySize = 3,
         MiniatureShapeSize = 6,
         Fill = null,
         Name = "注液量",
         XToolTipLabelFormatter = (chartPoint) =>
            $"温度:{chartPoint.Model.Temperature}  时间:{chartPoint.Model.InjectionTime:yy-MM-dd HH:mm:ss fff}",
      };

      ObservableCollection<ISeries> series = new ObservableCollection<ISeries>();
      series.Add(injtionVol);
      series.Add(lineSeriesUpper);
      series.Add(lineSeriesStd);
      series.Add(lineSeriesLower);
      return series;
   }

   private (Axis[] x, Axis[] y) CreateAxis()
   {
      var xAxes = new Axis
      {
         MinLimit = 0,
         MaxLimit = 100,
         ForceStepToMin = true,
         MinStep = 5,
         SeparatorsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint
         {
            Color = SkiaSharp.SKColors.LightGray,
            StrokeThickness = 1,
         },
      };
      var yAxes = new Axis
      {
         // LabelsRotation = 45,
         MinLimit = LowerLimit - 30,
         MaxLimit = UpperLimit + 30,
         ForceStepToMin = true,
         MinStep = 10,
         SeparatorsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint
         {
            Color = SkiaSharp.SKColors.LightGray,
            StrokeThickness = 1,
         },
      };
      return ([xAxes], [yAxes]);
   }

   public void ShowSingleChartCmd(InjectionCpkDto obj)
   {
      new InjectCPKChartSingleShowView(obj).Show();
   }

   public SolidColorPaint TextPaint { get; set; } =
      new SolidColorPaint() { Color = SKColors.DarkSlateGray, SKTypeface = SKFontManager.Default.MatchCharacter('汉') };

   /// <summary>
   ///
   /// </summary>
   /// <param name="data"></param>
   /// <param name="lsl">下限</param>
   /// <param name="usl">上限</param>
   /// <returns>CPK,平均值,标准差</returns>
   public static (double Cpk, double Average, double StdDev) CalculateCpk(double[] data, double lsl, double usl)
   {
      if (data == null || data.Length < 2)
      {
         Growl.Warning("样本数据必须至少包含两个值");
         return (0, 0, 0);
      }

      double mean = data.Average();
      double sumSquaredDiffs = data.Sum(x => Math.Pow(x - mean, 2));
      double stdDev = Math.Sqrt(sumSquaredDiffs / (data.Length - 1));

      if (stdDev == 0)
      {
         Growl.Warning("标准差为0，无法计算 Cpk。");
         return (0, 0, 0);
      }

      double cpkLower = (mean - lsl) / (3 * stdDev);
      double cpkUpper = (usl - mean) / (3 * stdDev);
      double cpk = Math.Min(cpkLower, cpkUpper);

      return (cpk, mean, stdDev);
   }
}

[AddINotifyPropertyChangedInterface]
public class InjectionCpkDto
{
   public string SelectedGroupType { get; set; } = "注液针";

   /// <summary>
   ///  注液针号
   /// </summary>
   public int InjectionNeedleNo { get; set; }

   /// <summary>
   /// 开始位置
   /// </summary>
   public int Start { get; set; }

   /// <summary>
   /// 统计时的个数
   /// </summary>
   public int Count { get; set; }

   /// <summary>
   /// 有效个数
   /// </summary>
   public int ValidCount { get; set; }

   /// <summary>
   /// 总数据个数
   /// </summary>
   public int TotalCount { get; set; }

   /// <summary>
   /// 有效果数据
   /// </summary>
   public InjectionDataModel[] ValidDatas { get; set; } = [];

   /// <summary>
   /// 原始数据
   /// </summary>
   public InjectionDataModel[] InjectionDatas { get; set; } = [];

   public Axis[]? XAxes { get; set; }
   public Axis[]? YAxes { get; set; }
   public ObservableCollection<ISeries> ChatSeries { get; set; } = new();

   /// <summary>
   /// cpk
   /// </summary>
   public double Cpk { get; set; }

   /// <summary>
   /// 平均值
   /// </summary>
   public double Average { get; set; }

   /// <summary>
   /// 标准差
   /// </summary>
   public double StdDev { get; set; }
}

public enum BatchDirection
{
   从后往前 = 1, // 从后往前
   从前往后 = 2, // 从前往后
}
