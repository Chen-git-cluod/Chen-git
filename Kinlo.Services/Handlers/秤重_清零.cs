namespace Kinlo.Services.Handlers;

[DeviceConnec(ProcessTypeEnum.前称重清零, [CommunicationEnum.None])]
[DeviceConnec(ProcessTypeEnum.后称重清零, [CommunicationEnum.None])]
[DeviceConnec(ProcessTypeEnum.补液称清零, [CommunicationEnum.None])]
[DeviceConnec(ProcessTypeEnum.下料称重清零, [CommunicationEnum.None])]
[DeviceConnec(ProcessTypeEnum.回氦称重清零, [CommunicationEnum.None])]
public class WeightZeroingHandler : ServiceHandlerBase
{
    //添加界面显示
    ConcurrentDictionary<int, string> _alarmDevice = new ConcurrentDictionary<int, string>();
    SignalAddressModel? _alarmAddress = null;

    public WeightZeroingHandler(
       IContainer container,
       IDevice plc,
       PLCInteractAddressModel plcInteractAddress,
       CancellationTokenSource taskToken
    )
       : base(container, plc, plcInteractAddress, taskToken)
    {
        _alarmAddress = Context.ProcessesType switch
        {
            ProcessTypeEnum.前称重清零 => _plcSignalConfig.PLCAlarmAddresses.Alarm_Zeroing_BeforeWeighing,
            ProcessTypeEnum.后称重清零 or ProcessTypeEnum.回氦称重清零 => _plcSignalConfig.PLCAlarmAddresses.Alarm_Zeroing_AfterWeighing,
            ProcessTypeEnum.补液称清零 => _plcSignalConfig.PLCAlarmAddresses.Alarm_Zeroing_RefillWeighing,
            ProcessTypeEnum.下料称重清零 => _plcSignalConfig.PLCAlarmAddresses.Alarm_Zeroing_DownWeighing,
        };
    }

    protected override Task HandleCore(short plcValue)
    {
        var devices = _devicesConfig.GetRunDevices(x =>
           x.DeviceInfo.ServiceName == Context.ServiceName
           && x.DeviceInfo.Communication == Context.DeviceCommunicationType
           && Context.ProcessesType.ToString().Contains(x.DeviceInfo.ProcessesType.ToString())
        );

        if (devices.Count < 1)
        {
            _isDeviceAlarm = true;
            $"找不到设备，不给PLC发结果及完成信号，请停机检查！;".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
            return Task.CompletedTask;
            ;
        }
        if (devices.Count < Context.DataLength)
        {
            $"注意：实际设备数量{devices.Count}小于设置数量{Context.DataLength};".LogProcess(_taskLogHeader, Log4NetLevelEnum.警告, true);
        }
        for (int i = 0; i < 3; i++)
        {
            Parallel.ForEach(
               devices,
               device =>
               {
                   device.WriteValue(0, null, Context.ToProcessLogHeader(device.DeviceInfo.Index));
               }
            );

            Thread.Sleep(500);
            _alarmDevice.Clear();

            Parallel.ForEach(
               devices,
               device =>
               {
                   var deviceResult = device.ReadValue<double>(null, Context.ToProcessLogHeader(device.DeviceInfo.Index));
                   ResultTypeEnum rs = ResultTypeEnum.OK;
                   if (deviceResult.Status == DeviceStatus.Success)
                   {
                       if (deviceResult.Value < -0.1 || deviceResult.Value > 0.1)
                       {
                           rs = ResultTypeEnum.NG;
                           _alarmDevice[device.DeviceInfo.Index] = $"清零后值超出范围";
                       }
                   }
                   else
                   {
                       rs = ResultTypeEnum.NG;
                       _alarmDevice[device.DeviceInfo.Index] = $"{deviceResult.Status}";
                   }

                   new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[{device.DeviceInfo.Index - 1}]").WritePlcResult(
                   rs,
                   ResultTypeEnum._,
                   _plc,
                   _parameterConfig,
                   Context.ToProcessLogHeader(device.DeviceInfo.Index)
                ); //写入PLC结果
               }
            );

            if (!_alarmDevice.Any())
                break;
        }

        if (_alarmDevice.Any())
        {
            $"清零失败：\r\n{string.Join(',', _alarmDevice.Select(x => $"{x.Key}号：{x.Value}\r\n"))}".LogProcess(
               _taskLogHeader,
               Log4NetLevelEnum.错误,
               true
            );
            // _isDeviceAlarm = true;
            // WritePlcSingle(1, _alarmAddress);报警 弃用
        }
        return Task.CompletedTask;
    }
}
