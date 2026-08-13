using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(IsSingleton = false)]
public class PLCInteractAddressViewModel : Screen
{
   private UsersStatusConfig _usersStatus;
   private PLCInteractAddressModel _plcInteractAddress;
   private StyletIoC.IContainer _container;
   private PLCSignalConfig _signalConfig;
   private DevicesConfig _deviceConfig;

   // private DisplayDataCollection _displayDataCollection;
   private int _code;
   public string Title { get; set; }
   public PLCInteractAddressModel PLCInteractAddressCopy { get; set; } = new PLCInteractAddressModel();
   public ObservableCollection<string> ServiceItems { get; set; } = new();
   public ObservableCollection<GenericCommandModel> StartSignaItems { get; set; } = new();
   public ObservableCollection<byte> DeviceIndexList { get; set; } = new();
   public List<int> DeviceCount { get; set; }
   public List<int> ProductCount { get; set; }
   public List<ExtensionType> ExtensionTypes { get; set; } = Enum.GetValues<ExtensionType>().ToList();

   public PLCInteractAddressViewModel(IContainer container)
   {
      _container = container;
      DeviceCount = Enumerable.Range(1, 98).ToList();
      ProductCount = Enumerable.Range(1, 32).ToList();
      _signalConfig = container.Get<PLCSignalConfig>();
      _deviceConfig = container.Get<DevicesConfig>();
      _usersStatus = container.Get<UsersStatusConfig>();
      ServiceItems = _signalConfig.PLCScanSignals.Select(x => x.ServiceName).ToObservableCollection();
   }

