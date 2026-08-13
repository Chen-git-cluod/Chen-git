namespace Kinlo.Equipment.Devices.CardReaders;
/// <summary>
/// 通用串口刷卡器
/// </summary>
[DeviceConnec([CommunicationEnum.通用串口刷卡器])]
public class CardReaderSerialPort : DeviceBase, ICardReader<string>, IContinuousReading
{
    public Action<string>? CardAction { get; set; }
    public ReconnectInfoModel ReconnectInfo { get; set; } = new ReconnectInfoModel();

    private readonly List<byte> _receiveBuffer = new List<byte>();
    public CardReaderSerialPort(DeviceInfoModel info)
    : base(info) { }
    public override bool Open()
    {
        if (base.Open())
        {
            _receiveBuffer.Clear();
            _ = ContinuousReading();
            return true;
        }
        return false;
    }
    /// <summary>
    /// 持续读取
    /// </summary>
    /// <returns></returns>
    public Task ContinuousReading()
    {
        return Task.Run(async () =>
        {
            var logHeader = DeviceInfo.ToDeviceLogHeader();
            Thread.Sleep(100);
            while (!IsShutdown)
            {
                try
                {
                    var res = Connect.TryRead(10, logHeader);
                    if (res.State == CommState.Failed)
                    {
                        if (res.Exception is not null and not TimeoutException)
                        {
                            $"{res.Exception}".LogProcess(logHeader);
                        }
                        Thread.Sleep(300);
                        continue;
                    }
                    else if (res.State == CommState.NeedReconnect)
                    {
                        if (CanReconnect())
                            this.Reconnect(logHeader);
                        else
                            $"在 {ReconnectInfo.TimeWindow} 分钟内重连次数超 {ReconnectInfo.MaxReconnectCount} 次上限，不重连！".LogProcess(logHeader);
                        break; // 一定要退出，重连打开时会重新开始一个任务
                    }
                    if (res.Data!.Any())
                    {
                        _receiveBuffer.AddRange(res.Data);
                        string tempCode = Encoding.ASCII.GetString(_receiveBuffer.ToArray()).Replace("\r\n", "").Replace("\n", "").Replace("\r","").Replace("\0","");
                        if (tempCode.Length >= 10)
                        {
                            string code = tempCode.Substring(0, 10);
                            $"[{DeviceInfo.Communication}]取到卡号：[{code}]".LogProcess(logHeader);
                            CardAction?.Invoke(code);
                            _receiveBuffer.Clear();
                        }
                    }
                    await Task.Delay(50);
                }
                catch (Exception ex)
                {
                    $"[{DeviceInfo.Communication}] Read 方法异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
                    Thread.Sleep(300);
                }
            }
        });
    }
    public override DeviceResult<TClass> ReadClass<TClass>(
    SignalAddressModel address,
    TClass obj,
    string logHeader,
    DeviceOperationOptions? options = null
    )
    where TClass : class
    {
        throw new NotImplementedException();
    }
    public override DeviceResult<TValue> ReadValue<TValue>(
    SignalAddressModel address,
    string logHeader,
    DeviceOperationOptions? options = null
    )
    where TValue : default
    {
        throw new NotImplementedException();
    }
    public override bool WriteClass<TClass>(
    TClass value,
    SignalAddressModel address,
    string logHeader,
    DeviceOperationOptions? options = null
    )
    {
        throw new NotImplementedException();
    }
    public override bool WriteValue(
    object value,
    SignalAddressModel address,
    string logHeader,
    DeviceOperationOptions? options = null,
    Encoding? encoding = null
    )
    {
        throw new NotImplementedException();
    }
    public bool CanReconnect() => ReconnectInfo.CanReconnect();
}