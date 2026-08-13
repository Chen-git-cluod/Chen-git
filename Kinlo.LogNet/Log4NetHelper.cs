using System.Windows;
using Kinlo.LogNet.Models;

namespace Kinlo.LogNet;

public static class Log4NetHelper
{
   public static char EndMarker { get; set; } = '\u001E';
   public static Action<string, Log4NetLevelEnum>? PromptAction { get; set; }
   public static Action<string, Log4NetLevelEnum, Log4NetTypeEnum>? DisplayRunLogAction { get; set; }
   public static Action<MesLogModel>? DisplayMesLogAction { get; set; }
   public static Action<WebLogModel>? DisplayWebLogAction { get; set; }
   private static ILog _runLog = LogManager.GetLogger(Log4NetTypeEnum.运行日志.ToString());
   private static ILog _configLog = LogManager.GetLogger(Log4NetTypeEnum.配置修改日志.ToString());
   private static ILog _deviceAlarmLog = LogManager.GetLogger(Log4NetTypeEnum.设备报警日志.ToString());
   private static ILog _databaseLog = LogManager.GetLogger(Log4NetTypeEnum.操作数据库日志.ToString());
   private static ILog _databaseTimeoutLog = LogManager.GetLogger(Log4NetTypeEnum.操作数据库超时日志.ToString());

   private static SemaphoreSlim _runLock = new SemaphoreSlim(1, 1);

   /// <summary>
   /// 生产工序日志
   /// </summary>
   /// <param name="msg"></param>
   /// <param name="header"></param>
   /// <param name="level"></param>
   /// <param name="isPrompt"></param>
   public static void LogProcess(this string msg, string header, Log4NetLevelEnum level = Log4NetLevelEnum.信息, bool isPrompt = false) =>
      LogRun(msg, level, isPrompt, header);

   /// <summary>
   /// 运行日志
   /// </summary>
   /// <param name="msg"></param>
   /// <param name="level"></param>
   /// <param name="isPrompt">是否在右上角弹出提示窗口</param>
   public static void LogRun(this string msg, Log4NetLevelEnum level = Log4NetLevelEnum.信息, bool isPrompt = false, string header = "")
   {
      try
      {
         msg = string.IsNullOrWhiteSpace(header) ? $"[{level}] {msg}{EndMarker}" : $"[{level}] {header}：{msg}{EndMarker}";
         msg.WriteLog(level, _runLog, null);
      }
      catch (Exception ex)
      {
         msg = $"LogRun 添加日志信息\r\n[{msg}]r\rn时发生异常：\r\n{ex}";
         _runLog.Error(msg);
      }

      _ = Task.Run(async () =>
      {
         await _runLock.WaitAsync();
         try
         {
            DisplayRunLogAction?.Invoke(msg, level, Log4NetTypeEnum.运行日志);
            if (isPrompt)
               PromptAction?.Invoke(msg, level);
         }
         catch (Exception ex)
         {
            _runLog.Error($"LogRun 添加日志信息\r\n[{msg}]r\rn至UI时发生异常：\r\n{ex}");
         }
         finally
         {
            _runLock.Release();
         }
      });
   }

   private static SemaphoreSlim _logSettingLock = new SemaphoreSlim(1, 1);

   /// <summary>
   /// 记录修改设置日志
   /// </summary>
   /// <param name="msg"></param>
   /// <param name="level"></param>
   /// <param name="isPrompt">是否在右上角弹出提示窗口</param>
   public static void LogSetting(this string msg, Log4NetLevelEnum level = Log4NetLevelEnum.信息, bool isPrompt = false)
   {
      try
      {
         msg = $"[{level}] {msg}{EndMarker}";
         msg.WriteLog(level, _configLog, null);
      }
      catch (Exception ex)
      {
         msg = $"LogSetting 添加日志信息\r\n[{msg}]r\rn时发生异常：\r\n{ex}";
         _runLog.Error(msg);
      }

      _ = Task.Run(async () =>
      {
         await _logSettingLock.WaitAsync();
         try
         {
            DisplayRunLogAction?.Invoke(msg, level, Log4NetTypeEnum.配置修改日志);
            if (isPrompt)
               PromptAction?.Invoke(msg, level);
         }
         catch (Exception ex)
         {
            _runLog.Error($"LogSetting 添加日志信息\r\n[{msg}]r\rn至UI时发生异常：\r\n{ex}");
         }
         finally
         {
            _logSettingLock.Release();
         }
      });
   }

