namespace Kinlo.Common.Converters;

public class DeviceStatusWidthConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      if (values == null)
         return 100.0;
      double _listViewActualWidth = (double)values[0];
      ObservableCollection<DeviceClientModel> _device = (ObservableCollection<DeviceClientModel>)values[1];

      var _actualWidth = _listViewActualWidth / (_device.Count()) - 1;
      return _actualWidth;
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
