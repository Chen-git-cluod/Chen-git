namespace Kinlo.Equipment.Models;

/// <summary>
/// 为兼容CIP读取标签协议和fins等读取地址而设计的类
/// </summary>
[AddINotifyPropertyChangedInterface]
public class SignalAddressModel
{
   /// <summary>
   /// Fins等读取地址的协议使用地址
   /// </summary>
   public int Address { get; set; }

   /// <summary>
   /// CIP等读取标签协议使用标签
   /// </summary>
   public string Lable { get; set; } = string.Empty;

   /// <summary>
   /// 长度
   /// </summary>
   public ushort Length { get; set; } = 1;

   /// <summary>
   /// 偏移
   /// </summary>
   public int Offset { get; set; }

   public SignalAddressModel() { }

   public SignalAddressModel(string lable)
   {
      Lable = lable;
   }

   public SignalAddressModel(int address)
   {
      Address = address;
   }

   public SignalAddressModel(string lable, int address)
   {
      Lable = lable;
      Address = address;
   }
}
