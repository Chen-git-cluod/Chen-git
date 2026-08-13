using System.Net;

namespace Kinlo.Common.Converters;

public class IPAddressConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      if (value is IPAddress ip)
      {
         return ip.ToString(); // 将 IP 地址转换为字符串
      }
      return string.Empty;
   }

   public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      if (value is string ipString)
      {
         return IPAddress.TryParse(ipString, out IPAddress? ip) ? ip : null;
      }
      return null;
   }
}
