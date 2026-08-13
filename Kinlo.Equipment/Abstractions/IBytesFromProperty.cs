namespace Kinlo.Equipment.Interfaces;

public interface IBytesFromProperty
{
   public double SetBytesFromProperty(
      object propertyValue,
      byte[] bytes,
      double numBytes,
      ref int count,
      CommunicationEnum endianTypes = CommunicationEnum.FinsTcp
   );
   public object? GetPropertyValue(
      Type propertyType,
      byte[] bytes,
      ref double numBytes,
      ref int count,
      CommunicationEnum endianTypes = CommunicationEnum.FinsTcp
   );
}
