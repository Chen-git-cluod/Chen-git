namespace Kinlo.Common.Converters;

public class DateTimeDisplayConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      if (value == null)
         return string.Empty;
      if (DateTime.TryParse(value.ToString(), out DateTime d))
      {
         return d.IsDefaultTime() ? string.Empty : d.ToString("MM-dd HH:mm:ss");
      }
      return value;
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
