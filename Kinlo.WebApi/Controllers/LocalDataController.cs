namespace Kinlo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")] // 占位路由，运行时会被 DynamicRouteConvention 替换
[Languages("本地数据交互接口", IsScanMethod = true)]
[Tags("本地数据交互接口")] //给Swagger分类以及过滤控制器
public class LocalDataController : ControllerBase
{
   ParameterConfig _parameterConfig;
   WebApiConfig _webApiConfig;
   IContainer _container;
   Lazy<IBatteryCache> _batteryCache;
   Lazy<OtherParameterConfig> _otherParameterConfig;
   Lazy<DbHelper> _sugarDB;

   public LocalDataController(IContainer container)
   {
      _container = container;
      _parameterConfig = container.Get<ParameterConfig>();
      _webApiConfig = container.Get<WebApiConfig>();
      _batteryCache = new Lazy<IBatteryCache>(() => container.Get<IBatteryCache>());
      _sugarDB = new Lazy<DbHelper>(() => container.Get<DbHelper>());
      _otherParameterConfig = new Lazy<OtherParameterConfig>(() => container.Get<OtherParameterConfig>());
   }

   #region  更新补液量接口
   public record UpdateReplenishVolumeDto(string traceId, string barcode, double replenishWeight);

   public record ReplenishVolumeResultDto(string barcode, IBatMainModel data);

   const string _updateReplenishVolumeName = "更新补液量接口";

   /// <summary>
   /// 更新补液量接口
   /// </summary>
   /// <returns></returns>
   [HttpPost("/product/updateReplenishVolume")]
   [ProducesResponseType(typeof(ApiResponse<ReplenishVolumeResultDto>), 200)] // 告诉 Swagger 成功时返回什么
   [ProducesResponseType(typeof(ApiResponse<ReplenishVolumeResultDto>), 400)] // 告诉 Swagger 失败时返回什么
   [Languages(_updateReplenishVolumeName)]
   public async Task<IActionResult> UpdateReplenishVolume([FromBody] UpdateReplenishVolumeDto request)
   {
      var deviceName = _parameterConfig.DeviceParameter.DeviceName;
      var process = _parameterConfig.AdvancedConfig.ProductionType.ToString();
      WebLogModel webLogInfo = _updateReplenishVolumeName.GetWebLogInfo(Request);
      ;

      try
      {
         if (request == null)
         {
            return BadRequest(ApiResponse.Fail("", process, deviceName, "参数错误"));
         }
         webLogInfo.TraceId = request.traceId;
         webLogInfo.Barcode = request.barcode ?? "";
         webLogInfo.RequestedMsg = JsonSerializer.Serialize(request);
         if (string.IsNullOrWhiteSpace(request.barcode))
         {
            return BadRequest(ApiResponse.Fail(webLogInfo.TraceId, process, deviceName, "参数无电池条码"));
         }

         string runlogHeader = $"[{_updateReplenishVolumeName}]-[{request.barcode}]";

         //取电池（2个月区间）
         var battery = await _batteryCache.Value.GetByBarcodeAsync(request.barcode, runlogHeader);
         if (battery == null) //无电池返回
         {
            return BadRequest(ApiResponse.Fail(webLogInfo.TraceId, process, deviceName, $"未找到条码：{request.barcode} 相关电池！"));
         }

         //更新注液量
         var replenishRs = battery.UpdateManualRefill(request.replenishWeight, _parameterConfig, runlogHeader);
         if (!replenishRs.state)
         {
            return BadRequest(ApiResponse.Fail(webLogInfo.TraceId, process, deviceName, $"条码：{request.barcode} {replenishRs.errMsg}"));
         }

         #region 更新数据库
         if (!await _sugarDB.Value.UpdateByObjectAsync(battery, runlogHeader))
         {
            ((IBatWeightAfterModel)battery).FinalInjectResult = ResultTypeEnum.保存数据库失败;
            $"[{_updateReplenishVolumeName}]条码：{battery.Barcode},重量：{request.replenishWeight},保存失败".LogProcess(runlogHeader);
            Growl.Warning("保存失败！");
         }
         else
         {
            $"[{_updateReplenishVolumeName}]条码：{battery.Barcode},重量：{request.replenishWeight},保存成功".LogProcess(runlogHeader);
            Growl.Success("保存成功！");
         }
         #endregion

         #region 发送MES
         //if (batteryAfterWeight.InjectResult == ResultTypeEnum.OK) // OK上传MES
         //{
         //    var mesService = _container.Get<MesService>();
         //    await MesOutboundHelper.ProductionMesOutput(_container, mesService, bat, logHeader);
         //}
         #endregion

         var data = new ReplenishVolumeResultDto(request.barcode, battery);
         var result = ApiResponse<ReplenishVolumeResultDto>.OK(webLogInfo.TraceId, process, deviceName, data);
         webLogInfo.Status = StatusTypeEnum.成功;
         webLogInfo.Level = LogNet.Enums.Log4NetLevelEnum.成功;
         return Ok(result);
      }
      catch (Exception ex)
      {
         // 返回异常
         return StatusCode(500, ApiResponse.Fail(webLogInfo.TraceId, process, deviceName, ex.Message));
      }
      finally
      {
         webLogInfo.EndTime = DateTime.Now;
         webLogInfo.LogWeb(_otherParameterConfig.Value.CurrentLanguagesDictionary);
      }
   }
   #endregion

