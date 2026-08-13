using Kinlo.Equipment.Devices.General;

namespace Kinlo.Equipment.Helpers;

internal static class CreateDeviceHelper
{
   public static IConnect? GetIConnect(this IDevice device)
   {
      switch (device.DeviceInfo)
      {
         case var d when d.ConnectType is ConnectTypeEnum.TCP:
            return new TcpConnection(device.DeviceInfo);
         case var d when d.ConnectType is ConnectTypeEnum.UDP:
            return new UDPConnection(device.DeviceInfo);
         case var d when d.ConnectType is ConnectTypeEnum.SerialPort:
            return new SerialPortConnection(device.DeviceInfo);
         default:
            return null;
      }
   }

   public static IConnect? GetIConnect(this DeviceInfoModel Info)
   {
      switch (Info)
      {
         case var d when d.ConnectType is ConnectTypeEnum.TCP:
            return new TcpConnection(Info);
         case var d when d.ConnectType is ConnectTypeEnum.UDP:
            return new UDPConnection(Info);
         case var d when d.ConnectType is ConnectTypeEnum.SerialPort:
            return new SerialPortConnection(Info);
         default:
            return null;
      }
   }

   /// <summary>
   /// 通用重连方法
   /// </summary>
   /// <param name="device"></param>
   public static bool Reconnect(this IDevice device, string logHeader, int reconnectCount = 3)
   {
      $"开始重连".LogProcess(logHeader);
      for (int i = 0; i < reconnectCount; i++)
      {
         try
         {
            try
            {
               device.Close();
            }
            catch (Exception) { }
            Thread.Sleep(2000);
            #region 针对注液泵，弃用
            //if (com.Types == DeviceTypes.注_CP1WCIF41 || com.Types == DeviceTypes.补_CP1WCIF41)
            //{
            //    $"{ipcom} 注液泵通信断开，重启网络模块......".RunLog(MessageLevelType.警告);
            //    string[] ip = ipcom.Split('.');
            //    if (ip[2].Restart_CP1WCIF41_Module(ip[3]))
            //    {
            //        Thread.Sleep(10000);//延时10S等待重启完成
            //        $"重启完成，重新发送指令".RunLog(MessageLevelType.警告);
            //    }
            //    else
            //    {
            //        $"重启注液泵执行失败".RunLog(MessageLevelType.错误);
            //    }
            //}
            #endregion
            if (device.Open())
            {
               $"重连成功！".LogProcess(logHeader, Log4NetLevelEnum.成功);
               return true;
            }
            $"重连失败...".LogProcess(logHeader, Log4NetLevelEnum.错误);
            Thread.Sleep(500);
         }
         catch (Exception ex)
         {
            $"重连异常：\r\n{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
            Thread.Sleep(500);
         }
      }
      return false;
   }

   /// <summary>
   /// 重连cip
   /// </summary>
   /// <param name="plcConn"></param>
   /// <param name="device"></param>
   /// <param name="logHeader"></param>
   /// <param name="reconnectCount"></param>
   /// <returns></returns>
   public static bool ReconnectCip(IDevice device, string logHeader, int reconnectCount, ref CipClient plcConn)
   {
      var mode = plcConn.ConnectMode;
      var index = plcConn.Index;
      $"开始CIP重连".LogProcess(logHeader);
      for (int i = 0; i < reconnectCount; i++)
      {
         plcConn.Close(logHeader);
         var newPlcConn = device.BuildCip(mode, index, logHeader);
         if (newPlcConn != null) //重连成功
         {
            plcConn = newPlcConn;
            $"CIP重连成功".LogProcess(logHeader);
            return true;
         }

         $"注意：CIP重连失败！".LogProcess(logHeader);
         Thread.Sleep(500);
      }
      return false;
   }

   /// <summary>
   /// 创建cip
   /// </summary>
   /// <param name="device"></param>
   /// <param name="clinetType"></param>
   /// <param name="index"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static CipClient? BuildCip(this IDevice device, CipMode clinetType, int index, string logHeader)
   {
      try
      {
         var tcpConnect = device.GetIConnect();
         if (tcpConnect == null || !tcpConnect.Open())
         {
            return null;
         }

         var sessionHandle = CipConnectionManager.Register(tcpConnect, logHeader);
         if (sessionHandle == null)
            return null;

         var plcConnect = new CipClient(tcpConnect, index, sessionHandle, device.DeviceInfo.TaskToken);

         plcConnect.ConnectMode = clinetType;
         if (clinetType == CipMode.无连接模式)
         {
            plcConnect.Protocol = new OmronCipUcmmTcpProtocol(0, sessionHandle);
         }
         else
         {
            var result = CipConnectionManager.ExplicitClass3ForwardOpen(plcConnect.Conn, plcConnect.Session, logHeader);
            plcConnect.ConnectionId = result.connectionId;
            plcConnect.SetForwardContext(result.context);
            plcConnect.Protocol = new OmronCipExplicitTcpProtocol(plcConnect.Session, plcConnect.ConnectionId);
         }

         return plcConnect;
      }
      catch (Exception ex)
      {
         $"创建CIP连接异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
         return null;
      }
   }