   private static SemaphoreSlim _logDeviceAlarmLock = new SemaphoreSlim(1, 1);

   /// <summary>
   /// 设备掉线报警日志
   /// </summary>
   /// <param name="msg"></param>
   /// <param name="level"></param>
   /// <param name="isPrompt"></param>
   public static void LogDeviceAlarm(this string msg, Log4NetLevelEnum level = Log4NetLevelEnum.信息, bool isPrompt = false)
   {
      try
      {
         msg = $"[{level}] {msg}{EndMarker}";
         msg.WriteLog(level, _deviceAlarmLog, null);
      }
      catch (Exception ex)
      {
         msg = $"LogDeviceAlarm 添加日志信息\r\n[{msg}]r\rn时发生异常：\r\n{ex}";
         _runLog.Error(msg);
      }

      _ = Task.Run(async () =>
      {
         await _logDeviceAlarmLock.WaitAsync();
         try
         {
            DisplayRunLogAction?.Invoke(msg, level, Log4NetTypeEnum.设备报警日志);
            if (isPrompt)
               PromptAction?.Invoke(msg, level);
         }
         catch (Exception ex)
         {
            _runLog.Error($"LogDeviceAlarm 添加日志信息\r\n[{msg}]r\rn至UI时发生异常：\r\n{ex}");
         }
         finally
         {
            _logDeviceAlarmLock.Release();
         }
      });
   }

   /// <summary>
   /// 数据库日志
   /// </summary>
   /// <param name="msg"></param>
   /// <param name="level"></param>
   /// <param name="isPrompt">是否在右上角弹出提示窗口</param>
   public static void LogDatabase(this string msg, Log4NetLevelEnum level = Log4NetLevelEnum.信息, bool isPrompt = false)
   {
      try
      {
         msg = $"[{level}] {msg}{EndMarker}\r\n";
         msg.WriteLog(level, _databaseLog, null);

         if (isPrompt)
            PromptAction?.Invoke(msg, level);
      }
      catch (Exception ex)
      {
         _runLog.Error($"LogSetting 添加日志信息\r\n[{msg}]r\rn时发生异常：\r\n{ex}");
      }
   }

   /// <summary>
   /// 数据库日志,sql语句超一定时间记录
   /// </summary>
   /// <param name="msg"></param>
   /// <param name="level"></param>
   /// <param name="isPrompt">是否在右上角弹出提示窗口</param>
   public static void LogDatabaseTimeout(this string msg, Log4NetLevelEnum level = Log4NetLevelEnum.警告, bool isPrompt = false)
   {
      try
      {
         msg = $"[{level}] {msg}{EndMarker}\r\n";
         msg.WriteLog(level, _databaseTimeoutLog, null);

         if (isPrompt)
            PromptAction?.Invoke(msg, level);
      }
      catch (Exception ex)
      {
         _runLog.Error($"LogSetting 添加日志信息\r\n[{msg}]r\rn时发生异常：\r\n{ex}");
      }
   }

   private static SemaphoreSlim _logMesLock = new SemaphoreSlim(1, 1);

