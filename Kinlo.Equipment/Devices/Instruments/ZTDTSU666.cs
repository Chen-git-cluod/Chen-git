using static Kinlo.Equipment.Helpers.Helper;

namespace Kinlo.Equipment.Devices.Instruments;

[DeviceConnec([CommunicationEnum.ZTDTSU666电能表])]
public class ZTDTSU666 : DeviceBase
{
   #region field
   /// <summary>
   /// 电压互感器倍率
   /// </summary>
   byte[] _urAtRequest = [0x01, 0x03, 0x00, 0x07, 0x00, 0x01, 0x35, 0xCB];

   /// <summary>
   /// 电流互感器倍率
   /// </summary>
   byte[] _irAtRequest = [0x01, 0x03, 0x00, 0x06, 0x00, 0x01, 0x64, 0x0B];

   /// <summary>
   /// 读当前正向有功总电能 ImpEp (101EH)
   /// </summary>
   byte[] _impEpRequest = [0x01, 0x03, 0x10, 0x1E, 0x00, 0x02, 0xA0, 0xCD];

   /// <summary>
   /// 其它数据
   /// </summary>
   byte[] _otberRequest = [0x01, 0x03, 0x20, 0x06, 0x00, 0x0E, 0x2F, 0xCF];

   public IProtocolHelper? ProtocolHelper { get; set; }
   #endregion
   public ZTDTSU666(DeviceInfoModel info)
      : base(info)
   {
      ProtocolHelper = new Modbus_RTU_Protocol();
   }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class
   {
      options ??= new DeviceOperationOptions();
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      try
      {
         var readRecord = new DeviceCommand(
            this,
            _irAtRequest,
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
         var irAt = BitConverter.ToInt16(readResult.Value!.Reverse().ToArray());

         readRecord = new DeviceCommand(
            this,
            _urAtRequest,
            1024,
            options.RetryCount,
            ProtocolHelper,
            null,
            DeviceInfo.TaskToken,
            logHeader
         );
         readResult = readRecord.ExecuteRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);
         var urAt = BitConverter.ToInt16(readResult.Value!.Reverse().ToArray());

         readRecord = new DeviceCommand(
            this,
            _impEpRequest,
            1024,
            options.RetryCount,
            ProtocolHelper,
            null,
            DeviceInfo.TaskToken,
            logHeader
         );
         readResult = readRecord.ExecuteRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);
         ZTDTSU666ResultModel data = new ZTDTSU666ResultModel();
         data.ImpEp = GetImpEp(readResult.Value!, irAt, urAt);

         readRecord = new DeviceCommand(
            this,
            _otberRequest,
            1024,
            options.RetryCount,
            ProtocolHelper,
            null,
            DeviceInfo.TaskToken,
            logHeader
         );
         readResult = readRecord.ExecuteRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);

         var bytes = readResult.Value!;
         if (bytes != null)
         {
            data.Ua = GetUabc(bytes.Take(4).ToArray(), urAt);
            data.Ub = GetUabc(bytes.Skip(4).Take(4).ToArray(), urAt);
            data.Uc = GetUabc(bytes.Skip(8).Take(4).ToArray(), urAt);
            data.Ia = GetIabc(bytes.Skip(12).Take(4).ToArray(), irAt);
            data.Ib = GetIabc(bytes.Skip(16).Take(4).ToArray(), irAt);
            data.Ic = GetIabc(bytes.Skip(20).Take(4).ToArray(), irAt);
            data.Pt = GetPt(bytes.Skip(24).Take(4).ToArray());
         }
         return DeviceResult<TClass>.Success(data as TClass);
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

   public override bool WriteValue(
      object value,
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null,
      Encoding? encoding = null
   )
   {
      throw new NotImplementedException();
   }

   #region
   /// <summary>
   ///  获取三相电压,单位：V
   /// </summary>
   /// <param name="bytes"></param>
   /// <returns></returns>
   private float GetUabc(byte[]? bytes, short urAt)
   {
      if (bytes == null)
         return 0;
      var _result = BitConverter.ToSingle(bytes.Reverse().ToArray()) * (urAt * 0.1) * 0.1f;
      return (float)Math.Round(_result, 3);
   }

   /// <summary>
   /// 获取三相电流,单位：A
   /// </summary>
   /// <param name="bytes"></param>
   /// <returns></returns>
   private float GetIabc(byte[]? bytes, short irAt)
   {
      if (bytes == null)
         return 0;
      float _result = BitConverter.ToSingle(bytes.Reverse().ToArray()) * 0.001f * irAt;
      return (float)Math.Round(_result, 3);
   }

   /// <summary>
   /// 读当前正向有功总电能,单位：kWh
   /// </summary>
   /// <param name="bytes"></param>
   /// <returns></returns>
   private float GetImpEp(byte[]? bytes, short irAt, short urAt)
   {
      if (bytes == null)
         return 0;
      var _result = BitConverter.ToSingle(bytes.Reverse().ToArray()) * irAt * (urAt * 0.1);
      return (float)Math.Round(_result, 3);
   }

   /// <summary>
   /// 读合相有功功率，单位 kW,采的是W,除1000换成KW
   /// </summary>
   /// <param name="bytes"></param>
   /// <returns></returns>
   private float GetPt(byte[]? bytes)
   {
      if (bytes == null)
         return 0;
      float _result = BitConverter.ToSingle(bytes.Reverse().ToArray()) / 10f;
      return (float)Math.Round(_result, 3);
   }
   #endregion
}
