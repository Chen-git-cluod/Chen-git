namespace Kinlo.Equipment.Models;

internal class CIPDataInfoModel
{
   /// <summary>
   /// 对应字节
   /// </summary>
   public byte PropertyByre { get; set; }
   public string PropertyName { get; set; } = string.Empty;

   /// <summary>
   /// 长
   /// </summary>
   public int Length { get; set; }
   public Type DataType { get; set; }

   public CIPDataInfoModel(byte b, string name, int length, Type type)
   {
      PropertyByre = b;
      PropertyName = name;
      Length = length;
      DataType = type;
   }
}
