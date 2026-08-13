namespace Kinlo.Common.Converters
{
   public class MultipleProcessDisplayConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value == null || string.IsNullOrEmpty(value.ToString()))
            return string.Empty;
         var _processes = value.ToString().Split(',');
         var _processeStrs = _processes.Select(x =>
         {
            if (Enum.TryParse(typeof(ProcessTypeEnum), x, out var processeStr))
               return processeStr;
            else
               return "转换失败";
         });
         return string.Join(",", _processeStrs);
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
