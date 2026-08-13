using static Kinlo.Equipment.Helpers.Helper;

namespace Kinlo.Equipment.Devices.ShortCircuitTester;

[DeviceConnec([CommunicationEnum.ShortCircuit_Ainuo_ANBTS7201])]
public class Ainuo_ANBTS7201_HipotTest : DeviceBase
{
   private byte[] _startBytes = [0x7B, 0x00, 0x08, 0x02, 0x0F, 0xFF, 0x18, 0x7D]; //启动测试
   private byte[] _queryBytes = [0x7B, 0x00, 0x08, 0x02, 0xF0, 0x7C, 0x76, 0x7D];

   public Ainuo_ANBTS7201_HipotTest(DeviceInfoModel info)
      : base(info) { }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions() { RetryCount = 3 };
      try
      {
         var readRecord = new DeviceCommand(
            this,
            _startBytes,
            1024,
            options.RetryCount,
            new RJ6900SeriesProtocol(9),
            bs => bs[4] == 0x0F,
            DeviceInfo.TaskToken,
            logHeader
         );
         //启动测试
         var readResult = readRecord.ExecuteRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);

         Thread.Sleep(700);
         //读取bytes
         var queryResult = PollWithTimeoutSync(6, logHeader);
         if (queryResult.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);

         var result = ParseResult(queryResult.Value!, logHeader);

         if (result is TClass rs)
            return DeviceResult<TClass>.Success(rs);
         else
         {
            var msg = $"[{Helper.GetCurrentMethodName()}]传入数据类型和实际数据类型不对应;";
            msg.LogProcess(logHeader, Log4NetLevelEnum.错误);
            return DeviceResult<TClass>.Failure(DeviceStatus.结果和期望数据类型不同, msg);
         }
      }
      catch (Exception ex)
      {
         var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
         errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return DeviceResult<TClass>.Failure(DeviceStatus.异常, errMsg);
      }
   }

   /// <summary>
   /// 执行带超时的同步轮询操作
   /// </summary>
   /// <param name="timeoutSeconds">超时时间（秒）</param>
   /// <returns>轮询得到的结果，如果超时或失败则返回 null</returns>
   private DeviceResult<byte[]> PollWithTimeoutSync(int timeoutSeconds, string logHeader)
   {
      Stopwatch stopwatch = Stopwatch.StartNew(); // 开始计时
      byte[]? bytes = null;
      try
      {
         //验证器
         Func<byte[], bool> validator = bs =>
            bs.Length > 9 && bytes[0] == 0x7B && bytes[bytes.Length - 1] == 0x7D && RJ6900SeriesProtocol.OnGetVerifySum(bytes);
         var readRecord = new DeviceSplitCommand(this, _queryBytes, 4096, 1, validator, DeviceInfo.TaskToken, logHeader, 20);
         var result = DeviceResult<byte[]>.Failure(DeviceStatus.Failure);

         while (stopwatch.Elapsed.TotalSeconds < timeoutSeconds)
         {
            var readResult = readRecord.ExecuteSplitRequest();
            if (readResult.Status == DeviceStatus.Success)
            {
               bytes = readResult.Value;
               $"查询成功".LogProcess(logHeader, Log4NetLevelEnum.成功);
               return DeviceResult<byte[]>.Success(bytes);
            }
            result = DeviceResult<byte[]>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);
            Thread.Sleep(100); // 每次轮询间隔 100 毫秒
         }
         return result;
      }
      catch (Exception ex)
      {
         $"查询异常：{ex}！".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return DeviceResult<byte[]>.Failure(DeviceStatus.异常);
      }
      finally
      {
         stopwatch.Stop();
      }
   }

   private int ToUInt(byte[] bytes, int start) =>
      (int)((bytes[start] << 24) | (bytes[start + 1] << 16) | (bytes[start + 2] << 8) | bytes[start + 3]);

   private ushort ToUShort(byte[] bytes, int start) => (ushort)((bytes[start] << 8) | bytes[start + 1]);

   /// <summary>
   /// 客户俩种仪器混用，此处解析为Ac3200HipotResultModel
   /// </summary>
   /// <param name="bytes1"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   Ac3200HipotResultModel ParseResult(byte[] bytes1, string logHeader)
   {
      var bytes = bytes1[6..];
      var result = new Ac3200HipotResultModel();
      result.HipotFallOne = ToUShort(bytes, 0);
      result.HipotFallTwo = ToUShort(bytes, 2);
      result.HipotFallThree = ToUShort(bytes, 4);
      result.HipotVpVoltage = ToUShort(bytes, 6);
      result.HipotPulseTp = (float)(ToUShort(bytes, 8) / 100.00);
      // result.InsulationTestValue = (float)(ToUInt(bytes, 12) / 10.0);
      ResultTypeEnum testResult = ParsePulseResult(bytes, 14); //脉冲结果
      result.HipotPulseResult = testResult.ToString();
      var resutl1 = ParseResult(bytes[23], ResultTypeEnum.电阻测试NG); //电阻结果
      result.ResistanceTestResult = resutl1 == ResultTypeEnum._ ? "" : resutl1.ToString();

      result.HipotResult = ParseResult(bytes[24], ResultTypeEnum.NG); //总结果

      if (result.HipotResult != ResultTypeEnum.OK && result.HipotResult != ResultTypeEnum._)
      {
         result.HipotResult = (testResult, resutl1) switch
         {
            var r when r.testResult != ResultTypeEnum.OK => r.testResult,
            var r when r.resutl1 != ResultTypeEnum.OK => r.resutl1,
            _ => result.HipotResult,
         };
      }

      if (bytes1.Length > 31) //取波形
      {
         var curBytes = bytes1[29..(bytes1.Length - 4)];
         result.CurvePoint = string.Join(',', curBytes.ToAcHipotCurve() ?? []);
      }
      else
      {
         $"未取到波形！".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         result.CurvePoint = "";
      }
      return result;
   }

   ResultTypeEnum ParseResult(byte b, ResultTypeEnum ngResult)
   {
      return b switch
      {
         0x00 => ResultTypeEnum._,
         0x01 => ResultTypeEnum.OK,
         _ => ngResult,
      };
   }

   /// <summary>
   /// 解析脉冲结果
   /// </summary>
   /// <param name="b"></param>
   /// <returns></returns>
   ResultTypeEnum ParsePulseResult(byte[] bytes, int start)
   {
      return bytes switch
      {
         var bs when bs[start + 0] == 0xff => ResultTypeEnum.开路,
         var bs when bs[start + 1] == 0xff => ResultTypeEnum.严重短路,
         var bs when bs[start + 2] == 0xff => ResultTypeEnum.欠压,
         var bs when bs[start + 3] == 0xff => ResultTypeEnum.过压,
         var bs when bs[start + 4] == 0xff => ResultTypeEnum.VD1_NG,
         var bs when bs[start + 5] == 0xff => ResultTypeEnum.VD2_NG,
         var bs when bs[start + 6] == 0xff => ResultTypeEnum.VD3_NG,
         var bs when bs[start + 7] == 0xff => ResultTypeEnum.TL_NG,
         var bs when bs[start + 8] == 0xff => ResultTypeEnum.TH_NG,
         _ => ResultTypeEnum.OK,
      };
   }

   public override DeviceResult<TValue> ReadValue<TValue>(
      SignalAddressModel address,
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
