namespace Kinlo.Equipment.Models;

/// <summary>
/// 目前用于仪器读取返回，后期也可以扩展到其它
/// </summary>
/// <typeparam name="T"></typeparam>
public class DeviceResult<T>
{
   public DeviceStatus Status { get; init; }
   public T? 
        Value { get; init; }

   //public ResultTypeEnum ErrCode { get; set; }
   public string? ErrorMessage { get; init; }
   public Exception? Exception { get; init; }

   public static DeviceResult<T> Success(T value) => new() { Status = DeviceStatus.Success, Value = value };

   public static DeviceResult<T> Failure(string message, Exception? ex = null) =>
      new()
      {
         Status = DeviceStatus.Failure,
         ErrorMessage = message,
         Exception = ex,
      };

   public static DeviceResult<T> Failure(DeviceStatus status, string message = "", Exception? ex = null) =>
      new()
      {
         Status = status,
         ErrorMessage = string.IsNullOrEmpty(message) ? status.ToString() : message,
         Exception = ex,
      };
}

public enum DeviceStatus
{
   Success,
   Failure, // 通用的失败
   #region 11~20共用失败状态
   设备已停机 = 11,
   通信失败 = 12,
   结果和期望数据类型不同 = 13,
   取值失败 = 14,
   报文校验不通过 = 15,
   数据类型不支持读取 = 16,
   异常 = 100,
   #endregion
   #region 101~110扫码失败状态

   #endregion
   #region 111~120电子称失败状态
   称重不稳定 = 111,
   #endregion
   #region 121~130Hipot失败状态

   #endregion
   #region 131~140测电压失败状态

   #endregion
}
