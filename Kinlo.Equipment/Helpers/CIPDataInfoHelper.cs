namespace Kinlo.Equipment.Helpers;

internal class CIPDataInfoHelper
{
   public static List<CIPDataInfoModel> CIPDataTypeInfos { get; set; } =
   [
      new(0xC1, "Boolean", 2, typeof(bool)),
      new(0xC2, "SInt", 2, typeof(byte)),
      new(0xC3, "Int16", 2, typeof(short)),
      new(0xC4, "Int32", 4, typeof(int)),
      new(0xC5, "Int64", 8, typeof(long)),
      new(0xC6, "USInt", 2, typeof(sbyte)),
      new(0xC7, "UInt16", 2, typeof(ushort)),
      new(0xC8, "UInt32", 4, typeof(uint)),
      new(0xC9, "UInt64", 8, typeof(long)),
      new(0xCA, "Single", 4, typeof(float)),
      new(0xCB, "Double", 8, typeof(double)),
      new(0xD0, "String", 0, typeof(string)),
      new(0xD1, "STRING2", 2, typeof(string)), //STRING2 最大长度 = 255
   ];
}
