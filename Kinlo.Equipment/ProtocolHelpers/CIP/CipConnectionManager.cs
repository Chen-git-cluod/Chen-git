namespace Kinlo.Equipment.ProtocolHelpers.CIP;

internal static class CipConnectionManager
{
   #region 注册,注销
   /// <summary>
   /// 注册报文
   /// </summary>
   private static byte[] RegisterMsg { get; set; } =
      new byte[28]
      {
         //-------------------Header 24byte------------
         0x65,
         0x00, //命令 2byte
         0x04,
         0x00, //Header后面数据的长度 2byte
         0x00,
         0x00,
         0x00,
         0x00, //会话句柄 4byte
         0x00,
         0x00,
         0x00,
         0x00, //状态默认0 4byte
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00, //发送方描述默认0 8byte
         0x00,
         0x00,
         0x00,
         0x00, //选项默认0 4byte
         //------------------CommandSpecificData 指令指定数据 4byte------------
         0x01,
         0x00, //协议版本 2byte
         0x00,
         0x00, //选项标记 2byte
      };

   public static byte[]? Register(IConnect tcpConnect, string logHeader)
   {
      tcpConnect.Write(RegisterMsg, logHeader);
      Thread.Sleep(20);
      var res = tcpConnect.Read(64, logHeader);
      if (res.State != CommState.Success)
         throw new Exception($"CIP注册失败：{res.Message}");
      var bytes = res.Data!;
      if (bytes.Length != 28)
         throw new Exception($"CIP注册失败：返回长度错误");

      int status = (bytes[8] + (bytes[9] << 8) + (bytes[10] << 16) + (bytes[11] << 24));
      if (status != 0)
      {
         throw new Exception($"CIP注册失败，状态码 [{ExplainEipStatus(status)}];");
      }

      var sessionHandle = bytes.Skip(4).Take(4).ToArray();
      return sessionHandle;
   }

   private static string ExplainEipStatus(int status)
   {
      return status switch
      {
         0x00 => "成功",
         0x01 => "不支持的命令",
         0x02 => "数据长度不足",
         0x03 => "无效的 Session Handle",
         0x64 => "无效的选项标志",
         0x65 => "不支持的协议版本",
         0x66 => "选项不被支持",
         0x67 => "会话数超过限制",
         _ => $"未知状态码: 0x{status:X8}",
      };
   }

   public static bool CloseCipConnect(IConnect connect, byte[] session, string logHeader)
   {
      byte[] cancellationCMD =
      [
         0x66,
         0,
         0,
         0,
         0x71,
         0x01,
         0x04,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
         0x00,
      ];
      cancellationCMD[4] = session[0];
      cancellationCMD[5] = session[1];
      cancellationCMD[6] = session[2];
      cancellationCMD[7] = session[3];

      var res = connect.Write(cancellationCMD, logHeader);
      if (res.State != CommState.Success)
      {
         $"写入关闭CIP指令失败！".LogProcess(logHeader, Log4NetLevelEnum.警告);
         return false;
      }
      connect.Close();
      return true;
   }
   #endregion

   #region Explicit Class3 [class3 显式连接(class1 为udp隐式连接)]
   static uint _connId = 0; //O→T Conn ID ✅ 必须唯一
   static short _connectionSerialId = 0; // Connection Serial Number ✅ 建议唯一
   static uint _originatorSerialId = 0; // Originator Serial Number ✅ 建议唯一
   static ulong _senderContext = 0; // Sender Context ⚠️ 建议唯一（用于回应匹配）
   static readonly byte[] _connectionPath = [0x20, 0x02, 0x24, 0x01]; //连接路径
   static readonly object lockObj = new object();

