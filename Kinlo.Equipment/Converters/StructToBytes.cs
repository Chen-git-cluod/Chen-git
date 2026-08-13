namespace Kinlo.Equipment.Converters;

public static class StructToBytes
{
   private static IEnumerable<PropertyInfo> GetAccessableProperties(Type classType)
   {
      var _properties = classType
#if NETSTANDARD1_3
      .GetTypeInfo().DeclaredProperties.Where(p => p.SetMethod != null);
#else
         .GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance)
         .Where(p => p.GetSetMethod() != null);
      return _properties;
#endif
   }

   private static double GetIncreasedNumberOfBytes(double numBytes, Type type)
   {
      switch (type.Name)
      {
         case "Boolean":
            numBytes += 0.125;
            break;
         case "Byte":
            numBytes = Math.Ceiling(numBytes);
            numBytes++;
            break;
         case "Int16":
         case "UInt16":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            numBytes += 2;
            break;
         case "Int32":
         case "UInt32":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            numBytes += 4;
            break;
         case "Single":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            numBytes += 4;
            break;
         case "Int64":
         case "UInt64":
         case "Double":
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
               numBytes++;
            numBytes += 8;
            break;
         default:
            var propertyClass = Activator.CreateInstance(type);
            numBytes = GetClassSize(propertyClass, numBytes, true);
            break;
      }

      return numBytes;
   }

   public static double GetClassSize(object instance, double numBytes = 0.0, bool isInnerProperty = false)
   {
      var properties = GetAccessableProperties(instance.GetType());
      foreach (var property in properties)
      {
         if (property.PropertyType.IsArray)
         {
            Type elementType = property.PropertyType.GetElementType();
            Array array = (Array)property.GetValue(instance, null);
            if (array.Length <= 0)
            {
               throw new Exception("无法确定类的大小，因为定义的数组没有大于零的固定大小;");
               //throw new Exception("Cannot determine size of class, because an array is defined which has no fixed size greater than zero.");
            }

            IncrementToEven(ref numBytes);
            for (int i = 0; i < array.Length; i++)
            {
               numBytes = GetIncreasedNumberOfBytes(numBytes, elementType);
            }
         }
         else
         {
            numBytes = GetIncreasedNumberOfBytes(numBytes, property.PropertyType);
         }
      }
      if (false == isInnerProperty)
      {
         // enlarge numBytes to next even number because S7-Structs in a DB always will be resized to an even byte count
         numBytes = Math.Ceiling(numBytes);
         if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
            numBytes++;
      }
      return numBytes;
   }

   public static double FromBytes(
      object sourceClass,
      byte[] bytes,
      ref int count,
      double numBytes = 0,
      CommunicationEnum endianTypes = CommunicationEnum.FinsTcp
   )
   {
      if (bytes == null)
         return numBytes;
      var properties = GetAccessableProperties(sourceClass.GetType());
      foreach (var property in properties)
      {
         if (property.PropertyType.IsArray)
         {
            Array array = (Array)property.GetValue(sourceClass, null);
            IncrementToEven(ref numBytes);
            Type elementType = property.PropertyType.GetElementType();
            for (int i = 0; i < array.Length && numBytes < bytes.Length; i++)
            {
               array.SetValue(
                  GetBytesFromProperty(endianTypes).GetPropertyValue(elementType, bytes, ref numBytes, ref count, endianTypes),
                  i
               );
            }
         }
         else
         {
            property.SetValue(
               sourceClass,
               GetBytesFromProperty(endianTypes).GetPropertyValue(property.PropertyType, bytes, ref numBytes, ref count, endianTypes),
               null
            );
         }
         if (count == 16 || property.PropertyType.Name != "Boolean")
         {
            count = 0;
         }
      }

      return numBytes;
   }

   public static double ToBytes(
      object sourceClass,
      byte[] bytes,
      ref int count,
      double numBytes = 0.0,
      CommunicationEnum endianTypes = CommunicationEnum.FinsTcp
   )
   {
      var properties = GetAccessableProperties(sourceClass.GetType());
      foreach (var property in properties)
      {
         if (property.PropertyType.IsArray)
         {
            Array array = (Array)property.GetValue(sourceClass, null);
            IncrementToEven(ref numBytes);
            Type elementType = property.PropertyType.GetElementType();
            for (int i = 0; i < array.Length && numBytes < bytes.Length; i++)
            {
               numBytes = GetBytesFromProperty(endianTypes)
                  .SetBytesFromProperty(array.GetValue(i), bytes, numBytes, ref count, endianTypes);
            }
         }
         else
         {
            numBytes = GetBytesFromProperty(endianTypes)
               .SetBytesFromProperty(property.GetValue(sourceClass, null), bytes, numBytes, ref count, endianTypes);
         }
         if (count == 16 || property.PropertyType.Name != "Boolean")
         {
            count = 0;
         }
      }
      return numBytes;
   }

   public static void IncrementToEven(ref double numBytes)
   {
      numBytes = Math.Ceiling(numBytes);
      if (numBytes % 2 > 0)
         numBytes++;
   }

   public static void GetBytes(
      object propertyValue,
      byte[] bytes,
      double numBytes = 0.0,
      CommunicationEnum endianTypes = CommunicationEnum.FinsTcp
   ) => GetBytesFromProperty(endianTypes).SetBytesFromProperty(propertyValue, bytes, numBytes, ref _count, endianTypes);

   private static int _count = 0;

   public static object GetValue(
      Type source,
      byte[] bytes,
      double numBytes = 0,
      CommunicationEnum endianTypes = CommunicationEnum.FinsTcp
   ) => GetBytesFromProperty(endianTypes).GetPropertyValue(source, bytes, ref numBytes, ref _count, endianTypes);

   private static IBytesFromProperty GetBytesFromProperty(CommunicationEnum communicationType) =>
      communicationType switch
      {
         CommunicationEnum.Modbus_TCP_ABCD => new BytesFromProperty_Modbus_ABCD(),
         CommunicationEnum.Modbus_TCP_DCBA => new BytesFromProperty_Modbus_DCBA(),
         CommunicationEnum.Modbus_TCP_BADC => new BytesFromProperty_Modbus_BADC(),
         CommunicationEnum.Modbus_TCP_CDAB => new BytesFromProperty_Modbus_CDAB(),
         CommunicationEnum.CipOrmonPlc or SharedBase.Enums.CommunicationEnum.CipInovance or CommunicationEnum.CipOrmonPlcLight =>
            new BytesFromProperty_CIP(),
         _ => new BytesFromProperty_Fins(),
      };
}
