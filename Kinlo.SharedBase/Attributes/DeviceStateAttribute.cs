using System.Windows.Media;

namespace Kinlo.SharedBase.Attributes;

/// <summary>
/// 设备状态特性
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
public class DeviceStateAttribute : Attribute
{
   // 存储颜色的字符串表示
   public string ColorHex { get; } = string.Empty;

   // 提供一个便捷属性来获取实际的 Brush 对象
   public Brush ColorBrush =>
      new Func<Brush>(() =>
      {
         if (string.IsNullOrEmpty(ColorHex))
            return (Brush)new BrushConverter().ConvertFromString("940078")!;
         try
         {
            return (Brush)new BrushConverter().ConvertFromString(ColorHex);
         }
         catch (Exception)
         {
            return (Brush)new BrushConverter().ConvertFromString("940078")!;
         }
      })();

   public DeviceStateAttribute(string colorHex)
   {
      ColorHex = colorHex;
   }
}
