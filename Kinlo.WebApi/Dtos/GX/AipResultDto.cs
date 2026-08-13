namespace Kinlo.WebApi.Dtos.GX;

public class GX_ApiResult
{
   /// <summary>
   /// 应答状态
   /// </summary>
   public string code { get; set; } = string.Empty;

   /// <summary>
   /// 上传状态
   /// </summary>
   public bool status { get; set; }

   /// <summary>
   /// 上传成功提醒
   /// </summary>
   public string message { get; set; } = string.Empty;

   /// <summary>
   /// 请将请求中的traceID在响应中返回
   /// </summary>
   public string traceID { get; set; } = string.Empty;

   /// <summary>
   /// 校验失败原因
   /// </summary>
   public string errorMsg { get; set; } = string.Empty;

   public static GX_ApiResult OK(string traceId) =>
      new GX_ApiResult
      {
         code = "200",
         status = true,
         message = "操作成功",
         traceID = traceId,
         errorMsg = "are no errors",
      };
}

public class GX_ApiResult<T> : GX_ApiResult
{
   public T? data { get; set; }

   public static GX_ApiResult<T?> Fail(string traceId, string errMsg, T? data = default, string code = "2002") =>
      new GX_ApiResult<T?>
      {
         code = code,
         status = false,
         message = "",
         traceID = traceId,
         errorMsg = errMsg,
         data = data,
      };
}
