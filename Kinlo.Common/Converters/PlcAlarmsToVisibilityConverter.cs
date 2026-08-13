namespace Kinlo.Common.Converters;

public class PlcAlarmsToVisibilityConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      if (values == null || values.Length < 3)
         return Visibility.Collapsed;

      if (values[3] is bool t && t)
         return Visibility.Visible;

      if (values[0] is ObservableCollection<PlcAlarmModel> alarmModels && values[1] is bool b)
      {
         return alarmModels.Count > 0 && b ? Visibility.Visible : Visibility.Collapsed;
      }
      return Visibility.Collapsed;
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
