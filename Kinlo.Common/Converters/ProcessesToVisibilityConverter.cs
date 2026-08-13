namespace Kinlo.Common.Converters;

public class ProcessesToVisibilityConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      if (value == null)
         return Visibility.Visible;
      ProcessTypeEnum processesTypeEnum = (ProcessTypeEnum)value;
      return processesTypeEnum switch
      {
         ProcessTypeEnum.注液量发送 or ProcessTypeEnum.补液量发送 => Visibility.Visible,
         _ => Visibility.Collapsed,
      };
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
