namespace Kinlo.Common.Converters;

public class LevelToRoleNameConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      if (values is null || values.Length < 2)
         return "";

      if (values[0] is not ulong level)
         return "";

      if (level == 0)
         return "";

      if (level == (ulong)DefaultRoleEnum.超级管理员)
         return DefaultRoleEnum.超级管理员.ToMesString();

      if (values[1] is not RoleConfig config)
         return "";

      var role = config.Roles.FirstOrDefault(x => x.Level == level);
      if (role == null)
         return "";

      return role.Name;
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