   /// <summary>
   ///
   /// </summary>
   /// <param name="interfaceName">接口名</param>
   /// <param name="state">成功or失败or异常</param>
   /// <param name="barcode">条码</param>
   /// <param name="startTime">开始时间</param>
   /// <param name="endTime">结束时间</param>
   /// <param name="url"></param>
   /// <param name="requestedMsg">发送报文</param>
   /// <param name="receiveMsg">接收报文</param>
   /// <param name="languageDic">语言字典</param>
   /// <param name="level">等级</param>
   /// <param name="isDisplay">是否在UI上显示（一般心跳可能不显示，因为太多了）</param>
   /// <param name="isPrompt">是否弹窗</param>
   public static void LogMes(
      this string interfaceName,
      StatusTypeEnum state,
      string barcode,
      DateTime startTime,
      DateTime endTime,
      string url,
      string requestedMsg,
      string receiveMsg,
      ResourceDictionary languageDic,
      Log4NetLevelEnum level = Log4NetLevelEnum.信息,
      bool isDisplay = true,
      bool isPrompt = false
   )
   {
      Task.Run(async () =>
      {
         MesLogModel mesLog = new MesLogModel();
         string msg = string.Empty;
         try
         {
            //string requestedCulture = $@"Languages\{language}.xaml";
            //ResourceDictionary? _dictionary = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d =>
            //                                 d.Source != null && d.Source.OriginalString.Equals(requestedCulture));
            string languageInterfaceName = interfaceName;
            if (languageDic.Contains(interfaceName))
               languageInterfaceName = languageDic[interfaceName].ToString()!;

            double spending = Math.Round(endTime.Subtract(startTime).TotalMilliseconds, 3);
            string startTimeStr = startTime.ToLogTime();
            string endTimeStr = endTime.ToLogTime();
            var logReceiveMsg = receiveMsg.ToCsvSafeString();
            List<string> sendList = SplitStringIntoChunks(requestedMsg);
            msg =
               @$"{languageInterfaceName},{state},{barcode},'{startTimeStr},'{endTimeStr},{spending},{url},{logReceiveMsg},{string.Join(',', sendList)}";

            mesLog.MesInterfaceName = languageInterfaceName;
            mesLog.Status = state;
            mesLog.Barcode = barcode;
            mesLog.StartTime = startTimeStr;
            mesLog.EndTime = endTimeStr;
            mesLog.Spending = spending.ToString();
            mesLog.Url = url;
            mesLog.RequestedMsg = requestedMsg;
            mesLog.ReceiveMsg = receiveMsg;
            ILog log = LogManager.GetLogger(interfaceName);
            msg.WriteLog(level, log, mesLog);
         }
         catch (Exception ex)
         {
            msg =
               $"LogMes 添加日志信息时发生异常,接口：[{interfaceName}]\r\n发送报文：[{requestedMsg}]\r\n返回报文：[{receiveMsg}]\r\n异常详情：{ex}";
            _runLog.Error(msg);
         }

         await _logMesLock.WaitAsync();
         try
         {
            if (isDisplay)
               DisplayMesLogAction?.Invoke(mesLog);
            if (isPrompt)
               PromptAction?.Invoke(msg, level);
         }
         catch (Exception ex)
         {
            _runLog.Error($"LogMes 添加日志信息\r\n[{msg}]r\rn至UI时发生异常：\r\n{ex}");
         }
         finally
         {
            _logMesLock.Release();
         }
      });
   }

   private static SemaphoreSlim _logWebLock = new SemaphoreSlim(1, 1);

