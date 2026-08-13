using System;

namespace Kinlo.Equipment.ProtocolHelpers;

/// <summary>
///  有连接模式 [有连接显式模式（Explicit），有连接分为显式和隐式]
/// </summary>
public class OmronCipExplicitTcpProtocol : IProtocolHelper
{
   /// <summary>
   /// 会话句柄
   /// </summary>
   private byte[] _sessionHandle = new byte[4];

   /// <summary>
   /// Cip有连接标识
   /// </summary>
   private byte[] _connectionId = new byte[4];

   /// <summary>
   /// 有连接模式 [有连接显式模式（Explicit），有连接分为显式和隐式]
   /// </summary>
   /// <param name="sessionHandle"></param>
   /// <param name="connectionId"></param>
   public OmronCipExplicitTcpProtocol(byte[] sessionHandle, byte[] connectionId)
   {
      _sessionHandle = sessionHandle;
      _connectionId = connectionId;
   }

   public bool Verify(IList<byte> bytes)
   {
      if (bytes.Count < 4)
      {
         System.Threading.Thread.Sleep(2);
         return false;
      }
      int length = (bytes[2] + (bytes[3] << 8)) + 24;

      if (bytes.Count < length)
      {
         // System.Threading.Thread.Sleep(2);
         return false;
      }
      if (bytes[8] != 0 && bytes[9] != 0)
      {
         throw new CIPHeaderException(new byte[] { bytes[8], bytes[9] });
      }
      return true;
   }

   object _serializeLock = new object();
   ushort _sequenceNumber = 0;

   public byte[] Serialize(IList<byte> bytes)
   {
      if (_sessionHandle == null)
      {
         throw new Exception("会话未注册！！！");
      }
      List<byte> tableBytes = new List<byte>();
      lock (_serializeLock)
      {
         _sequenceNumber++;
         tableBytes.Add((byte)_sequenceNumber); //Sequence Number
         tableBytes.Add((byte)(_sequenceNumber >> 8)); //Sequence Number
      }
      tableBytes.AddRange(bytes);
      int tableLength = tableBytes.Count;
      byte[] commandSpecificData =
      {
         0x00,
         0x00,
         0x00,
         0x00, //接口句柄 CIP默认为0x00000000 4byte
         0x00,
         0x00, //超时默认0x0001 4byte
         0x02,
         0x00, //项数默认0x0002 4byte
         0xA1,
         0x00,
         0x04,
         0x00,
         _connectionId[0],
         _connectionId[1],
         _connectionId[2],
         _connectionId[3], // Connection ID
         0xB1,
         0x00,
         (byte)tableLength,
         (byte)(tableLength >> 8),
      };

      int bodyLength = commandSpecificData.Length + tableBytes.Count;
      byte[] senderContext = BitConverter.GetBytes(DateTime.UtcNow.Ticks); // 或随意 8 字节唯一值
      byte[] header = new byte[24]
      {
         0x70,
         0x00, //命令 2byte
         (byte)bodyLength,
         (byte)(bodyLength >> 8), //长度 2byte（总长度-Header的长度）=40
         _sessionHandle[0],
         _sessionHandle[1],
         _sessionHandle[2],
         _sessionHandle[3], //会话句柄 4byte
         0x00,
         0x00,
         0x00,
         0x00, //状态默认0 4byte
         senderContext[0],
         senderContext[1],
         senderContext[2],
         senderContext[3], //发送方描述 Sender Context 8byte
         senderContext[4],
         senderContext[5],
         senderContext[6],
         senderContext[7], //发送方描述 Sender Context 8byte
         0x00,
         0x00,
         0x00,
         0x00, //选项默认0 4byte
      };

      List<byte> result = new List<byte>();
      result.AddRange(header);
      result.AddRange(commandSpecificData);
      result.AddRange(tableBytes);
      //var msg = BitConverter.ToString(result.ToArray()).Replace('-', ' ');
      //$"发送报文：{msg}".LogRun(Log4NetLevelEnum.警告);
      return result.ToArray();
   }

   public byte[] Deserialize(IList<byte> data)
   {
      // var f= ExtractCipPayload(data.ToArray());
      //  var msg = BitConverter.ToString(data.ToArray()).Replace('-', ' ');
      // $"响应：{msg}".LogRun(Log4NetLevelEnum.警告);
      if (data.Count < 24 || data.Count != ((data[2] + (data[3] << 8)) + 24))
      {
         throw new Exception("不满足正常CIP报文");
      }
      if (data[49] != 0 || data[48] != 0)
      {
         if (data.Count >= 52)
         {
            throw new Exception(
               $"报文异常代码：Main[0x{((data[49] << 8) + data[48]):X}],Sub[0x{((data[51] << 8) + data[50]):X}] 描述:[{ParseCipLabel.GetStatusCode(data[48])}]"
            );
         }
         else
         {
            throw new Exception($"报文异常代码：[0x{((data[49] << 8) + data[48]):X}] 描述:[{ParseCipLabel.GetStatusCode(data[48])}]");
         }
      }
      return data.Skip(50).ToArray();
   }
}
