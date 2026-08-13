namespace Kinlo.Common.Converters;

public class PlcAlarmsToStringConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      if (values == null || values.Length < 2)
         return string.Empty;
      if (values[0] is ObservableCollection<PlcAlarmModel> alarmModels)
      {
         var result = alarmModels.Count > 0 ? string.Join("     ", alarmModels.Select(x => x.AlarmMessage)) : string.Empty;
         return result;
      }
      return string.Empty;
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
