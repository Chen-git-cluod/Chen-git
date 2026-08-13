namespace Kinlo.Common.Converters;

public class MultiToBoolConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      if (values == null || values.Length < 2)
         return false;
      return (bool)values[0] && !(bool)values[1];
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
