namespace Kinlo.LogNet.Models;

public class Log4NetConfigModel
{
   /// <summary>
   /// 日志名
   /// </summary>
   public string LoggerName { get; set; } = string.Empty;

   /// <summary>
   /// 保存路径
   /// </summary>
   public string SavePath { get; set; } = string.Empty;

   /// <summary>
   /// 文件名
   /// </summary>
   public string DatePattern { get; set; } = string.Empty;

   /// <summary>
   /// 备份文件个数
   /// </summary>
   public int MaxSizeRollBackups { get; set; } = 24;

   /// <summary>
   /// 日志文件最大大小
   /// </summary>
   public string MaxFileSize { get; set; } = "10MB";

   /// <summary>
   /// 输出格式
   /// </summary>
   public string ConversionPattern { get; set; } = "%message %newline";

   /// <summary>
   /// MES抬头
   /// </summary>
   public string Header { get; set; } = string.Empty;
}
