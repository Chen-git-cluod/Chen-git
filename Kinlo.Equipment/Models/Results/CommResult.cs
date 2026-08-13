namespace Kinlo.Equipment.Models;

public class CommResult
{
   public CommState State { get; set; }
   public string Message { get; set; } = string.Empty;
   public Exception? Exception { get; init; }

   public static CommResult Ok() => new() { State = CommState.Success };

   public static CommResult Fail(CommState state, string msg, Exception? exception = null) =>
      new()
      {
         State = state,
         Message = msg,
         Exception = exception,
      };
}

public class CommResult<T>
{
   public CommState State { get; set; }
   public T? Data { get; set; }
   public string Message { get; set; } = string.Empty;
   public Exception? Exception { get; init; }

   public static CommResult<T> Ok(T data) => new() { State = CommState.Success, Data = data };

   public static CommResult<T> Fail(CommState state, string msg, Exception? exception = null) =>
      new()
      {
         State = state,
         Message = msg,
         Exception = exception,
      };
}

public enum CommState
{
   /// <summary>
   /// 成功
   /// </summary>
   Success = 0, // 成功

   /// <summary>
   /// 业务失败（PLC返回错误、数据错误）
   /// </summary>
   Failed = 1, // 业务失败（PLC返回错误、数据错误）

   /// <summary>
   /// 通信链路异常，需要重连
   /// </summary>
   NeedReconnect = 2, // 通信链路异常，需要重连
}
