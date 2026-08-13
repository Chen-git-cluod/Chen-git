namespace Kinlo.Common.Converters;

public class BoolToBrushConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (value is null)
         {
            return Brushes.Transparent;
         }

         var enumInt = (bool)value;
         if (enumInt)
         {
            return Application.Current.FindResource("PrimaryBrush"); //Brushes.Green;
         }
         else
         {
            return Brushes.Transparent;
         }
      }
      catch
      {
         return Brushes.Transparent;
      }
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
