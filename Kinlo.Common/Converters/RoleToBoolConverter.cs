namespace Kinlo.Common.Converters;

public class RoleToBoolConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      if (value == null)
         return true;
      RoleModel role = (RoleModel)value;

      return role.Level >= ulong.MaxValue >> 1 ? false : true;
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
