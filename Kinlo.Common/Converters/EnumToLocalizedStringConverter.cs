using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.Common.Converters
{
   public class EnumToLocalizedStringConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return value.ToString();
         if (value is Enum enumValue)
         {
            string resourceKey = enumValue.ToString();
            var Language = DelegateExtensions.GetLanguage();
            return Language.CurrentLanguagesDictionary[resourceKey];
         }
         return value;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
