using KinloControls;

namespace Kinlo.Services.Handlers;

public partial class PLcStatusAndAlarmHandler
{
    /// <summary>堵料待料开始 0</summary>
    private async Task StartMaterialShortageHandleAsync(TimelineItem shutdown)
    {
        //【修复 堵料待料独立缓存】
        if (!_materialShortageUuidPool.TryGetValue(shutdown.Id, out string bizUuid))
        {
            bizUuid = Guid.NewGuid().ToString("D");
            _materialShortageUuidPool.TryAdd(shutdown.Id, bizUuid);
        }
        var args = new MesRequestBuildNJGX.ArgsMaterialShortage(
           0, bizUuid, shutdown.Message, shutdown.StartTime, null, string.Empty);
        var call = _mesInterfaceParameterConfig.GetApiCall(args);
        if (call != null && call.IsEnable)
        {
            await _mesService.SendAsync(call, "", receiveMes => receiveMes.MesCommonParse(_taskLogHeader));
        }
    }

    /// <summary>堵料待料结束 1</summary>
    private async Task EndMaterialShortageHandleAsync(TimelineItem shutdown)
    {
        await UIThreadHelper.InvokeOnUiThreadAsync(() => shutdown.EndTime = DateTime.Now);
        var span = Math.Round((shutdown.EndTime - shutdown.StartTime).TotalSeconds);
        //【修复 堵料待料独立缓存】
        if (!_materialShortageUuidPool.TryGetValue(shutdown.Id, out string bizUuid))
        {
            bizUuid = Guid.NewGuid().ToString("D");
        }
        var args = new MesRequestBuildNJGX.ArgsMaterialShortage(
           1, bizUuid, shutdown.Message, shutdown.StartTime, shutdown.EndTime, span.ToString());
        var call = _mesInterfaceParameterConfig.GetApiCall(args);
        if (call != null && call.IsEnable)
        {
            await _mesService.SendAsync(call, "", receiveMes => receiveMes.MesCommonParse(_taskLogHeader));
            _materialShortageUuidPool.TryRemove(shutdown.Id, out _);
        }
    }
}
