namespace Kinlo.WebApi.Dtos.LocalData;

public class ApiResponse
{
   public int code { get; set; }
   public string message { get; set; } = string.Empty;

   /// <summary>
   /// 追踪ID
   /// </summary>
   public string traceID { get; set; } = string.Empty;
   public string process { get; set; } = string.Empty;
   public string device { get; set; } = string.Empty;

   public static ApiResponse Fail(string traceId, string process, string device, string msg) =>
      new ApiResponse
      {
         code = 400,
         message = msg,
         traceID = traceId,
         process = process,
         device = device,
      };
}

public class ApiResponse<T> : ApiResponse
{
   public T? Data { get; set; }

   public static ApiResponse<T> OK(string traceId, string process, string device, T data) =>
      new ApiResponse<T>
      {
         code = 200,
         message = "查询成功",
         traceID = traceId,
         process = process,
         device = device,
         Data = data,
      };
}

public class ApiExpandResponse : ApiResponse
{
   public string span { get; set; } = string.Empty;

   public static ApiExpandResponse Fail(string traceId, string process, string device, string span, string msg) =>
      new ApiExpandResponse
      {
         code = 400,
         message = msg,
         traceID = traceId,
         process = process,
         device = device,
         span = span,
      };
}

public class ApiExpandResponse<T> : ApiExpandResponse
{
   public T? Data { get; set; }

   public static ApiExpandResponse<T> OK(string traceId, string process, string device, string span, T data) =>
      new ApiExpandResponse<T>
      {
         code = 200,
         message = "查询成功",
         traceID = traceId,
         process = process,
         device = device,
         span = span,
         Data = data,
      };
}

/// <summary>
/// 设备状态
/// </summary>
public class DeviceStatusData
{
   public string startTime { get; set; } = string.Empty;
   public string endTime { get; set; } = string.Empty;
   public string shift { get; set; } = string.Empty;
   public string status { get; set; } = string.Empty;
}

/// <summary>
/// 设备异常
/// </summary>
public class DeviceExceptionData
{
   public string startTime { get; set; } = string.Empty;
   public string endTime { get; set; } = string.Empty;
   public string shift { get; set; } = string.Empty;

   /// <summary>
   /// 停机类型（主动停机/被动停机/堵料）
   /// </summary>
   public string stopType { get; set; } = string.Empty;

   /// <summary>
   /// 停机原因
   /// </summary>
   public string stopReason { get; set; } = string.Empty;
}

/// <summary>
/// 产量
/// </summary>
public class ProductData
{
   /// <summary>
   /// 生产时间
   /// </summary>
   public string createTime { get; set; } = string.Empty;

   /// <summary>
   /// 班次
   /// </summary>
   public string shift { get; set; } = string.Empty;

   /// <summary>
   /// 投入数
   /// </summary>
   public int input { get; set; }

   /// <summary>
   /// 产出数（合格数）
   /// </summary>
   public int output { get; set; }

   /// <summary>
   /// OEE百分比 (例如: "79.1%")
   /// </summary>
   public string oee { get; set; } = string.Empty;

   /// <summary>
   /// 不良率 (例如: "2.5%")
   /// </summary>
   public string defectRate { get; set; } = string.Empty;

   /// <summary>
   /// 扫码NG数量
   /// </summary>
   public int scanCodeNG { get; set; }

   /// <summary>
   /// 前重NG数量
   /// </summary>
   public int beforeWeightNG { get; set; }

   /// <summary>
   /// 未注液NG数量
   /// </summary>
   public int noFluidInjectionNG { get; set; }

   /// <summary>
   /// 多液NG数量
   /// </summary>
   public int multiLiquidNG { get; set; }

   /// <summary>
   /// 被动停机时间（秒/分，根据业务定）
   /// </summary>
   public int passiveDownTime { get; set; }

   /// <summary>
   /// 主动停机时间
   /// </summary>
   public int activeDownTime { get; set; }

   /// <summary>
   /// 堵料时间
   /// </summary>
   public int blockMaterialTime { get; set; }

   /// <summary>
   /// 待料时间
   /// </summary>
   public int waitMaterialTime { get; set; }
}

/// <summary>
/// OEE
/// </summary>
public class OutputOeeData
{
   public string startTime { get; set; } = string.Empty;
   public string endTime { get; set; } = string.Empty;
   public string output { get; set; } = string.Empty;
   public string oee { get; set; } = string.Empty;
}
