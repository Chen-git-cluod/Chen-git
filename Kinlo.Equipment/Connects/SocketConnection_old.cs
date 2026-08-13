//namespace Kinlo.Equipment.Connects;
///// <summary>
///// 弃用
///// </summary>
//public abstract class SocketConnection_old_d : IConnect
//{
//    public DeviceInfoModel DeviceInfo { get; set; }
//    public Action<IConnect> ReconnectAction { get; set; }
//    private readonly object _lock = new();
//    private Socket? _socket = null;
//    public SocketConnection_old_d(DeviceInfoModel info, Action<IConnect> reconnectAction)
//    {
//        DeviceInfo = info;
//        ReconnectAction = reconnectAction;
//    }

//    public virtual void Close()
//    {
//        if (_socket == null) return;
//        try
//        {
//            _socket.Shutdown(SocketShutdown.Both);
//        }
//        catch { } //忽略
//        finally
//        {
//            try
//            {
//                _socket.Close();
//                // socket.Dispose();
//            }
//            catch (Exception ex)
//            {
//                $"关闭异常：{ex}".LogProcess(DeviceInfo.ToDeviceLogHeader(), Log4NetLevelEnum.错误);
//            }
//        }
//    }

//    public virtual bool Open()
//    {
//        try
//        {
//            Close();

//            _socket = DeviceInfo.ConnectType switch
//            {
//                ConnectTypeEnum.UDP => new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp),
//                _ => new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
//            };

//            _socket.ReceiveTimeout = DeviceInfo.Timeout;
//            _socket.SendTimeout = DeviceInfo.Timeout;
//            _socket.ExclusiveAddressUse = false;
//            _socket.Bind(new System.Net.IPEndPoint(DeviceInfo.BindIP, 0));
//            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

//            _socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(DeviceInfo.IPCOM), DeviceInfo.Port), cts.Token).GetAwaiter().GetResult();

//            return true;
//        }
//        catch (Exception ex)
//        {
//            $"打开连接异常:{ex}".LogProcess(DeviceInfo.ToDeviceLogHeader(), Log4NetLevelEnum.错误);
//        }
//        return false;
//    }

//    public virtual byte[] Read(int length, string logHeader)
//    {
//        lock (_lock)
//        {
//            for (int i = 0; i < 2; i++)
//            {
//                try
//                {
//                    byte[] bytes = new byte[length];
//                    length = _socket.Receive(bytes, SocketFlags.None);
//                    return bytes.Take(length).ToArray();
//                }
//                catch (SocketException sex)
//                {
//                    var strategy = ReconnectStrategy(sex, i + 1, logHeader);
//                    switch (strategy)
//                    {
//                        case ReconnectStrategyEnum.退出当前方法:
//                            return new byte[0];
//                        case ReconnectStrategyEnum.忽略错误继续:
//                            continue;
//                        case ReconnectStrategyEnum.立即重连:
//                            Thread.Sleep(200);
//                            ReconnectAction?.Invoke(this);
//                            continue;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    if (DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
//                        return new byte[0];

//                    $"第{i + 1}次发生异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);

//                }
//                Thread.Sleep(100);
//            }
//        }
//        return new byte[0];
//    }
//    public byte[] TryRead(int length, string logHeader)
//    {
//        lock (_lock)
//        {
//            try
//            {
//                byte[] bytes = new byte[length];
//                length = _socket.Receive(bytes, SocketFlags.None);
//                return bytes.Take(length).ToArray();
//            }
//            catch (SocketException sex)
//            {

//                var strategy = ReconnectStrategy(sex, 1, logHeader);
//                if (strategy == ReconnectStrategyEnum.立即重连)
//                {
//                    Thread.Sleep(200);
//                    ReconnectAction?.Invoke(this);
//                }
//                return new byte[0];
//            }
//            catch (Exception ex)
//            {
//                if (DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
//                    return new byte[0];

//                $"发生异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);

//            }
//        }
//        return new byte[0];
//    }
//    public virtual void SetTimeOut(int timeout)
//    {
//        lock (_lock)
//        {
//            if (_socket == null) return;
//            _socket.SendTimeout = timeout;
//            _socket.ReceiveTimeout = timeout;
//        }
//    }

//    public virtual bool Write(byte[] buffer, string logHeader)
//    {
//        lock (_lock)
//            for (int i = 0; i < 2; i++)
//            {
//                try
//                {
//                    int length = _socket.Send(buffer, SocketFlags.None);
//                    if (length == buffer.Length)
//                    {
//                        return true;
//                    }
//                }
//                catch (SocketException sex)
//                {
//                    var strategy = ReconnectStrategy(sex, i + 1, logHeader);
//                    switch (strategy)
//                    {
//                        case ReconnectStrategyEnum.退出当前方法:
//                            return false;
//                        case ReconnectStrategyEnum.忽略错误继续:
//                            continue;
//                        case ReconnectStrategyEnum.立即重连:
//                            Thread.Sleep(200);
//                            ReconnectAction?.Invoke(this);
//                            continue;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    if (DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
//                        return false;
//                    $"第{i + 1}次发生异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
//                }
//                Thread.Sleep(100);
//            }
//        return false;
//    }

