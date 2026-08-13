using System.Net;

namespace Kinlo.Common.Tools;

public static class NetworkInterfaceHelper
{
   /// <summary>
   /// 取本地IP
   /// </summary>
   /// <returns></returns>
   public static ObservableCollection<UnicastIPAddressInformation> GetActiveInterfaceIPs()
   {
      ObservableCollection<UnicastIPAddressInformation> _ips = new ObservableCollection<UnicastIPAddressInformation>();
      try
      {
         foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
         {
            //Console.WriteLine($"网络接口名称: {ni.Name}");
            //Console.WriteLine($"描述: {ni.Description}");
            //Console.WriteLine($"类型: {ni.NetworkInterfaceType}");
            //Console.WriteLine($"状态: {ni.OperationalStatus}");
            if (ni.OperationalStatus != OperationalStatus.Up)
               continue;
            IPInterfaceProperties ipProps = ni.GetIPProperties();
            foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
            {
               if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
               {
                  _ips.Add(ip);
                  //Console.WriteLine($"  IP 地址: {ip.Address}");
                  //Console.WriteLine($"  子网掩码: {ip.IPv4Mask}");
               }
            }
            //  Console.WriteLine();
         }
      }
      catch (Exception ex)
      {
         $"[取本地IP]异常：{ex}".LogRun();
      }

      return _ips;
   }

   /// <summary>
   /// 对比两组IP是否在一个网段
   /// </summary>
   /// <param name="localIPAddressInfo"></param>
   /// <param name="deviceIP"></param>
   /// <returns></returns>
   public static bool AreInSameSubnet(UnicastIPAddressInformation localIPAddressInfo, IPAddress deviceIP)
   {
      // 将 IP 和子网掩码转换为字节数组
      byte[] _deviceIPBytes = deviceIP.GetAddressBytes();
      byte[] _localIPBytes = localIPAddressInfo.Address.GetAddressBytes();
      byte[] _localIPMaskBytes = localIPAddressInfo.IPv4Mask.GetAddressBytes();

      // 比较字节数组长度是否相同（IPv4 为 4 字节）
      if (_deviceIPBytes.Length != _localIPBytes.Length || _deviceIPBytes.Length != _localIPMaskBytes.Length)
      {
         "IP地址和子网掩码必须是相同类型（IPv4）".LogRun(Log4NetLevelEnum.错误);
         return false;
      }

      // 按位与操作，比较网络地址
      for (int i = 0; i < _deviceIPBytes.Length; i++)
      {
         if ((_deviceIPBytes[i] & _localIPMaskBytes[i]) != (_localIPBytes[i] & _localIPMaskBytes[i]))
         {
            return false;
         }
      }
      return true;
   }
}
