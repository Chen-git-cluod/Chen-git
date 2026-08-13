using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[Languages(["实时日志", "Sejarah Log", "Current logs"], IsScanProperty = false)]
[UIDisplayAttribute(isSingleton: true, 3, ulong.MaxValue, isRunEdit: true, "\xe61c")]
public class LogHistoryViewModel : Screen, IMenu
{
   [Inject]
   public ParameterConfig ParameterCopy { get; set; }
   public FrameworkElement? LogVM { get; set; }
   public ObservableCollection<LogHistoryModel> LogHistorys { get; set; } = new();
   private int _selectIndex = -1;

   public int SelectIndex
   {
      get { return _selectIndex; }
      set
      {
         if (_selectIndex != value)
         {
            _selectIndex = value;
            if (value >= 0 && value < LogHistorys.Count)
            {
               LogVM = LogHistorys[value].View;
               if (LogVM is LogRunAnalysisView view)
                  _ = view.Load();
            }
         }
      }
   }

   private string _historyLogNme = "分析运行日志";

   public LogHistoryViewModel(IContainer container)
   {
      foreach (var item in (Log4NetTypeEnum[])Enum.GetValues(typeof(Log4NetTypeEnum)))
      {
         if (item != Log4NetTypeEnum.操作数据库日志 && item != Log4NetTypeEnum.操作数据库超时日志)
         {
            var LogHistory = new LogHistoryModel { Log4NetType = item.ToString() };
            if (item == Log4NetTypeEnum.MES日志)
            {
               LogHistory.CurrentLogs = new ObservableCollection<MesLogModel>();
               LogHistory.View = new LogMesLogView(LogHistory.CurrentLogs as ObservableCollection<MesLogModel>);
            }
            else if (item == Log4NetTypeEnum.Web服务日志)
            {
               LogHistory.CurrentLogs = new ObservableCollection<WebLogModel>();
               LogHistory.View = new LogWebLogView(LogHistory.CurrentLogs as ObservableCollection<WebLogModel>);
            }
            else if (item == Log4NetTypeEnum.设备报警日志)
            {
               LogHistory.View = new LogPlcAlarmView(container);
            }
            else
            {
               LogHistory.CurrentLogs = new ObservableCollection<RunLogModel>();
               LogHistory.View = new LogRunView(LogHistory.CurrentLogs as ObservableCollection<RunLogModel>);
            }
            LogHistorys.Add(LogHistory);
         }
      }

      var logRunAnalysisView = new LogRunAnalysisView(container, _historyLogNme);
      _ = logRunAnalysisView.Load();
      LogHistorys.Insert(1, new LogHistoryModel { Log4NetType = _historyLogNme, View = logRunAnalysisView });

      Log4NetHelper.DisplayRunLogAction = AddRunLog;
      Log4NetHelper.DisplayMesLogAction = AddMesLog;
      Log4NetHelper.DisplayWebLogAction = AddWebLog;
   }

   private void AddRunLog(string message, Log4NetLevelEnum level, Log4NetTypeEnum type)
   {
      var logHistory = LogHistorys.First(x => x.Log4NetType == type.ToString());
      var runLog = new RunLogModel { Message = message, Time = DateTime.Now.ToString("MM-dd HH:mm:ss fff") };

      switch (level)
      {
         case Log4NetLevelEnum.错误:
            runLog.ErrVisibility = Visibility.Visible;
            break;
         case Log4NetLevelEnum.警告:
            runLog.WarningVisibility = Visibility.Visible;
            break;
         case Log4NetLevelEnum.成功:
            runLog.SuccessVisibility = Visibility.Visible;
            break;
         default:
            runLog.MessageVisibility = Visibility.Visible;
            break;
      }

      var _currentLogs = (ObservableCollection<RunLogModel>)logHistory.CurrentLogs;
      UIThreadHelper.InvokeOnUiThreadAsync(
         new Action(() =>
         {
            _currentLogs.Insert(0, runLog);
            if (_currentLogs.Count > 800)
               _currentLogs.RemoveAt(800);
         })
      );
   }

   private void AddMesLog(MesLogModel mesLog)
   {
      var logHistory = LogHistorys.First(x => x.Log4NetType == Log4NetTypeEnum.MES日志.ToString());
      var currentLogs = (ObservableCollection<MesLogModel>)logHistory.CurrentLogs;
      UIThreadHelper.InvokeOnUiThreadAsync(
         new Action(() =>
         {
            currentLogs.Insert(0, mesLog);
            if (currentLogs.Count > 800)
               currentLogs.RemoveAt(800);
         })
      );
   }

   private void AddWebLog(WebLogModel weblog)
   {
      var logHistory = LogHistorys.First(x => x.Log4NetType == Log4NetTypeEnum.Web服务日志.ToString());
      var currentLogs = (ObservableCollection<WebLogModel>)logHistory.CurrentLogs;
      UIThreadHelper.InvokeOnUiThreadAsync(
         new Action(() =>
         {
            currentLogs.Insert(0, weblog);
            if (currentLogs.Count > 800)
               currentLogs.RemoveAt(800);
         })
      );
   }

   string alarmNavStr = "请导航至 “图表统计”->“PLC报警” 处查看！";

   public void OpenLogCMD()
   {
      var selectedLog = LogHistorys[SelectIndex];

      if (selectedLog.Log4NetType == Log4NetTypeEnum.设备报警日志.ToString())
      {
         Growl.Info(alarmNavStr);
         return;
      }
      if (selectedLog.Log4NetType == _historyLogNme)
      {
         if (selectedLog.View is LogRunAnalysisView view)
            view.Open();
         return;
      }
      if (selectedLog.Log4NetType == Log4NetTypeEnum.MES日志.ToString())
      {
         OpenLogDirCMD(); //如果是MES日志，直接打开文件夹
         return;
      }
      string path = Path.Combine("Logs", selectedLog.Log4NetType);
      path = Path.Combine(
         AppDomain.CurrentDomain.BaseDirectory,
         path,
         DateTime.Now.ToString(@"yyyy-MM-dd"),
         $"{DateTime.Now.ToString("HH")}.txt"
      );

      if (File.Exists(path))
         System.Diagnostics.Process.Start("notepad.exe", path);
      else
         HandyControl.Controls.Growl.Warning($"未找到文件：{path}");
   }

   public void OpenLogDirCMD()
   {
      var selectedLog = LogHistorys[SelectIndex];

      if (selectedLog.Log4NetType == Log4NetTypeEnum.设备报警日志.ToString())
      {
         Growl.Info(alarmNavStr);
         return;
      }

      string path = Path.Combine("Logs", selectedLog.Log4NetType);

      if (selectedLog.Log4NetType != Log4NetTypeEnum.MES日志.ToString() && selectedLog.Log4NetType != _historyLogNme)
         path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path, DateTime.Now.ToString(@"yyyy-MM-dd"));

      if (Directory.Exists(path))
         System.Diagnostics.Process.Start("explorer.exe", path);
      else
         HandyControl.Controls.Growl.Warning($"未找到文件位置：{path}");
   }

   public void Load() { }

   public bool Unload()
   {
      return true;
   }
}
