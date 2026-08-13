namespace Kinlo.WebApi.Dtos.GX
{
   public class UploadParamRequestDto
   {
      /// <summary>
      /// 变更简要描述
      /// </summary>
      public string remark { get; set; } = string.Empty;

      /// <summary>
      /// 客户端名称
      /// </summary>
      public string clientName { get; set; } = string.Empty;

      /// <summary>
      /// 上传时间
      /// </summary>
      public string pushTime { get; set; } = string.Empty;

      /// <summary>
      /// 对账ID
      /// </summary>
      public string traceID { get; set; } = string.Empty;

      /// <summary>
      /// 设备编码
      /// </summary>
      public string equipCode { get; set; } = string.Empty;

      /// <summary>
      /// 设备租户号
      /// </summary>
      public string tenantID { get; set; } = string.Empty;

      // 参数列表
      public List<ParamItem> paramList { get; set; } = new();
   }

   public class ParamItem
   {
      /// <summary>
      /// 参数编码
      /// </summary>
      public string paramCode { get; set; } = string.Empty;

      /// <summary>
      /// 参数名称
      /// </summary>
      public string paramName { get; set; } = string.Empty;

      /// <summary>
      /// 设置值
      /// </summary>
      public string value { get; set; } = string.Empty;

      /// <summary>
      /// 版本号
      /// </summary>
      public int version { get; set; }
   }
}
