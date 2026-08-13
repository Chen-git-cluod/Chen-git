namespace Kinlo.Common.Converters;

public class ResectionMainToVisibilityConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      if (value == null)
         return Visibility.Collapsed;

      ObservableCollection<PLCScanSignalModel> PLCScanSignals = (ObservableCollection<PLCScanSignalModel>)value;
      if (PLCScanSignals.Any(x => x.PLCResections.Any(k => k.IsEnabled && k.IsExcision)))
         return Visibility.Visible;
      else
         return Visibility.Collapsed;
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
