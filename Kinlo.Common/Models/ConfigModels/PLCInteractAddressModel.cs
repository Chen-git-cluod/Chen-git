using NPOI.SS.Formula.Functions;

namespace Kinlo.Common.Models.ConfigModels;

[AddINotifyPropertyChangedInterface]
public class PLCInteractAddressModel
{
   /// <summary>
   /// 服务名
   /// </summary>
   public string ServiceName { get; set; } = string.Empty;

   /// <summary>
   ///  启动命令
   /// </summary>
   public GenericCommandModel StartCommand { get; set; } = new();

   /// <summary>
   ///  工序类型
   /// </summary>
   public ProcessTypeEnum ProcessesType { get; set; }

   /// <summary>
   /// 设备通信类型
   /// </summary>
   public CommunicationEnum DeviceCommunicationType { get; set; }

   /// <summary>
   /// 设备启始索引
   /// </summary>
   public byte DeviceStartIndex { get; set; }

   /// <summary>
   /// 设备数量或数据长度
   /// </summary>
   public int DataLength { get; set; } = 1;

   /// <summary>
   /// 工序获取数据标签
   /// </summary>
   public SignalAddressModel DataAddress { get; set; } = new();

   /// <summary>
   /// 备用：其它数据
   /// </summary>
   public SignalAddressModel ExtraDataAddress { get; set; } = new();

   /// <summary>
   /// 工位的扩展属性
   /// </summary>
   public ObservableCollection<ExtensionItem> ExtensionProps { get; set; } = new();

   /// <summary>
   /// 显示数据类型
   /// </summary>
   public ProductionDataTypeEnum ProductionDataType { get; set; } = ProductionDataTypeEnum.主页显示数据;

   /// <summary>
   /// 生产顺序
   /// </summary>
   public int ProductionIndex { get; set; } = 1;

   ///// <summary>
   ///// 是否最后一个工序，一般是最后一个工序出站
   ///// </summary>
   //public bool IsLastProcess  { get; set; }

   [JsonIgnore]
   private int _key;

   /// <summary>
   /// 扫码信号lock使用的KEY,避免每次重新生成KEY,每次初始化时生成
   /// </summary>
   [JsonIgnore]
   public int Key
   {
      get => _key;
   }

   public void CreateKey() =>
      _key =
         $"{ServiceName}{(int)ProcessesType}{(int)DeviceCommunicationType}{StartCommand.Tag}{DeviceStartIndex}{StartCommand.Index}".GetHashCode();

   /// <summary>
   /// 是否启用
   /// </summary>
   public bool IsEnable { get; set; } = true;
}

/// <summary>
/// 为兼容多场景，Value有三个类型，但  Str 及int 、double 三个值都应该是相同的同一样值，
/// </summary>
[AddINotifyPropertyChangedInterface]
public class ExtensionItem
{
   public ExtensionItem() { }

   public ExtensionItem(ExtensionType type, string val)
   {
      Type = type;
      ValueStr = val;
   }

   public ExtensionType Type { get; set; }
   private string _valueStr = "-1";

   /// <summary>
   /// UI绑定
   /// </summary>
   public string ValueStr
   {
      get { return _valueStr; }
      set
      {
         if (_valueStr != value)
         {
            _valueStr = value;
            if (int.TryParse(value, out var i))
               ValueInt = i;
            else
               ValueInt = -1; //默认值 -1
            if (double.TryParse(value, out var d))
               ValueDouble = d;
            else
               ValueDouble = -1; //默认值 -1
         }
      }
   }

   [JsonIgnore]
   public int ValueInt { get; private set; } = -1;

   [JsonIgnore]
   public double ValueDouble { get; private set; } = -1;
}

public enum ExtensionType
{
   行数,
   列数,
   层号,
   注液泵温度,
   注液泵温度补偿,
   注液泵工艺补偿,

   长度1 = 101,
   长度2 = 102,
   长度3 = 103,
}
