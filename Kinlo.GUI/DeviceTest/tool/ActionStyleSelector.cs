using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Kinlo.GUI.DeviceTest
{
   public class ActionStyleSelector : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is not ActionStyle style)
            return Application.Current.FindResource("ButtonPrimary");

         return style switch
         {
            ActionStyle.Primary => Application.Current.FindResource("ButtonPrimary"),
            ActionStyle.Info => Application.Current.FindResource("ButtonInfo"),
            ActionStyle.Success => Application.Current.FindResource("ButtonSuccess"),
            ActionStyle.Warning => Application.Current.FindResource("ButtonWarning"),
            ActionStyle.Danger => Application.Current.FindResource("ButtonDanger"),
            _ => Application.Current.FindResource("ButtonDefault"),
         };
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
         throw new NotImplementedException();
   }
}
