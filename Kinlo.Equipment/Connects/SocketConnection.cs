using System.Net.Sockets;
using System.Windows.Documents;
using System.Windows.Interop;

namespace Kinlo.Equipment.Connects;

public abstract class SocketConnection : IConnect
{
    public DeviceInfoModel DeviceInfo { get; set; }
    private readonly object _lock = new();
    private Socket? _socket = null;

    public SocketConnection(DeviceInfoModel info)
    {
        DeviceInfo = info;
    }

    public virtual void Close()
    {
        if (_socket == null)
            return;
        try
        {
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch { } //忽略
        finally
        {
            try
            {
                // _socket.Close();
                _socket.Dispose();
                _socket = null;
            }
            catch (Exception ex)
            {
                $"关闭异常：{ex}".LogProcess(DeviceInfo.ToDeviceLogHeader(), Log4NetLevelEnum.错误);
            }
        }
    }

    public virtual bool Open()
    {
        try
        {
            Close();

            _socket = DeviceInfo.ConnectType switch
            {
                ConnectTypeEnum.UDP => new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp),
                _ => new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
            };

            _socket.ReceiveTimeout = DeviceInfo.Timeout;
            _socket.SendTimeout = DeviceInfo.Timeout;
            _socket.ExclusiveAddressUse = false;
            _socket.Bind(new System.Net.IPEndPoint(DeviceInfo.BindIP, 0));
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            _socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(DeviceInfo.IPCOM), DeviceInfo.Port), cts.Token).GetAwaiter().GetResult();

