using HandyControl.Controls;
using Kinlo.Common.Models.OhtenModels;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(true)]
public class PlcWarningStatisticalViewModel : Screen
{
   public List<PlcAlarmStatEnum> PlcAlarmStats { get; init; }
   public PlcAlarmStatEnum SelectedPlcAlarmStat { get; set; }
   public List<PlcAalrmLevelEnum> PlcAlarmLevelTypes { get; set; }

   /// <summary>
   /// 开始时间
   /// </summary>
   public DateTime StartTime { get; set; }

   /// <summary>
   /// 结束时间
   /// </summary>
   public DateTime EndTime { get; set; }
   public ObservableCollection<string> PlcAlarms { get; set; } = new();

   public DbHelper _sugarDB;

   public PlcWarningStatisticalViewModel(IContainer container)
   {
      PlcAlarmStats = Enum.GetValues(typeof(PlcAlarmStatEnum)).Cast<PlcAlarmStatEnum>().ToList();
      PlcAlarmLevelTypes = Enum.GetValues(typeof(PlcAalrmLevelEnum)).Cast<PlcAalrmLevelEnum>().ToList();

      _sugarDB = container.Get<DbHelper>();
      EndTime = DateTime.Now;
      StartTime = EndTime.AddDays(-1);
   }

   public async Task QueryCmd(HandyControl.Controls.CheckComboBox checkComboBox)
   {
      try
      {
         var selectList = checkComboBox.SelectedItems.OfType<PlcAalrmLevelEnum>().ToArray();
         if (selectList is null || selectList.Length == 0)
         {
            Growl.Warning("请选择报警类型！");
            return;
         }
         await UIThreadHelper.InvokeOnUiThreadAsync(() => PlcAlarms.Clear());
         var plcAalarms = await _sugarDB.GetTimeRangePlcAlarmsAsync(StartTime, EndTime, selectList);
         if (plcAalarms.Count == 0)
         {
            Growl.Info($"此时间段无PLC报警！");
            return;
         }
         var alarms = SelectedPlcAlarmStat switch
         {
            PlcAlarmStatEnum.按报警次数 => AlarmGroup(plcAalarms, true),
            PlcAlarmStatEnum.按报警总时长 => AlarmGroup(plcAalarms, false),
            _ => NotGroup(plcAalarms),
         };
         await UIThreadHelper.InvokeOnUiThreadAsync(() => PlcAlarms = alarms);
      }
      catch (Exception ex)
      {
         $"查询PLC状态异常：{ex}".LogRun(Log4NetLevelEnum.警告);
      }
   }

   private ObservableCollection<string> NotGroup(List<PlcAlarmModel> plcAalarms)
   {
      ObservableCollection<string> plcAlarmsStr = new ObservableCollection<string>();
      foreach (var alarm in plcAalarms)
      {
         string time = alarm switch
         {
            var a when a.EndTime == null => "未消警",
            _ => new Func<string>(() =>
            {
               var duration = (TimeSpan)(alarm.EndTime - alarm.StartTime)!;
               return duration.TimeSpanToString();
            })(),
         };
         plcAlarmsStr.Add(
            $"[ {alarm.StartTime.ToMesDateTime()}~{(alarm.EndTime == null ? "" : ((DateTime)alarm.EndTime).ToMesDateTime())} ]-[{alarm.PlcAalrmLevel}] 报警时长：{time}  信息：{alarm.AlarmMessage};"
         );
      }
      return plcAlarmsStr;
   }

   private ObservableCollection<string> AlarmGroup(List<PlcAlarmModel> plcAlarms, bool isCount)
   {
      var results = ParseAalrm(plcAlarms);
      var list = isCount
         ? results.OrderByDescending(x => x.count).ThenByDescending(x => x.totalTime).ToList()
         : results.OrderByDescending(x => x.totalTime).ThenByDescending(x => x.count).ToList();
      ObservableCollection<string> plcAlarmsStr = new ObservableCollection<string>();
      for (int i = 0; i < list.Count; i++)
      {
         plcAlarmsStr.Add(
            $"第[{i + 1}]位 报警：{list[i].count}次，总时长：{TotalTimeFormatted(list[i].totalTime)}，等级：{list[i].level}，信息：{list[i].msg}"
         );
      }
      return plcAlarmsStr;
   }

   private List<(string msg, PlcAalrmLevelEnum level, TimeSpan totalTime, int count)> ParseAalrm(List<PlcAlarmModel> plcAalarms)
   {
      var group = plcAalarms.GroupBy(x => x.PlcTag); //用tag分组，性能相对msg会好些，如果想再想就要建立对应的code

      List<(string msg, PlcAalrmLevelEnum level, TimeSpan totalTime, int count)> plcAalrms = new();
      foreach (var item in group)
      {
         var first = item.First();
         TimeSpan totalTime = item.Aggregate(
            TimeSpan.Zero,
            (sum, entity) =>
            {
               if (entity.StartTime == null || entity.EndTime == null)
               {
                  $"PLC报警[{entity.AlarmMessage}]开始时间或结束时间为空，跳过！".LogRun(Log4NetLevelEnum.警告);
                  return sum + TimeSpan.Zero;
               }
               else
                  return sum + ((DateTime)entity.EndTime - (DateTime)entity.StartTime);
            }
         );
         plcAalrms.Add((first.AlarmMessage, first.PlcAalrmLevel, totalTime, item.Count()));
      }
      return plcAalrms;
   }

   public string TotalTimeFormatted(TimeSpan totalTime)
   {
      var ts = totalTime;
      var days = ts.Days;
      var hours = ts.Hours;
      var minutes = ts.Minutes;
      var seconds = ts.Seconds;

      var parts = new List<string>();

      if (days > 0)
         parts.Add($"{days}天");

      if (hours > 0)
         parts.Add($"{hours}时");

      if (minutes > 0)
         parts.Add($"{minutes}分");

      if (seconds > 0 || parts.Count == 0) // 至少显示秒（比如 0 天 0 时 0 分 5 秒）
         parts.Add($"{seconds}秒");

      if (parts.Count > 0)
         return $"{string.Join("", parts)}";
      else
         return "0分";
   }
}

public enum PlcAlarmStatEnum
{
   常规,
   按报警总时长,
   按报警次数,
}
