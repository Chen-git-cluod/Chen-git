using System.Windows;

namespace Kinlo.LogNet.Models;

public class RunLogModel : ILogInfo
{
   /// <summary>
   /// 时间
   /// </summary>
   public string Time { get; set; } = string.Empty;

   /// <summary>
   /// 消息
   /// </summary>
   public string Message { get; set; } = string.Empty;
   public Log4NetLevelEnum Level { get; set; }

   /// <summary>
   /// 状态
   /// </summary>
   public StatusTypeEnum Status { get; set; } = StatusTypeEnum.失败;

   public Visibility ErrVisibility { get; set; } = Visibility.Collapsed;
   public Visibility WarningVisibility { get; set; } = Visibility.Collapsed;
   public Visibility SuccessVisibility { get; set; } = Visibility.Collapsed;
   public Visibility MessageVisibility { get; set; } = Visibility.Collapsed;
}
