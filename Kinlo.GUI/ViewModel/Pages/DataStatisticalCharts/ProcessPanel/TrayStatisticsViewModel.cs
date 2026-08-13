using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(true)]
public class TrayStatisticsViewModel : Screen
{
   public bool IsQueryFinish { get; set; } = true;

   [Inject]
   public DbHelper SugarDb { get; set; }

   [Inject]
   public SnowflakeHelper Snowflake { get; set; }

   [Inject]
   public ParameterConfig Parameter { get; set; }

   /// <summary>
   /// 开始时间
   /// </summary>
   public DateTime StartTime { get; set; }

   /// <summary>
   /// 结束时间
   /// </summary>
   public DateTime EndTime { get; set; }

   /// <summary>
   /// 托盘列数
   /// </summary>
   public int Columns { get; set; }

   /// <summary>
   /// 托盘行数
   /// </summary>
   public int Rows { get; set; }
   public ObservableCollection<TrayIntItem> ColumnInfos { get; set; } = new();
   public ObservableCollection<TrayIntItem> RowInfos { get; set; } = new();
   public ObservableCollection<StatisticsDataItemModel> TrayDatas { get; set; } = new ObservableCollection<StatisticsDataItemModel>();
   public ObservableCollection<TrayStringItem> TrayNumbers { get; set; } = new();
   public ObservableCollection<TrayStringItem> CupNumbers { get; set; } = new();
   private IWindowManager _windowManager;

