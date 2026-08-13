namespace Kinlo.Common.Converters;

public class LevelToBoolConverter : IValueConverter
{
   /// <summary>
   /// 判断权限是否为管理员，返回bool
   /// </summary>
   /// <param name="value"></param>
   /// <param name="targetType"></param>
   /// <param name="parameter">为null时默认小于管理员返回true，如果为 "1"就大于管理返回true</param>
   /// <param name="culture"></param>
   /// <returns></returns>
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      if (value == null)
         return false;

      if (value is not ulong level)
         return false;

      return parameter == null ? level < (ulong)DefaultRoleEnum.管理员 : level >= (ulong)DefaultRoleEnum.管理员;
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
