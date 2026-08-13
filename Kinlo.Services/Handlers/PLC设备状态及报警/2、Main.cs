using KinloControls;

namespace Kinlo.Services.Handlers;

public partial class PLcStatusAndAlarmHandler
{
    /// <summary>
    /// 处理设备状态
    /// </summary>
    /// <param name="stateType"></param>
    /// <param name="oldPlcStatus"></param>
    /// <param name="newPlcStatus"></param>
    /// <returns></returns>
    public async Task PlcStatusHandleAsync(DeviceStateEnum stateType, bool oldPlcStatus, bool newPlcStatus)
    {
        try
        {
            if (!_plcStatusConfig.PlcStatusDisplayDic.TryGetValue(stateType, out var statusDescription))
            {
                $"未配置当前状态[{stateType}]".LogProcess(_taskLogHeader);
                return;
            }
            var now = DateTime.Now;
            if (newPlcStatus)
            {
                if (!oldPlcStatus)
                {
                    string stopReason = stateType == DeviceStateEnum.报警 ? await StartAlarmHandleAsync() : stateType.ToString();
                    var item = CreateTimeLine(stateType, statusDescription, now, stopReason);
                    if (stateType == DeviceStateEnum.待机)
                    {
                        await StartManualShutdownHandleAsync(item);
                    }
                    else if (stateType is DeviceStateEnum.堵料 or DeviceStateEnum.待料)
                    {
                        await StartMaterialShortageHandleAsync(item);
                    }
                    await _plcStatusConfig.AddTimeline(item);
                }
                else
                {
                    var lastItem = _plcStatusConfig.GetTimelineLastOrDefault(x => x.Value == (int)stateType);
                    if (lastItem != null && (now - lastItem.EndTime) > TimeSpan.FromSeconds(3))
                        await UIThreadHelper.InvokeOnUiThreadAsync(() => lastItem.EndTime = now);
                    if (stateType == DeviceStateEnum.报警)
                        await PendingAlarmHandleAsync();
                }
            }
            else
            {
                if (oldPlcStatus)
                {
                    var lastItem = _plcStatusConfig.GetTimelineLastOrDefault(x => x.Value == (int)stateType);
                    if (lastItem != null)
                    {
                        if (stateType == DeviceStateEnum.报警)
                        {
                            $"进入报警".LogRun(Log4NetLevelEnum.信息);
                            await EndAalrmHandleAsync();
                        }
                        else if (stateType == DeviceStateEnum.待机)
                        {
                            $"进入待机结束".LogRun(Log4NetLevelEnum.信息);
                            await EndManualShutdownHandleAsync(lastItem);
                        }
                        else if (stateType is DeviceStateEnum.堵料 or DeviceStateEnum.待料)
                        {
                            await EndMaterialShortageHandleAsync(lastItem);
                        }
                        if (_plcStatusConfig.PlcStatusPendingSaveTasks.TryRemove(lastItem.Id, out var task))
                            await task.Invoke(string.Empty);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            $"处理设备状态发生异常：{ex}".LogProcess(_taskLogHeader);
        }
    }

    /// <summary>
    /// 创建时间线及委托保存任务
    /// </summary>
    private TimelineItem CreateTimeLine(
       DeviceStateEnum plcStatus,
       PlcStatusDisplayModel plcStatusDisplay,
       DateTime startTime,
       string stopReason
    )
    {
        var uiItem = new TimelineItem
        {
            Id = _snowflakeHelper.NextId(),
            Value = (int)plcStatus,
            StartTime = startTime,
            EndTime = startTime.AddMilliseconds(100),
            Label = plcStatusDisplay.Description,
            Color = plcStatusDisplay.Color,
            Message = stopReason,
        };
        _plcStatusConfig.PlcStatusPendingSaveTasks.TryAdd(
           uiItem.Id,
           async msg =>
           {
               await UIThreadHelper.InvokeOnUiThreadAsync(() =>
               {
                   uiItem.EndTime = DateTime.Now;
                   if ((DeviceStateEnum)uiItem.Value == DeviceStateEnum.待机
                    && string.IsNullOrWhiteSpace(uiItem.Message)
                    && !string.IsNullOrWhiteSpace(msg))
                       uiItem.Message = msg;
               });
               var entity = ToPlcStatusSaveData(uiItem);
               await _sugarDB.InsertableAsync(entity, _taskLogHeader);
           }
        );
        return uiItem;
    }

    public PlcStatusModel ToPlcStatusSaveData(TimelineItem timeline)
    {
        return new PlcStatusModel
        {
            Id = timeline.Id,
            Shift = timeline.StartTime.GetShiftByTime(_parameterConfig),
            Status = (DeviceStateEnum)timeline.Value,
            StartTime = timeline.StartTime,
            EndTime = timeline.EndTime,
            Msg = timeline.Message.Length > 254 ? timeline.Message.Substring(0, 254) : timeline.Message,
        };
    }
}
