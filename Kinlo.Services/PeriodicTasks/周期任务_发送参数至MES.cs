using Kinlo.Common.Tools.ExpressionHelpers;

namespace Kinlo.Services.PeriodicTasks;

partial class PeriodicTasksHelper
{
   private int notifyLock = 0;
   private DateTime? _lastUploadTime = null;

   /// <summary>
   /// 参数通知MES
   /// </summary>
   /// <param name="t"></param>
   /// <param name="container"></param>
   /// <returns></returns>
   public async Task ParamSendMes(DateTime t, IContainer container)
   {
      if (Interlocked.Exchange(ref notifyLock, 1) == 1)
         return;

      bool mesRes = false;
      try
      {
         ParameterConfig parameter = container.Get<ParameterConfig>();
         var mesInterface = container.Get<MesInterfaceParameterConfig>();
         var mesService = container.Get<MesService>();
         List<UploadParamDto> paramlist = new List<UploadParamDto>();
         var call = mesInterface.GetApiCall(new MesRequestBuildNJGX.ArgsRunUploadParam(paramlist));
         if (call == null || !call.IsEnable)
            return;

         if (_lastUploadTime != null && (t - _lastUploadTime.Value).TotalSeconds < call.PollingIntervalSec)
            return;

         var plcParams = await GetPlcParamsAsync(container);
         var localParams = GetPcParams(container);
         paramlist = plcParams.Concat(localParams).ToList();
         call = mesInterface.GetApiCall(new MesRequestBuildNJGX.ArgsRunUploadParam(paramlist));
         mesRes = await paramlist.RunUploadParam(call!, mesService, "定时上传参数接口报文");
      }
      catch (Exception ex)
      {
         $"定时上传参数接口报文 异常:{ex}".LogRun(Log4NetLevelEnum.警告);
      }
      finally
      {
         // if (mesRes)
         _lastUploadTime = t;
         //else
         //   await Task.Delay(10000);
         Interlocked.Exchange(ref notifyLock, 0);
      }
   }

   private static readonly Dictionary<Type, PropertyInfo[]> _propertyCache = new();

   /// <summary>
   /// 取PC配置
   /// </summary>
   /// <param name="container"></param>
   /// <returns></returns>
   private List<UploadParamDto> GetPcParams(IContainer container)
   {
      var config = container.Get<ParameterConfig>();

      var paramList = new List<UploadParamDto>();

      var type = config.RunParameter.GetType();

      // 缓存反射结果
      if (!_propertyCache.ContainsKey(type))
      {
         _propertyCache[type] = type.GetProperties();
      }
      var properties = _propertyCache[type];

      var remoterService = container.Get<RemoteLocalParamSyncService>();
      foreach (var item in remoterService.RemotePcSyncList)
      {
         var basePaht = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter);
         var porpertyInfo = properties.FirstOrDefault(x => $"{basePaht}.{x.Name}" == item.PtyFullName);
         if (porpertyInfo != null)
         {
            var curr = porpertyInfo.GetValue(config.RunParameter);
            if (curr != null)
            {
               var upload = new UploadParamDto(item.MesCode, item.MesName) { CurrentValue = curr.ToString()! };
               paramList.Add(upload);
            }
         }
      }
      return paramList;
   }

   /// <summary>
   /// 取PLC参数
   /// </summary>
   /// <param name="container"></param>
   /// <returns></returns>
   private async Task<List<UploadParamDto>> GetPlcParamsAsync(IContainer container)
   {
      var result = new List<UploadParamDto>();
      PLCSignalConfig plcSignalConfig = container.Get<PLCSignalConfig>();
      var devicesConfig = container.Get<DevicesConfig>();
      var plcDataTag = plcSignalConfig.CustomPlcInteractAddresses.FirstOrDefault(x =>
         x.CustomInteractName == CustomInteractNameEnum.PLC参数上报MES
      );
      if (plcDataTag == null || !plcDataTag.IsEnable)
      {
         "[PLC参数上报MES]未配置PLC生产数据交互地址！".LogRun(Log4NetLevelEnum.警告);
         return result; // 直接返回空
      }
      var client = devicesConfig.DeviceList.FirstOrDefault(x => x.ProcessesType == ProcessTypeEnum.PLC);
      if (client == null)
      {
         $"[PLC参数上报MES] 未找到设备[PLC]设备配置信息！".LogRun(Log4NetLevelEnum.错误, true);
         return result;
      }
      var currentPlcDatas = await client.WithCreatedDeviceAsync(async d =>
      {
         return await Task.Run(() =>
         {
            var res = ((IPLC)d).ReadValues<float>(plcDataTag.DataAddress, "PLC参数上报MES");
            return Task.FromResult(res);
         });
      });

      if (currentPlcDatas.Status != DeviceStatus.Success || currentPlcDatas.Value!.Count == 0)
      {
         $"[PLC参数上报MES]未取到PLC参数！".LogRun();
         return result; // 直接返回空
      }
      result = currentPlcDatas.Value!.GetPlcParamCode(container.Get<ParameterConfig>(), container.Get<RemoteLocalParamSyncService>());
      return result;
   }
}
