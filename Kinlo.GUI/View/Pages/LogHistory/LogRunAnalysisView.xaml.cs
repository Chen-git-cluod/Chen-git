using System.Text.RegularExpressions;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using NPOI.SS.Formula.Functions;

namespace Kinlo.GUI.View
{
   /// <summary>
   /// LogRunAnalysisView.xaml 的交互逻辑
   /// </summary>
   [AddINotifyPropertyChangedInterface]
   public partial class LogRunAnalysisView : UserControl
   {
      public DateTime StartTime { get; set; }
      public DateTime EndTime { get; set; }
      public string PlcName { get; set; } = string.Empty;
      public ObservableCollection<string> PlcNames { get; set; } = new ObservableCollection<string>();
      public string Process { get; set; } = string.Empty;
      public ObservableCollection<string> Processes { get; set; } = new ObservableCollection<string>();
      public string Lane { get; set; } = string.Empty;
      public ObservableCollection<string> Lanes { get; set; } = new ObservableCollection<string>();
      public bool IsId { get; set; } = true;
      public string IdOrBarcode { get; set; } = string.Empty;
      public ObservableCollection<string> LogEntries { get; set; } = new ObservableCollection<string>();
      public bool IsInfoLoaded { get; set; }
      IContainer _container;
      private string All = "全部";
      private string _logName = string.Empty;

      public LogRunAnalysisView(IContainer container, string logName)
      {
         InitializeComponent();
         DataContext = this;
         _container = container;
         _logName = logName;
         EndTime = DateTime.Now;
         StartTime = EndTime - TimeSpan.FromHours(3);
         Lanes.Add(All);
         for (int i = 1; i <= 30; i++)
         {
            Lanes.Add($"{i}通道");
         }
         Lane = Lanes[0];
      }

      public async Task Load()
      {
         try
         {
            IsInfoLoaded = false;

            var (plcNames, processes) = await Task.Run(() =>
            {
               var plcSignal = _container.Get<PLCSignalConfig>();
               var plcNames = plcSignal.PLCScanSignals.Select(x => x.ServiceName).ToList();

               var processGroup = plcSignal.PLCInteractAddresses.GroupBy(x => x.ProcessesType).ToList();
               var processes = processGroup.Select(x => x.First().ProcessesType.ToString()).ToList();
               return (plcNames, processes);
            });

            var plcNmae = PlcName;
            var process = Process;
            PlcNames.Clear();
            PlcNames.Add(All);
            PlcNames.AddRange(plcNames);
            Processes.Clear();
            Processes.Add(All);
            Processes.AddRange(processes);
            if (!string.IsNullOrEmpty(plcNmae))
               PlcName = plcNmae;
            else
               PlcName = PlcNames[0];

            if (!string.IsNullOrEmpty(process))
               Process = process;
            else
               Process = Processes[0];
         }
         finally
         {
            IsInfoLoaded = true;
         }
      }

      string fileNmae = string.Empty;
      string pathBase = Path.Combine("Logs", Log4NetTypeEnum.运行日志.ToString());
      Regex pattern = new Regex(@"\[([^\[\]]*?)-([^\[\]]*?)-([^\[\]]*?)-([^\[\]]*?)-([^\[\]]*?)-([^\[\]]*?)\]"); //[^\[\]]*? 中括号内^为取反

