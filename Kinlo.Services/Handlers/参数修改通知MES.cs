namespace Kinlo.Services.Handlers;

[DeviceConnec(ProcessTypeEnum.参数修改通知MES, CommunicationEnum.None)]
public class NotifyMesOnParameterChangeHandler : ServiceHandlerBase
{
   public NotifyMesOnParameterChangeHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken) { }

   protected override async Task HandleCore(short plcValue)
   {
      //var address = _plcSignalConfig.CustomPlcInteractAddresses.FirstOrDefault(x =>
      //   x.CustomInteractName == CustomInteractNameEnum.PLC修改参数通知MES
      //);
      //if (address == null)
      //{
      //   $"未定义[PLC修改参数通知MES]接口地址，无法读取PLC修改参数；无法上传MES！".LogProcess(_taskLogHeader);
      //}
      var currentPlcDatas = _plc.ReadValues<float>(Context.DataAddress, _taskLogHeader);
      var originalPlcDatas = _plc.ReadValues<float>(Context.ExtraDataAddress, _taskLogHeader);
      if (currentPlcDatas.Status != DeviceStatus.Success || originalPlcDatas.Status != DeviceStatus.Success)
      {
         $"PLC修改参数未取到值".LogProcess(_taskLogHeader, Log4NetLevelEnum.警告);
         return;
      }
      string originalStr = JsonSerializer.Serialize(originalPlcDatas.Value);
      string currentStr = JsonSerializer.Serialize(currentPlcDatas.Value);
      $"PLC修改参数  修改后：{currentStr} 原始值:{originalStr}".LogProcess(_taskLogHeader);

      try
      {
         if (!currentPlcDatas.Value!.Any(x => x != -999)) //-999为默认值，如果所有都是-999就有问题
         {
            $"PLC上传参数：[{JsonSerializer.Serialize(currentPlcDatas.Value)}] 都是默认值，未修改，但PLC通知有修改！".LogProcess(
               _taskLogHeader,
               Log4NetLevelEnum.警告
            );
            return;
         }
         List<UploadParamDto> uploadParams = MesSendParamHelper.GetPlcParamChangCode(
            currentPlcDatas.Value,
            originalPlcDatas.Value,
            _parameterConfig,
            _container.Get<RemoteLocalParamSyncService>()
         );
         if (uploadParams.Count <= 0)
         {
            $"PLC参数：[{JsonSerializer.Serialize(currentPlcDatas.Value)}]在本地未定义上传Code;".LogProcess(_taskLogHeader);
            return;
         }
         var res = await MesSendParamHelper.ChangeParamSendAsync(uploadParams, _mesInterfaceParameterConfig, _mesService, _taskLogHeader);
         $"PLC修改参数上传MES：{(res ? "成功" : "失败")}".LogProcess(_taskLogHeader);
      }
      catch (Exception ex)
      {
         $"PLC修改参数上传MES出现异常：{ex}".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误);
      }
   }
}