   /// <summary>
   ///
   /// </summary>
   /// <param name="webLog"></param>
   /// <param name="languageDic">语言字典</param>
   /// <param name="isPrompt">是否弹窗</param>
   public static void LogWeb(this WebLogModel webLog, ResourceDictionary languageDic, bool isPrompt = false)
   {
      Task.Run(async () =>
      {
         string msg = string.Empty;
         try
         {
            //string requestedCulture = $@"Languages\{language}.xaml";
            //ResourceDictionary? _dictionary = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d =>
            //                                 d.Source != null && d.Source.OriginalString.Equals(requestedCulture));

            if (languageDic.Contains(webLog.InterfaceName))
               webLog.InterfaceName = languageDic[webLog.InterfaceName].ToString()!;

            if (webLog.StartTime != default && webLog.EndTime != default)
               webLog.Spending = Math.Round(webLog.EndTime.Subtract(webLog.StartTime).TotalMilliseconds, 3);

            string startTimeStr = webLog.StartTime.ToLogTime();
            string endTimeStr = webLog.EndTime.ToLogTime();
            var logrequestMsg = webLog.RequestedMsg.ToCsvSafeString();
            var logresponseMsg = webLog.ResponseMsg.ToCsvSafeString();

            msg =
               @$"{webLog.Level},{webLog.InterfaceName},{webLog.Status},{webLog.Barcode},'{startTimeStr},'{endTimeStr},{webLog.Spending},{webLog.Url},{logrequestMsg},{logresponseMsg},{webLog.TraceId}";

            ILog log = LogManager.GetLogger(webLog.InterfaceName);
            msg.WriteLog(webLog.Level, log, webLog);
         }
         catch (Exception ex)
         {
            msg =
               $"LogWeb 添加日志信息时发生异常,接口：[{webLog.InterfaceName}]\r\n接收报文：[{webLog.RequestedMsg}]\r\n回复报文：[{webLog.ResponseMsg}]\r\n异常详情：{ex}";
            _runLog.Error(msg);
         }

         await _logWebLock.WaitAsync();
         try
         {
            DisplayWebLogAction?.Invoke(webLog);
            if (isPrompt)
               PromptAction?.Invoke(msg, webLog.Level);
         }
         catch (Exception ex)
         {
            _runLog.Error(
               $"LogWeb 添加日志至UI时发生异常,接口：[{webLog.InterfaceName}]\r\n接收报文：[{webLog.RequestedMsg}]\r\n回复报文：[{webLog.ResponseMsg}]\r\n异常详情：{ex}"
            );
         }
         finally
         {
            _logWebLock.Release();
         }
      });
   }

   #region 辅助方法
   private static void WriteLog(this string msg, Log4NetLevelEnum level, ILog log, ILogInfo? webLog)
   {
      switch (level)
      {
         case Log4NetLevelEnum.错误:
            log.Error(msg);
            if (webLog != null)
               webLog.ErrVisibility = Visibility.Visible;
            break;
         case Log4NetLevelEnum.警告:
            log.Warn(msg);
            if (webLog != null)
               webLog.WarningVisibility = Visibility.Visible;
            break;
         case Log4NetLevelEnum.信息:
            log.Info(msg);
            if (webLog != null)
               webLog.MessageVisibility = Visibility.Visible;
            break;
         case Log4NetLevelEnum.成功:
            log.Debug(msg);
            if (webLog != null)
               webLog.SuccessVisibility = Visibility.Visible;
            break;
         default:
            log.Info(msg);
            if (webLog != null)
               webLog.MessageVisibility = Visibility.Visible;
            break;
      }
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="msg"></param>
   /// <returns></returns>
   public static string ToCsvSafeString(this string msg) => $"\"{msg?.Replace("\"", "\"\"") ?? ""}\"";

   public static string ToLogTime(this DateTime? dateTime) => dateTime?.ToString("yyyy/MM/dd HH:mm:ss.fff") ?? "";

   public static string ToLogTime(this DateTime dateTime) => dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff");

   /// <summary>
   /// 分割过长的报文
   /// </summary>
   /// <param name="input"></param>
   /// <param name="chunkSize"> 32000;// 32767 excel单元格字符最长长度，如超长分两个单元格;</param>
   /// <returns></returns>
   private static List<string> SplitStringIntoChunks(string input, int chunkSize = 32000)
   {
      if (string.IsNullOrEmpty(input))
         return new List<string> { string.Empty };

      var chunks = new List<string>();
      int length = input.Length;

      for (int i = 0; i < length; i += chunkSize)
      {
         int count = Math.Min(chunkSize, length - i);
         var str = input.Substring(i, count);
         str = str.ToCsvSafeString();
         chunks.Add(str);
      }
      return chunks;
   }
   #endregion
}
