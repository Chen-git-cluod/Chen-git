using System.Formats.Asn1;

namespace Kinlo.Equipment.Devices.CodeScanningGun;

/// <summary>
/// 扫码枪通用类
/// </summary>
[DeviceConnec([CommunicationEnum.ScanCode_SR1000, CommunicationEnum.ScanCode_SR700])]
public class CodeScanningGunGeneric : DeviceBase
{
   private byte[] _start = Encoding.ASCII.GetBytes("LON\r\n");
   private byte[] _end = Encoding.ASCII.GetBytes("LOFF\r\n");

   public CodeScanningGunGeneric(DeviceInfoModel info)
      : base(info) { }

   /// <summary>
   /// 扫码
   /// </summary>
   /// <typeparam name="TValue"></typeparam>
   /// <param name="address"></param>
   /// <param name="logHeader"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TValue : default
   {
      options ??= new DeviceOperationOptions();
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);

      try
      {
         //读取结果
         var readRecord = new DeviceSplitCommand(
            this,
            _start,
            1024,
            options.RetryCount,
            bs => bs != null && bs.Any(),
            DeviceInfo.TaskToken,
            logHeader,
            50
         );
         var readResult = readRecord.ExecuteSplitRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TValue>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);

         byte[] bytes = readResult.Value!;
         string barcode = Encoding.ASCII.GetString(bytes) ?? "";
         barcode = barcode.Trim('\u0002', '\u0003', '\r', '\n', ' ');
         return DeviceResult<TValue>.Success((TValue)(object)barcode);
      }
      catch (Exception ex)
      {
         var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
         errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return DeviceResult<TValue>.Failure(DeviceStatus.异常, errMsg);
      }
      finally
      {
         try
         {
            var writeRes = Connect.Write(_end, logHeader);
            if (writeRes.State != CommState.Success)
            {
               $"扫码枪写入完成失败".LogProcess(logHeader);
            }
         }
         catch { }
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
}