//    public virtual byte[]? WriteAndRead(byte[] rawBytes, IProtocolHelper? protocol, string logHeader, int readLength = 1024)
//    {
//        lock (_lock)
//        {
//            for (int i = 0; i < 2; i++)
//            {
//                Stopwatch _stopwatch = new Stopwatch();
//                try
//                {
//                    List<byte> list = new List<byte>();
//                    var finalBytes = protocol == null ? rawBytes.ToArray() : protocol.Serialize(rawBytes).ToArray();
//                    //string s = BitConverter.ToString(finalBytes).Replace('-', ' ');
//                    _socket.Send(finalBytes);
//                    byte[] buffer = new byte[readLength];
//                    Thread.Sleep(100);
//                    int length = _socket.Receive(buffer);
//                    list.AddRange(buffer.Take(length));
//                    _stopwatch.Restart();
//                    while (protocol != null && !protocol.Verify(list))
//                    {
//                        if (_stopwatch.ElapsedMilliseconds > 5000)
//                        {
//                            $"第{i + 1}次循环，读取超时，用时{_stopwatch.ElapsedMilliseconds};".LogProcess(logHeader, Log4NetLevelEnum.错误);
//                            return default;
//                        }
//                        length = _socket.Receive(buffer);
//                        list.AddRange(buffer.Take(length));
//                    }
//                    //  string s2 = BitConverter.ToString(list.ToArray()).Replace('-', ' ');
//                    if (protocol != null)
//                        return protocol.Deserialize(list);
//                    return list.ToArray();
//                }
//                catch (SocketException sex)
//                {
//                    var strategy = ReconnectStrategy(sex, i + 1, logHeader);
//                    switch (strategy)
//                    {
//                        case ReconnectStrategyEnum.退出当前方法:
//                            return default;
//                        case ReconnectStrategyEnum.忽略错误继续:
//                            continue;
//                        case ReconnectStrategyEnum.立即重连:
//                            Thread.Sleep(200);
//                            ReconnectAction?.Invoke(this);
//                            continue;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    if (DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
//                        return default;
//                    $"第{i + 1}次循环，发生异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
//                    Thread.Sleep(200);
//                }
//            }
//        }
//        return default;
//    }

//    public void ClearCache(string logHeader)
//    {
//        if (_socket == null) return;
//        logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, null);
//        lock (_lock)
//        {
//            int receiveTimeout = 2000;
//            try
//            {
//                receiveTimeout = _socket.ReceiveTimeout;
//                _socket.ReceiveTimeout = 100;
//                byte[] bytes = new byte[2048];
//                var length = _socket.Receive(bytes, SocketFlags.None);
//            }
//            catch (Exception)
//            {
//                $"清理缓存提示;".LogProcess(logHeader, Log4NetLevelEnum.信息);
//            }
//            finally
//            {
//                if (_socket != null)
//                {
//                    if (receiveTimeout == 0)
//                        receiveTimeout = 2000;
//                    _socket.ReceiveTimeout = receiveTimeout;
//                }
//            }
//        }
//    }

//    #region 重连策略
//    /// <summary>
//    /// 重连策略
//    /// </summary>
//    enum ReconnectStrategyEnum
//    {
//        退出当前方法 = 0,
//        忽略错误继续 = 1,
//        立即重连 = 2,
//    }
//    /// <summary>
//    /// 重连策略
//    /// </summary>
//    /// <param name="sex"></param>
//    /// <param name="count"></param>
//    /// <returns></returns>
//    private ReconnectStrategyEnum ReconnectStrategy(SocketException sex, int count, string logHeader)
//    {
//        if (DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
//            return ReconnectStrategyEnum.退出当前方法;

//        $"第{count}次发生Socket异常：{sex};".LogProcess(logHeader, Log4NetLevelEnum.错误);

//        return sex.SocketErrorCode switch
//        {
//            //  超时 → 不重连 再来过
//            SocketError.TimedOut => ReconnectStrategyEnum.忽略错误继续,
//            // 非致命错误 → 可忽略
//            SocketError.WouldBlock or SocketError.IOPending => ReconnectStrategyEnum.忽略错误继续,
//            // ❌ 致命错误 → 重连
//            SocketError.ConnectionReset
//            or SocketError.ConnectionAborted
//            or SocketError.NotConnected
//            or SocketError.NetworkDown
//            or SocketError.NetworkUnreachable
//            or SocketError.HostUnreachable
//              => ReconnectStrategyEnum.立即重连,
//            _ => ReconnectStrategyEnum.立即重连, // 未知错误
//        };
//    }
//    #endregion
//}
