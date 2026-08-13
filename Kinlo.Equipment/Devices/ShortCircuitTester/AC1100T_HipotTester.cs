namespace Kinlo.Equipment.Devices.ShortCircuitTester;

/// <summary>
/// 艾测短路测试仪
/// </summary>
[DeviceConnec([CommunicationEnum.ShortCircuit_AC1100T])]
public class AC1100T_HipotTester : DeviceBase
{
   byte[] _start = [0x7B, 0x08, 0x00, 0x01, 0xF2, 0x01, 0x00, 0x7D]; //启动测试，如果仪器为自动回传模式时，发送此指令可启动并拿的结果数据

   public AC1100T_HipotTester(DeviceInfoModel info)
      : base(info) { }

   public IProtocolHelper? ProtocolHelper { get; set; }

   public override DeviceResult<TClass> ReadClass<TClass>(
      SignalAddressModel address,
      TClass obj,
      string logHeader,
      DeviceOperationOptions? options = null
   )
   {
      logHeader = DeviceInfo.SplitDeviceLogHeader(logHeader, address);
      options ??= new DeviceOperationOptions();
      try
      {
         var readRecord = new DeviceCommand(
            this,
            _start,
            4096,
            options.RetryCount,
            ProtocolHelper,
            bs => bs != null,
            DeviceInfo.TaskToken,
            logHeader
         );
         var readResult = readRecord.ExecuteRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);

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

   AC1100THipotResultModel ParseResult(byte[] bytes1, string logHeader)
   {
      if (bytes1.Length < 57)
      {
         //$"短路测试结果数据长度不够，长度为{bytes1.Length},小于32;".LogProcess(logHeader);
         return new AC1100THipotResultModel { OverallResult = ResultTypeEnum.取值失败 };
      }
      var bytes = bytes1[6..];
      var result = new AC1100THipotResultModel();

      result.OverallResult = bytes[0] == 0 ? ResultTypeEnum.OK : ResultTypeEnum.NG;
      result.CaseResult = bytes[1] switch
      {
         0x00 => ResultTypeEnum.OK,
         //  0x01 => ResultTypeEnum.E01_开路,
         0x01 => ResultTypeEnum.外壳开路,
         0xFF => ResultTypeEnum.Hipot仪器未测试,
         _ => ResultTypeEnum.异常,
      };
      result.PositiveToNegativeResult = ParseResult(bytes[3]); //正负极结果
      result.PositiveToNegativeVpVoltage = (bytes[5] << 8) | bytes[4];
      result.PositiveToNegativeVD1 = (bytes[7] << 8) | bytes[6];
      result.PositiveToNegativeVD2 = (bytes[9] << 8) | bytes[8];
      result.PositiveToNegativeVD3 = (bytes[11] << 8) | bytes[10];
      result.PositiveToNegativeTP = ((bytes[13] << 8) | bytes[12]) / 10.0;
      result.PositiveToNegativeInsulation = ((bytes[16] << 16) | (bytes[15] << 8) | bytes[14]) / 10.0;

      result.PositiveToCaseResult = ParseResult(bytes[18]); //正极对壳结果
      result.PositiveToCaseVpVoltage = (bytes[20] << 8) | bytes[19];
      result.PositiveToCaseVD1 = (bytes[22] << 8) | bytes[21];
      result.PositiveToCaseVD2 = (bytes[24] << 8) | bytes[23];
      result.PositiveToCaseVD3 = (bytes[26] << 8) | bytes[25];
      result.PositiveToCaseTP = ((bytes[28] << 8) | bytes[27]) / 10.0;
      result.PositiveToCaseInsulation = ((bytes[31] << 16) | (bytes[30] << 8) | bytes[29]) / 10.0;
      result.PositiveToCaseWeakConduction = (bytes[33] << 8) | bytes[32];

      result.NegativeToCaseResult = ParseResult(bytes[35]); //正极对壳结果
      result.NegativeToCaseVpVoltage = (bytes[37] << 8) | bytes[36];
      result.NegativeToCaseVD1 = (bytes[39] << 8) | bytes[38];
      result.NegativeToCaseVD2 = (bytes[41] << 8) | bytes[40];
      result.NegativeToCaseVD3 = (bytes[43] << 8) | bytes[42];
      result.NegativeToCaseTP = ((bytes[45] << 8) | bytes[44]) / 10.0;
      result.NegativeToCaseInsulation = ((bytes[48] << 16) | (bytes[47] << 8) | bytes[46]) / 10.0;

      if (result.OverallResult != ResultTypeEnum.OK)
      {
         result.OverallResult = result switch
         {
            var rs when rs.CaseResult is ResultTypeEnum.E01_开路 or ResultTypeEnum.异常 => rs.CaseResult,
            var rs when rs.PositiveToNegativeResult != ResultTypeEnum.OK => rs.PositiveToNegativeResult,
            var rs when rs.PositiveToCaseResult != ResultTypeEnum.OK => rs.PositiveToCaseResult,
            var rs when rs.NegativeToCaseResult != ResultTypeEnum.OK => rs.NegativeToCaseResult,
            _ => new Func<ResultTypeEnum>(() =>
            {
               $"测试总结果为NG，但没有找到具体的NG原因;".LogProcess(logHeader);
               return ResultTypeEnum.异常;
            })(),
         };
      }
      return result;
   }

   /// <summary>
   /// 解析结果
   /// </summary>
   /// <param name="b"></param>
   /// <returns></returns>
   ResultTypeEnum ParseResult(byte b)
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
         0x08 => ResultTypeEnum.E08_电阻超下限报警,
         0x09 => ResultTypeEnum.E09_电阻超上限报警,
         0x0A => ResultTypeEnum.E10_电容超下限报警,
         0x0B => ResultTypeEnum.E11_电容超上限报警,
         0xFF => ResultTypeEnum.Hipot仪器未测试,
         _ => ResultTypeEnum.异常,
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
