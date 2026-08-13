namespace Kinlo.Common.Converters;

public class MultipleResectionToVisibilityConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      if (values == null)
         return Visibility.Collapsed;

      if ((bool)values[0] && (bool)values[1])
         return Visibility.Visible;
      else
         return Visibility.Collapsed;
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
