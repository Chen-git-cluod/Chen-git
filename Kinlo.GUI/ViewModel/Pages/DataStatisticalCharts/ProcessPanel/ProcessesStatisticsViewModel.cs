using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(true)]
public class ProcessStatisticsViewModel : Screen
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
   public List<FieldInfoModel> ResultFields { get; set; } = new List<FieldInfoModel>();
   string _okResult = ((int)ResultTypeEnum.OK).ToString();
   string _ignoreResult = ((int)ResultTypeEnum._).ToString();

   public ProcessStatisticsViewModel()
   {
      StartTime = DateTime.Now.AddHours(-8);
      EndTime = DateTime.Now;
      ChartView = new PercentChartView();
   }

   /// <summary>
   /// 统计数据
   /// </summary>
   /// <param name="type">1：统计注液；2：统计测漏</param>
   /// <returns></returns>
   public async Task QueryCmd()
   {
      try
      {
         IsQueryFinish = false;
         List<string> selects = ResultFields.Select(x => x.FieldName).ToList();
         var list = await SugarDb.GetStatisticsData<ExpandoObject>(StartTime, EndTime, selects);
         if (list == null || list.Count == 0)
         {
            Growl.Warning("没有查询到对应时间段统计数据！");
            IsQueryFinish = true;
            return;
         }
         List<string> displayNames = new();
         List<double> oks = new();
         List<double> ngs = new();
         foreach (var item in ResultFields)
         {
            displayNames.Add(item.Display);

            int ok = 0,
               ng = 0;
            foreach (var obj in list)
            {
               var rss = ((IDictionary<string, object>)obj)[item.FieldName].ToString();
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
         ChartView.UpdateChartData(oks.ToArray(), ngs.ToArray(), displayNames.ToArray(), "工序统计");
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
