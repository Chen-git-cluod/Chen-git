namespace Kinlo.Equipment.Converters;

internal static class ByteConverter
{
   /// <summary>
   ///
   /// </summary>
   /// <param name="data"></param>
   /// <param name="type"></param>
   /// <param name="count">为要取的byte长度；string要指定长度，一般其它类型无需指定，当然也可以强制指定</param>
   /// <param name="byteOffset"></param>
   /// <param name="bitOffset"></param>
   /// <returns></returns>
   /// <exception cref="ArgumentOutOfRangeException"></exception>
   /// <exception cref="NotSupportedException"></exception>
   public static object? ToValue(this byte[] data, Type type, int count, ref int byteOffset, ref byte bitOffset)
   {
      if (byteOffset < 0 || byteOffset >= data.Length)
         throw new ArgumentOutOfRangeException(nameof(byteOffset), "byteOffset 超出范围！");

      if (type == typeof(bool))
      {
         if (bitOffset < 0 || bitOffset >= 8)
            throw new ArgumentOutOfRangeException(nameof(bitOffset), "bitIndex 必须在 0~7 之间");

         bool b = ((data[byteOffset] >> bitOffset) & 1) == 1;

         // 更新位置
         bitOffset++;
         if (bitOffset >= 8)
         {
            byteOffset += 1;
            bitOffset = 0;
         }

         return b;
      }

      int size = count > 0 ? count : GetTypeSize(type);
      if (byteOffset + size > data.Length)
         return null;

      object val = type switch
      {
         var t when t == typeof(short) => BitConverter.ToInt16(data, byteOffset),
         var t when t == typeof(ushort) => BitConverter.ToUInt16(data, byteOffset),
         var t when t == typeof(int) => BitConverter.ToInt32(data, byteOffset),
         var t when t == typeof(uint) => BitConverter.ToUInt32(data, byteOffset),
         var t when t == typeof(long) => BitConverter.ToInt64(data, byteOffset),
         var t when t == typeof(ulong) => BitConverter.ToUInt64(data, byteOffset),
         var t when t == typeof(float) => BitConverter.ToSingle(data, byteOffset),
         var t when t == typeof(double) => BitConverter.ToDouble(data, byteOffset),
         var t when t == typeof(string) => Encoding.UTF8.GetString(data, byteOffset, count),
         _ => throw new NotSupportedException($"不支持类型 {type.Name}"),
      };

      byteOffset += size;
      return val;
   }

   private static int GetTypeSize(Type type) =>
      type switch
      {
         var t when t == typeof(short) || t == typeof(ushort) => 2,
         var t when t == typeof(int) || t == typeof(uint) || t == typeof(float) => 4,
         var t when t == typeof(long) || t == typeof(ulong) || t == typeof(double) => 8,
         _ => throw new NotSupportedException($"未知大小类型 {type.Name}"),
      };
}
