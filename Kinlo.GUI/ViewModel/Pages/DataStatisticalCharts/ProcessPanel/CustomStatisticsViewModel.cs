using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(true)]
public class CustomStatisticsViewModel : Screen
{
   [Inject]
   public DbHelper? SugarDb { get; set; }

   [Inject]
   public SnowflakeHelper? Snowflake { get; set; }

   [Inject]
   public ParameterConfig? Parameter { get; set; }

   /// <summary>
   /// 开始时间
   /// </summary>
   public DateTime StartTime { get; set; }

   /// <summary>
   /// 结束时间
   /// </summary>
   public DateTime EndTime { get; set; }
   public bool IsQueryFinish { get; set; } = true;
   public PercentChartView ChartView { get; set; }
   public ObservableCollection<FieldInfoModel> ResultFields { get; set; } = new ObservableCollection<FieldInfoModel>();
   public ObservableCollection<FieldInfoModel> IndexFields { get; set; } = new ObservableCollection<FieldInfoModel>();
   public FieldInfoModel SelectResultField { get; set; }
   public FieldInfoModel SelectIndexField { get; set; }

   public CustomStatisticsViewModel()
   {
      StartTime = DateTime.Now.AddHours(-8);
      EndTime = DateTime.Now;
      ChartView = new PercentChartView();
      //  double[] oks = [10, 4, 8, 9, 5, 2, 4, 7];
      //  ChartView.UpdateChartData(oks, [33, 3, 5, 6, 3, 4, 5, 3], oks.Select((x, i) => $"通道{i}").ToArray(), "前扫码");
   }

   string _okResult = ((int)ResultTypeEnum.OK).ToString();
   string _ignoreResult = ((int)ResultTypeEnum._).ToString();

   /// <summary>
   /// 统计数据
   /// </summary>
   /// <param name="type">1：统计注液；2：统计测漏</param>
   /// <returns></returns>
   public async Task QueryCmd()
   {
      try
      {
         if (SelectIndexField == null || SelectResultField == null)
         {
            Growl.Warning("请选择通道和结果！");
            IsQueryFinish = true;
            return;
         }
         IsQueryFinish = false;
         List<string> selects = [SelectIndexField.FieldName, SelectResultField.FieldName];
         List<string> displayNames = [SelectResultField.Display, SelectIndexField.Display];
         var list = await SugarDb.GetStatisticsData<ExpandoObject>(StartTime, EndTime, selects);
         if (list == null || list.Count == 0)
         {
            Growl.Warning("没有查询到对应时间段统计数据！");
            IsQueryFinish = true;
            return;
         }
         var group = list.GroupBy(x => ((IDictionary<string, object>)x)[SelectIndexField.FieldName]).OrderBy(x => x.Key).ToList();

         List<double> oks = new();
         List<double> ngs = new();
         List<string> names = new();
         foreach (var items in group)
         {
            names.Add(items.Key.ToString());

            int ok = 0,
               ng = 0;
            foreach (var obj in items)
            {
               var rss = ((IDictionary<string, object>)obj)[SelectResultField.FieldName].ToString();
               if (rss == _ignoreResult)
                  continue;

               if (rss == _okResult)
                  ++ok;
               else
                  ++ng;
            }
            oks.Add(ok);
            ngs.Add(ng);
         }
         ChartView.UpdateChartData(oks.ToArray(), ngs.ToArray(), names.ToArray(), SelectResultField.Display);
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
}