   public void SetPLCInteractAddress(PLCInteractAddressModel plcInteractAddress, int code)
   {
      _code = code;
      Title = code == 1 ? "新建交互信号" : "编辑交互信号";
      ServiceName = plcInteractAddress.ServiceName;
      ProcessesType = plcInteractAddress.ProcessesType;
      if (code == 2)
      {
         _plcInteractAddress = plcInteractAddress;
         ExpressionAssignmentMapper<PLCInteractAddressModel, PLCInteractAddressModel>.Trans(plcInteractAddress, PLCInteractAddressCopy);
         ExpressionAssignmentMapper<GenericCommandModel, GenericCommandModel>.Trans(
            plcInteractAddress.StartCommand,
            PLCInteractAddressCopy.StartCommand
         );
         ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
            plcInteractAddress.StartCommand.Tag,
            PLCInteractAddressCopy.StartCommand.Tag
         );
         ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
            plcInteractAddress.DataAddress,
            PLCInteractAddressCopy.DataAddress
         );
         ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
            plcInteractAddress.ExtraDataAddress,
            PLCInteractAddressCopy.ExtraDataAddress
         );
         foreach (var item in plcInteractAddress.ExtensionProps)
         {
            var newProp = new ExtensionItem();
            ExpressionAssignmentMapper<ExtensionItem, ExtensionItem>.Trans(item, newProp);
            PLCInteractAddressCopy.ExtensionProps.Add(newProp);
         }
      }
      else
      {
         PLCInteractAddressCopy = plcInteractAddress;
      }
   }

   private string _serviceName;

   public string ServiceName
   {
      get { return _serviceName; }
      set
      {
         if (_serviceName != value)
         {
            _serviceName = value;
            if (value != null)
            {
               var model = _signalConfig.PLCScanSignals.FirstOrDefault(x => x.ServiceName == value);
               if (model != null)
               {
                  StartSignaItems.Clear();
                  model.StartSignas.ForEach(x =>
                  {
                     StartSignaItems.Add(
                        new GenericCommandModel { Index = x.Index, Tag = new SignalAddressModel(x.Tag.Lable, x.Tag.Address) }
                     );
                  });
               }
               SetDeviceList();
            }
            PLCInteractAddressCopy.ServiceName = value;
         }
      }
   }

   private ProcessTypeEnum _processesType;

   public ProcessTypeEnum ProcessesType
   {
      get { return _processesType; }
      set
      {
         if (_processesType != value)
         {
            _processesType = value;
            SetDeviceList();

            if (value is ProcessTypeEnum.注液量发送 or ProcessTypeEnum.补液量发送)
               DeviceCount = [1, 2];
            else
               DeviceCount = Enumerable.Range(1, 98).ToList();

            PLCInteractAddressCopy.ProcessesType = value;
         }
      }
   }

   public string Tip { get; set; }
   public System.Windows.Media.SolidColorBrush TextColor { get; set; } = System.Windows.Media.Brushes.Green;
   string _deviceMsg = "使用设备索引";
   string _dataMsg = "注意：未找到设备，正使用数据索引！";

   void SetDeviceList()
   {
      DeviceIndexList.Clear();
      int length =
         _deviceConfig.DeviceList.Count(x =>
            x.ServiceName == _serviceName && _processesType.ToString().Contains(x.ProcessesType.ToString())
         ) + 1;
      if (length == 1)
      {
         Tip = _dataMsg;
         TextColor = Brushes.Red;
         length = 32;
      }
      else
      {
         Tip = _deviceMsg;
         TextColor = Brushes.Green;
      }

      for (byte i = 1; i < length; i++)
      {
         DeviceIndexList.Add(i);
      }
   }

   void TabToAddress(PLCInteractAddressModel pLCInteract)
   {
      if (int.TryParse(pLCInteract.StartCommand.Tag.Lable, out int _add))
         pLCInteract.StartCommand.Tag.Address = _add;
      else
         pLCInteract.StartCommand.Tag.Address = -1;
      if (int.TryParse(pLCInteract.DataAddress.Lable, out _add))
         pLCInteract.DataAddress.Address = _add;
      else
         pLCInteract.DataAddress.Address = -1;

      if (int.TryParse(pLCInteract.ExtraDataAddress.Lable, out _add))
         pLCInteract.ExtraDataAddress.Address = _add;
      else
         pLCInteract.ExtraDataAddress.Address = -1;
   }

   public void ConfirmCMD()
   {
      if (string.IsNullOrEmpty(PLCInteractAddressCopy.ServiceName) || PLCInteractAddressCopy.StartCommand == null)
      {
         Growl.Warning("触发命令标签或服务名不能为空！");
         return;
      }
      var extensionGroup = PLCInteractAddressCopy.ExtensionProps.GroupBy(x => x.Type).Where(x => x.Count() > 1);
      if (extensionGroup.Count() > 0)
      {
         string msg = string.Join(';', extensionGroup.Select(x => x.Key));
         var showRes = HandyControl.Controls.MessageBox.Show(
            $"扩展内类型--[{msg}]--重复，确认是否刻意为之？",
            "警告",
            MessageBoxButton.YesNo
         );
         if (showRes != MessageBoxResult.Yes)
            return;
      }
      bool isSave = false;
      bool isUpdateClassPro = false; //更新完整类的属性
      TabToAddress(PLCInteractAddressCopy);
      StringBuilder contrastMsg = new StringBuilder();
      switch (_code)
      {
         case 1:
            if (_signalConfig.PLCInteractAddresses.Any(x => x.StartCommand.Index == PLCInteractAddressCopy.StartCommand.Index))
            {
               Growl.Warning("启动命令标签重复！");
               return;
            }
            isUpdateClassPro = _signalConfig.PLCInteractAddresses.Any(x => x.ProcessesType == PLCInteractAddressCopy.ProcessesType)
               ? false
               : true;
            _signalConfig.PLCInteractAddresses.Add(PLCInteractAddressCopy);
            contrastMsg.Append($"添加[{PLCInteractAddressCopy.ProcessesType}]");
            isSave = true;
            break;
         case 2:
            contrastMsg = _plcInteractAddress.CompareObject(PLCInteractAddressCopy, new Dictionary<string, DifferenceResultDto>());
            if (!string.IsNullOrEmpty(contrastMsg.ToString()))
            {
               isUpdateClassPro =
                  _plcInteractAddress.ProcessesType != PLCInteractAddressCopy.ProcessesType
                  || _plcInteractAddress.ProductionIndex != PLCInteractAddressCopy.ProductionIndex
                  || _plcInteractAddress.ProductionDataType != PLCInteractAddressCopy.ProductionDataType
                  || _plcInteractAddress.DeviceCommunicationType != PLCInteractAddressCopy.DeviceCommunicationType
                     ? true
                     : false;

               ExpressionAssignmentMapper<PLCInteractAddressModel, PLCInteractAddressModel>.Trans(
                  PLCInteractAddressCopy,
                  _plcInteractAddress
               );
               ExpressionAssignmentMapper<GenericCommandModel, GenericCommandModel>.Trans(
                  PLCInteractAddressCopy.StartCommand,
                  _plcInteractAddress.StartCommand
               );
               ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
                  PLCInteractAddressCopy.StartCommand.Tag,
                  _plcInteractAddress.StartCommand.Tag
               );
               ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
                  PLCInteractAddressCopy.DataAddress,
                  _plcInteractAddress.DataAddress
               );
               ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
                  PLCInteractAddressCopy.ExtraDataAddress,
                  _plcInteractAddress.ExtraDataAddress
               );
               _plcInteractAddress.ExtensionProps.Clear();
               foreach (var item in PLCInteractAddressCopy.ExtensionProps)
               {
                  var newProp = new ExtensionItem();
                  ExpressionAssignmentMapper<ExtensionItem, ExtensionItem>.Trans(item, newProp);
                  _plcInteractAddress.ExtensionProps.Add(newProp);
               }

               isSave = true;
            }
            else
            {
               Growl.Success($"文件未修改！");
            }
            break;
      }
      if (isSave)
      {
         _signalConfig.PLCInteractAddresses.ForEach(x =>
         {
            if (x.ProcessesType == PLCInteractAddressCopy.ProcessesType)
            {
               x.ProductionIndex = PLCInteractAddressCopy.ProductionIndex;
               // x.IsLastProcess = PLCInteractAddressCopy.IsLastProcess;
            }
            //else
            //{
            //    if (PLCInteractAddressCopy.IsLastProcess)
            //        x.IsLastProcess = !PLCInteractAddressCopy.IsLastProcess;
            //}
         });

         _signalConfig.Save(_usersStatus.LocalLoggedinUser.Account, contrastMsg.ToString());

         if (isUpdateClassPro)
            _container.Get<MainViewModel>().SyncResourcesAfterConfigUpdate();

         this.RequestClose(true);
      }
   }

   public void AddExtensionCmd()
   {
      PLCInteractAddressCopy.ExtensionProps.Add(new ExtensionItem());
   }

   public void DeleteExtensionCmd(ExtensionItem extension)
   {
      PLCInteractAddressCopy.ExtensionProps.Remove(extension);
   }
}
