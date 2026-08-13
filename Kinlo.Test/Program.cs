using System.Text;

Console.WriteLine("324234");

var _deviceInfo = new Kinlo.Equipment.Models.DeviceInfoModel
{
   IPCOM = "192.168.11.101",
   Port = 10006,
   BindIP = new System.Net.IPAddress([(byte)192, (byte)168, (byte)10, (byte)221]),
   Timeout = 3000,
   TaskToken = new CancellationTokenSource(),
   Communication = Kinlo.SharedBase.Enums.CommunicationEnum.Scale_KZ313_RTU,
   ConnectType = Kinlo.SharedBase.Enums.ConnectTypeEnum.TCP,
   //ConnectionNumber = 1
};
StringBuilder msg = new StringBuilder();
var _device = new Kinlo.Equipment.Devices.ElectronicScales.Scale_KZ_KZ313Tcp(_deviceInfo);
_device.Open();

var _r = _device.ReadValue<float>(null, "Test");
Console.WriteLine(_r);

_device.Close();

Console.WriteLine("完成");
Console.ReadLine();


//var _deviceInfo = new Kinlo.Equipment.Models.DeviceInfoModel
//{
//    IPCOM = "192.168.10.100",
//    Port = 44818,
//    BindIP = new System.Net.IPAddress([(byte)192, (byte)168, (byte)10, (byte)222]),
//    Timeout = 3000,
//    TaskToken = new CancellationTokenSource(),
//    Communication = Kinlo.SharedBase.Enums.CommunicationEnum.CIP_Omron_PLC,
//    ConnectType = Kinlo.SharedBase.Enums.ConnectTypeEnum.TCP,
//    ConnectionNumber = 1
//};
//StringBuilder msg = new StringBuilder();
//var _device = new Kinlo.Equipment.Devices.OmronPLC.OmronCIP(_deviceInfo);
//_device.Open();
//await Handle(_device);
////Stopwatch stopwatch = new Stopwatch();
////stopwatch.Start();
////Parallel.For(0, 32, k =>
////{
////    for (int i = 0; i < 10; i++)
////    {
////        var rrr = _device.ReadClass<PlcGeneric2DTU>(new SignalAddressModel($"PC_ShortCicute[{i}].ToPCData[{k}]"));
////        Console.WriteLine(DateTime.Now.ToString("mm:ss ffff") + JsonSerializer.Serialize(rrr) + $"线程{k},第{i + 1}次\r\n");

////    }
////    for (int i = 0; i < 10; i++)
////    {
////        var rrr = _device.ReadObjects<PlcGeneric2DTU>(new SignalAddressModel($"PC_ShortCicute[{i}].ToPCData"));
////        Console.WriteLine(DateTime.Now.ToString("mm:ss ffff") + JsonSerializer.Serialize(rrr) + $"线程{k},OBJECK第{i + 1}次\r\n");

////    }
////});

////stopwatch.Stop();

////Console.WriteLine($"用时：{stopwatch.ElapsedMilliseconds}");

//_device.Close();

//Console.WriteLine("完成");
//Console.ReadLine();
//Console.ReadKe

//async Task Handle(IPLC _plc)
//{
//    PLCInteractAddressModel Context = new PLCInteractAddressModel();
//    Context.DataAddress = new SignalAddressModel("PC_InjectionMES");
//    var _trayCodeBytes = _plc.ReadValue<string>(new SignalAddressModel($"{Context.DataAddress.Lable}.TrayCode", 0));
//    var _cupCodeBytes = _plc.ReadValue<string>(new SignalAddressModel($"{Context.DataAddress.Lable}.CupCode", 0));
//    var _trayId = _plc.ReadValue<long>(new SignalAddressModel($"{Context.DataAddress.Lable}.TrayID", 0));

//    for (int lineIndex = 0; lineIndex < 4; lineIndex++)
//    {
//        PlcToPcInjectionLineDTU _lineData = new PlcToPcInjectionLineDTU(8);
//        _plc.ReadClass(new SignalAddressModel($"{Context.DataAddress.Lable}.Line[{lineIndex}]", 0), _lineData);
//        Console.WriteLine($"接收PLC套杯号：[{_cupCodeBytes}]，托盘号：[{_trayCodeBytes}]，注液站数据：{JsonSerializer.Serialize(_lineData, GenericHelper.SerializerOptions)}");

//        for (int columnIndex = 0; columnIndex < _lineData.LineInjections.Length; columnIndex++)
//        {
//            var _leakData = _lineData.LineInjections[columnIndex];
//            if (_leakData == null || _leakData.ID == 0) continue;

//            BatInjectStationModel _batInjectionStation = new BatInjectStationModel(_leakData.ID, "", Context.ProcessesType);
//            _batInjectionStation.LineIndex = (byte)lineIndex;
//            _batInjectionStation.ColumnIndex = (byte)columnIndex;
//            _batInjectionStation.InjectPumpNo = (byte)_leakData.InjectionPumpNo;
//            _batInjectionStation.InjectStationNo = (byte)_leakData.InjectionStationNo;
//            _batInjectionStation.InjectNozzleNo = (byte)_leakData.InjectionNozzle;
//            _batInjectionStation.InjectedProcessDuration = _leakData.InjectionTime;
//            _batInjectionStation.InjectedDuration = _leakData.InjectionDuration;
//            _batInjectionStation.TrayCode = _trayCodeBytes;
//            _batInjectionStation.CupCode = _cupCodeBytes;

//        }
//    }

//}
