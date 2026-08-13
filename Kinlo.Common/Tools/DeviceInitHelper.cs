namespace Kinlo.Common.Tools;

public static class DeviceInitHelper
{
   /// <summary>
   /// 本地IP集合
   /// </summary>
   static ObservableCollection<UnicastIPAddressInformation> LocalIPs { get; set; } =
      new ObservableCollection<UnicastIPAddressInformation>();

   /// <summary>
   /// 初始化全部设备
   /// </summary>
   /// <returns></returns>
   public static async Task<bool> InitDeviceLink(this DevicesConfig devicesConfig, CancellationTokenSource cancellationToken)
   {
      devicesConfig.ClearRunDevice();
      LocalIPs.Clear();
      LocalIPs = NetworkInterfaceHelper.GetActiveInterfaceIPs();

      return await Task.Run(() =>
         Parallel
            .ForEach(
               devicesConfig.DeviceList,
               (client, _parallelLoopState) =>
               {
                  if (!client.IsEnable || client.IsCustomBoot || client.ConnectType == ConnectTypeEnum.None)
                     return;

                  if (client.TryCreateDevice(cancellationToken, out IDevice? divace))
                     devicesConfig.AddRunDevice(divace);
                  else
                     _parallelLoopState.Stop();
               }
            )
            .IsCompleted
      );
   }

   /// <summary>
   /// 使用创建的设备执行操作
   /// </summary>
   /// <param name="deviceClient"></param>
   /// <param name="action"></param>
   /// <returns></returns>
   public static async Task WithCreatedDeviceAsync(this DeviceClientModel deviceClient, Func<IDevice, Task> action)
   {
      try
      {
         CancellationTokenSource cancellationToken = new CancellationTokenSource();
         if (deviceClient.TryCreateDeviceWithUpdatedIps(cancellationToken, out IDevice device))
         {
            await action(device);
            device.Close();
         }
         cancellationToken.Cancel();
         cancellationToken.Dispose();
      }
      catch (Exception ex)
      {
         $"初始化设备异常:{ex}".LogRun(Log4NetLevelEnum.错误);
      }
   }

   /// <summary>
   /// 使用创建的设备执行操作并返回结果
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="deviceClient"></param>
   /// <param name="action"></param>
   /// <returns></returns>
   public static async Task<T?> WithCreatedDeviceAsync<T>(this DeviceClientModel deviceClient, Func<IDevice, Task<T>> action)
   {
      T? result = default(T);
      try
      {
         CancellationTokenSource cancellationToken = new CancellationTokenSource();
         if (deviceClient.TryCreateDeviceWithUpdatedIps(cancellationToken, out IDevice device))
         {
            result = await action(device);
            device.Close();
         }
         cancellationToken.Cancel();
         cancellationToken.Dispose();
      }
      catch (Exception ex)
      {
         $"初始化设备异常:{ex}".LogRun(Log4NetLevelEnum.错误);
      }
      return result;
   }

   /// <summary>
   /// 更新IP后创建设备
   /// </summary>
   /// <param name="deviceClient"></param>
   /// <param name="cancellationToken"></param>
   /// <returns></returns>
   public static bool TryCreateDeviceWithUpdatedIps(
      this DeviceClientModel deviceClient,
      CancellationTokenSource cancellationToken,
      out IDevice device
   )
   {
      LocalIPs.Clear();
      LocalIPs = NetworkInterfaceHelper.GetActiveInterfaceIPs();
      return deviceClient.TryCreateDevice(cancellationToken, out device);
   }

