using HandyControl.Controls;

namespace Kinlo.Common.Models.ConfigModels;

[AddINotifyPropertyChangedInterface]
public class DeviceClientModel
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
   /// 超时时间
   /// </summary>
   public int OutTime { get; set; } = 2000;

   /// <summary>
   /// 连接类型
   /// </summary>
   public ConnectTypeEnum ConnectType { get; set; } = ConnectTypeEnum.TCP;

   /// <summary>
   /// 设备型号
   /// </summary>
   public HardwareTypeEnum HardwareType { get; set; }

   /// <summary>
   /// 设备通信接口
   /// </summary>
   //[JsonIgnore]
   //public IDevice? Device { get; set; }

   /// <summary>
   /// 设备通讯类型
   /// </summary>
   public CommunicationEnum Communication { get; set; } = CommunicationEnum.ScanCode_SR1000;

   /// <summary>
   /// 工序类型
   /// </summary>
   public ProcessTypeEnum ProcessesType { get; set; } = ProcessTypeEnum.前扫码;

   /// <summary>
   /// 自定义启动，不随点击运行时启动的仪器，而是另单独写启动，如果刷卡器这些
   /// </summary>
   public bool IsCustomBoot { get; set; }

   /// <summary>
   /// 是否启用
   /// </summary>
   public bool IsEnable { get; set; } = true;

   ///// <summary>
   ///// 连接数量，主要用来PLC多连接
   ///// </summary>
   //public ushort ConnectionNumber { get; set; } = 1;

   #region PING
   /// <summary>
   /// ping时间
   /// </summary>
   [JsonIgnore]
   public long PingTime { get; set; }

   /// <summary>
   /// Ping结果
   /// </summary>
   [JsonIgnore]
   public IPStatus ICMPResult { get; set; }

   /// <summary>
   /// 是否在线,1 在线，2 无法PING通，3 能Ping通，但无有效数据,99未启用或是串口
   /// </summary>
   [JsonIgnore]
   public int IsOnline { get; set; }

   [JsonIgnore]
   public int PingNGCount { get; set; }

   #endregion
}
