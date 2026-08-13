namespace Kinlo.Common.Converters;

public class ProcessesTypeToVisibilityConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (value == null)
            return Visibility.Visible;
         var _loginType = (ProcessTypeEnum)value;
         return _loginType switch
         {
            ProcessTypeEnum.PLC => Visibility.Visible,
            _ => Visibility.Collapsed,
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
