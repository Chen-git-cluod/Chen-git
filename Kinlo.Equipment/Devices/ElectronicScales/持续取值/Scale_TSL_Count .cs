namespace Kinlo.Equipment.Devices.ElectronicScales;

/// <summary>
/// Scale_TSL电子计数秤（TSL/TSC-i/E318）
/// 通讯格式：Co12 
/// </summary>
[DeviceConnec([CommunicationEnum.Scale_TSL_Count])]
public class Scale_TSL_Count : CachedWeighingScaleBase
{
    
    protected override int FrameLength => 30;

    protected override byte[] ZeroCommand => throw new NotImplementedException("TSL计数秤无下发置零串口指令，未实现");

    public Scale_TSL_Count(DeviceInfoModel info)
        : base(info) { }

    protected override DeviceResult<double> TryParseWeight(byte[] frame, string logHeader)
    {
        var result = DeviceResult<double>.Failure(DeviceStatus.Failure);
        List<byte[]> splitBytes = frame.SplitByteArray([0x0D, 0x0A]);
        int parseTimes = 1;

        // 倒序取最新报文
        for (int i = splitBytes.Count - 1; i >= 0; i--)
        {
            parseTimes++;
            var byteArr = splitBytes[i];
            // 最小合法报文长度过滤
            if (byteArr.Length < 15)
            {
                string errMsg = $"第{parseTimes}次 字节长度不足，非法帧：[{(byteArr == null ? "null" : BitConverter.ToString(byteArr))}]";
                errMsg.LogProcess(logHeader);
                result = DeviceResult<double>.Failure(DeviceStatus.取值失败, errMsg);
                if (parseTimes >= 5) break;
                continue;
            }

            // 判断稳定标识 ST = 0x53(S) 0x54(T)
            if (byteArr[0] == 0x53 && byteArr[1] == 0x54)
            {
                string fullText = Encoding.ASCII.GetString(byteArr);
                // 找到正负符号起点
                int weightStartIdx = fullText.IndexOfAny(new[] { '+', '-' });
                if (weightStartIdx < 0)
                {
                    string errMsg = $"第{parseTimes}次 报文无正负符号：{fullText}";
                    errMsg.LogProcess(logHeader);
                    result = DeviceResult<double>.Failure(DeviceStatus.称重不稳定, errMsg);
                    if (parseTimes >= 5) break;
                    continue;
                }

                // 截取符号到kg前的数字段，兼容2000.000长数字
                int kgTagIndex = fullText.IndexOf("kg", weightStartIdx);
                if (kgTagIndex <= weightStartIdx)
                {
                    string errMsg = $"第{parseTimes}次 未找到单位kg，报文：{fullText}";
                    errMsg.LogProcess(logHeader);
                    result = DeviceResult<double>.Failure(DeviceStatus.取值失败, errMsg);
                    if (parseTimes >= 5) break;
                    continue;
                }

                string weightRawStr = fullText.Substring(weightStartIdx, kgTagIndex - weightStartIdx).Replace(" ", "");
                if (double.TryParse(weightRawStr, out double realWeight))
                {
                    $"TSL秤读取稳定净重值[{realWeight}kg]，原始报文：{fullText}".LogProcess(logHeader);
                    return DeviceResult<double>.Success(realWeight);
                }
                else
                {
                    string errMsg = $"第{parseTimes}次 数字解析失败，提取字符串：{weightRawStr}，完整报文：{fullText}";
                    errMsg.LogProcess(logHeader);
                    result = DeviceResult<double>.Failure(DeviceStatus.称重不稳定, errMsg);
                }
            }
            else
            {
                string frameText = Encoding.ASCII.GetString(byteArr);
                string errMsg = $"第{parseTimes}次 非稳定帧(无ST标识)，报文：{frameText}";
                errMsg.LogProcess(logHeader);
                result = DeviceResult<double>.Failure(DeviceStatus.称重不稳定, errMsg);
            }

            if (parseTimes >= 5)
                break;
        }

        return result;
    }

    /// <summary>
    /// 无下发写入功能（设置单重/去皮/上下限仅面板操作）
    /// </summary>
    public override bool WriteValue(
        object value,
        SignalAddressModel address,
        string logHeader,
        DeviceOperationOptions? options = null,
        Encoding? encoding = null
    )
    {
        throw new NotImplementedException("TSL电子计数秤仅支持读取重量，无下发写入功能，未实现");
    }
}