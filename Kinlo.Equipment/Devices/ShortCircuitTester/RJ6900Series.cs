using static Kinlo.Equipment.Helpers.Helper;

namespace Kinlo.Equipment.Devices.ShortCircuitTester;

/// <summary>
/// 锐捷69系列
/// </summary>
[DeviceConnec([CommunicationEnum.ShortCircuit_RJ6902CAGX, CommunicationEnum.ShortCircuit_RJ6902R, CommunicationEnum.ShortCircuit_RJ6901A])]
public class RJ6900Series : DeviceBase
{
   private byte[] _sendBytes = new byte[0];
   public IProtocolHelper? ProtocolHelper { get; set; }

   public RJ6900Series(DeviceInfoModel info)
      : base(info)
   {
      switch (info.Communication)
      {
         case CommunicationEnum.ShortCircuit_RJ6902R:
            ProtocolHelper = new RJ6900SeriesProtocol(30);
            _sendBytes = new byte[] { 0x7B, 0x00, 0x08, 0x02, 0xF0, 0x51, 0x4B, 0x7D }; //查询测试数据
            break;
         case CommunicationEnum.ShortCircuit_RJ6902CAGX:
            ProtocolHelper = new RJ6900SeriesProtocol(36);
            _sendBytes = new byte[] { 0x7B, 0x00, 0x08, 0x02, 0xF0, 0x5B, 0x55, 0x7D }; //查询测试数据
            break;
         case CommunicationEnum.ShortCircuit_RJ6901A:
         default:
            ProtocolHelper = new RJ6900SeriesProtocol(32);
            _sendBytes = new byte[] { 0x7B, 0x00, 0x08, 0x02, 0xF0, 0x21, 0x1B, 0x7D }; //查询测试数据
            break;
      }
   }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class
   {
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions() { RetryCount = 3 };

      try
      {
         var readRecord = new DeviceCommand(
            this,
            _sendBytes,
            1024,
            options.RetryCount,
            ProtocolHelper,
            null,
            DeviceInfo.TaskToken,
            logHeader
         );
         var readResult = readRecord.ExecuteRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);

         var bytes = readResult.Value!;
         switch (DeviceInfo.Communication)
         {
            case CommunicationEnum.ShortCircuit_RJ6902R:
               return DeviceResult<TClass>.Success(ToResult_RJ6902R(bytes) as TClass);
            case CommunicationEnum.ShortCircuit_RJ6902CAGX:
               return DeviceResult<TClass>.Success(ToResult_RJ6902CAGX(bytes) as TClass);
            case CommunicationEnum.ShortCircuit_RJ6901A:
            default:
               return DeviceResult<TClass>.Success(ToResult_RJ6901A(bytes) as TClass);
         }
      }
      catch (Exception ex)
      {
         var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
         errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return DeviceResult<TClass>.Failure(DeviceStatus.异常, errMsg);
      }
   }

   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TValue : default
   {
      var result = DeviceResult<TValue>.Failure(DeviceStatus.Failure);
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions();
      string errMsg = string.Empty;
      for (int i = 0; i < options.RetryCount; i++)
      {
         if (IsShutdown)
            return DeviceResult<TValue>.Failure(DeviceStatus.设备已停机);

         try
         {
            address.Length = address.Length == 0 ? (ushort)1024 : address.Length;
            var res = Connect.Read(address.Length, logHeader);
            if (res.State == CommState.Failed)
            {
               result = DeviceResult<TValue>.Failure(DeviceStatus.Failure);
               Thread.Sleep(300);
               continue;
            }
            else if (res.State == CommState.NeedReconnect)
            {
               result = DeviceResult<TValue>.Failure(DeviceStatus.通信失败, res.Message);
               if (!this.Reconnect(logHeader))
                  return result;
               continue;
            }
            var bytes = res.Data!;
            return DeviceResult<TValue>.Success((TValue)(object)bytes);
         }
         catch (Exception ex)
         {
            errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
            result = DeviceResult<TValue>.Failure(DeviceStatus.异常, errMsg);
         }
      }
      return result;
   }

   public override bool WriteClass<TClass>(
      TClass value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      throw new NotImplementedException("未实现");
   }

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
      string errMsg = string.Empty;
      byte[] bytes = (byte[])value;
      for (int i = 0; i < options.RetryCount; i++)
      {
         if (IsShutdown)
            return false;
         try
         {
            var res = Connect.Write(bytes, logHeader);
            if (res.State == CommState.Failed)
            {
               Thread.Sleep(300);
               continue;
            }
            else if (res.State == CommState.NeedReconnect)
            {
               if (!this.Reconnect(logHeader))
                  return false;
               continue;
            }
            return true;
         }
         catch (Exception ex)
         {
            errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
            $"[取短路测试数据]WriteSingle方法异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
         }
      }
      return false;
   }

   private int ToInt(byte[] bytes) => (int)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);

   private ushort ToUShort(byte[] bytes) => (ushort)((bytes[0] << 8) | bytes[1]);

   private RJ6902RResultModel ToResult_RJ6902R(byte[] bytes)
   {
      return new RJ6902RResultModel
      {
         跌落1 = ToUShort(bytes.Skip(6).ToArray()),
         跌落2 = ToUShort(bytes.Skip(8).ToArray()),
         VP电压 = ToUShort(bytes.Skip(10).ToArray()),
         升压时间 = ToUShort(bytes.Skip(12).ToArray()),
         电阻测试数据 = ToInt(bytes.Skip(14).ToArray()) / 10.0F,
         开路结果 = bytes[18],
         严重短路结果 = bytes[19],
         欠压结果 = bytes[20],
         过压结果 = bytes[21],
         跌落1结果 = bytes[22],
         跌落2结果 = bytes[23],
         TL结果 = bytes[24],
         TH结果 = bytes[25],
         电阻测试结果 = bytes[26],
         总结果 = bytes[27],
      };
   }

   private RJ6902CAGXResultModel ToResult_RJ6902CAGX(byte[] bytes)
   {
      return new RJ6902CAGXResultModel()
      {
         跌落1 = ToUShort(bytes.Skip(6).ToArray()),
         跌落2 = ToUShort(bytes.Skip(8).ToArray()),
         跌落3 = ToUShort(bytes.Skip(10).ToArray()),
         VP电压 = ToUShort(bytes.Skip(12).ToArray()),
         升压时间 = ToUShort(bytes.Skip(14).ToArray()),
         电阻测试数据 = ToInt(bytes.Skip(16).ToArray()) / 10.0F,
         电容测试数据 = ToUShort(bytes.Skip(20).ToArray()),
         开路结果 = bytes[22],
         放电1结果 = bytes[23],
         VP结果 = bytes[24],
         放电2结果 = bytes[25],
         跌落1结果 = bytes[26],
         跌落2结果 = bytes[27],
         跌落3结果 = bytes[28],
         TL结果 = bytes[29],
         TH结果 = bytes[30],
         电阻测试结果 = bytes[31],
         电容测试结果 = bytes[32],
         总结果 = bytes[33],
         TestMsg = BitConverter.ToString(bytes),
      };
   }

   private RJ6091AResultStruct ToResult_RJ6901A(byte[] bytes)
   {
      return new RJ6091AResultStruct()
      {
         Voltage = ToUShort(bytes.Skip(6).ToArray()),
         Resistance = ToInt(bytes.Skip(8).ToArray()) / 1000.0F,
         ElectricCurrent = ToInt(bytes.Skip(12).ToArray()) / 1000.0F,
         Result = bytes[19],
         ResistanceLower = ToInt(bytes.Skip(20).ToArray()) / 10.0F,
         ResistanceUpper = ToInt(bytes.Skip(24).ToArray()) == 0 ? 0 : ToUShort(bytes.Skip(24).ToArray()),
         TestTime = ToUShort(bytes.Skip(28).ToArray()) / 10.0f,
      };
   }
}
