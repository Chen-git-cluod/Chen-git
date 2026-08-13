namespace Kinlo.Equipment.ProtocolHelpers.CIP;

/// <summary>
///  无连接模式 [UCMM 无连接模式既为UCMM (常用)]
/// </summary>
public class OmronCipUcmmTcpProtocol : IProtocolHelper
{
   /// <summary>
   /// 会话句柄
   /// </summary>
   private byte[] _sessionHandle = new byte[4];

   /// <summary>
   /// PLC槽号
   /// </summary>
   private byte _plcSlotNumber;

   #region MSG
   private byte[] _header = new byte[24]
   {
      0x6F,
      0x00, //命令 2byte
      0x28,
      0x00, //长度 2byte（总长度-Header的长度）=40
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
   };

   private byte[] _commandSpecificData = new byte[16]
   {
      0x00,
      0x00,
      0x00,
      0x00, //接口句柄 CIP默认为0x00000000 4byte
      0x01,
      0x00, //超时默认0x0001 4byte
      0x02,
      0x00, //项数默认0x0002 4byte
      0x00,
      0x00, //空地址项默认0x0000 2byte
      0x00,
      0x00, //长度默认0x0000 2byte
      0xB2,
      0x00, //未连接数据项默认为 0x00b2
      0x00,
      0x00, //后面数据包的长度 24个字节(总长度-Header的长度-CommandSpecificData的长度)
   };
   private byte[] _cipMessage =
   [
      0x52,
      0x02, //服务默认0x52  请求路径大小 默认2
      0x20,
      0x06,
      0x24,
      0x01, //请求路径 默认0x01240622 4byte
      0x0A,
      0xF0, //超时默认0xF00A 4byte
      0x00,
      0x00, //Cip指令长度  服务标识到服务命令指定数据的长度
   ];

   #endregion

   /// <summary>
   /// 无连接模式 [UCMM 无连接模式既为UCMM]
   /// </summary>
   /// <param name="slotNumber"></param>
   /// <param name="session"></param>
   public OmronCipUcmmTcpProtocol(byte slotNumber, byte[] session)
   {
      _plcSlotNumber = slotNumber;
      _sessionHandle = session;
   }

   public bool Verify(IList<byte> bytes)
   {
      if (bytes.Count < 4)
      {
         return false;
      }
      int length = (bytes[2] + (bytes[3] << 8)) + 24;

      if (bytes.Count < length)
      {
         return false;
      }
      if (bytes[8] != 0 && bytes[9] != 0)
      {
         throw new Exceptions.CIPHeaderException(new byte[] { bytes[8], bytes[9] });
      }
      return true;
   }

   public byte[] Serialize(IList<byte> data)
   {
      int command_length = _cipMessage.Length + data.Count + 4;
      int protocol_length = _commandSpecificData.Length + command_length;
      _header[2] = (byte)protocol_length;
      _header[3] = (byte)(protocol_length >> 8);
      _header[4] = _sessionHandle[0];
      _header[5] = _sessionHandle[1];
      _header[6] = _sessionHandle[2];
      _header[7] = _sessionHandle[3];
      _commandSpecificData[14] = (byte)command_length;
      _commandSpecificData[15] = (byte)(command_length >> 8);
      _cipMessage[8] = (byte)(data.Count);
      _cipMessage[9] = (byte)(data.Count >> 8);

      List<byte> result = new List<byte>();
      result.AddRange(_header);
      result.AddRange(_commandSpecificData);
      result.AddRange(_cipMessage);
      result.AddRange(data);
      result.Add(0x01);
      result.Add(0x00);
      result.Add(0x01);
      result.Add(_plcSlotNumber);

      return result.ToArray();
   }

   public byte[] Deserialize(IList<byte> data)
   {
      // var f = OmronCipExplicitTcpProtocol.ExtractCipPayload(data.ToArray());
      // string s = BitConverter.ToString(data.ToArray());
      if (data.Count < 44)
      {
         throw new Exception("不满足正常CIP报文");
      }
      string error = ParseCipLabel.GetStatusCode(data[42]);
      if (error != "Success")
      {
         throw new Exception($"CIP协议报错：{error}");
      }
      return data.Skip(44).ToArray();
   }
}
