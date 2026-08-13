using HandyControl.Themes;

namespace Kinlo.Common.Converters;

internal class ThemeENToCNConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      if (value == null)
         return string.Empty;
      ApplicationTheme theme = (ApplicationTheme)value;
      return theme switch
      {
         var p when p == ApplicationTheme.Dark => "深色",
         var p when p == ApplicationTheme.Light => "浅色",
         _ => string.Empty,
      };
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
