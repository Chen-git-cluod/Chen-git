using System.Windows;

namespace Kinlo.LogNet.Models;

public interface ILogInfo
{
   /// <summary>
   /// 状态
   /// </summary>
   public StatusTypeEnum Status { get; set; }
   public Log4NetLevelEnum Level { get; set; }
   Visibility ErrVisibility { get; set; }
   Visibility WarningVisibility { get; set; }
   Visibility SuccessVisibility { get; set; }
   Visibility MessageVisibility { get; set; }
}
