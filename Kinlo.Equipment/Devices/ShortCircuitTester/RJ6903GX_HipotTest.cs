using System.Text.Json;
using static Kinlo.Equipment.Helpers.Helper;

namespace Kinlo.Equipment.Devices.ShortCircuitTester;

[DeviceConnec([CommunicationEnum.ShortCircuit_RJ6903GX])]
public class RJ6903GX_HipotTest : DeviceBase
{
   private byte[] _startBytes = [0x7B, 0x00, 0x08, 0x02, 0x0F, 0xFF, 0x18, 0x7D]; //启动测试
   private byte[] _queryBytes = [0x7B, 0x00, 0x08, 0x02, 0xF0, 0xD1, 0xCB, 0x7D];
   private byte[] _queryCurvePoint = [0x7B, 0x00, 0x08, 0x02, 0xF0, 0xC2, 0xBC, 0x7D];

   public RJ6903GX_HipotTest(DeviceInfoModel info)
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
         //读取结果
         var readRecord = new DeviceCommand(
            this,
            _queryBytes,
            1024,
            options.RetryCount,
            new RJ6900SeriesProtocol(87),
            bs => bs.Length == 87,
            DeviceInfo.TaskToken,
            logHeader
         );
         var readResult = readRecord.ExecuteRequest();
         if (readResult.Status != DeviceStatus.Success)
            return DeviceResult<TClass>.Failure(readResult.Status, readResult.ErrorMessage, readResult.Exception);

