using System.Windows;
using Kinlo.LogNet.Models;

namespace Kinlo.LogNet;

public static class Log4NetInitializer
{
   public static bool LoadBasicLogConfig()
   {
      try
      {
         var list = CreateRunLogConfigure();
         LoadConfigureFromMemory(list);
         return true;
      }
      catch (Exception ex)
      {
         MessageBox.Show($"加载基础日志异常：{ex}");
         return false;
      }
   }

   public static bool LoadFullLogConfig(List<string> mesInterfaceNames, List<string> webInterfaceNames)
   {
      try
      {
         var list = CreateRunLogConfigure();
         list.AddRange(CreateMesLogConfigure(mesInterfaceNames));
         list.AddRange(CreateWebLogConfigure(webInterfaceNames));
         LoadConfigureFromMemory(list);
         return true;
      }
      catch (Exception ex)
      {
         MessageBox.Show($"加载完整日志异常：{ex}");
         return false;
      }
   }

   private static List<Log4NetConfigModel> CreateRunLogConfigure()
   {
      var logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
      var Log4NetConfigs = new List<Log4NetConfigModel>();
      //加入运行各种类型日志配置
      foreach (var model in Enum.GetValues(typeof(Log4NetTypeEnum)))
      {
         Log4NetConfigs.Add(
            new Log4NetConfigModel
            {
               LoggerName = model.ToString(),
               DatePattern = "yyyy-MM-dd/HH'.txt'",
               SavePath = Path.Combine(logDir, model.ToString()) + Path.DirectorySeparatorChar,
               ConversionPattern = "%date{yyyy-MM-dd HH:mm:ss fff} %message%newline",
            }
         );
      }
      return Log4NetConfigs;
   }

   private static List<Log4NetConfigModel> CreateMesLogConfigure(List<string> mesInterfaceNames)
   {
      var baseDir = AppContext.BaseDirectory;
      var logDir = Path.Combine(baseDir, "Logs", Log4NetTypeEnum.MES日志.ToString());
      var Log4NetConfigs = new List<Log4NetConfigModel>();

      foreach (var name in mesInterfaceNames)
      {
         if (string.IsNullOrWhiteSpace(name))
            continue;
         Log4NetConfigs.Add(
            new Log4NetConfigModel
            {
               LoggerName = name.ToString()!,
               DatePattern = "yyyy-MM-dd/HH'.csv'",
               SavePath = Path.Combine(logDir, name) + Path.DirectorySeparatorChar,
               ConversionPattern = "%message%newline",
               // Header = "信息等级,接口,状态,条码,开始时间,结束时间,耗时(ms),URL,请求参数1,请求参数2,返回参数,设备编码\r\n"
               Header = "等级,接口,状态,条码,开始时间,结束时间,耗时(ms),URL,返回报文,请求报文1,请求报文2,请求报文3,请求报文4,请求报文5\r\n",
            }
         );
      }
      return Log4NetConfigs;
   }

   private static List<Log4NetConfigModel> CreateWebLogConfigure(List<string> webInterfaceNames)
   {
      var baseDir = AppContext.BaseDirectory;
      var logDir = Path.Combine(baseDir, "Logs", Log4NetTypeEnum.Web服务日志.ToString());
      var Log4NetConfigs = new List<Log4NetConfigModel>();

      foreach (var name in webInterfaceNames)
      {
         if (string.IsNullOrWhiteSpace(name))
            continue;
         Log4NetConfigs.Add(
            new Log4NetConfigModel
            {
               LoggerName = name.ToString()!,
               DatePattern = "yyyy-MM-dd/HH'.csv'",
               SavePath = Path.Combine(logDir, name) + Path.DirectorySeparatorChar,
               ConversionPattern = "%message%newline",
               // Header = "信息等级,接口,状态,条码,开始时间,结束时间,耗时(ms),URL,请求参数1,请求参数2,返回参数,设备编码\r\n"
               Header = "等级,接口,状态,条码,开始时间,结束时间,耗时(ms),URL,接收报文,回复报文,跟踪ID\r\n",
            }
         );
      }
      return Log4NetConfigs;
   }

   /// <summary>
   /// log4配置直接从内存加载
   /// </summary>
   /// <param name="Log4NetConfigures"></param>
   /// <exception cref="InvalidOperationException"></exception>
   private static void LoadConfigureFromMemory(List<Log4NetConfigModel> Log4NetConfigures)
   {
      var xml = Log4NetXmlConfigBuilder.BuildLog4NetXml(Log4NetConfigures);

      // 加载为 XmlDocument
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.LoadXml(xml);

      // 找到 <log4net> 节点,内存加载须从<log4net> 节点开始
      var node = xmlDoc.SelectSingleNode("//log4net");
      if (node is not XmlElement element)
         throw new InvalidOperationException("未找到 <log4net> 根节点。");

      // 清除旧配置
      log4net.LogManager.ResetConfiguration();

      // 从内存加载配置
      log4net.Config.XmlConfigurator.Configure(element);
   }

   public static string _log4NetConfigName = "log4net.config";
   public static string _log4NetPath = $"{Environment.CurrentDirectory}\\{_log4NetConfigName}";

   /// <summary>
   /// log4配置先保存在本地,再读取,优先使用内存直取形式
   /// </summary>
   /// <param name="Log4NetConfigures"></param>
   private static void LoadConfigureFromFile(List<Log4NetConfigModel> Log4NetConfigures)
   {
      var xml = Log4NetXmlConfigBuilder.BuildLog4NetXml(Log4NetConfigures);

      File.WriteAllText($".\\{_log4NetConfigName}", xml);
      // 清除旧配置
      log4net.LogManager.ResetConfiguration();
      log4net.Config.XmlConfigurator.Configure(new FileInfo(_log4NetPath));
   }
}
