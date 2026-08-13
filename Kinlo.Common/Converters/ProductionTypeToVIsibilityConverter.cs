namespace Kinlo.Common.Converters;

public class ProductionTypeToVIsibilityConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      var visibilitys = parameter.ToString().Split(',');
      var vis = visibilitys.Any(x => x == value.ToString()) ? Visibility.Visible : Visibility.Collapsed;
      return vis;
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
