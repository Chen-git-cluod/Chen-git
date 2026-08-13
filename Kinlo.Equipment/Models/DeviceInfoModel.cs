namespace Kinlo.Equipment.Models;

public class DeviceInfoModel
{
   /// <summary>
   /// 服务名
   /// </summary>
   public string ServiceName { get; set; } = string.Empty;

   /// <summary>
   /// 序号
   /// </summary>
   public byte Index { get; set; } = 1;

   /// <summary>
   /// 波特率或端口号
   /// </summary>
   public int Port { get; set; }

   /// <summary>
   /// IP或CMD
   /// </summary>
   public string IPCOM { get; set; } = string.Empty;

   /// <summary>
   /// 实例Socket时Bind的IP及端口，用于指定网口等
   /// </summary>
   public IPAddress? BindIP { get; set; }

   /// <summary>
   /// Fins协议需要用到,网卡IP地址最后一位
   /// </summary>
   // public byte IPLastDigit { get; set; }

   public int Timeout { get; set; } = 3000;

   ///// <summary>
   ///// 连接数量，主要用来PLC多连接
   ///// </summary>
   //public ushort ConnectionNumber { get; set; }

   #region 串口
   /// <summary>
   /// 校验位
   /// </summary>
   public Parity Parity { get; set; } = Parity.None;
   public int DataBits { get; set; } = 8;
   public StopBits StopBits { get; set; } = StopBits.One;
   #endregion
   public CancellationTokenSource TaskToken { get; set; } = new CancellationTokenSource();

   /// <summary>
   /// 工序类型
   /// </summary>
   public ProcessTypeEnum ProcessesType { get; set; } = ProcessTypeEnum.前扫码;

   /// <summary>
   /// 设备通讯协议类型
   /// </summary>
   public CommunicationEnum Communication { get; set; } = CommunicationEnum.ScanCode_SR1000;

   /// <summary>
   /// 设备型号
   /// </summary>
   public HardwareTypeEnum HardwareType { get; set; }

   /// <summary>
   /// 连接类型
   /// </summary>
   public ConnectTypeEnum ConnectType { get; set; } = ConnectTypeEnum.TCP;
}
