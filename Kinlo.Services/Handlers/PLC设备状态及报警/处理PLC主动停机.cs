using KinloControls;

namespace Kinlo.Services.Handlers;

public partial class PLcStatusAndAlarmHandler
{
    /// <summary>主动停机开始 0</summary>
    private async Task StartManualShutdownHandleAsync(TimelineItem shutdown)
    {
        if (!_manualShutdownUuidPool.TryGetValue(shutdown.Id, out string bizUuid))
        {
            bizUuid = Guid.NewGuid().ToString("D");
            
        }
        var args = new MesRequestBuildNJGX.ArgsActiveShutdown(0, bizUuid, "", shutdown.StartTime, null, string.Empty);
        var call = _mesInterfaceParameterConfig.GetApiCall(args);
        if (call != null && call.IsEnable)
        {
            await _mesService.SendAsync(call, "", receiveMes => receiveMes.MesCommonParse(_taskLogHeader));
            _manualShutdownUuidPool.TryAdd(shutdown.Id, bizUuid);
        }
    }

    /// <summary>主动停机结束 1</summary>
    private async Task EndManualShutdownHandleAsync(TimelineItem shutdown)
    {
        try
        {
            string stopReason = string.Empty;
            if (_shutdownAddress == null)
            {
                stopReason = "未配置主动停机读取标签,无法读取主动停机原因；";
                stopReason.LogProcess(_taskLogHeader);
            }
            else
            {
                var res = _plc.ReadValue<short>(_shutdownAddress, _taskLogHeader);
                if (res.Status == DeviceStatus.Success)
                {
                    // 修复：short转字符串用Enum.TryParse，解决枚举底层类型不一致异常
                    string codeStr = res.Value.ToString();
                    if (Enum.TryParse<PlcStopReasonTypeEnum>(codeStr, out PlcStopReasonTypeEnum reasonEnum))
                    {
                        stopReason = reasonEnum.ToString();
                    }
                    else
                    {
                        stopReason = $"停机编码异常：{res.Value}";
                    }
                }
                else
                {
                    stopReason = $"读取停机地址失败：{res.ErrorMessage}";
                }
            }
            await UIThreadHelper.InvokeOnUiThreadAsync(() =>
            {
                shutdown.EndTime = DateTime.Now;
                shutdown.Message = stopReason;
            });
            var span = Math.Round((shutdown.EndTime - shutdown.StartTime).TotalSeconds);
            if (!_manualShutdownUuidPool.TryGetValue(shutdown.Id, out string bizUuid))
            {
                bizUuid = Guid.NewGuid().ToString("D");
            }
            var args = new MesRequestBuildNJGX.ArgsActiveShutdown(
               1, bizUuid, shutdown.Message, shutdown.StartTime, shutdown.EndTime, span.ToString());
            var call = _mesInterfaceParameterConfig.GetApiCall(args);
            if (call != null && call.IsEnable)
            {
                await _mesService.SendAsync(call, "", receiveMes => receiveMes.MesCommonParse(_taskLogHeader));
                _manualShutdownUuidPool.TryRemove(shutdown.Id, out _);
            }
        }
        catch (Exception ex)
        {
            $"主动停机：{ex.ToString()}".LogRun(Log4NetLevelEnum.信息);
        }
    }
}
