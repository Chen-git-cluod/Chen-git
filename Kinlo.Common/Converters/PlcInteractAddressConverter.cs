namespace Kinlo.Common.Converters;

public class PlcInteractAddressConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      StringBuilder sb = new StringBuilder();
      if (value is IEnumerable<ExtensionItem> models)
      {
         if (models.Any())
         {
            sb.Append(string.Join('；', models.Select(x => $"{x.Type}：{x.ValueStr}")));
         }
      }
      return sb.ToString();
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
