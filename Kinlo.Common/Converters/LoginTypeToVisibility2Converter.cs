namespace Kinlo.Common.Converters;

public class LoginTypeToVisibility2Converter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (value == null)
            return Visibility.Visible;
         LoginTypeEnum _loginType = (LoginTypeEnum)value;
         return _loginType switch
         {
            LoginTypeEnum.用户登陆 => Visibility.Collapsed,
            _ => Visibility.Visible,
         };
      }
      catch { }
      return Visibility.Visible;
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
