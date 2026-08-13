using System.Collections.Generic;
using System.Transactions;

namespace Kinlo.Equipment.ProtocolHelpers.Modbus;

public class Modbus_TCP_Protocol : IProtocolHelper
{
   private byte _slave;
   private static ushort transactionId = 0; // Transaction ID，从 0 开始
   private static readonly object _lockObj = new object();

   public Modbus_TCP_Protocol(byte slave)
   {
      _slave = slave;
   }

   public bool Verify(IList<byte> bytes)
   {
      if (bytes == null || bytes.Count < 6)
      {
         return false;
      }
      int length = bytes[4] << 8 | bytes[5];
      if (bytes.Count < length)
      {
         return false;
      }
      if (bytes[7] > 16)
      {
         throw new Exception($"Modbus tcp 错误代码：{bytes[7]}");
      }
      return true;
   }

   /// <summary>
   /// 多线程使用时注意每次请求TransactionId不要重复
   /// </summary>
   /// <returns></returns>
   public static byte[] GetTransactionId()
   {
      lock (_lockObj)
         return BitConverter.GetBytes(++transactionId);
   }

   public byte[] Serialize(IList<byte> data)
   {
      short _dataLength = (short)(data.Count + 1);
      List<byte> _bytes = new();
      _bytes.AddRange(GetTransactionId().Reverse()); //Transaction Id，2byte
      _bytes.AddRange([0x00, 0x00]); //protoco id 00表示Modbus，2byte
      _bytes.AddRange([(byte)(_dataLength >> 8), (byte)_dataLength]); //后续数据的长度，2byte
      _bytes.Add(_slave); //从设备ID， Unit ID，1byte
      _bytes.AddRange(data);
      // $"Serialize: {BitConverter.ToString(_bytes.ToArray())}".LogRun();
      return _bytes.ToArray();
      ;
   }

   public byte[] Deserialize(IList<byte> data)
   {
      switch (data[7])
      {
         case 0x03:
            return data.Skip(9).Take(data[8]).ToArray();
         case 0x06:

            break;
         case 0x10:

            break;
      }
      return data.ToArray();
   }
}
