using System.Xml.Linq;
using Kinlo.LogNet.Models;

namespace Kinlo.LogNet;

/// <summary>
/// 组装XML（XDocument版本）
/// </summary>
public class Log4NetXmlConfigBuilder
{
   public static string BuildLog4NetXml(List<Log4NetConfigModel> list)
   {
      var doc = new XDocument(
         new XDeclaration("1.0", "utf-8", null),
         new XElement(
            "configuration",
            // configSections
            new XElement(
               "configSections",
               new XElement(
                  "section",
                  new XAttribute("name", "log4net"),
                  new XAttribute("type", "log4net.Config.Log4NetConfigurationSectionHandler,log4net")
               )
            ),
            // log4net 内容
            new XElement("log4net", from item in list from el in CreateLoggerAndAppender(item) select el)
         )
      );

      return doc.ToString(SaveOptions.DisableFormatting);
   }

   private static IEnumerable<XElement> CreateLoggerAndAppender(Log4NetConfigModel item)
   {
      string appenderName = $"{char.ToUpper(item.LoggerName[0])}{item.LoggerName.Substring(1)}Appender";

      // logger
      var logger = new XElement(
         "logger",
         new XAttribute("name", item.LoggerName),
         new XElement("appender-ref", new XAttribute("ref", appenderName))
      );

      // appender
      var appender = new XElement(
         "appender",
         new XAttribute("name", appenderName),
         new XAttribute("type", "Kinlo.LogNet.MyRollingFileAppender"),
         new XElement("param", new XAttribute("name", "Encoding"), new XAttribute("value", "utf-8")),
         new XElement("file", new XAttribute("value", item.SavePath)),
         new XElement("appendToFile", new XAttribute("value", "true")),
         new XElement("maxSizeRollBackups", new XAttribute("value", item.MaxSizeRollBackups)),
         new XElement("maximumFileSize", new XAttribute("value", item.MaxFileSize)),
         new XElement("rollingStyle", new XAttribute("value", "Date")),
         new XElement("datePattern", new XAttribute("value", item.DatePattern)),
         new XElement("staticLogFileName", new XAttribute("value", "false")),
         new XElement(
            "layout",
            new XAttribute("type", "log4net.Layout.PatternLayout"),
            new XElement("header", new XAttribute("value", item.Header)),
            new XElement("conversionPattern", new XAttribute("value", item.ConversionPattern))
         )
      );

      return new[] { logger, appender };
   }

   public static List<Log4NetConfigModel> ReadLog4NetConfig(string path)
   {
      var doc = XDocument.Load(path);
      var log4net = doc.Root?.Element("log4net");
      if (log4net == null)
         return [];

      var loggers = log4net.Elements("logger");
      var appenders = log4net.Elements("appender").ToDictionary(a => a.Attribute("name")?.Value ?? string.Empty, a => a);

      var list = new List<Log4NetConfigModel>();

      foreach (var logger in loggers)
      {
         var loggerName = logger.Attribute("name")?.Value ?? string.Empty;
         var appenderRef = logger.Element("appender-ref")?.Attribute("ref")?.Value ?? string.Empty;

         if (appenders.TryGetValue(appenderRef, out var appender))
         {
            var layout = appender.Element("layout");
            var header = layout?.Element("header")?.Attribute("value")?.Value ?? string.Empty;
            var conversion = layout?.Element("conversionPattern")?.Attribute("value")?.Value ?? string.Empty;

            list.Add(
               new Log4NetConfigModel
               {
                  LoggerName = loggerName,
                  SavePath = appender.Element("file")?.Attribute("value")?.Value ?? string.Empty,
                  MaxFileSize = appender.Element("maximumFileSize")?.Attribute("value")?.Value ?? string.Empty,
                  DatePattern = appender.Element("datePattern")?.Attribute("value")?.Value ?? string.Empty,
                  MaxSizeRollBackups = int.TryParse(appender.Element("maxSizeRollBackups")?.Attribute("value")?.Value, out var max)
                     ? max
                     : 0,
                  ConversionPattern = conversion,
                  Header = header,
               }
            );
         }
      }

      return list;
   }
}
