namespace Kinlo.Common.Converters;

public class ProductionDataTypeToVisibilityConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      return value switch
      {
         null => Visibility.Visible,
         var v when (ProductionDataTypeEnum)v == ProductionDataTypeEnum.主页不显示数据 => Visibility.Collapsed,
         _ => Visibility.Visible,
      };
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