   /// <summary>
   /// cip修复
   /// </summary>
   /// <param name="client"></param>
   /// <returns></returns>
   public static bool RepairCip(this CipClient client, int count = 3)
   {
      string logHeader = string.Empty;
      $"开始CIP修复".LogProcess(logHeader);
      if (client.Conn == null)
      {
         $"Cip修复时Conn连接为空，无法修复".LogRun();
         return false;
      }
      logHeader = client.Conn.DeviceInfo.ToDeviceLogHeader();
      for (int i = 0; i < count; i++)
      {
         try
         {
            client.Close(logHeader);

            var tcpConnect = client.Conn.DeviceInfo.GetIConnect();
            if (tcpConnect == null || !tcpConnect.Open())
            {
               $"Cip第{i + 1}次修复时建立连接失败".LogProcess(logHeader);
               continue;
            }

            var sessionHandle = CipConnectionManager.Register(tcpConnect, logHeader);
            if (sessionHandle == null)
            {
               $"Cip第{i + 1}次修复时注册Cip失败".LogProcess(logHeader);
               continue;
            }

            IProtocolHelper protocol = null!;
            if (client.ConnectMode == CipMode.无连接模式)
            {
               protocol = new OmronCipUcmmTcpProtocol(0, sessionHandle);
               client.Repair(tcpConnect, sessionHandle, protocol, null, null);
            }
            else
            {
               var result = CipConnectionManager.ExplicitClass3ForwardOpen(tcpConnect, sessionHandle, logHeader);

               protocol = new OmronCipExplicitTcpProtocol(sessionHandle, result.connectionId);
               client.Repair(tcpConnect, sessionHandle, protocol, result.connectionId, result.context);
            }
            $"CIP修复成功".LogProcess(logHeader);
            return true;
         }
         catch (Exception ex)
         {
            $"Cip修复异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
         }
         finally
         {
            Thread.Sleep(300);
         }
      }
      return false;
   }

   /// <summary>
   /// 关闭CIP
   /// </summary>
   /// <param name="plcConn"></param>
   /// <param name="logHeader"></param>
   public static void Close(this CipClient? plcConn, string logHeader)
   {
      try
      {
         if (plcConn == null || plcConn.Conn == null)
            return;
         if (plcConn.ConnectMode == CipMode.无连接模式)
         {
            CipConnectionManager.CloseCipConnect(plcConn.Conn, plcConn.Session, logHeader);
            plcConn.Conn.Close();
         }
         else
         {
            CipConnectionManager.ExplicitClass3ForwardClose(plcConn.Conn, plcConn.Session, plcConn.ForwardContext, logHeader);
            CipConnectionManager.CloseCipConnect(plcConn.Conn, plcConn.Session, logHeader);
            plcConn.Conn.Close();
         }
      }
      catch (Exception ex)
      {
         string msg = plcConn != null ? $"CIP编号：{plcConn.Index},连接模式：{plcConn.ConnectMode} " : "";
         $"关闭{msg}异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }
   }

   /// <summary>
   /// fins重连
   /// </summary>
   /// <param name="device"></param>
   /// <param name="handshake"></param>
   /// <param name="reconnectCount"></param>
   /// <returns></returns>
   public static bool ReconnectFins(this FinsTcp device, List<byte> handshake, int reconnectCount = 3)
   {
      string logHeader = device.DeviceInfo.ToDeviceLogHeader();
      $"开始CIP重连".LogProcess(logHeader);
      for (int i = 0; i < reconnectCount; i++)
      {
         try
         {
            device.Close();
            if (device.BuildFins(handshake)) //重连成功
            {
               $"CIP重连成功".LogProcess(logHeader);
               return true;
            }

            $"注意：CIP重连失败！".LogProcess(logHeader);
            Thread.Sleep(500);
         }
         catch (Exception ex)
         {
            $"重连异常：\r\n{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
            Thread.Sleep(500);
         }
      }
      return false;
   }

   public static bool BuildFins(this FinsTcp device, List<byte> handshake)
   {
      string logHeader = device.DeviceInfo.ToDeviceLogHeader();
      try
      {
         if (device.Open())
         {
            device.Connect!.Write(handshake.ToArray(), logHeader);
            Thread.Sleep(5);
            var res = device.Connect.Read(24, logHeader);
            if (res.State != CommState.Success)
               return false;
            var bytes = res.Data!;
            int error = FinsTcp.ByteToInt(bytes.Skip(12).Take(4).ToArray());
            if (error == 0)
            {
               device.ProtocolHelper = new FinsTcpProtocol(
                  device.DeviceInfo.BindIP.GetAddressBytes()[3],
                  (byte)FinsTcp.ByteToInt(bytes.Skip(20).Take(4).ToArray())
               );
               return true;
            }
         }
      }
      catch (Exception ex)
      {
         $"[TCPFins]Open异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }

      return false;
   }
}
