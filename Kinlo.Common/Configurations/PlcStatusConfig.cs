using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NPOI.SS.UserModel;

namespace Kinlo.Common.Configurations;

/// <summary>
/// PLC设备状态及报警
/// </summary>
[AddINotifyPropertyChangedInterface]
public class PlcStatusConfig : ConfigurationBase
{
   public PlcStatusConfig(IContainer container, bool isStartup)
      : base(container, isStartup) { }

   #region 停机原因
   /// <summary>
   /// 主动停机原因
   /// </summary>
   [JsonIgnore]
   public PlcStopReasonModel CurrentStopReason { get; set; } = new();

   /// <summary>
   /// 被动停机的第一个报警
   /// </summary>
   [JsonIgnore]
   public string FirstAlarm { get; set; } = string.Empty;
   #endregion

   #region PLC设备状态
   /// <summary>
   /// 设备状态描述列表
   /// </summary>
   [JsonIgnore]
   public Dictionary<DeviceStateEnum, PlcStatusDisplayModel> PlcStatusDisplayDic { get; set; }

   ///// <summary>
   ///// 保存设备状态待处理任务
   ///// </summary>
   [JsonIgnore]
   public ConcurrentDictionary<long, Func<string, Task>> PlcStatusPendingSaveTasks { get; set; } = new();

   /// <summary>
   /// 最近24小时PLC实时状态UI显示
   /// </summary>
   [JsonIgnore]
   public ObservableCollection<TimelineItem> Last24HoursPlcStatus { get; private set; } = new();

   /// <summary>
   /// 取最近24小时PLC状态列表中符合条件的最后一个状态
   /// </summary>
   /// <param name="predicate"></param>
   /// <returns></returns>
   public TimelineItem? GetTimelineLastOrDefault(Func<TimelineItem, bool> predicate) =>
      UIThreadHelper.InvokeOnUiThread(() => Last24HoursPlcStatus.LastOrDefault(predicate));

   /// <summary>
   /// 添加PLC状态到最近24小时列表（UI显示）
   /// </summary>
   /// <param name="item"></param>
   public Task AddTimeline(TimelineItem item) =>
      UIThreadHelper.InvokeOnUiThreadAsync(() =>
      {
         Last24HoursPlcStatus.Add(item);
         return Task.CompletedTask;
      });

   /// <summary>
   /// 裁剪 PLC 状态到最近 24 小时
   /// </summary>
   /// <returns></returns>
   public async Task TrimPlcStatusToLast24HoursAsync()
   {
      var now = DateTime.Now;
      List<TimelineItem> removeList = new();
      List<TimelineItem> adjustList = new();

      foreach (var item in Last24HoursPlcStatus)
      {
         if ((now - item.EndTime).TotalHours >= 24)
            removeList.Add(item);
         else if ((now - item.StartTime).TotalHours > 24)
            adjustList.Add(item);
      }

      await UIThreadHelper.InvokeOnUiThreadAsync(() =>
      {
         foreach (var item in removeList)
            Last24HoursPlcStatus.Remove(item);

         foreach (var item in adjustList)
            item.StartTime = now - TimeSpan.FromHours(24);
      });
   }

   #endregion

   #region 报警相关
   /// <summary>
   /// PLC生产报警详情列表(电气给表，然后读取上来)
   /// </summary>
   [JsonIgnore]
   public Dictionary<string, PlcAlarmInfoDto> PlcAlarmInfoDic { get; set; } = new();

   /// <summary>
   /// 实时PLC报警列表
   /// </summary>
   [JsonIgnore]
   public ConcurrentDictionary<string, (PlcAlarmModel alarm, Func<Task<PlcAlarmModel>> func)> PlcCurrentAlarmTasks { get; set; } = new();

   /// <summary>
   /// UI报警显示回调
   /// </summary>
   [JsonIgnore]
   public Func<PlcAlarmModel?, Task<bool>>? DisplayPlcAlarmsCallback { get; set; }
   #endregion

   public bool IsTest { get; set; }

