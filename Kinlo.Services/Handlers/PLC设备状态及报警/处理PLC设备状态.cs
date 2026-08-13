namespace Kinlo.Services.Handlers;

public partial class PLcStatusAndAlarmHandler
{
    /// <summary>
    /// 设备状态改变发给MES
    /// </summary>
    private async Task DeviceStatusChangAsync(short newStatus)
    {
        try
        {
            var status = newStatus.ToDeviceState();
            var manualStatus = status.Any(x => x == DeviceStateEnum.待机) ? "0" : "1";
            var runStatus = status.Any(x => x != DeviceStateEnum.运行) ? "0" : "1";
            var waitStatus = status.Any(x => x != DeviceStateEnum.待机) ? "0" : "1";
            var faultStatus = status.Any(x => x == DeviceStateEnum.报警) ? "1" : "0";
            var repairStatus = "0";
            var stopStatus = "0";
            var equipSign = status switch
            {
                var s when s.Any(x => x == DeviceStateEnum.运行) => "1",
                var s when s.Any(x => x == DeviceStateEnum.待机) => "2",
                _ => "0",
            };
            var warningStatus = status.Any(x => x == DeviceStateEnum.报警) ? "1" : "0";
            var args = new MesRequestBuildNJGX.ArgsDeviceStatus(
               manualStatus, runStatus, waitStatus, faultStatus, repairStatus, stopStatus, equipSign, warningStatus);
            var call = _mesInterfaceParameterConfig.GetApiCall(args);
            if (call != null && call.IsEnable)
            {
                await _mesService.SendAsync(call, "", receiveMes => receiveMes.MesCommonParse(_taskLogHeader));
            }
        }
        catch (Exception ex)
        {
            $"设备状态改变发给MES异常：{ex}".LogProcess(_taskLogHeader);
        }
    }
}
