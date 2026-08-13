namespace Kinlo.Common.Converters;

public class LoginTypeToBoolConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (value == null)
            return false;
         LoginTypeEnum _loginType = (LoginTypeEnum)value;
         return _loginType switch
         {
            LoginTypeEnum.修改用户 => true,
            _ => false,
         };
      }
      catch { }
      return false;
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