      private void Button_Click_Read(object sender, RoutedEventArgs e)
      {
         try
         {
            IsInfoLoaded = false;
            List<string> pathList = new List<string>();
            var newStartTime = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day, StartTime.Hour, 0, 0, 0);
            var newEndTime = new DateTime(EndTime.Year, EndTime.Month, EndTime.Day, EndTime.Hour, 0, 0, 0);
            while (newStartTime <= newEndTime)
            {
               var path = Path.Combine(
                  AppDomain.CurrentDomain.BaseDirectory,
                  pathBase,
                  newStartTime.ToString(@"yyyy-MM-dd"),
                  $"{newStartTime.ToString("HH")}.txt"
               );
               pathList.Add(path);
               newStartTime = newStartTime.AddHours(1);
            }
            fileNmae =
               $"【时间：{StartTime:yyyy-MM-dd HH}~{EndTime:yyyy-MM-dd HH}】【PLC：{PlcName}】【工序：{Process}】【通道：{Lane}】{(string.IsNullOrEmpty(IdOrBarcode) ? "" : $"{(IsId ? "【ID：" : "【条码：")}{IdOrBarcode}】")}";
            LogEntries.Clear();

            _ = Task.Run(async () =>
            {
               List<string> buffer = new();
               foreach (var filePath in pathList)
               {
                  if (!File.Exists(filePath))
                     continue;
                  using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); //FileAccess.Read 只读  FileShare.ReadWrite 允许其他程序读写
                  using var reader = new StreamReader(stream, Encoding.UTF8);

                  StringBuilder sb = new();
                  bool isCollecting = false; //是否正在采集中，当前一条日志还未结束
                  string? line;

                  while ((line = reader.ReadLine()) != null)
                  {
                     if (string.IsNullOrEmpty(line))
                        continue;

                     if (!isCollecting)
                     {
                        // 提取符合 [xxx-xxx-xxx-xxx-xxx-xxx] 格式的设备标识（必须是 5 个 -）
                        var match = pattern.Match(line);
                        if (!match.Success) // 不符合 5 个 - 的格式，跳过
                           continue;

                        if (PlcName != All && match.Groups[1].Value != PlcName)
                           continue;
                        if (Process != All && match.Groups[2].Value != Process)
                           continue;
                        if (Lane != All && match.Groups[4].Value != Lane)
                           continue;
                        if (!string.IsNullOrEmpty(IdOrBarcode))
                        {
                           var idOrBarcode = IsId ? match.Groups[5].Value : match.Groups[6].Value;
                           if (IdOrBarcode != idOrBarcode)
                              continue;
                        }

                        // 是有效的起始行，开始收集
                        isCollecting = true;
                     }

                     if (line.EndsWith(Log4NetHelper.EndMarker.ToString(), StringComparison.Ordinal)) //出现结束符，重新下一轮
                     {
                        sb.Append(line);
                        buffer.Add(sb.ToString());
                        sb.Clear();
                        isCollecting = false;
                     }
                     else
                     {
                        sb.AppendLine(line);
                     }

                     // 每 200 行刷新一次 UI，避免卡顿
                     if (buffer.Count >= 200)
                     {
                        await UIThreadHelper.InvokeOnUiThreadAsync(() =>
                        {
                           foreach (var item in buffer)
                              LogEntries.Add(item);
                        });
                        buffer.Clear();
                     }
                  }
                  if (sb.Length > 0 && isCollecting) //提防有的日志出现意外没有结束符
                  {
                     buffer.Add(sb.ToString());
                     sb.Clear();
                  }
               }
               // 剩余的最后一批
               if (buffer.Count > 0)
               {
                  await UIThreadHelper.InvokeOnUiThreadAsync(() =>
                  {
                     foreach (var item in buffer)
                        LogEntries.Add(item);
                  });
                  buffer.Clear();
               }
               IsInfoLoaded = true;
            });
         }
         catch (Exception ex)
         {
            Growl.Warning(ex.Message);
            IsInfoLoaded = true;
         }
      }

      string savePath = string.Empty;

      private async void Button_Click_SaveAndOpen(object sender, RoutedEventArgs e)
      {
         if (LogEntries == null || LogEntries.Count == 0)
            return;
         savePath = Path.Combine("Logs", _logName, $"{fileNmae}{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.txt");
         if (await Save(savePath))
            Open();
      }

      public void Open()
      {
         if (!string.IsNullOrEmpty(savePath) && File.Exists(savePath))
            System.Diagnostics.Process.Start("notepad.exe", savePath);
         else
            HandyControl.Controls.Growl.Warning($"未找到文件：{savePath}");
      }

      private void Button_Click_Save(object sender, RoutedEventArgs e)
      {
         if (LogEntries == null || LogEntries.Count == 0)
            return;
         savePath = Path.Combine("Logs", _logName, $"{fileNmae}{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.txt");
         _ = Save(savePath);
      }

      private async Task<bool> Save(string path)
      {
         try
         {
            IsInfoLoaded = false;
            var sb = new StringBuilder();
            foreach (var item in LogEntries)
               sb.AppendLine(item);

            var dir = Path.GetDirectoryName(path);
            if (dir != null && !Directory.Exists(dir))
               Directory.CreateDirectory(dir);
            await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
            return true;
         }
         catch (Exception ex)
         {
            Growl.Warning(ex.ToString());
         }
         finally
         {
            IsInfoLoaded = true;
         }
         return false;
      }

      private void Button_Click_Copy(object sender, RoutedEventArgs e)
      {
         if (listBox.SelectedItem is null)
         {
            Growl.Warning("未选中行!");
            return;
         }
         if (listBox.SelectedItem is string str)
         {
            Clipboard.SetText(str);
            Growl.Success("复制成功");
         }
      }
   }
}
