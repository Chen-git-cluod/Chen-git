namespace Kinlo.Equipment.Converters;

public class BytesFromProperty_Fins : IBytesFromProperty
{
   public object? GetPropertyValue(
      Type propertyType,
      byte[] bytes,
      ref double numBytes,
      ref int count,
      CommunicationEnum endianTypes = CommunicationEnum.FinsTcp
   )
   {
      object? value = null;
      switch (propertyType.Name)
      {
         case "Boolean":
            count++;
            int bytePos = (int)Math.Floor(numBytes);
            int bitPos = (int)((numBytes - (double)bytePos) / 0.125);
            if (count > 8)
            {
               value = (bytes[bytePos - 1] & (int)Math.Pow(2, bitPos)) != 0;
            }
            else
            {
               value = (bytes[bytePos + 1] & (int)Math.Pow(2, bitPos)) != 0;
            }
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
            value = BitConverter.ToInt16(new byte[] { bytes[(int)numBytes + 1], bytes[(int)numBytes] });
            numBytes += 2;
            break;
         case "UInt16":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            // hier auswerten
            value = BitConverter.ToUInt16(new byte[] { bytes[(int)numBytes + 1], bytes[(int)numBytes] });
            numBytes += 2;
            break;
         case "Int32":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            value = BitConverter.ToInt32(
               new byte[] { bytes[(int)numBytes + 1], bytes[(int)numBytes], bytes[(int)numBytes + 3], bytes[(int)numBytes + 2] }
            );
            numBytes += 4;
            break;
         case "UInt32":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            // hier auswerten
            value = BitConverter.ToUInt32(
               new byte[] { bytes[(int)numBytes + 1], bytes[(int)numBytes], bytes[(int)numBytes + 3], bytes[(int)numBytes + 2] }
            );
            numBytes += 4;
            break;
         case "Single":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            value = BitConverter.ToSingle(
               new byte[] { bytes[(int)numBytes + 1], bytes[(int)numBytes], bytes[(int)numBytes + 3], bytes[(int)numBytes + 2] }
            );
            numBytes += 4;
            break;
         case "UInt64":
         case "Int64":
         case "Double":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            var buffer = new byte[8];
            buffer[0] = bytes[(int)numBytes + 1];
            buffer[1] = bytes[(int)numBytes];
            buffer[2] = bytes[(int)numBytes + 3];
            buffer[3] = bytes[(int)numBytes + 2];
            buffer[4] = bytes[(int)numBytes + 5];
            buffer[5] = bytes[(int)numBytes + 4];
            buffer[6] = bytes[(int)numBytes + 7];
            buffer[7] = bytes[(int)numBytes + 6];
            switch (propertyType.Name)
            {
               case "UInt64":
                  value = BitConverter.ToUInt64(buffer);
                  break;
               case "Int64":
                  value = BitConverter.ToInt64(buffer);
                  break;
               case "Double":
                  value = BitConverter.ToDouble(buffer);
                  break;
            }
            numBytes += 8;
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
            int myBytePos = count > 8 ? bytePos - 1 : bytePos + 1;
            if ((bool)propertyValue)
               bytes[myBytePos] |= (byte)Math.Pow(2, bitPos);
            else
               bytes[myBytePos] &= (byte)(~(byte)Math.Pow(2, bitPos));
            numBytes += 0.125;
            break;
         case "Byte":
            numBytes = (int)Math.Ceiling(numBytes);
            bytePos = (int)numBytes;
            bytes[bytePos] = (byte)propertyValue;
            numBytes++;
            break;
         case "Int16":
            bytes2 = BitConverter.GetBytes((short)propertyValue).Reverse().ToArray();
            break;
         case "UInt16":
            bytes2 = BitConverter.GetBytes((ushort)propertyValue).Reverse().ToArray();
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
            byte transit = bytes2[0];
            bytes2[0] = bytes2[1];
            bytes2[1] = transit;
            transit = bytes2[2];
            bytes2[2] = bytes2[3];
            bytes2[3] = transit;
            break;
         case "Int64":
         case "UInt64":
         case "Double":
            //bytes2 = BitConverter.GetBytes((double)propertyValue);
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
            transit = bytes2[0];
            bytes2[0] = bytes2[1];
            bytes2[1] = transit;
            transit = bytes2[2];
            bytes2[2] = bytes2[3];
            bytes2[3] = transit;
            transit = bytes2[4];
            bytes2[4] = bytes2[5];
            bytes2[5] = transit;
            transit = bytes2[6];
            bytes2[6] = bytes2[7];
            bytes2[7] = transit;
            break;
         default:
            numBytes = StructToBytes.ToBytes(propertyValue, bytes, ref count, numBytes, endianTypes);
            break;
      }

      if (bytes2 != null)
      {
         StructToBytes.IncrementToEven(ref numBytes);

         bytePos = (int)numBytes;
         for (int bCnt = 0; bCnt < bytes2.Length; bCnt++)
            bytes[bytePos + bCnt] = bytes2[bCnt];
         numBytes += bytes2.Length;
      }

      return numBytes;
   }
}
