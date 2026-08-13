namespace Kinlo.Equipment.Devices.CIP;

/// <summary>
/// 轻量级CIP连接，适用于低端PLC
/// </summary>
[DeviceConnec([CommunicationEnum.CipOrmonPlcLight])]
public class OmronCIipLight : CipBase
{
   public OmronCIipLight(DeviceInfoModel info)
      : base(info) { }

   public override bool Open()
   {
      Close();
      string logHeader = DeviceInfo.ToDeviceLogHeader();
      var plcConnect = this.BuildCip(CipMode.无连接模式, 1, logHeader);
      if (plcConnect == null)
      {
         Close();
         return false;
      }

      Unconnected.Add(plcConnect);
      return true;
   }

   public override void Close()
   {
      string logHeader = DeviceInfo.ToDeviceLogHeader();
      try
      {
         while (Unconnected.TryTake(out var plcConn))
         {
            plcConn.Close(logHeader);
         }
      }
      catch (Exception ex)
      {
         $"关闭异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }
   }

   public override DeviceResult<TClass> Scan<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class => base.ReadClass(address, obj, logHeader, options);

   #region Class3-在Light模式下因为只一个连接，所以要转换连接模式
   /// <summary>
   ///  有连接读取（最大支持 1996 byte）
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="address"></param>
   /// <param name="obj"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public override DeviceResult<T> ReadLargeClass<T>(
      SignalAddressModel address,
      T obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions();
      var lableBytes = ParseCipLabel.LabelReadRequest(address.Lable, address.Length);

      var result = DeviceResult<T>.Failure(DeviceStatus.Failure);
      if (!Unconnected.TryTake(out var plc, CipExtensions.ConnectionPoolTakeTimeoutMs))
      {
         result = DeviceResult<T>.Failure(
            $"从连接池获取PLC连接超时（{CipExtensions.ConnectionPoolTakeTimeoutMs}ms），当前连接池数量：{Unconnected.Count}"
         );
         result.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return result;
      }
      //读取
      var readRes = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);
      try
      {
         readRes = ReadLargeClassCore(lableBytes, options.RetryCount, logHeader, DeviceInfo.TaskToken, ref plc);
      }
      catch (Exception ex)
      {
         readRes = DeviceResult<byte[]>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         readRes.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }
      finally
      {
         Unconnected.Add(plc);
      }
      //解析
      try
      {
         if (readRes.Status != DeviceStatus.Success)
            return DeviceResult<T>.Failure(readRes.Status, readRes.ErrorMessage!, readRes.Exception);

         var bytes = readRes.Value!;
         int boolSize = 0;
         StructToBytes.FromBytes(obj, bytes.Skip(4).ToArray(), ref boolSize, 0, DeviceInfo.Communication);
         return DeviceResult<T>.Success(obj);
      }
      catch (Exception ex)
      {
         var res = DeviceResult<T>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         res.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return res;
      }
   }

   /// <summary>
   ///  有连接读取（最大支持 1996 byte）
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="address"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public override DeviceResult<List<T>> ReadLargeObjects<T>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      Type type = typeof(T);
      if (type.Name == "String")
      {
         return DeviceResult<List<T>>.Failure("协议不支持字符串数组!!!");
      }
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions();
      var lableBytes = ParseCipLabel.LabelReadRequest(address.Lable, address.Length);

      var result = DeviceResult<List<T>>.Failure(DeviceStatus.Failure);
      if (!Unconnected.TryTake(out var plc, CipExtensions.ConnectionPoolTakeTimeoutMs))
      {
         result = DeviceResult<List<T>>.Failure(
            $"从连接池获取PLC连接超时（{CipExtensions.ConnectionPoolTakeTimeoutMs}ms），当前连接池数量：{Unconnected.Count}"
         );
         result.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return result;
      }
      //读取
      var readRes = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);
      try
      {
         readRes = ReadLargeClassCore(lableBytes, options.RetryCount, logHeader, DeviceInfo.TaskToken, ref plc);
      }
      catch (Exception ex)
      {
         readRes = DeviceResult<byte[]>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         readRes.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }
      finally
      {
         Unconnected.Add(plc);
      }
      //解析
      try
      {
         if (readRes.Status != DeviceStatus.Success)
            return DeviceResult<List<T>>.Failure(readRes.Status, readRes.ErrorMessage!, readRes.Exception);

         var bytes = readRes.Value!;
         int size = 0;
         List<T> datas = new List<T>();
         int stat = 4;
         if (type.IsValueType)
         {
            while (stat < bytes.Length)
            {
               switch (bytes[0])
               {
                  case 0xC3:
                     datas.Add((T)StructToBytes.GetValue(type, bytes.Skip(stat).ToArray(), 0, DeviceInfo.Communication));
                     stat += 2;
                     break;
               }
               datas.Add((T)StructToBytes.GetValue(type, bytes.Skip(stat).ToArray(), 0, DeviceInfo.Communication));
               switch (bytes[0])
               {
                  case 0xC4:
                  case 0xC8:
                  case 0xCA:
                     stat += 4;
                     break;
                  case 0xC5:
                  case 0xC9:
                  case 0xCB:
                     stat += 8;
                     break;
                  case 0xC7:
                     stat += 2;
                     break;
               }
            }
         }
         else
         {
            try
            {
               while (stat < bytes.Length)
               {
                  if (stat == 504) { }
                  var obj = Activator.CreateInstance<T>();
                  double sizt_count = StructToBytes.FromBytes(obj, bytes.Skip(stat).ToArray(), ref size, 0, DeviceInfo.Communication);
                  datas.Add(obj);
                  stat += (int)sizt_count;
                  //if (!isConnection)
                  //{
                  //    stat += 4;
                  //}
               }
            }
            catch (Exception ex)
            {
               ex.ToString().LogProcess(logHeader, Log4NetLevelEnum.错误);
            }
         }
         return DeviceResult<List<T>>.Success(datas);
      }
      catch (Exception ex)
      {
         var res = DeviceResult<List<T>>.Failure(DeviceStatus.异常, $"[{Helper.GetCurrentMethodName()}]异常：{ex};", ex);
         res.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return res;
      }
   }

   private DeviceResult<byte[]> ReadLargeClassCore(
      byte[] request,
      int retryCount,
      string logHeader,
      CancellationTokenSource tokenSource,
      ref CipClient plc
   )
   {
      var result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);
      for (int i = 0; i < retryCount; i++)
      {
         if (DeviceInfo.TaskToken.IsShutdown())
            return DeviceResult<byte[]>.Failure(DeviceStatus.设备已停机);

         try
         {
            var connectResult = CipConnectionManager.ExplicitClass3ForwardOpen(plc.Conn, plc.Session, logHeader);
            plc.ConnectionId = connectResult.connectionId;
            plc.SetForwardContext(connectResult.context);
            plc.Protocol = new OmronCipExplicitTcpProtocol(plc.Session, plc.ConnectionId);
            plc.ConnectMode = CipMode.有连接模式_每次重连;

            var res = plc.Conn.WriteAndRead(request, plc.Protocol, logHeader, 2048);

            CipConnectionManager.ExplicitClass3ForwardClose(plc.Conn, plc.Session, plc.ForwardContext, logHeader);
            CipConnectionManager.CloseCipConnect(plc.Conn, plc.Session, logHeader);
            plc.Protocol = new OmronCipUcmmTcpProtocol(0, plc.Session);
            plc.ConnectMode = CipMode.无连接模式;

            if (res.State == CommState.Success)
            {
               plc.OnWatchdogFed();
               return DeviceResult<byte[]>.Success(res.Data!);
            }

            result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure, res.Message, res.Exception);
            if (res.State == CommState.NeedReconnect)
            {
               var mode = plc.ConnectMode;
               var index = plc.Index;
               plc.Close(logHeader);
               plc = this.BuildCip(mode, index, logHeader);
               if (plc == null) //重连失败
               {
                  $"注意：重连失败！".LogProcess(logHeader);
                  return DeviceResult<byte[]>.Failure(res.Message);
               }
            }
         }
         catch (Exception ex)
         {
            result = DeviceResult<byte[]>.Failure(DeviceStatus.异常, $"第{i + 1}次 PLC执行操作异常：{ex}", ex);
            result.ErrorMessage!.LogProcess(logHeader, Log4NetLevelEnum.错误);
         }
         Thread.Sleep(CipExtensions.RetryIntervalMs);
      }
      return result;
   }
   #endregion
}
