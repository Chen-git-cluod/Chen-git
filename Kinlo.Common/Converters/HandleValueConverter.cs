namespace Kinlo.Common.Converters;

public class HandleValueConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            return value;
         double valued = (double)value;
         if (valued == 0)
            return value;
         var parameters = parameter.ToString().Split(',');
         foreach (var item in parameters)
         {
            double handlerValue = 0;
            switch (item[0])
            {
               case '+':
                  if (double.TryParse(item.Substring(1), out handlerValue))
                  {
                     valued += handlerValue;
                  }
                  break;
               case '-':
                  if (double.TryParse(item.Substring(1), out handlerValue))
                  {
                     valued -= handlerValue;
                  }
                  break;
               case '*':
                  if (double.TryParse(item.Substring(1), out handlerValue))
                  {
                     valued *= handlerValue;
                  }
                  break;
               case '/':
                  if (double.TryParse(item.Substring(1), out handlerValue))
                  {
                     valued /= handlerValue;
                  }
                  break;
            }
         }
         if (valued <= 0)
            return 0.0d;

         return value;
      }
      catch (Exception ex)
      {
         $"[HandleValueConverter] 异常：{ex}".LogRun(Log4NetLevelEnum.警告);
      }

      return 0.0d;
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