   /// <summary>
   /// 创建设备
   /// </summary>
   /// <param name="deviceClient"></param>
   /// <param name="cancellationToken"></param>
   /// <returns></returns>
   public static bool TryCreateDevice(this DeviceClientModel deviceClient, CancellationTokenSource cancellationToken, out IDevice device)
   {
      device = null;
      try
      {
         if (deviceClient.IsOnline != 1 && (deviceClient.ConnectType is ConnectTypeEnum.TCP or ConnectTypeEnum.UDP))
         {
            $"设备：[{deviceClient.ProcessesType}] 序号：[{deviceClient.Index}] IP：[{deviceClient.IPCOM}] 端口号：[{deviceClient.Port}] 网络无法连接！".LogRun(
               Log4NetLevelEnum.错误
            );
            return false;
         }

         #region 绑定本地IP
         IPAddress? bindIp = null;
         if (deviceClient.ConnectType != ConnectTypeEnum.SerialPort)
         {
            bindIp = GetSameSubnetIp(deviceClient);
            if (bindIp == null)
               return false;
         }
         #endregion

         IDevice? runDevice = deviceClient.ToDeviceInfo(bindIp, cancellationToken).CreateDevice();

         if (runDevice == null)
         {
            $"[初始化设备]：[{deviceClient.IPCOM}]创建设备失败！".LogRun(Log4NetLevelEnum.警告);
            return false;
         }

         bool? _openResult = runDevice.Open();

         if (_openResult != null && _openResult == true)
         {
            device = runDevice;
            return true;
         }
         else
         {
            $"设备：[{deviceClient.ProcessesType}] 序号：[{deviceClient.Index}] IP：[{deviceClient.IPCOM}] 端口号：[{deviceClient.Port}] 连接失败".LogRun(
               Log4NetLevelEnum.错误
            );
            return false;
         }
      }
      catch (Exception ex)
      {
         $"设备：[{deviceClient.ProcessesType}] 序号：[{deviceClient.Index}] IP：[{deviceClient.IPCOM}] 端口号：[{deviceClient.Port}] 出现异常：{ex}".LogRun(
            Log4NetLevelEnum.错误
         );
      }
      return false;
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="deviceClient"></param>
   /// <param name="bindIP"></param>
   /// <param name="cancellationToken"></param>
   /// <returns></returns>
   /// <exception cref="Exception"></exception>
   private static DeviceInfoModel ToDeviceInfo(
      this DeviceClientModel deviceClient,
      IPAddress? bindIP,
      CancellationTokenSource cancellationToken
   )
   {
      try
      {
         return new DeviceInfoModel
         {
            ServiceName = deviceClient.ServiceName,
            Index = deviceClient.Index,
            IPCOM = deviceClient.IPCOM,
            Port = deviceClient.Port,
            BindIP = bindIP,
            Timeout = deviceClient.OutTime,
            TaskToken = cancellationToken,
            ProcessesType = deviceClient.ProcessesType,
            HardwareType = deviceClient.HardwareType,
            Communication = deviceClient.Communication,
            ConnectType = deviceClient.ConnectType,
            //ConnectionNumber = deviceClient.ConnectionNumber,
         };
      }
      catch (Exception ex)
      {
         throw new Exception(ex.Message);
      }
   }

   /// <summary>
   /// 取设备相同本在IP
   /// </summary>
   /// <param name="deviceClient"></param>
   /// <returns></returns>
   private static IPAddress? GetSameSubnetIp(this DeviceClientModel deviceClient)
   {
      if (deviceClient.ConnectType != ConnectTypeEnum.SerialPort)
      {
         if (!IPAddress.TryParse(deviceClient.IPCOM, out var ip))
         {
            $"[初始化设备]：[{deviceClient.IPCOM}]无法转换为IP；".LogRun(Log4NetLevelEnum.警告, true);
            return null;
         }
         else
         {
            var sameSubnetIps = new List<IPAddress>(); //同一子网IP
            foreach (var item in LocalIPs)
            {
               if (NetworkInterfaceHelper.AreInSameSubnet(item, ip))
                  sameSubnetIps.Add(item.Address);
            }
            if (!sameSubnetIps.Any())
            {
               $"[初始化设备]：[{deviceClient.IPCOM}]本地没有相同网段IP，请检查本地网络设置！".LogRun(Log4NetLevelEnum.警告, true);
               return null;
            }
            if (sameSubnetIps.Count == 1)
               return sameSubnetIps[0];

            //默认 .200 的是通信网口
            var localIp = sameSubnetIps.FirstOrDefault(x => x.GetAddressBytes()[3] == 200);
            if (localIp != null)
               return localIp;

            $"[初始化设备]：[{deviceClient.IPCOM}]IP在本地有多个相同网段[{string.Join(',', sameSubnetIps)}],且没有默认200地址，请重新设备本地网络IP，确保相同网段只有一组IP或有默认IP 200；".LogRun(
               Log4NetLevelEnum.警告,
               true
            );
            return null;
         }
      }
      return null;
   }
}