   #region 通过条码（模糊查询）获取电池信息接口
   public record GetBatteryByBarcodeDto(string traceId, string barcode, int days);

   public record GetBatteryByBarcodeResultDto(string barcode, IReadOnlyList<string> data);

   const string _getBatterysByBarcodeName = "模糊查询电池信息接口";

   /// <summary>
   /// 模糊查询电池信息接口
   /// </summary>
   /// <returns></returns>
   [HttpPost("/product/getBatterysByBarcode")]
   [ProducesResponseType(typeof(ApiResponse<GetBatteryByBarcodeResultDto>), 200)] // 告诉 Swagger 成功时返回什么
   [ProducesResponseType(typeof(ApiResponse<GetBatteryByBarcodeResultDto>), 400)] // 告诉 Swagger 失败时返回什么
   [Languages(_getBatterysByBarcodeName)]
   public async Task<IActionResult> GetBatterysByBarcode([FromBody] GetBatteryByBarcodeDto request)
   {
      var deviceName = _parameterConfig.DeviceParameter.DeviceName;
      var process = _parameterConfig.AdvancedConfig.ProductionType.ToString();
      WebLogModel webLogInfo = _getBatterysByBarcodeName.GetWebLogInfo(Request);

      try
      {
         if (request == null)
         {
            return BadRequest(ApiResponse.Fail("", process, deviceName, "参数错误"));
         }
         webLogInfo.TraceId = request.traceId;
         webLogInfo.Barcode = request.barcode ?? "";
         webLogInfo.RequestedMsg = JsonSerializer.Serialize(request);
         if (string.IsNullOrWhiteSpace(request.barcode))
         {
            return BadRequest(ApiResponse.Fail(webLogInfo.TraceId, process, deviceName, "参数无电池条码"));
         }
         string runLogHeader = $"[{_getBatterysByBarcodeName}]-[{request.barcode}]";

         var batterys = await _sugarDB.Value.GetProcessByBarcodeFuzzyAsync(request.barcode, runLogHeader, request.days);
         if (batterys == null || batterys.Count == 0) //无电池返回
         {
            return BadRequest(ApiResponse.Fail(webLogInfo.TraceId, process, deviceName, $"未找到条码：{request.barcode} 相关电池！"));
         }

         var data = new GetBatteryByBarcodeResultDto(request.barcode, batterys.Select(x => x.Barcode).ToList());
         var result = ApiResponse<GetBatteryByBarcodeResultDto>.OK(webLogInfo.TraceId, process, deviceName, data);
         webLogInfo.Status = StatusTypeEnum.成功;
         webLogInfo.Level = LogNet.Enums.Log4NetLevelEnum.成功;
         return Ok(result);
      }
      catch (Exception ex)
      {
         // 返回异常
         return StatusCode(500, ApiResponse.Fail(webLogInfo.TraceId, process, deviceName, ex.Message));
      }
      finally
      {
         webLogInfo.EndTime = DateTime.Now;
         webLogInfo.LogWeb(_otherParameterConfig.Value.CurrentLanguagesDictionary);
      }
   }
   #endregion
}
