namespace Kinlo.Services.Handlers;

/// <summary>
/// 设备状态
/// </summary>
[DeviceConnec(ProcessTypeEnum.设备状态, [CommunicationEnum.None])] //指定工艺，可指定多个
public partial class PLcStatusAndAlarmHandler : ServiceHandlerBase
{
    private short _lastPlcValue { get; set; } = -1;
    private readonly List<DeviceStateEnum> _deviceStateEnums = new List<DeviceStateEnum>();
    protected SignalAddressModel? _shutdownAddress = null;

 
    private readonly ConcurrentDictionary<long, string> _alarmBizUuidPool = new ConcurrentDictionary<long, string>();

    private readonly ConcurrentDictionary<long, string> _manualShutdownUuidPool = new ConcurrentDictionary<long, string>();

    private readonly ConcurrentDictionary<long, string> _materialShortageUuidPool = new ConcurrentDictionary<long, string>();

    public PLcStatusAndAlarmHandler(
       IContainer container,
       IDevice plc,
       PLCInteractAddressModel plcInteractAddress,
       CancellationTokenSource taskToken
    )
       : base(container, plc, plcInteractAddress, taskToken)
    {
        _deviceStateEnums = Enum.GetValues(typeof(DeviceStateEnum)).Cast<DeviceStateEnum>().ToList();
        if (!string.IsNullOrWhiteSpace(Context.ExtraDataAddress.Lable))
        {
            _shutdownAddress = Context.ExtraDataAddress;
        }
        else
        {
            var add = _plcSignalConfig.CustomPlcInteractAddresses.FirstOrDefault(x =>
               x.CustomInteractName == CustomInteractNameEnum.PLC主动停机原因
            );
            if (add != null)
            {
                _shutdownAddress = add.DataAddress;
            }
        }
    }

    /// <summary>
    /// 设备状态因无需写完成信号，需重写Handle方法
    /// </summary>
    /// <param name="plcValue"></param>
    /// <returns></returns>
    public override async Task<int> Handle(short plcValue)
    {
        try
        {
            await HandleCore(plcValue);
        }
        catch (Exception ex)
        {
            $"出现异常:{ex};".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
        }
        finally
        {
            _lastPlcValue = plcValue;
            try
            {
                await Task.Delay(_parameterConfig.AdvancedConfig.DeviceStatusSpanMillisecond, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException) { }
        }
        return await Task.FromResult(Context.Key);
    }
    protected override async Task<int> HandleCore(short plcValue)
    {
        //if (_lastPlcValue == -1)
        //    return Context.Key;
        //var tasks = _deviceStateEnums
        //   .Select(stateType =>
        //   {
        //       var newPlcStatus = plcValue.GetPlcStatus(stateType);
        //       var oldPlcStatus = _lastPlcValue.GetPlcStatus(stateType);
        //       return PlcStatusHandleAsync(stateType, oldPlcStatus, newPlcStatus);
        //   })
        //   .ToList();
        //if (plcValue != _lastPlcValue)
        //{
        //    tasks.Add(DeviceStatusChangAsync(plcValue));
        //}
        //await Task.WhenAll(tasks);
        //await _plcStatusConfig.TrimPlcStatusToLast24HoursAsync();
        //return Context.Key;
        //if (_lastPlcValue == -1)
        //    return Context.Key;
        //await PendingAlarmHandleAsync();
        // 删掉并行Task.WhenAll，改用foreach串行执行，避免时序丢失
        foreach (var stateType in _deviceStateEnums)
        {
            var newPlcStatus = plcValue.GetPlcStatus(stateType);
            var oldPlcStatus = _lastPlcValue.GetPlcStatus(stateType);
            await PlcStatusHandleAsync(stateType, oldPlcStatus, newPlcStatus);
        }

        if (plcValue != _lastPlcValue)
        {
            await DeviceStatusChangAsync(plcValue);
        }

        await _plcStatusConfig.TrimPlcStatusToLast24HoursAsync();
        return Context.Key;
    }
}
