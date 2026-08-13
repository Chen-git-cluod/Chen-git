namespace Kinlo.Common.Converters;

public class BoolToEnableTextConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      return value == null || !(bool)value ? "关闭" : "启用";
      //return value == null || !(bool)value ? "Off" : "On";
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