   public TrayStatisticsViewModel(IWindowManager windowManager)
   {
      _windowManager = windowManager;
      StartTime = DateTime.Now.AddHours(-8);
      EndTime = DateTime.Now;
      TrayNumbers.Insert(0, new TrayStringItem("全部"));
      CupNumbers.Insert(0, new TrayStringItem("全部"));
      _ = Task.Run(() =>
      {
         _ = UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            List<InjectDisplay> list = new();
            Random random = new Random();
            byte li = 1,
               ci = 1,
               ti = 1;
            for (int i = 0; i < 200; i++)
            {
               if (li >= 7)
                  li = 1;
               if (ci >= 13)
                  ci = 1;
               if (ti >= 19)
                  ti = 1;
               InjectDisplay obj = new();
               obj.LineIndex = li;
               obj.ColumnIndex = ci;
               obj.FirstInjectResult = i % (random.Next(1, 33)) != 0 ? ResultTypeEnum.OK : ResultTypeEnum.注液量偏多;
               obj.LeakResult = i % 4 == 0 ? ResultTypeEnum.OK : ResultTypeEnum.NG;
               obj.TrayCode = $"托盘{ti}";
               obj.CupCode = $"套杯{ti}";
               obj.Barcode = $"这是演示数据！！！";
               list.Add(obj);
               ++li;
               ++ci;
               ++ti;
            }
            SetTrayCupNumber(list);
            Parse(list);
         });
      });
   }

   #region 托盘统计

   QeryType _type = QeryType.查注液;
   List<InjectDisplay>? _results = new List<InjectDisplay>();
   List<string> _selects =
   [
      nameof(BatInjectStationModel.LineIndex),
      nameof(BatInjectStationModel.ColumnIndex),
      nameof(BatWeightAfterModel.FirstInjectResult),
      nameof(BatTestLeakModel.LeakResult),
      nameof(BatInjectStationModel.TrayCode),
      nameof(BatInjectStationModel.CupCode),
      nameof(BatMainModel.Barcode),
      nameof(BatWeightBeforeModel.TargetInjectionVolume),
      nameof(BatWeightAfterModel.ActualInjectionVolume),
      nameof(BatWeightAfterModel.TargetInjectionVolumeDeviation),
   ];

   /// <summary>
   /// 统计托盘数据
   /// </summary>
   /// <param name="type">1：统计注液；2：统计测漏</param>
   /// <returns></returns>
   public async Task QueryTrayCmd(string type)
   {
      try
      {
         _type = type == "2" ? QeryType.查测漏 : QeryType.查注液;
         if (Parameter.AdvancedConfig.ProductionType is ProductionTypeEnum.回氦 or ProductionTypeEnum.清洗机)
         {
            Growl.Warning("回氦、清洗机不支持托盘统计！");
            return;
         }
         TrayDatas.Clear();
         IsQueryFinish = false;

         _results = await SugarDb.GetStatisticsData<InjectDisplay>(StartTime, EndTime, _selects);
         if (_results == null || _results.Count == 0)
         {
            Growl.Warning("没有查询到对应时间段统计数据！");
            IsQueryFinish = true;
            return;
         }
         SetTrayCupNumber(_results);
         Parse(_results);
      }
      catch (Exception e)
      {
         $"查询统计数据异常：{e}".LogRun(Log4NetLevelEnum.错误, true);
      }
      finally
      {
         IsQueryFinish = true;
      }
   }

   private void SetTrayCupNumber(List<InjectDisplay> list)
   {
      TrayNumbers.Clear();
      CupNumbers.Clear();
      TrayNumbers.Insert(0, new TrayStringItem("全部", true));
      CupNumbers.Insert(0, new TrayStringItem("全部"));

      var trayNumbers = list.GroupBy(x => x.TrayCode).OrderBy(x => x.Key);
      var cupNumbers = list.GroupBy(x => x.CupCode).OrderBy(x => x.Key);

      foreach (var item in trayNumbers)
      {
         if (item.Key == null || string.IsNullOrWhiteSpace(item.Key.ToString()))
            continue;
         TrayNumbers.Add(new TrayStringItem(item.Key.ToString()));
      }
      foreach (var item in cupNumbers)
      {
         if (item.Key == null || string.IsNullOrWhiteSpace(item.Key.ToString()))
            continue;
         CupNumbers.Add(new TrayStringItem(item.Key.ToString()));
      }
   }

   public void SelectedTrayNumber(TrayStringItem number) => OnSelectedTrayCupNumber(number, inj => inj.TrayCode == number.No);

   public void SelectedCupNumber(TrayStringItem number) => OnSelectedTrayCupNumber(number, inj => inj.CupCode == number.No);

   public void OnSelectedTrayCupNumber(TrayStringItem number, Func<InjectDisplay, bool> func)
   {
      TrayDatas.Clear();
      if (_results == null)
         return;

      if (number.No == "全部")
         Parse(_results);
      else
      {
         var list = _results.Where(func).ToList();
         Parse(list);
      }
   }

   private void Parse(List<InjectDisplay>? list)
   {
      try
      {
         if (list == null)
            return;
         Rows = list.Max(x => x.LineIndex); //取最大行号
         RowInfos.Clear();
         for (int i = 0; i < Rows; i++)
            RowInfos.Add(new TrayIntItem(i + 1));

         Columns = list.Max(x => x.ColumnIndex); //取最大列号
         ColumnInfos.Clear();
         for (int i = 0; i < Columns; i++)
            ColumnInfos.Add(new TrayIntItem(i + 1));

         for (int r = 0; r < Rows; r++)
         {
            for (int c = 0; c < Columns; c++)
            {
               var injData = list.Where(x => x.LineIndex == r + 1 && x.ColumnIndex == c + 1).ToList();
               int ok = 0,
                  ng = 0;
               foreach (var obj in injData)
               {
                  var res = _type == QeryType.查注液 ? obj.FirstInjectResult : obj.LeakResult;
                  var resArea = res.GetResultArea();
                  if (resArea == ResultArea.Ignore)
                     continue;

                  if (resArea == ResultArea.OK)
                     ++ok;
                  else
                     ++ng;
               }
               TrayDatas.Add(
                  new StatisticsDataItemModel
                  {
                     Type = _type,
                     Row = r + 1,
                     Column = c + 1,
                     OK = ok,
                     NG = ng,
                     Total = ok + ng,
                     Datas = injData,
                  }
               );
            }
         }
      }
      catch (Exception ex)
      {
         $"解析统计数据异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   #endregion


   public void ShowDetailCmd(StatisticsDataItemModel statisticsDataItem)
   {
      TrayCupShowDetailDialog trayCupShowDetailDialog = new TrayCupShowDetailDialog(statisticsDataItem);
      trayCupShowDetailDialog.ShowDialog();
   }
}

public class TrayStringItem
{
   public bool IsChecked { get; set; }
   public string No { get; set; } = string.Empty;

   public TrayStringItem(string no, bool isChecked = false)
   {
      No = no;
      IsChecked = isChecked;
   }
}

public class TrayIntItem
{
   public bool IsChecked { get; set; }
   public int No { get; set; }

   public TrayIntItem(int no, bool isChecked = false)
   {
      No = no;
      IsChecked = isChecked;
   }
}

/// <summary>
/// 查询类型
/// </summary>
public enum QeryType
{
   查注液,
   查测漏,
}
