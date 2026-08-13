namespace Kinlo.Equipment.Models;

public class CipClient : IDisposable
{
   private readonly CancellationTokenSource _taskToken;
   private Timer? _dogTimer;
   private readonly object _lock = new();
   private readonly TimeSpan _intervalTime;
   public int Index { get; }
   public IConnect? Conn { get; set; }

   /// <summary>
   /// TCP 连接层级的标识
   /// </summary>
   public byte[]? Session { get; private set; }

   /// <summary>
   /// Connection ID,当前CIP为有连接模式时，执行 ForwardOpen 成功之后，返回ID
   /// </summary>
   public byte[]? ConnectionId { get; set; }

   /// <summary>
   /// 连接模式
   /// </summary>
   public CipMode ConnectMode { get; set; } = CipMode.无连接模式;

   /// <summary>
   ///
   /// </summary>
   public ForwardOpenContext? ForwardContext { get; private set; } = null;
   private IProtocolHelper? _protocol;

   public IProtocolHelper? Protocol
   {
      get { return _protocol; }
      set
      {
         if (_protocol != value)
         {
            _protocol = value;
            StartFeedingDog(); //启动喂狗
         }
      }
   }

   public CipClient(IConnect conn, int index, byte[]? session, CancellationTokenSource TaskToken)
   {
      Index = index;
      Conn = conn;
      Session = session;
      _taskToken = TaskToken;
      _intervalTime = TimeSpan.FromSeconds(conn.DeviceInfo.Communication == CommunicationEnum.CipInovance ? 3 : 16); //汇川3秒喂狗，欧姆龙16秒喂狗
      _taskToken.Token.Register(Dispose);
   }

   /// <summary>
   /// 修复
   /// </summary>
   public void Repair(IConnect conn, byte[] session, IProtocolHelper protocol, byte[]? connectionId, ForwardOpenContext? context)
   {
      Conn = conn;
      Session = session;
      Protocol = protocol;
      if (connectionId != null)
         ConnectionId = connectionId;
      if (context != null)
         ForwardContext = context;
   }

   // 设置 ForwardContext 的方法，只能调用一次
   public void SetForwardContext(ForwardOpenContext context)
   {
      if (ForwardContext != null)
         throw new InvalidOperationException("ForwardContext 已经被设置，不能修改。");

      ForwardContext = context;
   }

   #region 喂狗计划

   /// <summary>
   /// 开始喂狗
   /// </summary>
   private void StartFeedingDog()
   {
      lock (_lock)
      {
         _dogTimer ??= new Timer(
            _ =>
            {
               FeedWatchdog();
               OnWatchdogFed(); // 每次喂狗后手动启动，重新计时
            },
            null,
            Timeout.Infinite,
            Timeout.Infinite
         ); //Timeout.Infinite 无限等待，除非手动启动

         OnWatchdogFed(); //手动启动第一次
      }
   }

   int _dogFoodIndex = 0;

   /// <summary>
   /// 喂狗
   /// </summary>
   private void FeedWatchdog()
   {
      if (Conn == null)
         return;

      byte[] dogFood = [];
      string logHeader = $"{Conn.DeviceInfo.ToDeviceLogHeader()}-[Feed dog]";

      try
      {
         if (_taskToken == null || _taskToken.IsCancellationRequested) //已停机
            return;

         dogFood = _dogFoodBag[_dogFoodIndex % _dogFoodBag.Count];
         _dogFoodIndex++;
         var res = Conn.WriteAndRead(dogFood, Protocol, logHeader);
         if (res.State == CommState.NeedReconnect)
            this.RepairCip();
         // $"Connect number：[{Index}] Feed dog finish: food [{BitConverter.ToString(dogFood).Replace('-', ' ')}],bark [{BitConverter.ToString(bytes).Replace('-', ' ')}]".LogProcess(logHeader, Log4NetLevelEnum.成功);
      }
      catch (Exception ex)
      {
         $"Connect number：[{Index}] Feed dog 异常: 报文[{BitConverter.ToString(dogFood).Replace('-', ' ')}]\r\n详情：{ex}".LogRun(
            Log4NetLevelEnum.错误
         );
      }
   }

   /// <summary>
   /// 狗粮包：用于喂狗（定时保活）的小型、通用 Get_Attribute_Single 请求路径集合
   /// </summary>
   private readonly List<byte[]> _dogFoodBag = new()
   {
      new byte[] { 0x0E, 0x03, 0x20, 0x01, 0x24, 0x01, 0x30, 0x01 }, // Vendor ID
      new byte[] { 0x0E, 0x03, 0x20, 0x01, 0x24, 0x01, 0x30, 0x07 }, // Product Name
      new byte[] { 0x0E, 0x03, 0x20, 0x01, 0x24, 0x01, 0x30, 0x04 }, // Revision
      new byte[] { 0x0E, 0x03, 0x20, 0x01, 0x24, 0x01, 0x30, 0x06 }, // Serial Number
   };

   /// <summary>
   /// 喂狗完成,重置计时器,准备下一次喂狗
   /// </summary>
   public void OnWatchdogFed()
   {
      lock (_lock)
         _dogTimer?.Change(_intervalTime, Timeout.InfiniteTimeSpan);
   }

   /// <summary>
   /// 清除资源
   /// </summary>
   public void Dispose()
   {
      lock (_lock)
      {
         _dogTimer?.Dispose();
         _dogTimer = null;
      }
   }
   #endregion
}

/// <summary>
///
/// </summary>
/// <param name="ConnectionId"></param>
/// <param name="ConnectionPath">连接路径</param>
/// <param name="ConnectionSerialNo"></param>
/// <param name="OriginatorSerialNo"></param>
/// <param name="SenderContext">发送者上下文，可能是用于跟踪会话的唯一标识符</param>
/// <param name="VendorId">供应商 ID，通常用于标识设备或协议的供应商</param>
public record ForwardOpenContext(
   byte[] ConnectionId,
   byte[] ConnectionPath,
   byte[] ConnectionSerialNo,
   byte[] OriginatorSerialNo,
   byte[] SenderContext,
   byte[] VendorId
);

public enum CipMode
{
   无连接模式,
   有连接模式,
   有连接模式_每次重连,
}
