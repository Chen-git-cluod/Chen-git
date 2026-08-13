namespace Kinlo.Common.Converters;

public class MultipleResultDisplayConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      if (value == null || string.IsNullOrEmpty(value.ToString()))
         return string.Empty;
      var _results = value.ToString().Split(',');
      var _resultStrs = _results.Select(x =>
      {
         if (Enum.TryParse(typeof(ResultTypeEnum), x, out var resultStr))
            return resultStr;
         else
            return "转换失败";
      });
      return string.Join(",", _resultStrs);
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
