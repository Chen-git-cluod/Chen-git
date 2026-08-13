namespace Kinlo.Common.Converters
{
   public class ResultToCollectMultiConverter : IMultiValueConverter
   {
      public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
      {
         ObservableCollection<ResultInfoItemDto> _results = new();
         try
         {
            //_results = values[0] == null || string.IsNullOrEmpty(values[0].ToString()) ? _results : values[0].ToString().ResultDeserializeDisplay();
            //if (values.Length < 2)
            //    return _results;
            //if (values[1] is not ProcessesTypeEnum)
            //    return _results;
            //var _process = (ProcessesTypeEnum)values[1];
            //var _result = _process switch
            //{
            //    var p when p is ProcessesTypeEnum.Node => _results,
            //    var p when p is ProcessesTypeEnum.前扫码 => _results.Where(x => x.Processes is ResultProcessEnum.前扫码),
            //    var p when p is ProcessesTypeEnum.短路测试 => _results.Where(x => x.Processes is ResultProcessEnum.短路测试),
            //    var p when p is ProcessesTypeEnum.前称重 => _results.Where(x => x.Processes is ResultProcessEnum.前称重),
            //    var p when p is ProcessesTypeEnum.后称重 => _results.Where(x => x.Processes is ResultProcessEnum.后称重 or ResultProcessEnum.注液量 or ResultProcessEnum.测漏),
            //    var p when p is ProcessesTypeEnum.回氦 => _results.Where(x => x.Processes is ResultProcessEnum.回氦),
            //    var p when p is ProcessesTypeEnum.密封钉 => _results.Where(x => x.Processes is ResultProcessEnum.密封钉),
            //    _ => _results,
            //};
            //return _result.ToObservableCollection();
            return "";
         }
         catch (Exception ex)
         {
            $"结果转换异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         }

         return _results;
      }

      public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
