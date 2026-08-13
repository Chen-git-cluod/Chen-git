using static Kinlo.Common.RemoteLocalParamSyncService;

namespace Kinlo.WebApi.Controllers;

/// <summary>
/// 国轩使用的接口
/// </summary>
[ApiController]
[Route("api/dynamic")] // 占位路由，运行时会被 DynamicRouteConvention 替换
[Languages("国轩接口", IsScanMethod = true)]
[Tags("国轩接口")] //给Swagger分类以及过滤控制器
public class GX_Controller : ControllerBase
{
   IContainer _container;
   ParameterConfig _localConfig;
   RemoteLocalParamSyncService _remoteLocalParamSyncService;
   Lazy<OtherParameterConfig> _otherParameterConfig;

   public GX_Controller(IContainer container)
   {
      _container = container;
      _localConfig = container.Get<ParameterConfig>();
      _remoteLocalParamSyncService = container.Get<RemoteLocalParamSyncService>();
      _otherParameterConfig = new Lazy<OtherParameterConfig>(() => container.Get<OtherParameterConfig>());
   }

   const string _uploadParamName = "国轩接收工艺参数接口";

   /// <summary>
   /// 接收国轩上传的参数
   /// </summary>
   /// <returns></returns>
   [HttpPost("/api/tpm/produce/uploadParam")]
   [ProducesResponseType(typeof(GX_ApiResult<string>), 200)] // 告诉 Swagger 成功时返回什么
   [ProducesResponseType(typeof(GX_ApiResult<object>), 400)] // 告诉 Swagger 失败时返回什么
   [Languages(_uploadParamName)]
   public async Task<IActionResult> UploadParam([FromBody] UploadParamRequestDto param)
   {
      WebLogModel webLogInfo = _uploadParamName.GetWebLogInfo(Request);
      try
      {
         if (param == null)
            return BadRequest(GX_ApiResult<object>.Fail("", "参数不全/参数错误"));

         webLogInfo.TraceId = param.traceID;
         webLogInfo.RequestedMsg = JsonSerializer.Serialize(param);

         StringBuilder stringBuilder = new StringBuilder();
         if (string.IsNullOrEmpty(param.traceID))
            stringBuilder.Append("对账ID不能为空；");
         if (string.IsNullOrEmpty(param.equipCode))
            stringBuilder.Append("设备编码不能为空；");
         if (param.paramList == null || param.paramList.Count == 0)
            stringBuilder.Append("参数列表不能为空；");

         if (stringBuilder.Length > 0)
            return BadRequest(GX_ApiResult<object>.Fail(param.traceID ?? "", stringBuilder.ToString()));

         if (_localConfig.DeviceParameter.DeviceCode != param.equipCode)
            return BadRequest(GX_ApiResult<object>.Fail(param.traceID, "设备编码不对应！"));

         var downloadParams = param.paramList.Select(x => new DownloadParamItem(x.paramCode, x.paramName, x.value));
         var downloadResult = await _remoteLocalParamSyncService.MesDownloadParamAsync(downloadParams.ToList());

         if (downloadResult.status)
         {
            webLogInfo.Status = StatusTypeEnum.成功;
            webLogInfo.Level = LogNet.Enums.Log4NetLevelEnum.成功;
            return Ok(GX_ApiResult.OK(param.traceID));
         }
         else
         {
            return BadRequest(GX_ApiResult<object>.Fail(param.traceID, downloadResult.msg));
         }
      }
      catch (Exception ex)
      {
         //string traceID = param switch
         //{
         //    { traceID: string id } => id,// 如果 param 不为 null 且 traceID 不为 null，拿出来给 id
         //    _ => ""
         //};
         // 返回异常
         return StatusCode(500, GX_ApiResult<object>.Fail(webLogInfo.TraceId, ex.Message, null));
      }
      finally
      {
         webLogInfo.EndTime = DateTime.Now;
         webLogInfo.LogWeb(_otherParameterConfig.Value.CurrentLanguagesDictionary);
      }
   }
}
