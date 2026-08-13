namespace Kinlo.Common.Models.LogModels;

[AddINotifyPropertyChangedInterface]
public class LogHistoryModel
{
   public string Log4NetType { get; set; } = string.Empty;
   public object CurrentLogs { get; set; }
   public FrameworkElement View { get; set; }
}
