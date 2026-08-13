using System.Windows;
using Kinlo.LogNet.Models;

namespace Kinlo.LogNet;

/// <summary>
/// 保存为文件再加载入内存，弃用
/// </summary>
[Obsolete("已弃用，不使用文件写入硬盘，直接载入内存")]
public class Log4NetInitialization
{
   public static string _log4NetConfigName = "log4net.config";
   public static string _log4NetPath = $"{Environment.CurrentDirectory}\\{_log4NetConfigName}";
   public static List<Log4NetConfigModel> Log4NetConfigs { get; set; } = new List<Log4NetConfigModel>();

   public static void SaveLog4NetXmlConfig() =>
      File.WriteAllText($".\\{_log4NetConfigName}", Log4NetXmlConfigBuilder.BuildLog4NetXml(Log4NetConfigs));

   public static void Log4NetXmlLoadConfiguration()
   {
      var r = log4net.Config.XmlConfigurator.Configure(new FileInfo(_log4NetPath));
      //if (r.Count != 0)
      //{
      //    MessageBox.Show($"[Log4Net加载有错误] {string.Join(",", r)}");
      //   // Log4NetHelper.LogRun($"[Log4Net加载有错误] {string.Join(",", r)}", Log4NetLevelEnum.错误);
      //}
   }
}
