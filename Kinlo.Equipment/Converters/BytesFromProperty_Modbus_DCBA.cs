namespace Kinlo.Equipment.Converters;

public class BytesFromProperty_Modbus_DCBA : IBytesFromProperty
{
   public object GetPropertyValue(
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
               new byte[] { bytes[(int)numBytes], bytes[(int)numBytes + 1], bytes[(int)numBytes + 2], bytes[(int)numBytes + 3] }
            );
            numBytes += 4;
            break;
         case "UInt32":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            // hier auswerten
            value = BitConverter.ToUInt32(
               new byte[] { bytes[(int)numBytes], bytes[(int)numBytes + 1], bytes[(int)numBytes + 2], bytes[(int)numBytes + 3] }
            );
            numBytes += 4;
            break;
         case "Single":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;

            // hier auswerten
            value = BitConverter.ToSingle(
               new byte[] { bytes[(int)numBytes + 1], bytes[(int)numBytes], bytes[(int)numBytes + 3], bytes[(int)numBytes + 2] }
            );
            numBytes += 4;
            break;
         case "Double":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            var buffer = new byte[8];
            Array.Copy(bytes, (int)numBytes, buffer, 0, 8);
            value = BitConverter.ToDouble(buffer);
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
            bytePos = (int)Math.Floor(numBytes);
            bitPos = (int)((numBytes - (double)bytePos) / 0.125);
            if ((bool)propertyValue)
               bytes[bytePos] |= (byte)Math.Pow(2, bitPos); // is true
            else
               bytes[bytePos] &= (byte)(~(byte)Math.Pow(2, bitPos)); // is false
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
            bytes2 = BitConverter.GetBytes((int)propertyValue);
            break;
         case "UInt32":
            bytes2 = BitConverter.GetBytes((uint)propertyValue);
            break;
         case "Int64":
            bytes2 = BitConverter.GetBytes((long)propertyValue);
            break;
         case "Single":
            bytes2 = BitConverter.GetBytes((float)propertyValue);
            byte transit = bytes2[0];
            bytes2[0] = bytes2[1];
            bytes2[1] = transit;
            transit = bytes2[2];
            bytes2[2] = bytes2[3];
            bytes2[3] = transit;
            break;
         case "Double":
            bytes2 = BitConverter.GetBytes((double)propertyValue);
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
