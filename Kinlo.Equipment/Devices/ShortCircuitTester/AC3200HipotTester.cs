using static Kinlo.Equipment.Helpers.Helper;

namespace Kinlo.Equipment.Devices.ShortCircuitTester;

/// <summary>
/// 艾测短路测试仪
/// </summary>
[DeviceConnec([CommunicationEnum.ShortCircuit_AC3200])]
public class AC3200HipotTester : DeviceBase
{
   byte[] _switchResultMode = [0x7B, 0x09, 0x00, 0x01, 0xF1, 0x02, 0x00, 0x00, 0x7D]; //切换为只查结果模式
   byte[] _switchResultAndCurvePointMode = [0x7B, 0x09, 0x00, 0x01, 0xF1, 0x02, 0x00, 0x00, 0x7D]; //切换为只查结果+曲线模式
   byte[] _start = [0x7B, 0x08, 0x00, 0x01, 0xF2, 0x01, 0x00, 0x7D]; //启动测试，如果仪器为自动回传模式时，发送此指令可启动并拿的结果数据
   byte[] _queryResultRequest = [0x7B, 0x09, 0x00, 0x01, 0xF0, 0x02, 0x01, 0x00, 0x7D]; //查结果(不启动)
   byte[] _queryResultRequestCurve = [0x7B, 0x09, 0x00, 0x01, 0xF0, 0x03, 0x01, 0x00, 0x7D]; //查结果+曲线(不启动)

   public AC3200HipotTester(DeviceInfoModel info)
      : base(info) { }

   public IProtocolHelper? ProtocolHelper { get; set; }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
      where TClass : class
   {
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions();
      try
      {
         var request = options.OperationType == 1 ? _queryResultRequestCurve : _start;
         Func<byte[], bool> func = bs => bs != null && bs.Length >= 32 && bs[0] == 0x7B && bs[^1] == 0x7D;

         var readRecord = new DeviceCommand(this, request, 8192, options.RetryCount, ProtocolHelper, func, DeviceInfo.TaskToken, logHeader);
         //读取结果
         var readResult = readRecord.ExecuteRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);
         //解析结果
         var data = ParseResult(readResult.Value!, logHeader);
         if (data is TClass rs)
            return DeviceResult<TClass>.Success(rs);
         else
         {
            var errMsg = $"[{Helper.GetCurrentMethodName()}]传入数据类型和实际数据类型不对应;";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误);
            return DeviceResult<TClass>.Failure(DeviceStatus.结果和期望数据类型不同, errMsg);
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

   /// <summary>
   ///
   /// </summary>
   /// <param name="bytes1"></param>
   /// <param name="type">为1时要取波形</param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   private Ac3200HipotResultModel ParseResult(byte[] bytes1, string logHeader)
   {
      if (bytes1.Length < 32)
      {
         $"短路测试结果数据长度不够，长度为{bytes1.Length},小于32;".LogProcess(logHeader);
         return new Ac3200HipotResultModel { HipotResult = ResultTypeEnum.未知NG原因 };
      }
      var bytes = bytes1[6..];
      var result = new Ac3200HipotResultModel();

      ResultTypeEnum testResult = ParsePulseResult(bytes[2]); //脉冲结果
      result.HipotPulseResult = testResult.ToString();
      result.HipotVpVoltage = (bytes[4] << 8) | bytes[3];
      result.HipotFallOne = (bytes[6] << 8) | bytes[5];
      result.HipotFallTwo = (bytes[8] << 8) | bytes[7];
      result.HipotFallThree = (bytes[10] << 8) | bytes[9];
      result.HipotPulseTp = (float)(((bytes[12] << 8) | bytes[11]) / 10.0);

      var r = ParseInsulationResistanceResult(bytes[14]); //电阻结果
      result.ResistanceTestResult = r.ToString();
      if (r != ResultTypeEnum.Hipot仪器未测试 && (testResult is ResultTypeEnum.OK or ResultTypeEnum.Hipot仪器未测试))
         testResult = r;
      result.InsulationTestValue = (float)(((bytes[18] << 16) | (bytes[17] << 8) | bytes[16]) / 10.0);

      r = ParseCapacitanceResult(bytes[20]); //电容结果
      result.CapacitorsResult = r.ToString();
      if (r != ResultTypeEnum.Hipot仪器未测试 && (testResult is ResultTypeEnum.OK or ResultTypeEnum.Hipot仪器未测试))
         testResult = r;
      result.Capacitors = (float)(((bytes[23] << 16) | (bytes[22] << 8) | bytes[21]) / 100.0);

      if (bytes1.Length > 34) //取波形
      {
         var curBytes = bytes1[30..(bytes1.Length - 4)];
         result.CurvePoint = string.Join(',', curBytes.ToAcHipotCurve() ?? []);
      }
      else
      {
         result.CurvePoint = "";
      }

      if (bytes[0] == 0) //总结果
         result.HipotResult = ResultTypeEnum.OK;
      else
      {
         if (testResult is ResultTypeEnum.Hipot仪器未测试 or ResultTypeEnum.OK)
         {
            result.HipotResult = ResultTypeEnum.Hipot_NG;
            $"测试总结果为NG，但没有找到具体的NG原因;".LogProcess(logHeader);
         }
         else
            result.HipotResult = testResult;
      }
      return result;
   }

   /// <summary>
   /// 解析脉冲结果
   /// </summary>
   /// <param name="b"></param>
   /// <returns></returns>
   ResultTypeEnum ParsePulseResult(byte b)
   {
      return b switch
      {
         0x00 => ResultTypeEnum.OK,
         0x01 => ResultTypeEnum.E01_开路,
         0x02 => ResultTypeEnum.E02_未达到设定电压值,
         0x03 => ResultTypeEnum.E03_电压上升过快_TL报警,
         0x04 => ResultTypeEnum.E04_电压上升过慢_TH报警,
         0x05 => ResultTypeEnum.E05_上升阶段VD1超限,
         0x06 => ResultTypeEnum.E06_电压保持VD2超限,
         0x07 => ResultTypeEnum.E07_自由放电VD3超限,
         0xFF => ResultTypeEnum.Hipot仪器未测试,
         _ => ResultTypeEnum.未知NG原因,
      };
   }

   /// <summary>
   /// 解析电阻结果
   /// </summary>
   /// <param name="b"></param>
   /// <returns></returns>
   ResultTypeEnum ParseInsulationResistanceResult(byte b)
   {
      return b switch
      {
         0x00 => ResultTypeEnum.OK,
         0x08 => ResultTypeEnum.E08_电阻超下限报警,
         0x09 => ResultTypeEnum.E09_电阻超上限报警,
         0xFF => ResultTypeEnum.Hipot仪器未测试,
         _ => ResultTypeEnum.未知NG原因,
      };
   }

   /// <summary>
   /// 解析电容结果
   /// </summary>
   /// <param name="b"></param>
   /// <returns></returns>
   ResultTypeEnum ParseCapacitanceResult(byte b)
   {
      return b switch
      {
         0x00 => ResultTypeEnum.OK,
         0x0A => ResultTypeEnum.E10_电容超下限报警,
         0x0B => ResultTypeEnum.E11_电容超上限报警,
         0xFF => ResultTypeEnum.Hipot仪器未测试,
         _ => ResultTypeEnum.未知NG原因,
      };
   }
}