   public override void Load()
   {
      try
      {
         PlcStatusDisplayDic = ResetPlcStatus();

         UIThreadHelper.InvokeOnUiThreadAsync(() =>
            Last24HoursPlcStatus.Add(
               new TimelineItem
               {
                  StartTime = DateTime.Now,
                  EndTime = DateTime.Now.AddMilliseconds(100),
                  Color = Brushes.White,
                  Value = 99,
                  Label = "无状态",
               }
            )
         );

         _ = LoadAlarmFromExcelAsync(); //加载PLC报警表格
      }
      catch (Exception ex)
      {
         $"[初始化PlcAlarmConfig]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   /// <summary>
   /// 取所有设备状态
   /// </summary>
   public Dictionary<DeviceStateEnum, PlcStatusDisplayModel> ResetPlcStatus()
   {
      Dictionary<DeviceStateEnum, PlcStatusDisplayModel> keyValuePairs = new();
      foreach (DeviceStateEnum state in Enum.GetValues(typeof(DeviceStateEnum)))
      {
         var fieldInfo = state.GetType().GetField(state.ToString());
         var attribute = fieldInfo?.GetCustomAttribute<DeviceStateAttribute>();
         if (attribute == null)
            continue;

         keyValuePairs[state] = new PlcStatusDisplayModel { Description = state.ToString(), Color = attribute.ColorBrush };
      }

      return keyValuePairs;
   }

   #region 加载PLC报警表格
   private async Task LoadAlarmFromExcelAsync()
   {
      string directory = $@"{ExternalTablesDirectory}\PLCAlarm";
      await directory.UseFirstExcelFromDirectoryAsync(w => Task.Run(() => LoadAlarm(w)));
   }

   private Regex _numRegex1 = new Regex(@"(\[\d+\]\.[A-Za-z_]\w*\[\d+\])$"); //从后面匹配一次包含两个中括号及之间的字符
   private Regex _numRegex2 = new Regex(@"\.?([A-Za-z_]\w*\[\d+\])$"); //从后面匹配一次包含最后中括号及从中括号到前面有点的部分，如果无点就整个标签

   /// <summary>
   /// 加载PLC报警表格
   /// </summary>
   private void LoadAlarm(IWorkbook workbook)
   {
      try
      {
         StringBuilder errmsg = new StringBuilder();
         PlcAlarmInfoDic.Clear();
         for (int s = 0; s < workbook.NumberOfSheets; s++)
         {
            ISheet sheet = workbook.GetSheetAt(s);
            for (int i = 1; i <= sheet.LastRowNum; i++) //从第一行开始
            {
               string plcAlarmTag = ExcelHelper.GetCellValue(sheet.GetRow(i).GetCell(0)) ?? string.Empty;
               string msg = ExcelHelper.GetCellValue(sheet.GetRow(i).GetCell(1)) ?? string.Empty;
               string mesCode = ExcelHelper.GetCellValue(sheet.GetRow(i).GetCell(2)) ?? string.Empty;

               if (string.IsNullOrEmpty(msg) || string.IsNullOrEmpty(plcAlarmTag))
               {
                  errmsg.Append($"第[{i + 1}]行，代码、消息或位置有空，不加载;");
                  continue;
               }
               if (string.IsNullOrEmpty(mesCode))
                  mesCode = "无MES_Code";

               var matches = _numRegex1.Match(plcAlarmTag); // 提取 tag
               if (!matches.Success)
               {
                  matches = _numRegex2.Match(plcAlarmTag); // 提取 tag
                  if (!matches.Success)
                  {
                     errmsg.Append($"第[{i + 1}]行[{plcAlarmTag}]中未包括两个或一个中括号或中括号中不是数字，不加载此行！");
                     continue;
                  }
               }
               var tagKey = matches.Groups[1].Value;
               if (PlcAlarmInfoDic.ContainsKey(tagKey))
               {
                  errmsg.Append($"第[{i + 1}]行[{plcAlarmTag}]标签重复，之前报警为:{PlcAlarmInfoDic[tagKey].AlarmMessage}！");
                  continue;
               }
               PlcAlarmInfoDic[tagKey] = new PlcAlarmInfoDto
               {
                  MesCode = mesCode,
                  AlarmMessage = msg,
                  OriginalTag = plcAlarmTag,
               };
            }
         }

         if (errmsg.Length > 0)
         {
            $"[导入报警Excel]{errmsg.ToString()}".LogRun(Log4NetLevelEnum.警告);
         }
         $"[导入报警Excel]导入{PlcAlarmInfoDic.Count}条报警信息!".LogRun();
      }
      catch (Exception ex)
      {
         $"[导入报警Excel]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }
   #endregion
}
