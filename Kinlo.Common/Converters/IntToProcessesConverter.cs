namespace Kinlo.Common.Converters;

/// <summary>
///  工序显示转换
/// </summary>
public class IntToProcessesConverter : IValueConverter
{
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
   {
      try
      {
         if (value == null)
            return string.Empty;
         else if (value is int intResult)
         {
            return intResult switch
            {
               var r when r <= 0 || r == 255 => string.Empty,
               > 0 => (ProcessTypeEnum)intResult,
               _ => string.Empty,
            };
         }
         else
         {
            if (Enum.TryParse(typeof(ProcessTypeEnum), value.ToString(), out var processType))
            {
               if (processType is ProcessTypeEnum.PLC or ProcessTypeEnum._)
                  return string.Empty;
               return processType;
            }
            return value.ToString();
         }
      }
      catch (Exception)
      {
         return "转换异常";
      }
   }

   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
