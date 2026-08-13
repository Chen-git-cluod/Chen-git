using HandyControl.Controls;
using Kinlo.MESDocking.Ftp;
using Kinlo.Services.PeriodicTasks;

namespace Kinlo.GUI.ViewModel;

[Languages(["MES配置", "Konfigurasi MES", "MES Interface"], IsScanProperty = false)]
[UIDisplayAttribute(isSingleton: true, 51, (ulong)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺), isRunEdit: true, "\xe61e")]
public class ConfigurationMesViewModel : Screen, IMenu
{
   public string InterfaceName { get; set; } = string.Empty;
   public string Url { get; set; } = string.Empty;
   public MesInterfaceParameterConfig MesParameter { get; set; }
   public MesInterfaceParameterConfig MesParameterCopy { get; set; }
   public List<byte> BaseIndexs { get; set; } = [0, 1, 2];
   public string TestUser { get; set; } = "tlszx055";
   public string SendTxt { get; set; } = string.Empty;
   public string ReceiveTxt { get; set; } = string.Empty;
   private UsersStatusConfig _usersStatus;
   private IContainer _container;
   private ParameterConfig _parameter;
   private MqttHelper _mqttHelper;
   private MesService _mesService;
   private FtpService _ftpService;
   private DisplayDataCollection _displayDataCollection;

   private MesInterfaceInfoModel _selectMesInterfacceInfo = new MesInterfaceInfoModel();

   public ConfigurationMesViewModel(IContainer container)
   {
      _container = container;
      MesParameter = _container.Get<MesInterfaceParameterConfig>();
      _displayDataCollection = _container.Get<DisplayDataCollection>();
      _parameter = _container.Get<ParameterConfig>();
      _usersStatus = _container.Get<UsersStatusConfig>();
      _mqttHelper = _container.Get<MqttHelper>();
      _mesService = _container.Get<MesService>();
      _ftpService = _container.Get<FtpService>();
      Copy();
   }

   public void SelectAllCmd()
   {
      foreach (var item in MesParameterCopy.MesInterfaceInfo.MesParameterItems)
      {
         item.IsEnable = true;
      }
   }

   public void DeselectAllCmd()
   {
      foreach (var item in MesParameterCopy.MesInterfaceInfo.MesParameterItems)
      {
         item.IsEnable = false;
      }
   }

   public void InvertSelectionCmd()
   {
      foreach (var item in MesParameterCopy.MesInterfaceInfo.MesParameterItems)
      {
         item.IsEnable = !item.IsEnable;
      }
   }

   public void MqttTestCMD()
   {
      InterfaceName = "mes_Topic";
      Url = MesParameterCopy.MqttServiceInfo.Topic;
      SendTxt = "";
   }

   public void FtpTestCMD()
   {
      InterfaceName = "FTP";
      Url = MesParameterCopy.FtpServiceInfo.Host;
      SendTxt = "在此处输入要上传FTP的文件地址";
   }

   List<IBatMainModel>? _testBatts = null;

   public void MesTestCMD(MesInterfaceInfoModel mesInterfacceInfo)
   {
      try
      {
         if (_testBatts == null)
         {
            _testBatts = new List<IBatMainModel>();
            for (var i = 0; i < 24; i++)
            {
               var bat = (IBatMainModel)Activator.CreateInstance(_displayDataCollection.CompleteBatteryDatas.RuntimeBatteryType);
               bat.Barcode = $"TestCode{i}";
               bat.GlueNailBatch = _parameter.DeviceParameter.GlueNailCode;
               bat.ElectrolyteBatch = _parameter.DeviceParameter.ElectrolyteCode;
               ((IBatWeightAfterModel)bat).TotalInjectionVolume = 168.89f + i * 1.06f;
               _testBatts.Add(bat);
            }
         } //准备测试电池
         _selectMesInterfacceInfo = mesInterfacceInfo;
         InterfaceName = mesInterfacceInfo.InterfaceName.ToString();
         Url = MesParameter.GetUrl(_selectMesInterfacceInfo);

         SendTxt = mesInterfacceInfo.ParameterType switch
         {
            var t when t == typeof(MesRequestBuildNJGX.ArgsMesLogin) => _mesService
               .RequestBuild(new MesRequestBuildNJGX.ArgsMesLogin("testAccount", "testPassword"))
               .request,
            var t when t == typeof(MesRequestBuildNJGX.ArgsMaterialIn) => _mesService
               .RequestBuild(new MesRequestBuildNJGX.ArgsMaterialIn("TestInMaterialCode", 168.88))
               .request,
            var t when t == typeof(MesRequestBuildNJGX.ArgsMaterialOut) => _mesService
               .RequestBuild(new MesRequestBuildNJGX.ArgsMaterialOut("TestOutMaterialCode", 168.88))
               .request,
            var t when t == typeof(MesRequestBuildNJGX.ArgsProductEntry) => _mesService
               .RequestBuild(new MesRequestBuildNJGX.ArgsProductEntry("TestBarcode"))
               .request,
            var t when t == typeof(MesRequestBuildNJGX.ArgsWorkOrder) => _mesService
               .RequestBuild(new MesRequestBuildNJGX.ArgsWorkOrder("TestWorkOrderBarcode"))
               .request,
            var t when t == typeof(MesRequestBuildNJGX.ArgsMesExit) => _mesService
               .RequestBuild(new MesRequestBuildNJGX.ArgsMesExit(_testBatts[0]))
               .request,

            var t when t == typeof(MesRequestBuildNJGX.ArgsSendNetWeight) => _mesService
               .RequestBuild(new MesRequestBuildNJGX.ArgsSendNetWeight("TestBarcode123", 2162.05))
               .request,

            _ => "",
         };
      }
      catch (Exception ex)
      {
         Growl.Warning(ex.Message);
      }
   }

