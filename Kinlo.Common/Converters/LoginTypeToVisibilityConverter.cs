namespace Kinlo.Common.Converters;

public class LoginTypeToVisibilityConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (value == null)
            return Visibility.Collapsed;
         LoginTypeEnum _loginType = (LoginTypeEnum)value;
         return _loginType switch
         {
            LoginTypeEnum.用户注册 => Visibility.Visible,
            LoginTypeEnum.修改用户 => Visibility.Visible,
            _ => Visibility.Collapsed,
         };
      }
      catch { }
      return Visibility.Collapsed;
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