         //解析结果
         var data = ParseResult(readResult.Value!, logHeader);
         if (data is not TClass rs)
         {
            var errMsg = $"[{Helper.GetCurrentMethodName()}]传入数据类型和实际数据类型不对应;";
            errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误);
            return DeviceResult<TClass>.Failure(DeviceStatus.结果和期望数据类型不同, errMsg);
         }

         //如果结果失败就读取波形，结果成功就不读取波形
         //if (data.OverallResult != ResultTypeEnum.OK) //20250715 客户要求OKNG都需要读取波形
         //{
            readRecord = new DeviceCommand(
               this,
               _queryCurvePoint,
               6000,
               options.RetryCount,
               null,
               bs => bs.Length >= 5768,
               DeviceInfo.TaskToken,
               logHeader
            );
            var readCurveResult = readRecord.ExecuteRequest();

            if (readCurveResult.Status == DeviceStatus.Success)
            {
               var curvePoint = readCurveResult.Value!.ParseLineCurvePoint();
               data.CurvePoint = curvePoint;
            }
         //}

         return DeviceResult<TClass>.Success(rs);
      }
      catch (Exception ex)
      {
         var errMsg = $"[{Helper.GetCurrentMethodName()}]异常：{ex};";
         errMsg.LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return DeviceResult<TClass>.Failure(DeviceStatus.异常, errMsg);
      }
   }

   /// <summary>
   /// 解析测试结果
   /// </summary>
   /// <param name="bytes"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   private RJ6903GXHipotResultModel ParseResult(byte[] bytes, string logHeader)
   {
      var result = new RJ6903GXHipotResultModel();
      if (bytes == null || bytes.Length < 87)
      {
         $"测短路结果字节长度过短或为空！".LogProcess(logHeader);
         result.OverallResult = ResultTypeEnum.NG;
         return result;
      }
      //总结果
      result.OverallResult = bytes[6] == 1 ? ResultTypeEnum.OK : ResultTypeEnum.NG;

      //使用 ReadOnlySpan 只读，AsSpan切片性能较高
      ReadOnlySpan<byte> positiveToNegativeBytes = bytes.AsSpan(8, 25); // 正负极: 8-32 (25 bytes)
      ReadOnlySpan<byte> positiveToCaseBytes = bytes.AsSpan(34, 25); // 正极壳: 34-58 (25 bytes)
      ReadOnlySpan<byte> negativeToCaseBytes = bytes.AsSpan(60, 25); // 负极壳: 60-84 (25 bytes)

      result.PositiveToNegative = ParseChanne(positiveToNegativeBytes, "正负极", logHeader);
      result.PositiveToCase = ParseChanne(positiveToCaseBytes, "正极壳", logHeader);
      result.NegativeToCase = ParseChanne(negativeToCaseBytes, "负极壳", logHeader);

      List<ResultTypeEnum> hipotChannels =
      [
         result.PositiveToNegative.ChannelResult,
         result.PositiveToCase.ChannelResult,
         result.NegativeToCase.ChannelResult,
      ];

      // 结果仲裁
      result.OverallResult = EvaluateResult(result.OverallResult, hipotChannels, "测短路总结果");
      return result;
   }

   private HipotChannelResult ParseChanne(ReadOnlySpan<byte> bytes, string channelName, string logHeader)
   {
      var result = new HipotChannelResult(channelName);

      if (bytes.Length < 25)
      {
         $"{channelName}结果字节长度过短或为空！".LogProcess(logHeader);
         result.ChannelResult = ResultTypeEnum.NG;
         return result;
      }

      result.Vd1 = ToUShort(bytes, 0);
      result.Vd2 = ToUShort(bytes, 2);
      result.Vd3 = ToUShort(bytes, 4);
      result.VpVoltage = ToUShort(bytes, 6);
      result.TpTime = ToUShort(bytes, 8) / 100.00;
      result.Insulation = ToUInt(bytes, 10) / 10.0;

      //  细分项检查 (使用 Span 避免内存分配,比如[14..24]这种性能更高)
      ReadOnlySpan<byte> itemSpan = bytes.Slice(14, 10);
      var itemResList = new List<ResultTypeEnum>(itemSpan.Length);

      //小项目结果判定
      for (int i = 0; i < itemSpan.Length; i++)
         itemResList.Add(ByteToItemResult(itemSpan[i], i));

      // 总体项目结果判定
      result.ChannelResult = ByteToTotalResult(bytes[^1]);

      // 结果仲裁
      result.ChannelResult = EvaluateResult(result.ChannelResult, itemResList, channelName);
      return result;
   }

   /// <summary>
   /// 结果仲裁
   /// </summary>
   /// <param name="totalRes"></param>
   /// <param name="itemRes"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   private ResultTypeEnum EvaluateResult(ResultTypeEnum totalRes, List<ResultTypeEnum> itemRes, string logHeader)
   {
      var hasNg = itemRes.TryFindFirst(x => x != ResultTypeEnum.OK && x != ResultTypeEnum._, out var value);

      if (totalRes == ResultTypeEnum.OK)
      {
         // 总体显示OK，但细分有错 -> 以细分错误为准
         if (hasNg)
         {
            $"{logHeader}总体结果为OK，但细分项发现异常({JsonSerializer.Serialize(itemRes)})，修正结果。".LogProcess(logHeader);
            return value;
         }
      }
      else if (totalRes == ResultTypeEnum.NG)
      {
         // 总体显示OK，但细分有错 -> 以细分错误为准
         if (hasNg)
         {
            return value;
         }
         else
         {
            $"{logHeader}总体结果为NG，但细分项未检测到具体错误，判定为未知异常。".LogProcess(logHeader);
            return ResultTypeEnum.异常;
         }
      }
      return totalRes;
   }

   /// <summary>
   /// 字节转总结果
   /// </summary>
   /// <param name="b"></param>
   /// <returns></returns>
   private ResultTypeEnum ByteToTotalResult(byte b) =>
      b switch
      {
         0x01 => ResultTypeEnum.OK,
         0X00 => ResultTypeEnum._,
         _ => ResultTypeEnum.NG,
      };

   /// <summary>
   /// 字节转小项结果
   /// </summary>
   /// <param name="bt"></param>
   /// <param name="index"></param>
   /// <returns></returns>
   private ResultTypeEnum ByteToItemResult(byte bt, int index) =>
      bt switch
      {
         0x01 => ResultTypeEnum.OK,
         0X00 => ResultTypeEnum._,
         0XFF => Rj6903GxResultDic.TryGetValue(index, out var value) ? value : ResultTypeEnum.NG,
         _ => ResultTypeEnum.NG,
      };

   private Dictionary<int, ResultTypeEnum> Rj6903GxResultDic = new Dictionary<int, ResultTypeEnum>
   {
      { 0, ResultTypeEnum.E01_开路1 },
      { 1, ResultTypeEnum.E02_严重短路 },
      { 2, ResultTypeEnum.E03_电压欠压 },
      { 3, ResultTypeEnum.E04_电压过压 },
      { 4, ResultTypeEnum.E05_升压阶段Vd1超限 },
      { 5, ResultTypeEnum.E06_电压保持Vd2超限 },
      { 6, ResultTypeEnum.E11_自由放电Vd3超限 },
      { 7, ResultTypeEnum.E07_TLNg_Tp超下限 },
      { 8, ResultTypeEnum.E08_THNg_Tp超上限 },
      { 9, ResultTypeEnum.E09_电阻超下限 },
   };

   private int ToUInt(ReadOnlySpan<byte> bytes, int start) =>
      (int)((bytes[start] << 24) | (bytes[start + 1] << 16) | (bytes[start + 2] << 8) | bytes[start + 3]);

   private ushort ToUShort(ReadOnlySpan<byte> bytes, int start) => (ushort)((bytes[start] << 8) | bytes[start + 1]);

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
