using System.Windows;

namespace Kinlo.LogNet.Models;

public class MesLogModel : ILogInfo
{
   /// <summary>
   /// 接口名
   /// </summary>
   public string MesInterfaceName { get; set; } = string.Empty;

   /// <summary>
   /// 状态
   /// </summary>
   public StatusTypeEnum Status { get; set; }

   /// <summary>
   /// 条码
   /// </summary>
   public string Barcode { get; set; } = string.Empty;

   /// <summary>
   /// 开始时间
   /// </summary>
   public string StartTime { get; set; } = string.Empty;

   /// <summary>
   /// 结束时间
   /// </summary>
   public string EndTime { get; set; } = string.Empty;

   /// <summary>
   /// 耗时(ms)
   /// </summary>
   public string Spending { get; set; } = string.Empty;
   public string Url { get; set; } = string.Empty;

   /// <summary>
   /// 请求报文
   /// </summary>
   public string RequestedMsg { get; set; } = string.Empty;

   /// <summary>
   /// 返回报文
   /// </summary>
   public string ReceiveMsg { get; set; } = string.Empty;
   public Log4NetLevelEnum Level { get; set; }
   public Visibility ErrVisibility { get; set; } = Visibility.Collapsed;
   public Visibility WarningVisibility { get; set; } = Visibility.Collapsed;
   public Visibility SuccessVisibility { get; set; } = Visibility.Collapsed;
   public Visibility MessageVisibility { get; set; } = Visibility.Collapsed;
}
