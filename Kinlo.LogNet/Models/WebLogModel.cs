using System.Windows;

namespace Kinlo.LogNet.Models;

public enum StatusTypeEnum
{
   失败,
   成功,
}

public class WebLogModel : ILogInfo
{
   /// <summary>
   /// 跟踪ID
   /// </summary>
   public string TraceId { get; set; } = string.Empty;

   /// <summary>
   /// 接口名
   /// </summary>
   public string InterfaceName { get; set; } = string.Empty;

   /// <summary>
   /// 状态
   /// </summary>
   public StatusTypeEnum Status { get; set; } = StatusTypeEnum.失败;

   /// <summary>
   /// 开始时间
   /// </summary>
   public DateTime StartTime { get; set; }

   /// <summary>
   /// 结束时间
   /// </summary>
   public DateTime EndTime { get; set; }

   /// <summary>
   /// 耗时(ms)
   /// </summary>
   public double Spending { get; set; }

   /// <summary>
   /// 条码
   /// </summary>
   public string Barcode { get; set; } = string.Empty;
   public string Url { get; set; } = string.Empty;

   /// <summary>
   /// 接收报文
   /// </summary>
   public string RequestedMsg { get; set; } = string.Empty;

   /// <summary>
   /// 回复报文
   /// </summary>
   public string ResponseMsg { get; set; } = string.Empty;
   public Log4NetLevelEnum Level { get; set; }
   public Visibility ErrVisibility { get; set; } = Visibility.Collapsed;
   public Visibility WarningVisibility { get; set; } = Visibility.Collapsed;
   public Visibility SuccessVisibility { get; set; } = Visibility.Collapsed;
   public Visibility MessageVisibility { get; set; } = Visibility.Collapsed;
}