            return true;
        }
        catch (Exception ex)
        {
            $"打开连接异常:{ex}".LogProcess(DeviceInfo.ToDeviceLogHeader(), Log4NetLevelEnum.错误);
        }
        return false;
    }

    public virtual CommResult<byte[]> Read(int length, string logHeader)
    {
        lock (_lock)
        {
            try
            {
                if (_socket == null)
                    return CommResult<byte[]>.Fail(CommState.NeedReconnect, "Socket为空");
                //if (CheckExit<byte[]>(logHeader, out var res))
                //    return res!;
                byte[] bytes = new byte[length];
                length = _socket.Receive(bytes, SocketFlags.None);
                var data = bytes.Take(length).ToArray();

                return CommResult<byte[]>.Ok(data);
            }
            catch (SocketException sex)
            {
                if (CheckExit<byte[]>(logHeader, out var res))
                    return res!;

                $"发生Socket异常：{sex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
                return CommResult<byte[]>.Fail(ReconnectStrategy(sex), sex.ToString());
            }
            catch (Exception ex)
            {
                if (CheckExit<byte[]>(logHeader, out var res))
                    return res!;

                $"第发生异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
                return CommResult<byte[]>.Fail(CommState.Failed, ex.ToString());
            }
        }
    }

    public CommResult<byte[]> TryRead(int length, string logHeader)
    {
        lock (_lock)
        {
            try
            {
                if (_socket == null)
                    return CommResult<byte[]>.Fail(CommState.NeedReconnect, "Socket为空");
                //if (CheckExit<byte[]>(logHeader, out var res))
                //    return res!;

                //if (_socket.Available == 0)
                //    return CommResult<byte[]>.Fail(CommState.Failed, "无数据");

                byte[] bytes = new byte[length];
                length = _socket.Receive(bytes, SocketFlags.None);
                var data = bytes.Take(length).ToArray();
                return CommResult<byte[]>.Ok(data);
            }
            catch (SocketException sex)
            {
                if (CheckExit<byte[]>(logHeader, out var res))
                    return res!;

                $"发生Socket异常：{sex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
                return CommResult<byte[]>.Fail(ReconnectStrategy(sex), sex.ToString());
            }
            catch (Exception ex)
            {
                if (CheckExit<byte[]>(logHeader, out var res))
                    return res!;

                $"第发生异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
                return CommResult<byte[]>.Fail(CommState.NeedReconnect, ex.ToString());
            }
        }
    }

    public virtual void SetTimeOut(int timeout)
    {
        lock (_lock)
        {
            if (_socket == null || DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
                return;

            _socket.SendTimeout = timeout;
            _socket.ReceiveTimeout = timeout;
        }
    }

    public virtual CommResult Write(byte[] buffer, string logHeader)
    {
        lock (_lock)
        {
            //if (CheckExit(logHeader, out var res))
            //    return res!;

            return WriteCore(buffer, logHeader);
        }
    }

    public virtual CommResult WriteCore(byte[] buffer, string logHeader)
    {
        try
        {
            if (_socket == null)
                return CommResult.Fail(CommState.NeedReconnect, "Socket为空");

            int totalSent = 0;
            int dataLength = buffer.Length;
            while (totalSent < dataLength)
            {
                int bytesRemaining = dataLength - totalSent;

                int sentThisTime = _socket.Send(buffer, totalSent, bytesRemaining, SocketFlags.None);

                if (sentThisTime == 0)
                {
                    string msg = "连接被对方关闭";
                    msg.LogProcess(logHeader);
                    return CommResult.Fail(CommState.NeedReconnect, msg);
                }

                totalSent += sentThisTime;
            }
            if (totalSent == dataLength)
                return CommResult.Ok();
            return CommResult.Fail(CommState.Failed, "写入长度和实际长度不符");
        }
        catch (SocketException sex)
        {
            if (CheckExit(logHeader, out var res))
                return res!;

            $"发生Socket异常：{sex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return CommResult.Fail(ReconnectStrategy(sex), sex.ToString());
        }
        catch (Exception ex)
        {
            if (CheckExit(logHeader, out var res))
                return res!;

            $"第发生异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return CommResult.Fail(CommState.Failed, ex.ToString());
        }
    }

    public virtual CommResult<byte[]> WriteAndRead(byte[] sendBytes, IProtocolHelper? protocol, string logHeader, int readLength = 1024)
    {
        try
        {
            lock (_lock)
            {
                if (_socket == null)
                    return CommResult<byte[]>.Fail(CommState.NeedReconnect, "Socket为空");
                //if (CheckExit<byte[]>(logHeader, out var res))
                //    return res!;

                var data = protocol == null ? sendBytes : protocol.Serialize(sendBytes);

                var writeRes = WriteCore(data, logHeader);

                if (writeRes.State != CommState.Success)
                {
                    writeRes.Message!.LogProcess(logHeader, Log4NetLevelEnum.错误);
                    return CommResult<byte[]>.Fail(writeRes.State, writeRes.Message!);
                }

                byte[] buffer = new byte[readLength];
                List<byte> bufferList = new List<byte>();
                Stopwatch stopwatch = Stopwatch.StartNew();

                string errMsg = string.Empty;
                while (true)
                {
                    if (stopwatch.ElapsedMilliseconds > DeviceInfo.Timeout * 2)
                    {
                        errMsg = "协议超时未匹配!";
                        errMsg.LogProcess(logHeader, Log4NetLevelEnum.警告);
                        return CommResult<byte[]>.Fail(CommState.NeedReconnect, errMsg);
                    }

                    const int MaxBufferSize = 1024 * 1024; // 1MB

                    if (bufferList.Count > MaxBufferSize)
                    {
                        errMsg = "数据异常过大（可能协议错位）!";
                        errMsg.LogProcess(logHeader, Log4NetLevelEnum.警告);
                        return CommResult<byte[]>.Fail(CommState.Failed, errMsg);
                    }

                    int len = _socket.Receive(buffer);

                    bufferList.AddRange(buffer.Take(len));

                    //  无协议 ,直接返回
                    if (protocol == null)
                        return CommResult<byte[]>.Ok(bufferList.ToArray());

                    if (protocol.Verify(bufferList))
                        return CommResult<byte[]>.Ok(protocol.Deserialize(bufferList));
                }
                //errMsg = "读取失败!";
                //errMsg.LogProcess(logHeader, Log4NetLevelEnum.警告);
                //return CommResult<byte[]>.Fail(CommState.Failed, errMsg);
            }
        }
        catch (SocketException sex)
        {
            if (CheckExit<byte[]>(logHeader, out var res))
                return res!;

            $"发生Socket异常：{sex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return CommResult<byte[]>.Fail(ReconnectStrategy(sex), sex.ToString());
        }
        catch (Exception ex)
        {
            if (CheckExit<byte[]>(logHeader, out var res))
                return res!;

            $"发生异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return CommResult<byte[]>.Fail(CommState.NeedReconnect, ex.ToString());
        }
    }

    public void ClearCache(string logHeader)
    {
        if (_socket == null || DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
            return;

        lock (_lock)
        {
            try
            {
                byte[] buffer = new byte[4096];
                while (_socket is { Available: > 0 }) //// 只有当缓冲区有数据时才处理
                    _socket.Receive(buffer);

                #region 另一个更粗暴的方法，没有上一个优雅，暂时不用
                //bool originalBlocking = _socket.Blocking;
                //try
                //{
                //   _socket.Blocking = false;
                //   byte[] trashBin = new byte[4096];

                //   while (true)
                //   {
                //      try
                //      {
                //         int len = _socket.Receive(trashBin);
                //         if (len <= 0)
                //            break; // 对端关闭
                //      }
                //      catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
                //      {
                //         // 成功清空缓冲区
                //         break;
                //      }
                //   }
                //}
                //finally
                //{
                //   _socket.Blocking = originalBlocking;
                //}
                #endregion
            }
            catch (Exception)
            {
                logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, null);
                $"清理缓存提示;".LogProcess(logHeader, Log4NetLevelEnum.信息);
            }
        }
    }

    #region 重连策略
    ///// <summary>
    ///// 重连策略
    ///// </summary>
    //enum ReconnectStrategyEnum
    //{
    //    退出当前方法 = 0,
    //    忽略错误继续 = 1,
    //    立即重连 = 2,
    //}

    /// <summary>
    /// 重连策略
    /// </summary>
    /// <param name="sex"></param>
    /// <returns></returns>
    private CommState ReconnectStrategy(SocketException sex) =>
       sex.SocketErrorCode switch
       {
           // 非致命错误 → 可忽略
           SocketError.WouldBlock or SocketError.IOPending => CommState.Failed,
           // ❌ 致命错误 → 重连
           SocketError.ConnectionReset
          or SocketError.TimedOut
          or SocketError.ConnectionAborted
          or SocketError.NotConnected
          or SocketError.NetworkDown
          or SocketError.NetworkUnreachable
          or SocketError.HostUnreachable => CommState.NeedReconnect,
           _ => CommState.NeedReconnect, // 未知错误
       };

    #endregion

    private bool CheckExit<T>(string logHeader, out CommResult<T>? result)
    {
        if (DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
        {
            // "设备已关闭".LogProcess(logHeader);
            result = CommResult<T>.Fail(CommState.Failed, "设备已关闭");
            return true;
        }

        if (_socket == null)
        {
            // "Socket为空".LogProcess(logHeader);
            result = CommResult<T>.Fail(CommState.NeedReconnect, "Socket为空");
            return true;
        }
        result = null;
        return false;
    }

    private bool CheckExit(string logHeader, out CommResult result)
    {
        if (DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
        {
            // "设备已关闭".LogProcess(logHeader);
            result = CommResult.Fail(CommState.Failed, "设备已关闭");
            return true;
        }
        if (_socket == null)
        {
            // "Socket为空".LogProcess(logHeader);
            result = CommResult.Fail(CommState.NeedReconnect, "Socket为空");
            return true;
        }
        result = null;
        return false;
    }
}
