namespace Kinlo.Common.Converters;

public class OnlineToBrushConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (value is null || string.IsNullOrEmpty(value.ToString()))
         {
            return Brushes.Transparent;
         }

         var enumInt = (int)value;

         return enumInt switch
         {
            1 => Brushes.Lime, //Brushes.Green;
            2 => Brushes.Red,
            3 => Application.Current.FindResource("WarningBrush"),
            99 => Brushes.White,
            // 99 => Application.Current.FindResource("DarkInfoBrush"),
            _ => Brushes.Transparent,
         };
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
