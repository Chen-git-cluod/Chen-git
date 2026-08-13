namespace Kinlo.Common.Converters;

public class IntToVisibilityConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      if (value == null || parameter == null)
         return Visibility.Visible;

      if (value is int intValue && parameter is int intParam)
      {
         if (intValue < 0)
            intValue = 0;
         return intValue == intParam ? Visibility.Visible : Visibility.Collapsed;
      }
      return Visibility.Visible;
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
