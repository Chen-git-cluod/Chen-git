using HandyControl.Controls;
using KinloControls;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(true)]
public class PlcStatusStatisticalViewModel : Screen
{
   /// <summary>
   /// 开始时间
   /// </summary>
   public DateTime StartTime { get; set; }

   /// <summary>
   /// 结束时间
   /// </summary>
   public DateTime EndTime { get; set; }
   public PlcStatusConfig PlcStatus { get; set; }
   public DbHelper _sugarDB;
   public ObservableCollection<TimelineItem> TimelineItems { get; }

   public PlcStatusStatisticalViewModel(IContainer container)
   {
      PlcStatus = container.Get<PlcStatusConfig>();
      _sugarDB = container.Get<DbHelper>();
      EndTime = DateTime.Now;
      StartTime = EndTime.AddDays(-1);
      TimelineItems = new ObservableCollection<TimelineItem>();
      //默认数据
      Task.Run(() =>
      {
         UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            DateTime startTime = DateTime.Now;
            Random random = new Random();
            var array = Enum.GetValues(typeof(DeviceStateEnum)).Cast<DeviceStateEnum>().ToArray();
            for (int i = 0; i < 100; i++)
            {
               try
               {
                  var index = (ushort)(random.Next(0, array.Length));
                  var status = array[index];

                  DateTime endTime = startTime.AddMinutes(random.Next(1, 60));
                  if (!PlcStatus.PlcStatusDisplayDic.TryGetValue(status, out var statusDescription))
                     continue;

                  var t = new TimelineItem
                  {
                     Id = i,
                     // Value = statusDescription.Code,
                     StartTime = startTime,
                     EndTime = endTime,
                     Label = statusDescription.Description,
                     Color = statusDescription.Color,
                  };
                  TimelineItems.Add(t);
                  startTime = endTime;
               }
               catch { }
            }
         });
      });
   }

   public async Task QueryCmd()
   {
      try
      {
         var plcStatus = await _sugarDB.GetTimeRangePlcStatusAsync(StartTime, EndTime, null, null);
         if (plcStatus.Count == 0)
         {
            Growl.Info($"此时间段无PLC状态信息！");
            return;
         }
         await UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            TimelineItems.Clear();
            foreach (var item in plcStatus)
            {
               if (item.StartTime == null || item.EndTime == null)
               {
                  $"PLC状态[{item.Msg}]开始时间或结束时间为空，跳过！".LogRun(Log4NetLevelEnum.警告);
                  continue;
               }
               if (!Enum.IsDefined(typeof(DeviceStateEnum), item.Status))
               {
                  $"PLC状态[{item.Msg}]未找到对应配置；".LogRun(Log4NetLevelEnum.警告);
                  continue;
               }

               if (!PlcStatus.PlcStatusDisplayDic.TryGetValue((DeviceStateEnum)item.Status, out var displayInfo))
                  continue;

               var startTime = item.StartTime;
               var endTime = (DateTime)item.EndTime;
               startTime = startTime < StartTime ? StartTime : startTime;
               endTime = endTime > EndTime ? EndTime : endTime;
               TimelineItems.Add(
                  new TimelineItem
                  {
                     Id = item.Id,
                     Color = displayInfo.Color,
                     StartTime = startTime,
                     EndTime = endTime,
                     Label = item.Msg,
                     // Value = item.Status
                  }
               );
            }
         });
      }
      catch (Exception ex)
      {
         $"查询PLC状态异常：{ex}".LogRun(Log4NetLevelEnum.警告);
      }
   }
}