   /// <summary>
   /// 获取 Forward 上下文
   /// </summary>
   /// <returns></returns>
   private static ForwardOpenContext GetForwardOpenContext()
   {
      byte[] vendorId = [0x34, 0x12]; //Vendor ID 标识供应商的唯一标识符。（随便写）
      byte[]? connIdBytes = null;
      byte[]? connectionSerialIdBytes = null;
      byte[]? originatorSerialIdBytes = null;
      byte[]? senderContextBytes = null;
      lock (lockObj)
      {
         _connId++;
         _connectionSerialId++;
         _originatorSerialId--;
         _senderContext--;
         connIdBytes = BitConverter.GetBytes(_connId);
         connectionSerialIdBytes = BitConverter.GetBytes(_connectionSerialId);
         originatorSerialIdBytes = BitConverter.GetBytes(_originatorSerialId);
         senderContextBytes = BitConverter.GetBytes(_senderContext);
      }
      return new ForwardOpenContext(
         connIdBytes,
         _connectionPath,
         connectionSerialIdBytes,
         originatorSerialIdBytes,
         senderContextBytes,
         vendorId
      );
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="session"></param>
   /// <param name="ips"></param>
   /// <returns></returns>
   private static byte[] GetForwardOpenMsg(byte[] session, ForwardOpenContext forwardOpenContext, CommunicationEnum Communication)
   {
      byte[] timeoutMultiplier = Communication == CommunicationEnum.CipInovance ? [0x00, 0x00] : [0x03, 0x00]; // Connection Timeout Multiplier	2 B	连接超时乘数
      byte[] rpi =
         Communication == CommunicationEnum.CipInovance
            ? [0x80, 0x84, 0x1E, 0x00] //= 2,000,000 μs = 2 秒   //O→T RPI	4 B	Originator → Target 的 Requested Packet Interval
            : [0x40, 0x42, 0x0F, 0x00]; // = 1,000,000 μs = 1 秒   //O→T RPI	4 B	Originator → Target 的 Requested Packet Interval
      List<byte> forwardOpenMsg =
      [
         0x5B, // [40]    Service Code = 0x5B（Forward_Open）
         0x02, // [41]    Path size = 2 words = 4 bytes
         0x20,
         0x06, // [42-43] Class ID = 0x06（Connection Manager）
         0x24,
         0x01, // [44-45] Instance ID = 0x01
         0x03, // [46]    Priority & Timeout ticks = 0x03
         0x64, // [47]    Timeout ticks = 100
         forwardOpenContext.ConnectionId[0],
         forwardOpenContext.ConnectionId[1],
         forwardOpenContext.ConnectionId[2],
         forwardOpenContext.ConnectionId[3], // [48-51] O→T Conn ID ✅ 必须唯一
         0x00,
         0x00,
         0x00,
         0x00, // [52-55] T→O Conn ID（可设 0，目标设备生成）
         forwardOpenContext.ConnectionSerialNo[0],
         forwardOpenContext.ConnectionSerialNo[1], // [56-57] Connection Serial Number  建议唯一
         forwardOpenContext.VendorId[0],
         forwardOpenContext.VendorId[1], // [58-59] Vendor ID（随便写）
         forwardOpenContext.OriginatorSerialNo[0],
         forwardOpenContext.OriginatorSerialNo[1],
         forwardOpenContext.OriginatorSerialNo[2],
         forwardOpenContext.OriginatorSerialNo[3], // [60-63] Originator Serial Number ✅ 建议唯一
         timeoutMultiplier[0],
         timeoutMultiplier[1], // Connection Timeout Multiplier	2 B	连接超时乘数
         0x00,
         0x00, //Reserved	2 B	保留（设为0）
         rpi[0],
         rpi[1],
         rpi[2],
         rpi[3], // = 1,000,000 μs = 1 秒   //O→T RPI	4 B	Originator → Target 的 Requested Packet Interval
         0xCC,
         0x07,
         0x00,
         0x42, //O→T Connection Parameters	2 B	数据大小 + 2 B传输类型等
         //
         rpi[0],
         rpi[1],
         rpi[2],
         rpi[3], // = 1,000,000 μs = 1 秒   //	T→O RPI	4 B	Target → Originator 的周期时间
         0xCC,
         0x07,
         0x00,
         0x42, //O→T Connection Parameters	2 B	数据大小 + 2 B传输类型等
         0xA3, // [79]    Transport Type/Trigger（0xA3 = Class 3, Trigger=Cyclic）
         0x02, // [80]    Path size again = 2
         forwardOpenContext.ConnectionPath[0],
         forwardOpenContext.ConnectionPath[1], // [81-82] Class = 0x02（Message Router）
         forwardOpenContext.ConnectionPath[2],
         forwardOpenContext.ConnectionPath[3], // [83-84] Instance = 0x01
      ];

      var msgLenghtBytes = BitConverter.GetBytes((ushort)forwardOpenMsg.Count);

      List<byte> sendRRDataContent =
      [
         0x00,
         0x00,
         0x00,
         0x00, // [24-27] Interface Handle = 0（固定）
         0x0A,
         0x00, // [28-29] Timeout（10秒）
         0x02,
         0x00, // [30-31] Item Count = 2（地址项 + 数据项）
         // Item 1: Address Item (Null, 4 bytes)
         0x00,
         0x00, // [32-33] Type ID = 0x0000（Null Address）
         0x00,
         0x00, // [34-35] Length = 0（固定）
         // Item 2: Data Item (Forward_Open, 42 bytes)
         0xB2,
         0x00, // [36-37] Type ID = 0x00B2（Unconnected Message）
         msgLenghtBytes[0],
         msgLenghtBytes[1], // [38-39] Length = 40 bytes
      ];

      List<byte> body = new List<byte>();
      body.AddRange(sendRRDataContent);
      body.AddRange(forwardOpenMsg);

      byte[] bodyLenght = BitConverter.GetBytes((ushort)body.Count);

      List<byte> bytes =
      [
         // ———— Encapsulation Header (24 bytes) ————
         0x6F,
         0x00, // [0-1] Command: 0x006F = SendRRData（固定）
         bodyLenght[0],
         bodyLenght[1], // [2-3] Length: 0x002E = 46 bytes（下方数据长度）
         session[0],
         session[1],
         session[2],
         session[3], // [4-7] Session Handle ✅ 必须唯一（来自 RegisterSession）
         0x00,
         0x00,
         0x00,
         0x00, // [8-11] Status = 0（固定）
         forwardOpenContext.SenderContext[0],
         forwardOpenContext.SenderContext[1],
         forwardOpenContext.SenderContext[2],
         forwardOpenContext.SenderContext[3],
         forwardOpenContext.SenderContext[4],
         forwardOpenContext.SenderContext[5],
         forwardOpenContext.SenderContext[6],
         forwardOpenContext.SenderContext[7], // [12-19] Sender Context ⚠️ 建议唯一（用于回应匹配）
         0x00,
         0x00,
         0x00,
         0x00, // [20-23] Options = 0（固定）
      ];

      bytes.AddRange(body);
      return bytes.ToArray();
   }

   /// <summary>
   ///  通信转入有连接状态，返回连接标识
   /// </summary>
   /// <param name="tcpConnect"></param>
   /// <param name="sessionHandle"></param>
   /// <returns></returns>
   /// <exception cref="Exception"></exception>
   public static (ForwardOpenContext context, byte[] connectionId) ExplicitClass3ForwardOpen(
      IConnect tcpConnect,
      byte[] sessionHandle,
      string logHeader
   )
   {
      ForwardOpenContext forwardOpenContext = GetForwardOpenContext();
      var openBytes = GetForwardOpenMsg(sessionHandle, forwardOpenContext, tcpConnect.DeviceInfo.Communication);
      //  $"ForwardOpen:{BitConverter.ToString(openBytes, 0, openBytes.Length).Replace('-', ' ')}".LogRun(Log4NetLevelEnum.信息);
      tcpConnect.Write(openBytes, logHeader);
      var res = tcpConnect.Read(320, logHeader);
      if (res.State != CommState.Success)
         throw new Exception($"ForwardOpen失败:{res.Message!}");
      // $"ForwardReceive:{BitConverter.ToString(bytes).Replace('-',' ')}".LogRun(Log4NetLevelEnum.信息);
      var bytes = res.Data!;
      if (bytes.Length < 48 || bytes.Length != (bytes[2] + 24))
         throw new Exception($"ForwardOpen失败：返回长度错误");
      if (bytes[42] != 0)
      {
         throw new Exception($"ForwardOpen错误：{ExplainCipGeneralStatus(bytes[42])}");
      }
      return (forwardOpenContext, [bytes[44], bytes[45], bytes[46], bytes[47]]);
   }

   private static string ExplainCipGeneralStatus(byte status) =>
      status switch
      {
         0x00 => "成功",
         0x01 => "不支持的服务（Unsupported service）",
         0x02 => "对象不存在（Object does not exist）",
         0x03 => "属性不存在（Property does not exist）",
         0x04 => "属性不可写（Property not writable）",
         0x05 => "非法数据值（Illegal data value）",
         0x0B => "未满足的条件（Condition not met）",
         0x13 => "服务不被目标支持（Forward Open not supported）",
         0x14 => "连接资源不足（Out of connection resources）",
         0x15 => "重复连接（Duplicate Forward Open）",
         0x16 => "无效连接类型（Unsupported connection type）",
         0x1F => "参数错误（Invalid parameter）",
         0x20 => "Transport Trigger 无效（Invalid transport trigger）",
         _ => $"未知状态码: 0x{status:X2}",
      };

   private static byte[] GetForwardCloseMsg(byte[] session, ForwardOpenContext context)
   {
      byte[] bytes =
      [
         // Encapsulation Header (24 bytes)
         //-------------------------------
         0x6F,
         0x00, // Command = SendRRData (0x006F)
         0x26,
         0x00, // Length = 24 bytes (SendRRData payload from byte 0x00 to 0x17)
         session[0],
         session[1],
         session[2],
         session[3], // Session Handle = 0x01020304
         0x00,
         0x00,
         0x00,
         0x00, // Status
         context.SenderContext[0],
         context.SenderContext[1],
         context.SenderContext[2],
         context.SenderContext[3],
         context.SenderContext[4],
         context.SenderContext[5],
         context.SenderContext[6],
         context.SenderContext[7], // Sender Context
         0x00,
         0x00,
         0x00,
         0x00, // Options
         //SendRRData Payload (24 bytes)
         //-------------------------------
         0x00,
         0x00, // Interface Handle
         0x00,
         0x00, // Timeout
         0x0B,
         0x00,
         0x02,
         0x00, // Item Count
         0x00,
         0x00, // Null Address Type
         0x00,
         0x00, // Null Address Length
         0xB2,
         0x00, // Unconnected Data Type
         0x16,
         0x00, // Length = 18 bytes (CIP payload)
         //CIP Payload (18 bytes)
         //-------------------------------
         0x4E, // Service: Forward_Close
         0x02, // Path size = 2 words (4 bytes)
         0x20,
         0x06, // Class = Connection Manager
         0x24,
         0x01, // Instance 1
         0x0A,
         0x0E, // Priority/Timeout
         context.ConnectionSerialNo[0],
         context.ConnectionSerialNo[1], // Connection Serial = 0x1234
         context.VendorId[0],
         context.VendorId[1], // Vendor ID = 0x0001
         context.OriginatorSerialNo[0],
         context.OriginatorSerialNo[1],
         context.OriginatorSerialNo[2],
         context.OriginatorSerialNo[3], // Originator Serial = 0xABCDEF12
         0x01,
         0x00, // Port 0
         _connectionPath[0],
         _connectionPath[1], // [81-82] Class = 0x02（Message Router）
         _connectionPath[2],
         _connectionPath[3], // [83-84] Instance = 0x01
      ];
      return bytes;
   }

   /// <summary>
   /// 终止CIP连接(是CIP连接，而非TCP)
   /// </summary>
   /// <returns></returns>
   public static bool ExplicitClass3ForwardClose(IConnect Connect, byte[] sessionHandle, ForwardOpenContext context, string logHeader)
   {
      byte[] btyes = GetForwardCloseMsg(sessionHandle, context);
      // $"ForwardClose发送:{BitConverter.ToString(btyes)}".LogRun(Log4NetLevelEnum.信息);
      var writeRes = Connect.Write(btyes, logHeader);
      if (writeRes.State != CommState.Success)
      {
         $"释放CIP有连接指令失败！".LogProcess(logHeader, Log4NetLevelEnum.警告);
         return false;
      }
      var response = Connect.Read(100, logHeader);
      // $"ForwardClose返回:{BitConverter.ToString(response).Replace('-',' ')}".LogRun(Log4NetLevelEnum.信息);
      if (response.State == CommState.Success)
      {
         if (response.Data!.Length == 54)
            return true;
         else
         {
            $"获取释放CIP有连接状态长度错误！".LogProcess(logHeader, Log4NetLevelEnum.警告);
            return false;
         }
      }
      return false;
   }

   #endregion
}
