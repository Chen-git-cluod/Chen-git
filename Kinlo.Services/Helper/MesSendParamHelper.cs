namespace Kinlo.Services.Helper;

public static class MesSendParamHelper
{
   public static async Task<bool> RunUploadParam(
      this List<UploadParamDto> paramList,
      MesApiCall call,
      MesService mesService,
      string logHeader
   )
   {
      var inputResult = await mesService.SendAsync(call, "参数修改上报MES", receiveMes => receiveMes.MesCommonParse(logHeader)); //修改参数上传

      if (inputResult.ResultStatus == MesResultStatusEnum.成功)
         return true;
      else
      {
         $"参数修改上报MES失败！".LogProcess(logHeader, Log4NetLevelEnum.错误);
         return false;
      }
   }

   /// <summary>
   /// 修改本地参数发送MES
   /// </summary>
   /// <param name="pairs"></param>
   /// <param name="config"></param>
   /// <param name="mesService"></param>
   /// <param name="productType"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static async Task<bool> LocalChangeParamNotifyMes(
      IReadOnlyDictionary<string, ChangeEntry> pairs,
      MesInterfaceParameterConfig config,
      RemoteLocalParamSyncService remoteService,
      MesService mesService,
      ProductionTypeEnum productType,
      string logHeader
   )
   {
      try
      {
         List<UploadParamDto> paramList = new List<UploadParamDto>();

         foreach (var item in pairs)
         {
            //string key = Path.GetExtension(item.Value.Path);
            //if (string.IsNullOrWhiteSpace(key))
            //   key = item.Value.Path;
            //else
            //   key = key.TrimStart('.');
            var par = remoteService.RemotePcSyncList.FirstOrDefault(x => x.PtyFullName == item.Value.Path);

            if (par != null)
            {
               paramList.Add(
                  new UploadParamDto(par.MesCode, par.MesName)
                  {
                     OriginalValue = item.Value.OriginalValue?.ToString() ?? "",
                     CurrentValue = item.Value.CurrentValue?.ToString() ?? "",
                  }
               );
            }
         }
         if (paramList.Count > 0)
            return await ChangeParamSendAsync(paramList, config, mesService, logHeader);
         return false;
      }
      catch (Exception ex)
      {
         $"本地参数修改发送给MES异常：{ex}".LogRun();
         return false;
      }
   }

   public static async Task<bool> ChangeParamSendAsync(
      List<UploadParamDto> paramList,
      MesInterfaceParameterConfig config,
      MesService mesService,
      string logHeader
   )
   {
      var call = config.GetApiCall(new MesRequestBuildNJGX.ArgsChangeParam(paramList));
      if (call != null && call.IsEnable) //有接口并已启用
      {
         var inputResult = await mesService.SendAsync(call, "参数修改上报MES", receiveMes => receiveMes.MesCommonParse(logHeader)); //修改参数上传

         if (inputResult.ResultStatus == MesResultStatusEnum.成功)
            return true;
         else
         {
            $"参数修改上报MES失败！".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return false;
         }
      }
      return false;
   }

   /// <summary>
   /// 获取PLC参数修改时编码
   /// </summary>
   /// <param name="indexs"></param>
   /// <param name="config"></param>
   /// <returns></returns>
   public static List<UploadParamDto> GetPlcParamChangCode(
      List<float>? currentVals,
      List<float>? originalVals,
      ParameterConfig config,
      RemoteLocalParamSyncService remoteService
   )
   {
      var resultDic = new List<UploadParamDto>();
      if (currentVals == null || originalVals == null || currentVals.Count < 1 || currentVals.Count != originalVals.Count)
         return resultDic;

      for (var i = 0; i < currentVals.Count; i++)
      {
         if (currentVals[i] == originalVals[i])
            continue;
         var par = remoteService.RemotePlcSyncList.FirstOrDefault(x => x.Index == i);
         if (par == null)
            continue;

         resultDic.Add(
            new UploadParamDto(par.MesCode, par.MesName)
            {
               CurrentValue = currentVals[i].ToString(),
               OriginalValue = originalVals[i].ToString(),
            }
         );
      }

      return resultDic;
   }

   /// <summary>
   /// 获取PLC参数编码
   /// </summary>
   /// <param name="indexs"></param>
   /// <param name="config"></param>
   /// <returns></returns>
   public static List<UploadParamDto> GetPlcParamCode(
      this List<float>? currentVals,
      ParameterConfig config,
      RemoteLocalParamSyncService remoteService
   )
   {
      var resultDic = new List<UploadParamDto>();
      if (currentVals == null || currentVals.Count < 1)
         return resultDic;

      for (var i = 0; i < currentVals.Count; i++)
      {
         if (currentVals[i] == -999.00)
            continue;
         var par = remoteService.RemotePlcSyncList.FirstOrDefault(x => x.Index == i);
         if (par == null)
            continue;

         resultDic.Add(new UploadParamDto(par.MesCode, par.MesName) { CurrentValue = currentVals[i].ToString() });
      }

      return resultDic;
   }
}
