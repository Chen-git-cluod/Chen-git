using Accessibility;
using System.Reflection.PortableExecutable;

namespace Kinlo.Equipment.Connects;

internal class SerialPortConnection : IConnect
{
   public DeviceInfoModel DeviceInfo { get; set; }
   private readonly object _lock = new();
   protected SerialPort? _serialPort;

   public SerialPortConnection(DeviceInfoModel info)
   {
      DeviceInfo = info;
   }

   public void Close()
   {
      if (_serialPort != null)
      {
         try
         {
            _serialPort.Close();
            _serialPort.Dispose();
            _serialPort = null;
         }
         catch (Exception ex)
         {
            $"关闭异常：{ex}".LogProcess(DeviceInfo.ToDeviceLogHeader(), Log4NetLevelEnum.错误);
         }
            finally { _serialPort.Dispose();
                _serialPort = null;
            }
      }
   }

   public bool Open()
   {
      try
      {
         _serialPort = new SerialPort(DeviceInfo.IPCOM, DeviceInfo.Port);
         _serialPort.DataBits = DeviceInfo.DataBits;
         _serialPort.StopBits = DeviceInfo.StopBits;
         _serialPort.Parity = DeviceInfo.Parity; //检验位
         _serialPort.ReadBufferSize = 1024;
         _serialPort.WriteBufferSize = 1024;
         //_serialPort.Handshake = Handshake.None;
         _serialPort.ReadTimeout = _serialPort.WriteTimeout = DeviceInfo.Timeout;
         if (!_serialPort.IsOpen)
         {
            _serialPort.Open();
         }
         return _serialPort.IsOpen;
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
         Stopwatch _stopwatch = Stopwatch.StartNew();
         try
         {
            if (CheckExit<byte[]>(logHeader, out var res))
               return res!;

            _stopwatch.Restart();
            byte[] data = new byte[1024];
            int offset = 0;
            while (offset < length)
            {
               if (_stopwatch.ElapsedMilliseconds >= _serialPort.ReadTimeout + 500)
               {
                  $"等待数据返回超时;".LogProcess(logHeader, Log4NetLevelEnum.错误);
                  return CommResult<byte[]>.Fail(CommState.NeedReconnect, "等待数据返回超时");
               }
               offset += _serialPort.Read(data, offset, length - offset);
            }

            return CommResult<byte[]>.Ok(data.Take(length).ToArray());
         }
         catch (Exception ex)
         {
            if (CheckExit<byte[]>(logHeader, out var res))
               return res!;

            $"[SerialPort.Read] 异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return CommResult<byte[]>.Fail(CommState.NeedReconnect, $"{ex}");
         }
         finally
         {
            _stopwatch.Stop();
         }
      }
   }

   public CommResult<byte[]> TryRead(int length, string logHeader)
   {
      lock (_lock)
      {
         try
         {
            byte[] data = new byte[1024];
            _serialPort.Read(data, 0, length);
            return CommResult<byte[]>.Ok(data.Take(length).ToArray());
         }
         catch (TimeoutException tex)
         {
            //由于是try，固不打印日志，由业务端决定
            return CommResult<byte[]>.Fail(CommState.Failed, $"{tex}", tex);
         }
         catch (Exception ex)
         {
            //由于是try，固不打印日志，由业务端决定
            // $"[SerialPort.Read] 异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return CommResult<byte[]>.Fail(CommState.NeedReconnect, $"{ex}", ex);
         }
      }
   }

   public CommResult<byte[]> WriteAndRead(byte[] sendBytes, IProtocolHelper? protocolHelper, string logHeader, int readLength = 1024)
   {
      lock (_lock)
      {
         Stopwatch _stopwatch = new Stopwatch();
         try
         {
            if (CheckExit<byte[]>(logHeader, out var res))
               return res!;
            var _sendBytes = protocolHelper == null ? sendBytes : protocolHelper.Serialize(sendBytes);
            _serialPort.Write(_sendBytes, 0, _sendBytes.Length);

            Thread.Sleep(10);
            List<byte> list = new List<byte>();
            byte[] buffer = new byte[readLength];
            int length = _serialPort.Read(buffer, 0, readLength);
            list.AddRange(buffer.Take(length));
            while (protocolHelper != null && !protocolHelper.Verify(list))
            {
               if (_stopwatch.ElapsedMilliseconds > 5000)
                  return default;
               Thread.Sleep(5);
               length = _serialPort.Read(buffer, 0, readLength);
               list.AddRange(buffer.Take(length));
            }
            if (protocolHelper != null)
               return CommResult<byte[]>.Ok(protocolHelper.Deserialize(list));
            return CommResult<byte[]>.Ok(list.ToArray());
         }
         catch (Exception ex)
         {
            if (CheckExit<byte[]>(logHeader, out var res))
               return res!;
            $"[SerialPort.Read] 异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return CommResult<byte[]>.Fail(CommState.NeedReconnect, $"{ex}");
         }
         finally
         {
            _stopwatch.Stop();
         }
      }
   }

   public CommResult Write(byte[] buffer, string logHeader)
   {
      lock (_lock)
      {
         try
         {
            if (CheckExit(logHeader, out var res))
               return res!;

            _serialPort.Write(buffer, 0, buffer.Length);
            return CommResult.Ok();
         }
         catch (Exception ex)
         {
            if (CheckExit(logHeader, out var res))
               return res!;
            $"[SerialPort.Read] 异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return CommResult.Fail(CommState.NeedReconnect, $"{ex}");
         }
      }
   }

   public void SetTimeOut(int timeout)
   {
      lock (_lock)
      {
         if (_serialPort != null)
         {
            _serialPort.ReadTimeout = timeout;
            _serialPort.WriteTimeout = timeout;
         }
      }
   }

   public void ClearCache(string logHeader)
   {
      lock (_lock)
      {
         if (_serialPort != null && _serialPort.IsOpen)
         {
            logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, null);

            try
            {
               _serialPort.DiscardInBuffer();
            }
            catch (Exception)
            {
               $"清理缓存提示！".LogProcess(logHeader, Log4NetLevelEnum.信息);
            }
         }
      }
   }

   private bool CheckExit<T>(string logHeader, out CommResult<T>? result)
   {
      if (DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
      {
         "设备已关闭".LogProcess(logHeader);
         result = CommResult<T>.Fail(CommState.Failed, "设备已关闭");
         return true;
      }

      if (_serialPort == null)
      {
         "SerialPort".LogProcess(logHeader);
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
         "设备已关闭".LogProcess(logHeader);
         result = CommResult.Fail(CommState.Failed, "设备已关闭");
         return true;
      }
      if (_serialPort == null)
      {
         "SerialPort".LogProcess(logHeader);
         result = CommResult.Fail(CommState.NeedReconnect, "设备已关闭");
         return true;
      }
      result = null;
      return false;
   }
}
