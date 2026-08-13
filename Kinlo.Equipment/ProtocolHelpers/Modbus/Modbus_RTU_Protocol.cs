namespace Kinlo.Equipment.ProtocolHelpers.Modbus;

public class Modbus_RTU_Protocol : IProtocolHelper
{
   public bool Verify(IList<byte> bytes)
   {
      if (bytes == null || bytes.Count < 6)
         return false;
      int _length = bytes[2];
      if (bytes.Count < _length + 5)
         return false;

      var _crc16 = GetModbusCRC16(bytes.ToArray(), bytes.Count - 2);
      if (_crc16[0] == bytes[bytes.Count - 2] && _crc16[1] == bytes[bytes.Count - 1])
         return true;
      else
         return false;
   }

   public byte[] Serialize(IList<byte> data)
   {
      return data.ToArray();
   }

   public byte[] Deserialize(IList<byte> data)
   {
      return data.Skip(3).Take(data.Count - 5).ToArray();
   }

   /// <summary>
   /// CRC16_Modbus效验
   /// </summary>
   /// <param name="byteData">要进行计算的字节数组</param>
   /// <param name="byteLength">长度</param>
   /// <returns>计算后的数组</returns>
   public static byte[] GetModbusCRC16(byte[] byteData, int byteLength)
   {
      byte[] CRC = new byte[2];

      ushort wCrc = 0xFFFF;
      for (int i = 0; i < byteLength; i++)
      {
         wCrc ^= Convert.ToUInt16(byteData[i]);
         for (int j = 0; j < 8; j++)
         {
            if ((wCrc & 0x0001) == 1)
            {
               wCrc >>= 1;
               wCrc ^= 0xA001; //异或多项式
            }
            else
            {
               wCrc >>= 1;
            }
         }
      }

      CRC[1] = (byte)((wCrc & 0xFF00) >> 8); //高位在后
      CRC[0] = (byte)(wCrc & 0x00FF); //低位在前
      return CRC;
   }

   /// <summary>
   /// CRC16_Modbus效验
   /// </summary>
   /// <param name="byteData">要进行计算的字节数组</param>
   /// <returns>计算后的数组</returns>
   public static byte[] GetModbusCRC16(byte[] byteData)
   {
      byte[] CRC = new byte[2];

      ushort wCrc = 0xFFFF;
      for (int i = 0; i < byteData.Length; i++)
      {
         wCrc ^= Convert.ToUInt16(byteData[i]);
         for (int j = 0; j < 8; j++)
         {
            if ((wCrc & 0x0001) == 1)
            {
               wCrc >>= 1;
               wCrc ^= 0xA001; //异或多项式
            }
            else
            {
               wCrc >>= 1;
            }
         }
      }

      CRC[1] = (byte)((wCrc & 0xFF00) >> 8); //高位在后
      CRC[0] = (byte)(wCrc & 0x00FF); //低位在前
      return CRC;
   }
}
