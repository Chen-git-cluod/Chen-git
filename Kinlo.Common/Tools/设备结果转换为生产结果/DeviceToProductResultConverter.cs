namespace Kinlo.Common.Tools;

public static class DeviceToProductResultConverter
{
   public static ResultTypeEnum ToProductResult(this DeviceStatus deviceStatus) =>
      deviceStatus switch
      {
         DeviceStatus.Success => ResultTypeEnum.OK,
         DeviceStatus.Failure => ResultTypeEnum.NG,
         DeviceStatus.设备已停机 => ResultTypeEnum.设备已停机,
         DeviceStatus.通信失败 => ResultTypeEnum.通信失败,
         DeviceStatus.结果和期望数据类型不同 => ResultTypeEnum.结果和期望数据类型不同,
         DeviceStatus.取值失败 => ResultTypeEnum.取值失败,
         DeviceStatus.报文校验不通过 => ResultTypeEnum.报文校验不通过,
         DeviceStatus.数据类型不支持读取 => ResultTypeEnum.数据类型不支持读取,
         DeviceStatus.称重不稳定 => ResultTypeEnum.称重不稳定,
         DeviceStatus.异常 => ResultTypeEnum.异常,
         _ => ResultTypeEnum.NG,
      };
}
