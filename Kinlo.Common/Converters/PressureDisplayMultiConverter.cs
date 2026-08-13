using Dm.util;

namespace Kinlo.Common.Converters
{
   public class PressureDisplayMultiConverter : IMultiValueConverter
   {
      public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
      {
         try
         {
            if (
               values == null
               || values.Count() < 2
               || values[0] == null
               || string.IsNullOrEmpty(values[0].toString())
               || values[1] == null
               || string.IsNullOrEmpty(values[1].toString())
            )
            {
               return string.Empty;
            }
            var arr = values[0].toString().Split(',').ToList();
            var para = values[1].toString().Split(',').ToList();

            return GenericHelper.PressursToString(values[0].ToString(), values[1].ToString());
         }
         catch (Exception ex)
         {
            $"压力转换异常:{ex}".LogRun(Log4NetLevelEnum.错误);
         }

         return string.Empty;
      }

      public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
