namespace Kinlo.Common.Converters;

public class ResultToBrushConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (value is null || string.IsNullOrEmpty(value.ToString()))
         {
            return Brushes.Transparent;
         }

         if (value is Enum || value.GetType() == typeof(int))
         {
            int _resultInt = (int)value;
            return _resultInt switch
            {
               (int)
                  ResultTypeEnum._ /*(int)ResultTypeEnum.出站_未启用MES or (int)ResultTypeEnum.进站_未启用MES*/
               => Brushes.Transparent,
               // (int)ResultTypeEnum.发送MES测试数据 => Brushes.Orange,
               (int)ResultTypeEnum.OK => Application.Current.FindResource("SuccessBrush"),
               _ => Brushes.Red,
            };
         }

         var v = value.ToString();

         if (int.TryParse(v, out int val))
         {
            if (val == 0)
            {
               return Brushes.Transparent;
            }
            else if (val == 1)
            {
               return Application.Current.FindResource("SuccessBrush"); //Brushes.Green;
            }
            else
            {
               return Brushes.Red;
            }
         }
         else
         {
            if (value.ToString().ToUpper() == "Undefined")
            {
               return Brushes.Transparent;
            }
            else if (
               value.ToString().ToUpper() == "TRUE"
               || value.ToString().ToUpper() == "OK"
               || value.ToString().ToUpper() == "01"
               || value.ToString().ToUpper().Contains("PASS")
            )
            {
               return Application.Current.FindResource("SuccessBrush"); //Brushes.Green;
            }
            else if (value.ToString().ToUpper() == "FALSE" || value.ToString().ToUpper() == "NG" || value.ToString().ToUpper() == "FF")
            {
               return Brushes.Red;
            }
            else
            {
               return Brushes.Red;
            }
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
