using System.Text.RegularExpressions;
using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(IsSingleton = false)]
public class DeviceViewModel : Screen
{
   #region private field
   private UsersStatusConfig _usersStatus;
   private DeviceClientModel _deviceClient;
   private IContainer _container;
   private PLCSignalConfig _signalConfig;
   private DevicesConfig _deviceConfig;
   private int _code;
   private string _comPattern = ".*";
   private string _ipPatternAdd = "((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}";
   private string _ipPatternEdit = "^((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}$";
   private string _ipPattern2 = "(((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})\\.){3}";
   #endregion

   #region public property
   public string Title { get; set; } = string.Empty;
   public string Ports { get; set; } = string.Empty;
   public DeviceClientModel DeviceClientCopy { get; set; } = new DeviceClientModel();
   public ObservableCollection<string> ServiceItems { get; set; } = new();

   #endregion
   public DeviceViewModel(IContainer container)
   {
      _container = container;
      _deviceConfig = container.Get<DevicesConfig>();
      _signalConfig = container.Get<PLCSignalConfig>();
      _usersStatus = container.Get<UsersStatusConfig>();
      ServiceItems = new ObservableCollection<string>(_signalConfig.PLCScanSignals.Select(x => x.ServiceName));
   }

   public void SetDeviceClient(DeviceClientModel deviceClient, int code)
   {
      _code = code;
      Title = code == 1 ? "新建设备" : "编辑设备";
      if (code == 2)
      {
         _deviceClient = deviceClient;
         Ports = deviceClient.Port.ToString();
         ExpressionAssignmentMapper<DeviceClientModel, DeviceClientModel>.Trans(deviceClient, DeviceClientCopy);
      }
      else
      {
         DeviceClientCopy = deviceClient;
      }
   }

   public void ConfirmCMD()
   {
      if (string.IsNullOrEmpty(DeviceClientCopy.ServiceName))
      {
         Growl.Warning("服务名为空，请重新选择，如无选择项请先新建服务！");
         return;
      }

      if (DeviceClientCopy.ConnectType == ConnectTypeEnum.None)
      {
         int.TryParse(Ports, out int p);
         switch (_code)
         {
            case 1:
               AddDeviceSingle(DeviceClientCopy.IPCOM, p);
               this.RequestClose(true);
               _deviceConfig.DeviceList = _deviceConfig
                  .DeviceList.OrderBy(x => x.ProcessesType)
                  .ThenBy(x => x.Index)
                  .ToObservableCollection();

               _deviceConfig.Save(_usersStatus.LocalLoggedinUser.Account, string.Empty);
               break;
            case 2:
               EditDevice(p);
               break;
         }
         return;
      }

      if (DeviceClientCopy.OutTime < 100)
      {
         Growl.Warning("超时时间低于100ms,请重新输入！");
         return;
      }

      var _portMatch = Regex.Match(Ports, @"\d+");
      if (!_portMatch.Success)
      {
         Growl.Warning("端口(波特率)地址不合法，请重新输入！");
         return;
      }
      string _pattern =
         DeviceClientCopy.ConnectType == ConnectTypeEnum.SerialPort ? _comPattern
         : _code == 1 ? _ipPatternAdd
         : _ipPatternEdit;

      var _ipMatched = Regex.Match(DeviceClientCopy.IPCOM, _pattern);

      if (!_ipMatched.Success)
      {
         Growl.Warning("IP地址不合法，请重新输入！");
         return;
      }

      switch (_code)
      {
         case 1:
            AddDevices(_ipMatched, _portMatch);
            break;
         case 2:
            EditDevice(int.Parse(_portMatch.Captures[0].Value));
            break;
      }
   }

   private void AddDevices(Match _ipMatched, Match _portMatch)
   {
      if (DeviceClientCopy.ConnectType != ConnectTypeEnum.SerialPort)
      {
         var _portInfos = Ports.Split(',');
         var _ipInfos = DeviceClientCopy.IPCOM.Split(',');
         int _portCount =
            _portInfos.Length == 1 ? 1
            : int.TryParse(_portInfos[1], out int _pc) ? _pc
            : 1;
         int _ipCount =
            _ipInfos.Length == 1 ? 1
            : int.TryParse(_ipInfos[1], out int _ic) ? _ic
            : 1;

         if (_portCount > 1 && _ipCount > 1)
         {
            Growl.Warning("生成多条时只能指定IP或端口其中之一，请重新输入！");
            return;
         }

         if (_ipCount > 1)
         {
            var _ipLast = short.Parse(_ipMatched.Captures[0].Value.Split('.')[3]);
            for (int i = 0; i < _ipCount; i++)
            {
               string _ip = $"{Regex.Match(_ipMatched.Captures[0].Value, _ipPattern2).Captures[0].Value}{_ipLast}";
               int _port = int.Parse(_portMatch.Value);
               AddDeviceSingle(_ip, _port);
               _ipLast++;
            }
         }
         else
         {
            int _port = int.Parse(_portMatch.Value);
            for (int i = 0; i < _portCount; i++)
            {
               AddDeviceSingle(_ipMatched.Value, _port);
               _port++;
            }
         }

         _deviceConfig.DeviceList = _deviceConfig.DeviceList.OrderBy(x => x.ProcessesType).ThenBy(x => x.Index).ToObservableCollection();

         _deviceConfig.Save(_usersStatus.LocalLoggedinUser.Account, string.Empty);
         this.RequestClose(true);
      }
   }

   private void AddDeviceSingle(string ip, int port) =>
      _deviceConfig.DeviceList.Add(
         new DeviceClientModel
         {
            ServiceName = DeviceClientCopy.ServiceName,
            ConnectType = DeviceClientCopy.ConnectType,
            Communication = DeviceClientCopy.Communication,
            ProcessesType = DeviceClientCopy.ProcessesType,
            Index = (byte)(_deviceConfig.DeviceList.Count(x => x.ProcessesType == DeviceClientCopy.ProcessesType) + 1),
            IPCOM = ip,
            Port = port,
            OutTime = DeviceClientCopy.OutTime,
            IsEnable = true,
            IsOnline = 1,
         }
      );

   private void EditDevice(int port)
   {
      DeviceClientCopy.Port = port;
      StringBuilder _contrastMsg = _deviceClient.CompareObject(DeviceClientCopy, new Dictionary<string, DifferenceResultDto>());

      if (!string.IsNullOrEmpty(_contrastMsg.ToString()))
      {
         ExpressionAssignmentMapper<DeviceClientModel, DeviceClientModel>.Trans(DeviceClientCopy, _deviceClient);
         _deviceConfig.Save(_usersStatus.LocalLoggedinUser.Account, _contrastMsg.ToString());
         this.RequestClose(true);
      }
      else
      {
         Growl.Success($"文件未修改！");
      }
   }
}
