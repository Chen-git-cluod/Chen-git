namespace Kinlo.Equipment.Devices.ElectronicScales;
/// <summary>
/// 中航电测 ZH8010电子称 
/// </summary>
[DeviceConnec([CommunicationEnum.Scale_ZH8010_TCP])]
public class Scale_ZH8010Tcp : DeviceBase
{
    #region Modbus TCP报文
    private readonly byte[] _readRequest = { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, 0x00, 0x70, 0x00, 0x02 };
    private readonly byte[] _zeroClear = { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x06, 0x00, 0x31, 0x00, 0x04 };
    private readonly byte[] _heartbeatPacket = { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, 0x00, 0x70, 0x00, 0x02 };
    #endregion

    #region 心跳长连接管控
    private readonly SemaphoreSlim _commLock = new SemaphoreSlim(1, 1);
    private Task? _heartbeatTask;
    private bool _isHeartbeatRunning;
    private long _lastCommSuccessTime;

    // 心跳配置参数
    private const int HeartbeatIntervalMs = 1000;
    private const long LinkDeadThresholdMs = 3000;
    private const int HeartbeatReadLength = 20;
    #endregion

    public Scale_ZH8010Tcp(DeviceInfoModel info)
        : base(info)
    {
        _lastCommSuccessTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public override bool Open()
    {
        bool openResult = Connect.Open();
        if (openResult && !_isHeartbeatRunning)
        {
            StartHeartbeatLoop();
        }
        return openResult;
    }

    public override void Close()
    {
        StopHeartbeatLoop();
        Connect.Close();
        base.Close();
    }
    private void StartHeartbeatLoop()
    {
        if (_isHeartbeatRunning) return;
        _isHeartbeatRunning = true;

        _heartbeatTask = Task.Run(async () =>
        {
            while (_isHeartbeatRunning && !IsShutdown)
            {
                try
                {
                    await Task.Delay(HeartbeatIntervalMs);
                    await RunSingleHeartbeat();
                }
                catch (Exception ex)
                {
                    string logHeader = $"Scale_ZH8010_TCP_心跳";
                    $"心跳循环异常：{ex.Message}".LogProcess(logHeader, Log4NetLevelEnum.警告, true);
                }
            }
        });
    }

    private void StopHeartbeatLoop()
    {
        _isHeartbeatRunning = false;
        _heartbeatTask?.Wait(1000);
        _heartbeatTask = null;
    }

    private async Task RunSingleHeartbeat()
    {
        string logHeader = $"Scale_ZH8010_TCP_心跳";
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // 超过阈值判定链路死亡，先停心跳再重连，重连成功重启心跳
        if (now - _lastCommSuccessTime > LinkDeadThresholdMs)
        {
            $"{Connect.DeviceInfo.IPCOM} 链路长时间无响应，间隔{(now - _lastCommSuccessTime)}ms，停止心跳执行重连".LogProcess(logHeader, Log4NetLevelEnum.警告, true);
            StopHeartbeatLoop();
            Connect.Close();
            await Task.Delay(150);
            bool openOk = Connect.Open();
            if (openOk)
            {
                $"{Connect.DeviceInfo.IPCOM} 重连成功，重新启动心跳".LogProcess(logHeader, Log4NetLevelEnum.警告, true);
                Interlocked.Exchange(ref _lastCommSuccessTime, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                StartHeartbeatLoop();
            }
            return;
        }

        // 200ms抢不到锁，直接跳过本次心跳，不阻塞业务读写
        if (!await _commLock.WaitAsync(200))
            return;

        try
        {
            var res = Connect.WriteAndRead(_heartbeatPacket, null, logHeader, HeartbeatReadLength);
            if (res.State == CommState.Success && res.Data != null && res.Data.Length >= 13)
            {
                if (CheckModbusTcpResponse(_heartbeatPacket, res.Data))
                {
                    Interlocked.Exchange(ref _lastCommSuccessTime, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    return;
                }
            }

            $"心跳响应异常，通信状态:{res.State},数据长度:{res.Data?.Length ?? 0}".LogProcess(logHeader, Log4NetLevelEnum.警告);
            Connect.Close();
        }
        catch (SocketException sex)
        {
            $"心跳Socket异常[{sex.ErrorCode}]".LogProcess(logHeader, Log4NetLevelEnum.警告);
            Connect.Close();
        }
        catch (Exception ex)
        {
            $"心跳收发异常：{ex.Message}".LogProcess(logHeader, Log4NetLevelEnum.警告);
            Connect.Close();
        }
        finally
        {
            _commLock.Release();
        }
    }

    public override DeviceResult<TClass> ReadClass<TClass>(SignalAddressModel address, TClass obj, string logHeader, DeviceOperationOptions? options = null) where TClass : class
    {
        throw new NotImplementedException();
    }

    public override DeviceResult<TValue> ReadValue<TValue>(SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null) where TValue : default
    {
        logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
        options ??= new DeviceOperationOptions() { RetryCount = 3 };
        string errMsg = string.Empty;

        for (int k = 0; k < options.RetryCount; k++)
        {
            if (IsShutdown)
                return DeviceResult<TValue>.Failure(DeviceStatus.设备已停机);

            _commLock.Wait();
            try
            {
                var res = Connect.WriteAndRead(_readRequest, null, logHeader, readLength: 20);
                if (res.State != CommState.Success || res.Data == null || res.Data.Length < 13)
                {
                    errMsg = $"读取重量响应异常，状态：{res.State}，数据长度：{res.Data?.Length ?? 0}";
                    errMsg.LogProcess(logHeader, Log4NetLevelEnum.警告, true);
                    if (res.State == CommState.NeedReconnect)
                        Connect.Close();
                    Thread.Sleep(300);
                    continue;
                }

                if (!CheckModbusTcpResponse(_readRequest, res.Data))
                {
                    errMsg = "Modbus响应报文校验失败，协议错位";
                    errMsg.LogProcess(logHeader, Log4NetLevelEnum.警告, true);
                    Thread.Sleep(300);
                    continue;
                }

                // 读取成功刷新通信时间戳，心跳不再触发断链重连
                Interlocked.Exchange(ref _lastCommSuccessTime, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                double weight = ParseWeight(res.Data);
                return DeviceResult<TValue>.Success((TValue)(object)weight);
            }
            catch (SocketException sex)
            {
                if (sex.ErrorCode == 10061)
                    errMsg = $"读取重量Socket异常[{sex.ErrorCode}]：设备主动拒绝连接，设备仅支持单客户端";
                else
                    errMsg = $"读取重量Socket异常[{sex.ErrorCode}]：{sex.Message}";
                errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
                Connect.Close();
                // 异常后主动重连，复用原有逻辑
                Thread.Sleep(150);
                Connect.Open();
            }
            catch (Exception ex)
            {
                errMsg = $"读取重量异常：{ex}";
                errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
                Connect.Close();
                Thread.Sleep(150);
                Connect.Open();
            }
            finally
            {
                _commLock.Release();
            }
            Thread.Sleep(300);
        }

        return DeviceResult<TValue>.Failure(DeviceStatus.取值失败, errMsg);
    }

    public override bool WriteClass<TClass>(TClass value, SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public override bool WriteValue(object value, SignalAddressModel address, string logHeader, DeviceOperationOptions? options = null, Encoding? encoding = null)
    {
        logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
        options ??= new DeviceOperationOptions() { RetryCount = 3 };

        for (int i = 0; i < options.RetryCount; i++)
        {
            if (IsShutdown)
                return false;

            _commLock.Wait();
            try
            {
                var r = Connect.WriteAndRead(_zeroClear, null, logHeader, readLength: 20);
                if (r.State != CommState.Success || r.Data == null || r.Data.Length < 12)
                {
                    Connect.Close();
                    Thread.Sleep(150);
                    Connect.Open();
                    continue;
                }
                if (CheckModbusTcpResponse(_zeroClear, r.Data))
                {
                    Interlocked.Exchange(ref _lastCommSuccessTime, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    return true;
                }
            }
            catch (SocketException sex)
            {
                Connect.Close();
                Thread.Sleep(150);
                Connect.Open();
            }
            catch (Exception ex)
            {
                $"清零异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
                Connect.Close();
                Thread.Sleep(150);
                Connect.Open();
            }
            finally
            {
                _commLock.Release();
            }
            Thread.Sleep(150);
        }
        return false;
    }

    public static double ParseWeight(byte[] responseBytes)
    {
        if (responseBytes == null || responseBytes.Length != 13)
            return 0;
        ushort lowWord = (ushort)(responseBytes[9] << 8 | responseBytes[10]);
        ushort highWord = (ushort)(responseBytes[11] << 8 | responseBytes[12]);
        int totalValue = highWord * 65536 + lowWord;
        return totalValue / 100.0f;
    }

    private bool CheckModbusTcpResponse(byte[] send, byte[] recv)
    {
        if (recv.Length < 8)
            return false;
        if (send[0] != recv[0] || send[1] != recv[1])
            return false;
        if (send[6] != recv[6] || send[7] != recv[7])
            return false;
        if ((recv[7] & 0x80) != 0)
            return false;
        return true;
    }
}