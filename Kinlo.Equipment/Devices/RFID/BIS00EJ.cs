using System.Formats.Asn1;
using System.Windows.Interop;

namespace Kinlo.Equipment.Devices.RFID;

/// <summary>
/// RFID扫码
/// </summary>
[DeviceConnec([CommunicationEnum.RFID_BIS00EJ])]
public class BIS00EJ : DeviceBase
{
   //以下报文长度都是6个字节，即6个ascii
   private readonly byte[] _writeCmd = new byte[] { 0x50, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x36, 0x31, 0x31, 0x0D };
   private readonly byte[] _readCmd = new byte[] { 0x4C, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x36, 0x31, 0x31, 0x0D };
   private readonly byte[] _writeEnterCmd = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0D };
   private readonly byte[] _readEnterCmd = new byte[] { 0x02, 0x0D };

   private readonly Dictionary<string, string> _errDic = new Dictionary<string, string>
   {
      { "1", "不存在数据载体" },
      { "2", "读取时出错" },
      { "4", "写入时出错" },
      { "6", "接口上出现故障" },
      { "7", "电报格式错误" },
   };

   public BIS00EJ(DeviceInfoModel info)
      : base(info) { }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class
   {
      throw new NotImplementedException();
   }

   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TValue : default
   {
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions() { RetryCount = 3 };

      try
      {
         //读取第一次
         var readRecord = new DeviceSplitCommand(
            this,
            _readCmd,
            3,
            options.RetryCount,
            bs => Check(bs, 3, true, logHeader),
            DeviceInfo.TaskToken,
            logHeader,
            100
         );
         var readResult = readRecord.ExecuteSplitRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TValue>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);
         //读取第二次
         readRecord = new DeviceSplitCommand(
            this,
            _readEnterCmd,
            7,
            options.RetryCount,
            bs => Check(bs, 7, false, logHeader),
            DeviceInfo.TaskToken,
            logHeader,
            100
         );
         readResult = readRecord.ExecuteSplitRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TValue>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);

         var bytes = readResult.Value;
         var barcode = Encoding.ASCII.GetString(bytes).Replace("\r", "");

         if (barcode is TValue tv)
            return DeviceResult<TValue>.Success(tv);
         else
         {
            var msg = $"[{Helper.GetCurrentMethodName()}]传入数据类型和实际数据类型不对应;";
            msg.LogProcess(logHeader, Log4NetLevelEnum.错误);
            return DeviceResult<TValue>.Failure(DeviceStatus.结果和期望数据类型不同, msg);
         }
      }
      catch (Exception ex)
      {
         var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
         errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return DeviceResult<TValue>.Failure(DeviceStatus.异常, errMsg);
      }
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

   public override bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   )
   {
      string errMsg = string.Empty;
      options ??= new DeviceOperationOptions();
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      for (int r = 0; r < options.RetryCount; r++)
      {
         CommState commState = CommState.Failed;
         try
         {
            string sendCode = (string)value;
            encoding ??= Encoding.ASCII;
            byte[] sendBytes = encoding.GetBytes(sendCode);
            if (sendBytes.Length != _writeEnterCmd.Length - 2)
            {
               $"[{DeviceInfo.Communication}] 写入长度不匹配!".LogProcess(logHeader, Log4NetLevelEnum.错误);
               return false;
            }
            Connect.ClearCache(logHeader);
            var writeRes = Connect.Write(_writeCmd, logHeader);
            commState = writeRes.State;
            errMsg = writeRes.Message;
            if (commState != CommState.Success)
               continue;

            Thread.Sleep(100);
            var readRes = Connect.Read(3, logHeader);
            commState = readRes.State;
            errMsg = readRes.Message;
            if (commState != CommState.Success)
               continue;

            if (!Check(readRes.Data!, 3, true, logHeader))
            {
               commState = CommState.Failed;
               continue;
            }

            for (int i = 0; i < sendBytes.Length; i++)
            {
               _writeEnterCmd[i + 1] = sendBytes[i];
            }

            writeRes = Connect.Write(_writeEnterCmd, logHeader);
            commState = writeRes.State;
            errMsg = writeRes.Message;
            if (commState != CommState.Success)
               continue;

            Thread.Sleep(100);
            readRes = Connect.Read(3, logHeader);
            commState = readRes.State;
            errMsg = readRes.Message;
            if (commState != CommState.Success)
               continue;

            if (Check(readRes.Data!, 3, true, logHeader))
               return true;
         }
         catch (Exception ex)
         {
            errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         }
         if (commState == CommState.Failed)
         {
            Thread.Sleep(300);
            continue;
         }
         else if (commState == CommState.NeedReconnect)
         {
            if (!this.Reconnect(logHeader))
               return false;
            continue;
         }
      }
      return false;
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="bytes"></param>
   /// <param name="lenght"></param>
   /// <param name="isFist">第一次命令检查第一位，第二次检查最后一位</param>
   /// <returns></returns>
   private bool Check(byte[] bytes, int lenght, bool isFist, string logHeader)
   {
      if (bytes == null)
      {
         $"字节不能为空！".LogProcess(logHeader, Log4NetLevelEnum.错误);
         return false;
      }

      if (bytes.Length < 3 || bytes.Length < lenght)
      {
         $"字节长度过短！".LogProcess(logHeader, Log4NetLevelEnum.错误);
         return false;
      }

      if (isFist)
      {
         if (bytes[0] == 6)
         {
            return true;
         }
         else
         {
            var errCode = Encoding.ASCII.GetString([bytes[1]]);

            if (!_errDic.TryGetValue(errCode, out string? errMsg))
               errMsg = "出现错误！";

            $"{errMsg}！".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
            return false;
         }
      }
      else
      {
         if (bytes[lenght - 1] == 13)
         {
            return true;
         }
         else
         {
            $"数据出现错误！".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return false;
         }
      }
   }
}
