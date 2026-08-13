namespace Kinlo.Common.Models.ConfigModels;

[AddINotifyPropertyChangedInterface]
public class PLCScanSignalModel
{
   /// <summary>
   /// 服务名称
   /// </summary>
   public string ServiceName { get; set; } = string.Empty;

   /// <summary>
   /// 设备通信类型
   /// </summary>
   public CommunicationEnum DeviceCommunicationType { get; set; }

   /// <summary>
   /// 命令地址
   /// </summary>
   public SignalAddressModel AddressStart { get; set; } = new SignalAddressModel();

   /// <summary>
   /// 信号长度
   /// </summary>
   public int LengthSignal { get; set; }

   /// <summary>
   /// 切除长度
   /// </summary>
   public int LengthResection { get; set; }

   /// <summary>
   /// 寻址方式
   /// </summary>
   public string AddressingMethod { get; set; } = string.Empty;

   /// <summary>
   /// 心跳标签
   /// </summary>
   public SignalAddressModel Heartbeat { get; set; } = new SignalAddressModel();

   /// <summary>
   /// plc心跳间隔时间，单位：毫秒，默认500ms
   /// </summary>
   public int HeartbeatIntervalMs { get; set; } = 500;

   /// <summary>
   /// PLC状态
   /// </summary>
   public SignalAddressModel Status { get; set; } = new SignalAddressModel();

   /// <summary>
   /// 触发信号列表
   /// </summary>
   public ObservableCollection<GenericCommandModel> StartSignas { get; set; } = new ObservableCollection<GenericCommandModel>();

   /// <summary>
   /// PLC切除信号列表
   /// </summary>
   public ObservableCollection<GenericCommandModel> PLCResections { get; set; } = new ObservableCollection<GenericCommandModel>();
}
