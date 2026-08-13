namespace Kinlo.Common.Converters;

public class LevelToVisibilityMultiConverter : IMultiValueConverter
{
   /// <summary>
   /// 默认无需权限的页面
   /// </summary>
   private readonly string[] _defaultPermissionFreePage =
   [
      "MainViewModel",
      "HomeViewModel",
      "ConfigurationDeviceViewModel",
      "ProductionHistoryLayoutViewModel",
      "LogHistoryViewModel",
      "DataStatisticalChartsViewModel",
   ];

   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (values == null || values.Length < 3 || values.Any(x => x == null))
            return Visibility.Visible;

         var _userLevel = (ulong)values[0];
         ControlInfoModel _controlInfo = (ControlInfoModel)values[1];

         //if (_controlInfo.Index <= 4)
         //   return Visibility.Collapsed;

         if (_defaultPermissionFreePage.Any(x => x == _controlInfo.BindingOrKey)) //260512修改此处
            return Visibility.Collapsed;

         bool _isRun = (bool)values[2];
         var edit = (_userLevel & _controlInfo.EditLevel) > 0;
         var _displayLevel = edit && (_isRun ? _controlInfo.IsRunEdit : true);

         return _displayLevel ? Visibility.Collapsed : Visibility.Visible;
      }
      catch
      {
         return Visibility.Visible;
      }
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
