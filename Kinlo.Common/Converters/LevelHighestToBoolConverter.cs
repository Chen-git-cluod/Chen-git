namespace Kinlo.Common.Converters;

/// <summary>
/// 不允许选最高权限
/// </summary>
public class LevelHighestToBoolConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      if (value == null)
         return false;

      if (value is ulong level)
      {
         var _ret = level >= (ulong)1 << 62 ? false : true;
         return _ret;
      }
      else if (value is ObservableCollection<ControlInfoModel> levels)
      {
         foreach (var item in levels)
         {
            level = item.EditLevel;
            if (level >= (ulong)1 << 62)
               return false;
         }
         return true;
      }
      return false;
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
