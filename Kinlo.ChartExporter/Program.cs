using System.IO.Pipes;
using System.Text.Json;

namespace Kinlo.ChartExporter;

internal class Program
{
   static void Main()
   {
      // ================================
      // IPC 通道1：命令管道（WPF → Worker）
      // 作用：接收导出任务 / 控制命令
      // ================================
      using var cmdPipe = new NamedPipeClientStream(".", "ChartPipe_CMD", PipeDirection.In);

      // ================================
      // IPC 通道2：进度管道（Worker → WPF）
      // 作用：回传导出进度 / 状态
      // ================================
      using var progressPipe = new NamedPipeClientStream(".", "ChartPipe_PROGRESS", PipeDirection.Out);

      // 连接WPF端 PipeServer（阻塞直到连接成功）
      cmdPipe.Connect();
      progressPipe.Connect();

      // 文本读取命令（WPF发送 JSON / EXIT）
      using var reader = new StreamReader(cmdPipe);

      // 文本写入进度（Worker回传状态）
      using var writer = new StreamWriter(progressPipe)
      {
         AutoFlush = true, // 每次Write立即发送，避免缓存延迟
      };

      while (true)
      {
         // 阻塞等待 WPF 下发任务
         var cmd = reader.ReadLine();

         // 退出信号：关闭 Worker
         if (cmd == "EXIT")
            break;

         // 空数据保护（避免异常解析）
         if (string.IsNullOrWhiteSpace(cmd))
            continue;

         try
         {
            var data = JsonSerializer.Deserialize<HipotCurveModel[]>(cmd);
            if (data == null)
               continue;
            ChartExporterHelper.ExportCurveChart(data!, writer);
            //完成信号
            writer.WriteLine("DONE");
         }
         catch (Exception ex)
         {
            // 异常回传（避免WPF卡死无反馈）
            writer.WriteLine($"ERROR|{ex.Message}");
         }
      }
   }
}
