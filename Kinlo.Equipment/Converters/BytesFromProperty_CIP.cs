namespace Kinlo.Equipment.Converters;

public class BytesFromProperty_CIP : IBytesFromProperty
{
   public object GetPropertyValue(
      Type propertyType,
      byte[] bytes,
      ref double numBytes,
      ref int count,
      CommunicationEnum endianTypes = CommunicationEnum.FinsTcp
   )
   {
      object value = null;
      switch (propertyType.Name)
      {
         case "Boolean":
            int bytePos = (int)Math.Floor(numBytes);
            int bitPos = (int)((numBytes - (double)bytePos) / 0.125);
            value = (bytes[bytePos] & (int)Math.Pow(2, bitPos)) != 0;
            numBytes += 0.125;
            break;
         case "Byte":
            numBytes = Math.Ceiling(numBytes);
            value = (byte)(bytes[(int)numBytes]);
            numBytes++;
            break;
         case "Int16":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            // hier auswerten
            value = BitConverter.ToInt16(bytes, (int)numBytes);
            numBytes += 2;
            break;
         case "UInt16":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            // hier auswerten
            value = BitConverter.ToUInt16(bytes, (int)numBytes);
            numBytes += 2;
            break;
         case "Int32":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            value = BitConverter.ToInt32(bytes, (int)numBytes);
            numBytes += 4;
            break;
         case "UInt32":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            // hier auswerten
            value = BitConverter.ToUInt32(bytes, (int)numBytes);
            numBytes += 4;
            break;
         case "Single":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            value = BitConverter.ToSingle(bytes, (int)numBytes);
            numBytes += 4;
            break;
         case "UInt64":
         case "Int64":
         case "Double":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            switch (propertyType.Name)
            {
               case "UInt64":
                  value = BitConverter.ToUInt64(bytes, (int)numBytes);
                  break;
               case "Int64":
                  value = BitConverter.ToInt64(bytes, (int)numBytes);
                  break;
               case "Double":
                  value = BitConverter.ToDouble(bytes, (int)numBytes);
                  break;
            }
            numBytes += 8;
            break;
         case "String":

            break;
         default:
            var propClass = Activator.CreateInstance(propertyType);
            numBytes = StructToBytes.FromBytes(propClass, bytes, ref count, numBytes, endianTypes);
            value = propClass;
            break;
      }
      return value;
   }

   public double SetBytesFromProperty(
      object propertyValue,
      byte[] bytes,
      double numBytes,
      ref int count,
      CommunicationEnum endianTypes = CommunicationEnum.FinsTcp
   )
   {
      int bytePos = 0;
      int bitPos = 0;
      byte[]? bytes2 = null;

      switch (propertyValue.GetType().Name)
      {
         case "Boolean":
            count++;
            bytePos = (int)Math.Floor(numBytes);
            bitPos = (int)((numBytes - (double)bytePos) / 0.125);

            if ((bool)propertyValue)
               bytes[bytePos] |= (byte)Math.Pow(2, bitPos);

            numBytes += 0.125;
            break;
         case "Byte":
            numBytes = (int)Math.Ceiling(numBytes);
            bytes[(int)numBytes] = (byte)propertyValue;
            numBytes++;
            break;
         case "Int16":
            bytes2 = BitConverter.GetBytes((short)propertyValue);
            break;
         case "UInt16":
            bytes2 = BitConverter.GetBytes((ushort)propertyValue);
            break;
         case "Int32":
         case "UInt32":
         case "Single":
            switch (propertyValue.GetType().Name)
            {
               case "Single":
                  bytes2 = BitConverter.GetBytes((float)propertyValue);
                  break;
               case "Int32":
                  bytes2 = BitConverter.GetBytes((int)propertyValue);
                  break;
               case "UInt32":
                  bytes2 = BitConverter.GetBytes((uint)propertyValue);
                  break;
            }
            break;
         case "Int64":
         case "UInt64":
         case "Double":
            switch (propertyValue.GetType().Name)
            {
               case "UInt64":
                  bytes2 = BitConverter.GetBytes((ulong)propertyValue).Reverse().ToArray();
                  break;
               case "Int64":
                  bytes2 = BitConverter.GetBytes((long)propertyValue);
                  break;
               case "Double":
                  bytes2 = BitConverter.GetBytes((double)propertyValue);
                  break;
            }
            break;
         case "String":
            //string wirten = propertyValue as string;
            //if (!string.IsNullOrEmpty(wirten))
            //{
            //    var ascii = Encoding.ASCII.GetBytes(wirten);

            //    int ascii_length = ascii.Length % 2 != 0 ? ascii.Length + 1 : ascii.Length;//判断是否长度为奇数，是则补零

            //    byte[] strBytes = new byte[ascii_length + 2];

            //    strBytes[0] = BitConverter.GetBytes(ascii_length)[0];
            //    strBytes[1] = BitConverter.GetBytes(ascii_length)[1];

            //    Array.Copy(ascii, 0, strBytes, 2, ascii.Length);
            //    bytes2 = strBytes;
            //}
            break;
         default:
            numBytes = StructToBytes.ToBytes(propertyValue, bytes, ref count, numBytes, endianTypes);
            break;
      }

      if (bytes2 != null)
      {
         StructToBytes.IncrementToEven(ref numBytes);

         for (int bCnt = 0; bCnt < bytes2.Length; bCnt++)
            bytes[(int)numBytes + bCnt] = bytes2[bCnt];
         numBytes += bytes2.Length;
      }

      return numBytes;
   }
}
