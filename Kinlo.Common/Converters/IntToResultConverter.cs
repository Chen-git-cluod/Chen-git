namespace Kinlo.Common.Converters;

public class IntToResultConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (value == null)
            return string.Empty;
         int _intResult = (int)value;
         return _intResult switch
         {
            <= 0 => string.Empty,
            > 0 => (ResultTypeEnum)_intResult,
         };
      }
      catch (Exception)
      {
         return string.Empty;
      }
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