   public async Task UploadOldDataCMD()
   {
      try
      {
         var _mesInterfaceParameterConfig = _container.Get<MesInterfaceParameterConfig>();
         var _globalStaticTemporary = _container.Get<GlobalStaticTemporary>();
         var _sugarDB = _container.Get<DbHelper>();
         var _otherParameterConfig = _container.Get<OtherParameterConfig>();
         DateTime dateTime = new DateTime(2026, 1, 1);
         var parameterConfig = _container.Get<ParameterConfig>();
         var dayShifTime = parameterConfig.DeviceParameter.DayShift;
         var nightShiftTime = parameterConfig.DeviceParameter.NightShift;
         while (dateTime < DateTime.Now)
         {
            for (int i = 0; i < 2; i++)
            {
               var exportTime = i == 0 ? (dateTime.Date + dayShifTime).AddHours(1) : (dateTime.Date + nightShiftTime).AddHours(1);
               await PeriodicTasksHelper.ExportProductedDataAsync(
                  exportTime,
                  _ftpService,
                  _mesInterfaceParameterConfig,
                  _parameter,
                  _sugarDB,
                  _otherParameterConfig,
                  _displayDataCollection
               );
               await PeriodicTasksHelper.ExportPlcAlarmDataAsync(
                  exportTime,
                  _ftpService,
                  _mesInterfaceParameterConfig,
                  _parameter,
                  _sugarDB
               );
               await Task.Delay(50);
            }

            dateTime = dateTime.AddDays(1);
         }
      }
      catch (Exception ex)
      {
         Growl.Warning($"异常：{ex}");
      }
   }

