using System.Data;
using System.IO.Pipes;
using HandyControl.Controls;
using Kinlo.GUI.Helpers;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace Kinlo.GUI.ViewModel;

public partial class ProductionHistoryViewModel : Screen
{
   // Hipot导出相关字段集合（用于反射判断字段完整性）
   private HashSet<string> _hipotFieldNames = [];

   // SQL select字段缓存，避免重复拼接
   private string _hipotSelectFields = string.Empty;

   /// <summary>
   /// 单条波形展示
   /// </summary>
   public async Task HipotCurveShowAsync(ListView listView)
   {
      try
      {
         // 未选择数据直接提示
         if (listView == null || listView.SelectedItems.Count == 0)
         {
            Growl.Warning("请先选择列！");
            return;
         }

         if (listView.SelectedItems[0] is not IDictionary<string, object> dic)
            return;

         // UI线程处理（避免绑定/弹窗线程问题）
         await UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            var barcode = dic[nameof(BatMainModel.Barcode)] as string;

            // 提取波形必要字段
            if (
               !dic.TryGetValue(nameof(BatHipotAc3200Model.CurvePoint), out var curveObj)
               || curveObj is not string curveStr
               || !dic.TryGetValue(nameof(BatHipotAc3200Model.HipotTime), out var timeObj)
               || timeObj is not DateTime time
               || !dic.TryGetValue(nameof(BatHipotAc3200Model.HipotIndex), out var indexObj)
               || indexObj is not byte index
            )
            {
               Growl.Warning("未找到Hipot波形数据！");
               return;
            }

            // 曲线数据解析
            var curve = curveStr.ToChartPoint();
            if (curve == null)
            {
               Growl.Warning("Hipot波形数据转换失败！");
               return;
            }

            // 构建弹窗ViewModel
            var hipotCurveVM = new HipotCurveViewModel();

            // 标题（波形基本信息）
            hipotCurveVM.Title = new LabelVisual
            {
               Text = ChartExporter.GetCurveTiele(barcode, index, time),
               TextSize = 15,
               Padding = new LiveChartsCore.Drawing.Padding(15),
               Paint = new SolidColorPaint(SKColors.Black),
            };

            // 曲线数据绑定
            hipotCurveVM.ChatSeries = [ChartExporter.CreateSeries(curve)];

            _windowManager.ShowDialog(hipotCurveVM);
         });
      }
      catch (Exception ex)
      {
         $"[Hipot波形]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   public bool IsExporting { get; set; }
   public double ExportProgress { get; set; }
   public string ExportButtonContent { get; set; } = "导出Hipot波形";

   //是否隐藏控制台黑框，用于显示详情
   public bool IsHiddenConsole { get; set; } = true;

   /// <summary>
   /// 导出Hipot波形
   /// </summary>
   public async void ExportHipotCurveCMD()
   {
      if (IsExporting)
         return;
      if (FilterOption.TimeFilter.EndTime <= FilterOption.TimeFilter.StartTime)
      {
         Growl.Warning("结束时间应大于开始时间！");
         return;
      }
      IsExporting = true;

      try
      {
         // 初始化字段集合
         if (_hipotFieldNames.Count == 0)
         {
            _hipotFieldNames = typeof(HipotCurveModel)
               .GetProperties(BindingFlags.Instance | BindingFlags.Public)
               .Select(x => x.Name)
               .ToHashSet();
         }

         // 拼接SQL字段
         if (string.IsNullOrWhiteSpace(_hipotSelectFields))
         {
            _hipotSelectFields = string.Join(", ", _hipotFieldNames.Select(f => $"{_sugarDB.AliasName}.{f.Trim()}"));
         }

         // 检查数据库字段是否完整
         if (!_hipotFieldNames.IsSubsetOf(_sugarDB.BatteryFieldNames))
         {
            Growl.Warning($"未找到Hipot波形数据！");
            return;
         }

         ExportButtonContent = "波形导出中...";
         sw.Restart();

         var queryFilter = FilterOption.ToQueryFilter();

         // 查询波形数据
         var data = await _sugarDB.QueryHipotCurveAsync<HipotCurveModel>(queryFilter, _sugarDB.AliasName, _hipotSelectFields);

         if (data != null)
         {
            var dataCount = data.Count();

            // 按1000条分组（避免单批过大）
            var len = (int)Math.Ceiling(dataCount / 1000.0);
            var dataList = new List<List<HipotCurveModel>>(len);
            var start = 0;

            for (int i = 0; i < len; i++)
            {
               var end = Math.Min(1000, dataCount - start) + start;
               dataList.Add(data[start..end]);
               start = end;
            }

            var groupCount = dataList.Count;
            // 逐组调用 Worker 导出
            for (int i = 0; i < groupCount; i++)
            {
               var json = JsonSerializer.Serialize(dataList[i]);
               await ExporterChartAsync(json, i + 1, groupCount);
            }
         }

         sw.Stop();
         $"导出数据用时:{sw.ElapsedMilliseconds}ms".LogRun();
      }
      finally
      {
         // UI状态复位
         ExportProgress = 0;
         ExportButtonContent = "导出Hipot波形";
         IsExporting = false;
      }
   }

   /// <summary>
   /// 调用外部Worker进行导出
   /// 每次调用：启动进程 + 建立Pipe + 等待结果
   /// </summary>
   public async Task ExporterChartAsync(string json, int groupCurrent, int groupCount)
   {
      // WPF → Worker
      var cmdServer = new NamedPipeServerStream("ChartPipe_CMD", PipeDirection.Out);
      // Worker → WPF
      var progressServer = new NamedPipeServerStream("ChartPipe_PROGRESS", PipeDirection.In);

      var psi = new ProcessStartInfo
      {
         FileName = @"ChartExporter\Kinlo.ChartExporter.exe",
         UseShellExecute = false,

         // 是否隐藏窗口
         CreateNoWindow = IsHiddenConsole ? true : false,
         WindowStyle = IsHiddenConsole ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
      };
      // 启动外部导出进程
      Process.Start(psi);

      cmdServer.WaitForConnection();
      progressServer.WaitForConnection();

      var cmdWriter = new StreamWriter(cmdServer) { AutoFlush = true };
      var progressReader = new StreamReader(progressServer);

      // 发送任务数据
      cmdWriter.WriteLine(json);
      cmdWriter.Flush();

      // 后台监听Worker进度
      await Task.Run(async () =>
      {
         while (true)
         {
            try
            {
               var msg = progressReader.ReadLine();
               if (msg == null)
                  break;

               if (msg.StartsWith("PROGRESS")) // 进度上报
               {
                  var p = msg.Split('|');
                  int current = int.Parse(p[1]);
                  int total = int.Parse(p[2]);

                  var progress = (double)current / total * 100;
                  var progressDes = $"总进度：{groupCurrent}/{groupCount}---当前进度：{current}/{total}";

                  // UI线程更新
                  await UIThreadHelper.InvokeOnUiThreadAsync(() =>
                  {
                     ExportProgress = progress;
                     ExportButtonContent = progressDes;
                  });
               }
               else if (msg == "DONE") // 完成信号
               {
                  await UIThreadHelper.InvokeOnUiThreadAsync(() =>
                  {
                     ExportButtonContent = "导出完成";
                     ExportProgress = 100;
                  });

                  break;
               }
               else
               {
                  await UIThreadHelper.InvokeOnUiThreadAsync(() =>
                  {
                     ExportButtonContent = "导出异常";
                  });
                  Growl.Warning(msg);
                  break;
               }
            }
            catch (Exception ex)
            {
               Growl.Warning($"导出异常：{ex}");
               break;
            }
         }

         // 关闭Worker
         cmdWriter.WriteLine("EXIT");
         cmdWriter.Flush();

         cmdServer.Close();
         progressServer.Close();
      });
   }
}
