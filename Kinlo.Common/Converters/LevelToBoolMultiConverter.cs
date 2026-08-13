namespace Kinlo.Common.Converters;

public class LevelToBoolMultiConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (values == null || values.Length < 4)
            return false;

         //用户等级
         var _userLevel = (ulong)values[0];
         //控件所需等级
         var _controlEditLevel = (ulong)values[1];

         //本地是否显示
         var _displayLevel = (_userLevel & _controlEditLevel) > 0;

         if (!_displayLevel)
            return _displayLevel;

         if (values[2] is RuntimeInterlockState interlockState && parameter is string propName)
         {
            var startIndex = propName.LastIndexOf('.') + 1;
            if (startIndex >= propName.Length)
               return true;

            var name = propName[startIndex..];
            return !interlockState.LockedProperties.Contains(name);
         }
         return false;

         //if (values.Length == 2)
         //{
         //   return _displayLevel;
         //}
         //else
         //{
         //   bool _isRunLevel = (bool)values[2]; //运行时是否可编辑
         //   bool _isRun = (bool)values[3];

         //   return _displayLevel && (_isRun ? _isRunLevel : true);
         //}
      }
      catch
      {
         return false;
      }
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