   public async Task SendMes()
   {
      if (string.IsNullOrEmpty(SendTxt))
      {
         Growl.Warning("请输入要发送的数据！");
         return;
      }
      try
      {
         await Task.Run(async () =>
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(() => ReceiveTxt = string.Empty);
            if (InterfaceName == "mes_Topic")
            {
               await _mqttHelper.SendMessageAsync(Url, SendTxt);
            }
            else if (InterfaceName == "FTP")
            {
               $"测试ftp，本地文件:{SendTxt}".LogRun();
               var _mesInterfaceParameterConfig = _container.Get<MesInterfaceParameterConfig>();
               string remoteDirectory = $"{_mesInterfaceParameterConfig.FtpServiceInfo.RemoteDirectory.TrimEnd('/')}/生产数据"; //远程目录
               string remoteFileName = Path.GetFileName(SendTxt); //远程文件名
               await _ftpService.UploadFileWithNewConnectionAsync(SendTxt, remoteDirectory, remoteFileName); //些处SendTxt为本地文件目录
            }
            else
            {
               var call = new MesApiCall(
                  null,
                  _selectMesInterfacceInfo.InterfaceName,
                  Url,
                  SendTxt,
                  _selectMesInterfacceInfo.PollingIntervalSec,
                  true
               );
               var result = await _mesService.SendAsync(call, "Test", receive => receive.MesCommonParse("Test"));

               await UIThreadHelper.InvokeOnUiThreadAsync(() => ReceiveTxt = result.Response);
            }
         });
      }
      catch (Exception ex)
      {
         Growl.Error(ex.Message);
      }
   }

   private void Copy()
   {
      MesParameterCopy = new MesInterfaceParameterConfig(_container, false);
      MesParameterCopy.MesName = MesParameter.MesName;
      ExpressionAssignmentMapper<MqttServiceInfoModel, MqttServiceInfoModel>.Trans(
         MesParameter.MqttServiceInfo,
         MesParameterCopy.MqttServiceInfo
      );
      ExpressionAssignmentMapper<FtpServiceInfoModel, FtpServiceInfoModel>.Trans(
         MesParameter.FtpServiceInfo,
         MesParameterCopy.FtpServiceInfo
      );
      ExpressionAssignmentMapper<MesInterfaceCollectionModel, MesInterfaceCollectionModel>.Trans(
         MesParameter.MesInterfaceInfo,
         MesParameterCopy.MesInterfaceInfo
      );
      UIThreadHelper.InvokeOnUiThreadAsync(() => MesParameterCopy.MesInterfaceInfo.MesParameterItems.Clear());
      foreach (var item in MesParameter.MesInterfaceInfo.MesParameterItems)
      {
         MesInterfaceInfoModel mesInterfacceInfo = new MesInterfaceInfoModel();
         ExpressionAssignmentMapper<MesInterfaceInfoModel, MesInterfaceInfoModel>.Trans(item, mesInterfacceInfo);
         mesInterfacceInfo.ParameterType = item.ParameterType;
         UIThreadHelper.InvokeOnUiThreadAsync(() => MesParameterCopy.MesInterfaceInfo.MesParameterItems.Add(mesInterfacceInfo));
      }
   }

   public async Task SaveCMD() =>
      await Save(MesParameter.CompareObject(MesParameterCopy, new Dictionary<string, DifferenceResultDto>()).ToString());

   public async Task Save(string contrastMsg)
   {
      await Task.Run(async () =>
      {
         if (!string.IsNullOrEmpty(contrastMsg))
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(() =>
            {
               MesParameter.MesName = MesParameterCopy.MesName;
               ExpressionAssignmentMapper<MqttServiceInfoModel, MqttServiceInfoModel>.Trans(
                  MesParameterCopy.MqttServiceInfo,
                  MesParameter.MqttServiceInfo
               );
               ExpressionAssignmentMapper<FtpServiceInfoModel, FtpServiceInfoModel>.Trans(
                  MesParameterCopy.FtpServiceInfo,
                  MesParameter.FtpServiceInfo
               );
               ExpressionAssignmentMapper<MesInterfaceCollectionModel, MesInterfaceCollectionModel>.Trans(
                  MesParameterCopy.MesInterfaceInfo,
                  MesParameter.MesInterfaceInfo
               );
               MesParameter.MesInterfaceInfo.MesParameterItems.Clear();
               foreach (var item in MesParameterCopy.MesInterfaceInfo.MesParameterItems)
               {
                  MesInterfaceInfoModel mesInterfacceInfo = new MesInterfaceInfoModel();
                  ExpressionAssignmentMapper<MesInterfaceInfoModel, MesInterfaceInfoModel>.Trans(item, mesInterfacceInfo);
                  mesInterfacceInfo.ParameterType = item.ParameterType;
                  MesParameter.MesInterfaceInfo.MesParameterItems.Add(mesInterfacceInfo);
               }
            });
            MesParameter.Save(_usersStatus.LocalLoggedinUser.Account, contrastMsg.ToString());
            Growl.Success("保存成功！");
         }
         else
         {
            Growl.Success($"文件未修改！");
         }
      });
   }

   public void Load() { }

   public bool Unload()
   {
      var msg = MesParameter.CompareObject(MesParameterCopy, new Dictionary<string, DifferenceResultDto>()).ToString();
      if (!string.IsNullOrEmpty(msg))
      {
         var rs = System.Windows.MessageBox.Show("有修改未保存，是否保存？", "提示", MessageBoxButton.YesNoCancel);
         if (rs == MessageBoxResult.Yes)
         {
            _ = Save(msg);
            return true;
         }
         else if (rs == MessageBoxResult.No)
         {
            UIThreadHelper.InvokeOnUiThreadAsync(() => Copy());
            return true;
         }
         else
         {
            return false;
         }
      }
      return true;
   }
}
