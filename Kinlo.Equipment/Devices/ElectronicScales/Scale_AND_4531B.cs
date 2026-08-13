namespace Kinlo.Equipment.Devices.ElectronicScales;

/// <summary>
/// AND电子称
/// </summary>
[DeviceConnec([CommunicationEnum.Scale_AND_4531B])]
public class Scale_AND_4531B : DeviceBase
{
   private byte[] _zeroCommand = Encoding.ASCII.GetBytes("Z\r\n");
   private int _frameLength = 15;

   public Scale_AND_4531B(DeviceInfoModel info)
      : base(info) { }

   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      var result = DeviceResult<TValue>.Failure(DeviceStatus.Failure);
      options ??= new DeviceOperationOptions() { RetryCount = 3 };

      for (int k = 0; k < options.RetryCount; k++)
      {
         try
         {
            List<DeviceResult<double>> weighSamples = new(2);
            for (int i = 0; i < 2; i++) //每组取n次，n次须全部稳定
            {
               Connect.Close();
               Connect.Open();
               var res = Connect.Read(512, logHeader);
               Connect.Close();
               var stableState = ParseWeight(res.Data!, logHeader);
               weighSamples.Add(stableState);
               Thread.Sleep(100);
            }
            if (weighSamples.All(x => x.Status == DeviceStatus.Success))
            {
               var max = weighSamples.Max(x => x.Value);
               var min = weighSamples.Min(x => x.Value);
               if (max - min <= 0.2) //接受误差0.2g
               {
                  $"通过重量比对，取得正确重量：[{weighSamples[^1].Value}]".LogProcess(logHeader);
                  if (weighSamples[^1].Value is TValue value)
                  {
                     return DeviceResult<TValue>.Success(value);
                  }
                  else
                  {
                     var msg = $"[{DeviceInfo.ProcessesType}]传入数据类型非double！";
                     msg.LogProcess(logHeader, Log4NetLevelEnum.错误);
                     return DeviceResult<TValue>.Failure(DeviceStatus.结果和期望数据类型不同, msg);
                  }
               }
               else
               {
                  var msg = $"未通过重量比对：{string.Join(',', weighSamples.Select(x => x.Value))}";
                  msg.LogProcess(logHeader);
                  result = DeviceResult<TValue>.Failure(DeviceStatus.称重不稳定, msg);
                  weighSamples.RemoveAt(0);
               }
            }
            else
            {
               var r = weighSamples.Last(x => x.Status != DeviceStatus.Success);
               result = DeviceResult<TValue>.Failure(r.Status, r.ErrorMessage, r.Exception);
            }
            Thread.Sleep(100);
         }
         catch (Exception ex)
         {
            var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
            result = DeviceResult<TValue>.Failure(DeviceStatus.异常, errMsg, ex);
         }
      }
      return result;
   }

   protected DeviceResult<double> ParseWeight(byte[] frame, string logHeader)
   {
      if (frame == null || frame.Length < _frameLength)
      {
         var msg = $"返回字节为空！";
         msg.LogProcess(logHeader);
         return DeviceResult<double>.Failure(DeviceStatus.取值失败, msg);
      }

      var strWeights = Encoding.ASCII.GetString(frame);
      var weightArray = strWeights
         .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
         .Where(x => x.Length == 13)
         .ToArray();
      if (!weightArray.Any())
      {
         var msg = $"[{strWeights}] 未取到正确字节";
         msg.LogProcess(logHeader);
         return DeviceResult<double>.Failure(DeviceStatus.取值失败, msg);
      }
      var str = weightArray.LastOrDefault(x => x.Substring(0, 2).ToUpper() == "WT");
      if (str == null)
      {
         var msg = $"[{strWeights}] 未取到稳定值";
         msg.LogProcess(logHeader);
         return DeviceResult<double>.Failure(DeviceStatus.称重不稳定, msg);
      }

      string weighStr = str.Substring(3, 8).Replace(" ", "");
      if (double.TryParse(weighStr, out var weight))
      {
         $"取到稳定值[{weight}]！".LogProcess(logHeader);
         return DeviceResult<double>.Success(weight);
      }
      else
      {
         var msg = $"称稳定，但值[{weighStr}]未能正常转换!";
         msg.LogProcess(logHeader);
         return DeviceResult<double>.Failure(DeviceStatus.取值失败, msg);
      }
   }

   /// <summary>
   /// 清零
   /// </summary>
   /// <param name="value"></param>
   /// <param name="address"></param>
   /// <param name="logHeader"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public override bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   )
   {
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions() { RetryCount = 3 };
      for (int i = 0; i < options.RetryCount; i++)
      {
         try
         {
            var res = Connect.Write(_zeroCommand, logHeader);
            if (res.State == CommState.Failed)
            {
               Thread.Sleep(300);
               continue;
            }
            else if (res.State == CommState.NeedReconnect)
            {
               this.Reconnect(logHeader);
               continue;
            }
            return true;
         }
         catch (Exception ex)
         {
            if (DeviceInfo.TaskToken == null || DeviceInfo.TaskToken.Token.IsCancellationRequested)
               break;
            $"[清零]异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
            Thread.Sleep(300);
         }
      }
      return false;
   }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
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
}
