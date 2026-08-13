namespace Kinlo.Common.Tools;

public static class ResultHelper
{
   #region  结果序列化
   /// <summary>
   /// 结果序列化
   /// </summary>
   /// <param name="resultInfos"></param>
   /// <returns></returns>
   public static string ResultSerialize(this IEnumerable<ResultInfoItemDto> resultInfos) =>
      resultInfos == null || resultInfos.Count() == 0
         ? string.Empty
         : string.Join(',', resultInfos.Select(r => $"{(int)r.Processes}:{(int)r.Result}"));

   /// <summary>
   /// 结果序列化（导出EXCEL用）
   /// </summary>
   /// <param name="resultInfos"></param>
   /// <returns></returns>
   public static string ResultSerializeExprot(this IEnumerable<ResultInfoItemDto> resultInfos) =>
      resultInfos == null || resultInfos.Count() == 0
         ? string.Empty
         : string.Join(',', resultInfos.Select(r => $"{r.Processes}:{r.Result}"));

   /// <summary>
   /// 结果反序列化
   /// </summary>
   /// <param name="result"></param>
   /// <returns></returns>
   public static ObservableCollection<ResultInfoItemDto> ResultDeserialize(this string result)
   {
      ObservableCollection<ResultInfoItemDto> _resultInfos = new();
      try
      {
         if (result != null && !string.IsNullOrEmpty(result))
         {
            var _list = result.Split(',');
            foreach (var item in _list)
            {
               var _arr = item.Split(":");
               if (_arr.Length > 1)
               {
                  if (Enum.TryParse(_arr[0], out ProcessTypeEnum p) && Enum.TryParse(_arr[1], out ResultTypeEnum r))
                     _resultInfos.Add(new ResultInfoItemDto(p, r));
               }
            }
         }
      }
      catch (Exception ex)
      {
         $"结果转换异常{ex}".LogRun(Log4NetLevelEnum.错误);
      }
      return _resultInfos;
   }

   /// <summary>
   /// 更新结果
   /// </summary>
   /// <param name="resultInfos"></param>
   /// <param name="processes"></param>
   /// <param name="result"></param>
   public static void UpdateResult(
      this ObservableCollection<ResultInfoItemDto> resultInfos,
      ProcessTypeEnum processes,
      ResultTypeEnum result
   )
   {
      var _process = resultInfos.FirstOrDefault(x => x.Processes == processes);
      if (_process == null)
      {
         resultInfos.Add(new ResultInfoItemDto(processes, result));
      }
      else
      {
         _process.Result = result;
      }
   }

   /// <summary>
   /// 更新结果
   /// </summary>
   /// <param name="resultStr"></param>
   /// <param name="processes"></param>
   /// <param name="result">返回更新后的结果字符</param>
   /// <returns></returns>
   public static string UpdateResult(this string resultStr, ProcessTypeEnum processes, ResultTypeEnum result)
   {
      try
      {
         var _reslutInfo = resultStr.ResultDeserialize();
         _reslutInfo.UpdateResult(processes, result);
         return _reslutInfo.ResultSerialize();
      }
      catch (Exception ex)
      {
         $"更新结果异常{ex}".LogRun(Log4NetLevelEnum.错误);
      }
      return resultStr;
   }

   /// <summary>
   /// 获取指定工序结果
   /// </summary>
   /// <param name="resultStr"></param>
   /// <param name="processes"></param>
   /// <returns></returns>
   public static ResultTypeEnum GetResult(this string resultStr, ProcessTypeEnum processes)
   {
      var _resultInfo = resultStr.ResultDeserialize();
      var _process = _resultInfo.FirstOrDefault(x => x.Processes == processes);

      if (_process != null)
      {
         return _process.Result;
      }

      return ResultTypeEnum._;
   }

   /// <summary>
   /// 是否有NG结果，如果有就返回NG结果，如果无则返回合格
   /// </summary>
   /// <param name="resultStr"></param>
   /// <returns></returns>
   public static ResultTypeEnum GetNGResult(this string resultStr)
   {
      var _resultInfo = resultStr.ResultDeserialize();
      foreach (var x in _resultInfo)
      {
         if (x.Result != ResultTypeEnum._ && x.Result != ResultTypeEnum.OK)
            return x.Result;
      }

      return ResultTypeEnum.OK;
   }
   #endregion
}
