namespace Kinlo.Common.Configurations;

public class MesInterfaceParameterConfig : ConfigurationBase
{
   /// <summary>
   /// 当前使用的MES名称
   /// </summary>
   public string MesName { get; set; } = string.Empty;
   public MesInterfaceCollectionModel MesInterfaceInfo { get; set; } = new();
   public MqttServiceInfoModel MqttServiceInfo { get; set; } = new();
   public FtpServiceInfoModel FtpServiceInfo { get; set; } = new();

   public MesInterfaceParameterConfig(StyletIoC.IContainer container, bool isStartup)
      : base(container, isStartup) { }

   public override void Load()
   {
      try
      {
         var dic = FileHelper.LoadToDictionary(this.GetType().Name);

         if (dic != null)
         {
            if (dic.TryGetValue(nameof(MqttServiceInfo), out object mqtt) && mqtt != null)
               MqttServiceInfo = JsonSerializer.Deserialize<MqttServiceInfoModel>(mqtt.ToString())!;
            else
               InitMqtt();
            if (dic.TryGetValue(nameof(FtpServiceInfo), out object ftp) && ftp != null)
               FtpServiceInfo = JsonSerializer.Deserialize<FtpServiceInfoModel>(ftp.ToString())!;
            else
               InitMqtt();
         }
         else
         {
            InitMqtt();
         }

         void InitMqtt()
         {
            MqttServiceInfo.BrokerAddress = "10.0.0.2";
            MqttServiceInfo.Port = 1883;
            ParameterConfig parameterConfig = _container.Get<ParameterConfig>();
            //TOPIC：/aiipc/{工序编码}/{设备编码}
            MqttServiceInfo.Topic = $"/aiipc/{parameterConfig.DeviceParameter.DeviceCode}";
         }
      }
      catch (Exception e)
      {
         $"[初始化MqttServiceInfo]异常：{e}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   /// <summary>
   /// 加载MES接口
   /// </summary>
   /// <param name="interfaceInfo"></param>
   public void LoadMesInterfaceInfo(MesInterfaceInfoDto interfaceInfo)
   {
      MesName = interfaceInfo.MesName;
      MesInterfaceInfo.MesParameterItems.Clear();
      foreach (var item in interfaceInfo.InterfaceDescriptions)
      {
         MesInterfaceInfo.MesParameterItems.Add(
            new MesInterfaceInfoModel
            {
               InterfaceName = item.interfaceName,
               Url = item.url,
               ParameterType = item.parameterType,
               PollingIntervalSec = item.pollingInterval,
            }
         );
      }

      var _dic = FileHelper.LoadToDictionary(this.GetType().Name);
      if (_dic != null && _dic.TryGetValue(nameof(MesInterfaceInfo), out object mesInterfaceInfoObj) && mesInterfaceInfoObj != null)
      {
         try
         {
            var localInfo = JsonSerializer.Deserialize<MesInterfaceCollectionModel>(mesInterfaceInfoObj.ToString())!;
            if (localInfo != null)
            {
               foreach (var item in MesInterfaceInfo.MesParameterItems)
               {
                  var info = localInfo.MesParameterItems.FirstOrDefault(x => x.InterfaceName == item.InterfaceName);
                  if (info == null)
                     continue;

                  item.Url = info.Url;
                  item.BserUrlIndex = info.BserUrlIndex;
                  item.IsEnable = info.IsEnable;
                  item.PollingIntervalSec = info.PollingIntervalSec;
               }
               MesInterfaceInfo.BaseAddress = localInfo.BaseAddress;
               MesInterfaceInfo.BaseAddress2 = localInfo.BaseAddress2;
               MesInterfaceInfo.LocalMesIP = localInfo.LocalMesIP;
               MesInterfaceInfo.Timeout = localInfo.Timeout;
            }
         }
         catch (Exception ex)
         {
            $"[加载MesInterfaceInfo]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
         }
      }
   }

   /// <summary>
   /// 获取MES请求相关
   /// </summary>
   /// <param name="mesArgs"></param>
   /// <returns></returns>
   public MesApiCall? GetApiCall(IMesArgs mesArgs)
   {
      Type argsType = mesArgs.GetType();
      //$"{JsonSerializer.Serialize(MesInterfaceInfo.MesParameterItems)}".LogRun();
      var info = MesInterfaceInfo.MesParameterItems.FirstOrDefault(x => x.ParameterType == argsType);
      if (info == null)
      {
         $"未找到参数类型为 {argsType.Name} 的接口相关设置，直接退出；".LogRun(Log4NetLevelEnum.错误, true);
         return null;
      }
      string url = GetUrl(info);

      return new MesApiCall(mesArgs, info.InterfaceName, url, string.Empty, info.PollingIntervalSec, info.IsEnable);
   }

   public string GetUrl(MesInterfaceInfoModel mesInterfacceInfo)
   {
      return mesInterfacceInfo.BserUrlIndex switch
      {
         1 => $"{MesInterfaceInfo.BaseAddress}{mesInterfacceInfo.Url}",
         2 => $"{MesInterfaceInfo.BaseAddress2}{mesInterfacceInfo.Url}",
         _ => $"{mesInterfacceInfo.Url}",
      };
   }
}
