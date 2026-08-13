using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;

namespace Kinlo.Equipment.Helpers;

public static class Helper
{
   /// <summary>
   /// 用byte[]分割另一个byte[]
   /// </summary>
   /// <param name="input"></param>
   /// <param name="separator"></param>
   /// <returns></returns>
   public static List<byte[]> SplitByteArray(this byte[] input, byte[] separator)
   {
      List<byte[]> result = new List<byte[]>();
      List<byte> current = new List<byte>();

      for (int i = 0; i < input.Length; i++)
      {
         if (MatchSeparator(input, i, separator))
         {
            if (current.Count > 0)
            {
               result.Add(current.ToArray());
               current.Clear();
            }
            i += separator.Length - 1; // Skip over the separator
         }
         else
         {
            current.Add(input[i]);
         }
      }

      if (current.Count > 0)
      {
         result.Add(current.ToArray());
      }

      return result;
   }

   static bool MatchSeparator(byte[] input, int index, byte[] separator)
   {
      if (index + separator.Length > input.Length)
         return false;

      for (int i = 0; i < separator.Length; i++)
      {
         if (input[index + i] != separator[i])
            return false;
      }
      return true;
   }

   public static string ToDeviceLogHeader(this DeviceInfoModel deviceInfo)
   {
      if (deviceInfo == null)
         return "[未知设备]";
      return $"[{deviceInfo.ServiceName}-{deviceInfo.ProcessesType}-{deviceInfo.Communication}-{deviceInfo.Index}-{deviceInfo.IPCOM}:{deviceInfo.Port}]";
   }

   public static string SplitDeviceLogHeader(this DeviceInfoModel deviceInfo, string logHeader, params SignalAddressModel[]? signalAddress)
   {
      string des = string.Empty;
      if (signalAddress != null)
      {
         List<string> tags = new List<string>();
         foreach (var item in signalAddress)
         {
            if (item != null && !string.IsNullOrEmpty(item.Lable))
            {
               tags.Add(item.Lable);
            }
         }
         des = $" [标签或地址：{string.Join(',', tags)}]";
      }
      return $"{logHeader}{(deviceInfo == null ? "" : $" [{deviceInfo.IPCOM}-{deviceInfo.Port}]")}{des}";
   }

   /// <summary>
   /// 取方法名
   /// </summary>
   /// <param name="memberName"></param>
   /// <returns></returns>
   public static string GetCurrentMethodName([CallerMemberName] string memberName = "") => memberName;

   //public record DeviceCommand(DeviceBase Device, byte[] Request, int Length, int RetryCount, IProtocolHelper? Protocol, Func<byte[], bool>? Func, CancellationTokenSource TokenSource, string LogHeader);
   ///// <summary>
   ///// 请求设备
   ///// </summary>
   ///// <param name="record"></param>
   ///// <returns></returns>
   //public static DeviceResult<byte[]> ExecuteRequest(this DeviceCommand record)
   //{
   //    var result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);
   //    for (int i = 0; i < record.RetryCount; i++)
   //    {
   //        if (record.TokenSource.IsShutdown())
   //            return DeviceResult<byte[]>.Failure(DeviceStatus.设备已停机);

   //        try
   //        {
   //            record.Device.Connect.ClearCache(record.LogHeader);
   //            var res = record.Device.Connect.WriteAndRead(record.Request, record.Protocol, record.LogHeader, record.Length == 0 ? 1024 : record.Length);
   //            if (res.State == CommState.Failed)
   //            {
   //                result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);
   //                Thread.Sleep(300);
   //                continue;
   //            }
   //            else if (res.State == CommState.NeedReconnect)
   //            {
   //                result = DeviceResult<byte[]>.Failure(DeviceStatus.通信失败, res.Message);
   //                if (!record.Device.Reconnect(record.LogHeader))
   //                    return result;
   //                continue;
   //            }
   //            var bytes = res.Data!;
   //            if (record.Func != null && !record.Func(bytes))
   //            {
   //                var errMsg = $"第[{i + 1}]次取数据长度不对bytes[{BitConverter.ToString(bytes)}]";
   //                errMsg.LogProcess(record.LogHeader, Log4NetLevelEnum.错误);
   //                result = DeviceResult<byte[]>.Failure(DeviceStatus.报文校验不通过, errMsg);
   //                continue;
   //            }

   //            return DeviceResult<byte[]>.Success(bytes);
   //        }
   //        catch (Exception ex)
   //        {
   //            var errMsg = $"第[{i + 1}]次[{Helper.GetCurrentMethodName()}]异常：{ex};";
   //            errMsg.LogProcess(record.LogHeader, Log4NetLevelEnum.错误, true);
   //            result = DeviceResult<byte[]>.Failure(DeviceStatus.异常, errMsg);
   //        }
   //    }
   //    return result;
   //}
   //public record DeviceSplitCommand(DeviceBase Device, byte[] Request, int Length, int RetryCount, Func<byte[], bool>? Func, CancellationTokenSource TokenSource, string LogHeader);

   //public static DeviceResult<byte[]> ExecuteSplitRequest(this DeviceSplitCommand record)
   //{
   //    var result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);
   //    for (int i = 0; i < record.RetryCount; i++)
   //    {
   //        if (record.TokenSource.IsShutdown())
   //            return DeviceResult<byte[]>.Failure(DeviceStatus.设备已停机);

   //        try
   //        {
   //            record.Device.Connect.ClearCache(record.LogHeader);
   //            var res = record.Device.Connect.Write(record.Request, record.LogHeader);
   //            if (res.State == CommState.Failed)
   //            {
   //                result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);
   //                Thread.Sleep(300);
   //                continue;
   //            }
   //            else if (res.State == CommState.NeedReconnect)
   //            {
   //                result = DeviceResult<byte[]>.Failure(DeviceStatus.通信失败, res.Message);
   //                if (!record.Device.Reconnect(record.LogHeader))
   //                    return result;
   //                continue;
   //            }

   //            Thread.Sleep(100);
   //            var readRes = record.Device.Connect.Read(record.Length, record.LogHeader);
   //            var bytes = readRes.Data!;
   //            if (record.Func != null && !record.Func(bytes))
   //            {
   //                var errMsg = $"第[{i + 1}]次取数据长度不对bytes[{BitConverter.ToString(bytes)}]";
   //                errMsg.LogProcess(record.LogHeader, Log4NetLevelEnum.错误);
   //                result = DeviceResult<byte[]>.Failure(DeviceStatus.报文校验不通过, errMsg);
   //                continue;
   //            }

   //            return DeviceResult<byte[]>.Success(bytes);
   //        }
   //        catch (Exception ex)
   //        {
   //            var errMsg = $"第[{i + 1}]次[{Helper.GetCurrentMethodName()}]异常：{ex};";
   //            errMsg.LogProcess(record.LogHeader, Log4NetLevelEnum.错误, true);
   //            result = DeviceResult<byte[]>.Failure(DeviceStatus.异常, errMsg);
   //        }
   //    }
   //    return result;
   //}
}
