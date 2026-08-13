using Dm.util;

namespace Kinlo.Services.Handlers;

public partial class PLcStatusAndAlarmHandler
{
   /// <summary>
   /// 报警状态开始，旧状态无报警，设备由稳定运行转为报警中，记录报警原因
   /// </summary>
   /// <returns></returns>
   private async Task<string> StartAlarmHandleAsync()
   {
      var currentAlarm = ReadCurrentAlarm(_taskLogHeader); //读取当前报警

      if (!currentAlarm.Status)
         return currentAlarm.msg;
      var alarmInfo = await HandleAlarmAsync(currentAlarm.Info, true);

      string stopReason = string.Empty; //报警原因
      foreach (var newAlarmItem in alarmInfo.newAlarms)
      {
         if (newAlarmItem.PlcAalrmLevel == PlcAalrmLevelEnum.提示)
            continue;

         if (stopReason.Length + newAlarmItem.AlarmMessage.Length < 254)
            stopReason += $"{newAlarmItem.AlarmMessage}；";

         #region 只有报警最初上传MES
         await AlarmStartSendMesAsync(newAlarmItem);
         #endregion
      }
      if (string.IsNullOrWhiteSpace(stopReason))
      {
         $"PLC有新报警状态，未取到报警！".LogProcess(_taskLogHeader);
      }
      return stopReason;
   }

   /// <summary>
   /// 报警状态持续，在持续报警过程中如果有新报警或结束的报警只用来显示UI及保存，
   /// 不上传MES
   /// </summary>
   /// <returns></returns>
   private async Task PendingAlarmHandleAsync()
   {
      var currentAlarm = ReadCurrentAlarm(_taskLogHeader); //读取当前报警
      if (!currentAlarm.Status)
         return;

      var alarminfo = await HandleAlarmAsync(currentAlarm.Info, false);
       foreach (var StopitemItem in alarminfo.stopAlarms)
        {
            if (StopitemItem.PlcAalrmLevel == PlcAalrmLevelEnum.提示)
                continue;
            await AlarmEndSendMesAsync(StopitemItem);
       }
   }

   /// <summary>
   /// 报警状态结束
   /// </summary>
   /// <returns></returns>
   private async Task EndAalrmHandleAsync()
   {
      List<PlcAlarmModel> runClearAlarm = new();
      var endedAlarms = _plcStatusConfig.PlcCurrentAlarmTasks.Values.ToList();

      foreach (var task in endedAlarms)
      {
         var alarm = await task.func.Invoke(); //结束了的报警
         runClearAlarm.Add(alarm);

         #region 上传MES
         await AlarmEndSendMesAsync(alarm); //只在状态结束时才上传MES
         #endregion
      }

      if (runClearAlarm.Count > 0)
      {
         $"PLC转运行时消警：{AlarmRecordToString(runClearAlarm)}".LogProcess(_taskLogHeader);
      }
      _plcStatusConfig.PlcCurrentAlarmTasks.Clear();
      _ = _plcStatusConfig.DisplayPlcAlarmsCallback?.Invoke(null); //UI显示,参数为null是代表设备运行了
   }

   /// <summary>
   /// 提取报警并创建保存任务
   /// </summary>
   /// <param name="currentAlarm"></param>
   /// <param name="isStart">是否新报警</param>
   /// <returns></returns>
   private async Task<(List<PlcAlarmModel> newAlarms, List<PlcAlarmModel> stopAlarms)> HandleAlarmAsync(
      List<(PlcAalrmLevelEnum PlcAalrmLevel, string tag)> currentAlarm,
      bool isStart
   )
   {
      #region 新的报警
      List<PlcAlarmModel> newAlarmRecords = new();
      foreach (var item in currentAlarm)
      {
         if (!_plcStatusConfig.PlcAlarmInfoDic.TryGetValue(item.tag, out var value))
         {
            $"有报警但未来在导入的PLC报警列表中找到相关信息；Tag：[{item.tag}]，等级：[{item.PlcAalrmLevel}]；".LogProcess(_taskLogHeader);
            continue;
         }

         if (!_plcStatusConfig.PlcCurrentAlarmTasks.TryGetValue(item.tag, out _)) //有新报警
         {
            var newAlarmTask = CreatePlcAlarm(item.tag, item.PlcAalrmLevel, value.AlarmMessage, value.MesCode); //创建报警信息
            _plcStatusConfig.PlcCurrentAlarmTasks.TryAdd(item.tag, newAlarmTask); //添加报警保存任务
            newAlarmRecords.Add(newAlarmTask.alarm);
         }
         else
         {
            if (isStart)
            {
               $"PLC报警状态开始，但旧报警未消除；Tag：[{item.tag}]，等级：[{item.PlcAalrmLevel}]，报警信息：[{value.AlarmMessage}]；".LogProcess(
                  _taskLogHeader
               );
            }
         }
      }
      #endregion

      #region 结束的报警
      List<PlcAlarmModel> stopAlarmRecords = new();
      var keys = _plcStatusConfig.PlcCurrentAlarmTasks.Keys.ToList();
      foreach (var key in keys)
      {
         if (!currentAlarm.Any(x => x.tag == key)) //报警消除
         {
            if (!_plcStatusConfig.PlcCurrentAlarmTasks.TryRemove(key, out var task))
               continue;

            var endAlarm = await task.func.Invoke(); //结束报警，保存及更新UI ;
            stopAlarmRecords.Add(endAlarm);
         }
      }

      if (newAlarmRecords.Count > 0)
      {
         $"PLC新报警：{AlarmRecordToString(newAlarmRecords)}".LogProcess(_taskLogHeader);
      }
      if (stopAlarmRecords.Count > 0)
      {
         $"PLC消警：{AlarmRecordToString(stopAlarmRecords)}".LogProcess(_taskLogHeader);
      }
      #endregion
      return (newAlarmRecords, stopAlarmRecords);
   }

   string AlarmRecordToString(List<PlcAlarmModel> alarms) =>
      string.Join('；', alarms.Select(x => $"等级：{x.PlcAalrmLevel}，Tag：{x.PlcTag}，内容：{x.AlarmMessage}"));

   #region 读取PLC报警
   /// <summary>
   /// 块大小
   /// </summary>
   const ushort chunkSize = 50;

   private record AlarmInfoRecord(bool Status, List<(PlcAalrmLevelEnum PlcAalrmLevel, string tag)> Info, string msg);

   /// <summary>
   /// 读取PLC实时报警
   /// </summary>
   /// <param name="logHeader"></param>
   /// <returns>返回以Tag为key的报警集合</returns>
   private AlarmInfoRecord ReadCurrentAlarm(string logHeader)
   {
      var currentAlarms = new List<(PlcAalrmLevelEnum PlcAalrmLevel, string tag)>();

      //  $"未配置报警读取标签，退出设备报警！".LogProcess(_taskLogHeader);
      if (string.IsNullOrEmpty(Context.DataAddress.Lable))
      {
         var stopReason = "未配置报警读取标签,无法读取设备报警";
         stopReason.LogProcess(_taskLogHeader);
         return new AlarmInfoRecord(false, currentAlarms, stopReason);
      }
      try
      {
         List<PlcAlarmDut> productAlarms = new List<PlcAlarmDut>();
         uint readIndex = 0;
         while (Context.DataLength > readIndex)
         {
            ushort currentReadLength = (ushort)Math.Min(chunkSize, Context.DataLength - readIndex);
            //此处为常规报警代码，因PLC报警版本不统一需注意路径是否正确
            //var alarmTag = new SignalAddressModel($"{Context.DataAddress.Lable}.ST[{readIndex}]") { Length = readLength };
            var alarmTag = new SignalAddressModel($"{Context.DataAddress.Lable}[{readIndex}]") { Length = currentReadLength };
            var deviceRs = _plc.ReadLargeObjects<PlcAlarmDut>(alarmTag, ""); //正常生产报警
            if (deviceRs.Status == DeviceStatus.Success)
               productAlarms.AddRange(deviceRs.Value!);
            readIndex += currentReadLength;
         }
         if (!productAlarms.Any())
            return new AlarmInfoRecord(false, currentAlarms, "未取到任务报警信息！");

         for (int i = 0; i < productAlarms.Count; i++)
         {
            for (int r = 0; r < productAlarms[i].Alarm.Length; r++)
            {
               if (productAlarms[i].Alarm[r])
               {
                  currentAlarms.Add((PlcAalrmLevelEnum.报警, $"[{i}].Alarm[{r}]"));
               }
            }
            for (int r = 0; r < productAlarms[i].Warning.Length; r++)
            {
               if (productAlarms[i].Warning[r])
               {
                  currentAlarms.Add((PlcAalrmLevelEnum.警告, $"[{i}].Warning[{r}]"));
               }
            }
            for (int r = 0; r < productAlarms[i].Tip.Length; r++)
            {
               if (productAlarms[i].Tip[r])
               {
                  currentAlarms.Add((PlcAalrmLevelEnum.提示, $"[{i}].Tip[{r}]"));
               }
            }
         }

         #region 因PLC报警版本不统一，如无下列报警的项目可以不用读取
         if (false)
         {
            var doorAlarm = _plc.ReadClass<DoorAlarm>(new SignalAddressModel($"{Context.DataAddress.Lable}.Door"), null, ""); //门禁
            var estopAlarm = _plc.ReadClass<EstopAlarm>(new SignalAddressModel($"{Context.DataAddress.Lable}.Estop"), null, ""); //急停
            var instrument = _plc.ReadLargeObjects<bool>(new SignalAddressModel($"{Context.DataAddress.Lable}.Instrument"), ""); //设备仪器
            var communicationAlarm = _plc.ReadLargeObjects<bool>(new SignalAddressModel($"{Context.DataAddress.Lable}.Communication"), ""); //通讯
            if (doorAlarm.Status == DeviceStatus.Success)
               for (int i = 0; i < doorAlarm.Value.Doors.Length; i++)
               {
                  for (int r = 0; r < doorAlarm.Value.Doors[i].Alarms.Length; r++)
                  {
                     if (doorAlarm.Value.Doors[i].Alarms[r])
                     {
                        currentAlarms.Add((PlcAalrmLevelEnum.报警, $"[{i}].Door[{r}]"));
                     }
                  }
               }
            if (estopAlarm.Status == DeviceStatus.Success)
               for (int i = 0; i < estopAlarm.Value.Estops.Length; i++)
               {
                  for (int r = 0; r < estopAlarm.Value.Estops[i].Alarms.Length; r++)
                  {
                     if (estopAlarm.Value.Estops[i].Alarms[r])
                     {
                        currentAlarms.Add((PlcAalrmLevelEnum.报警, $"[{i}].Estop[{r}]"));
                     }
                  }
               }
            if (instrument.Status == DeviceStatus.Success)
               for (int i = 0; i < instrument.Value.Count; i++)
               {
                  if (instrument.Value[i])
                  {
                     currentAlarms.Add((PlcAalrmLevelEnum.报警, $"Instrument[{i}]"));
                  }
               }
            if (communicationAlarm.Status == DeviceStatus.Success)
               for (int i = 0; i < communicationAlarm.Value.Count; i++)
               {
                  if (communicationAlarm.Value[i])
                  {
                     currentAlarms.Add((PlcAalrmLevelEnum.报警, $"Communication[{i}]"));
                  }
               }
         }
         #endregion

         return new AlarmInfoRecord(true, currentAlarms, "");
      }
      catch (Exception ex)
      {
         ex.ToString().LogProcess(logHeader, Log4NetLevelEnum.错误);
         return new AlarmInfoRecord(false, currentAlarms, ex.toString());
      }
   }
   #endregion

   /// <summary>
   /// 创建报警实例、UI委托及保存委托
   /// </summary>
   /// <param name="alarmTag"></param>
   /// <param name="alarmMsg"></param>
   /// <param name="mesCode"></param>
   /// <returns></returns>
   private (PlcAlarmModel alarm, Func<Task<PlcAlarmModel>> func) CreatePlcAlarm(
      string alarmTag,
      PlcAalrmLevelEnum PlcAalrmLevel,
      string alarmMsg,
      string mesCode
   )
   {
      var saveAlarm = new PlcAlarmModel
      {
         Id = _snowflakeHelper.NextId(),
         AlarmMessage = alarmMsg,
         PlcAalrmLevel = PlcAalrmLevel,
         MesCode = mesCode,
         PlcTag = alarmTag,
         StartTime = DateTime.Now,
      };
      _ = _plcStatusConfig.DisplayPlcAlarmsCallback?.Invoke(saveAlarm); //UI显示
      //保存数据库委托
      var func = new Func<Task<PlcAlarmModel>>(async () =>
      {
         await UIThreadHelper.InvokeOnUiThreadAsync(() => saveAlarm.EndTime = DateTime.Now);

         _ = _plcStatusConfig.DisplayPlcAlarmsCallback?.Invoke(saveAlarm); //UI显示
         await _sugarDB.InsertableAsync(saveAlarm, _taskLogHeader);
         return saveAlarm;
      });

      return (saveAlarm, func);
   }

    #region 报警上传MES
    /// <summary>
    /// 报警开始上传MES
    /// </summary>
    /// <param name="alarm"></param>
    /// <returns></returns>
    private async Task AlarmStartSendMesAsync(PlcAlarmModel alarm)
    {
        if (alarm.PlcAalrmLevel == PlcAalrmLevelEnum.提示) //提示不上传MES
            return;
        if (!_alarmBizUuidPool.TryGetValue(alarm.Id, out string sn))
        {
            sn = Guid.NewGuid().ToString("D");
        }
        var args = new MesRequestBuildNJGX.ArgsPassiveShutdown(0, sn, alarm.MesCode, alarm.StartTime, null, null);
        var call = _mesInterfaceParameterConfig.GetApiCall(args);
        if (call != null && call.IsEnable) //有接口并已启用
        {
            var inputResult = await _mesService.SendAsync(call, "", receiveMes => receiveMes.MesCommonParse(_taskLogHeader));
            if (inputResult.ResultStatus == MesResultStatusEnum.成功)
            {
                alarm.SendMesStatus = SendMesStatusEnum.只上传了报警开始;
                _alarmBizUuidPool.TryAdd(alarm.Id, sn);
            }
        }
        //else
        //{
        //    $"未找到MES报警接口或未接口未启用，不上传送报警！".LogProcess(_taskLogHeader);
        //}
    }

   /// <summary>
   /// 报警结束上传MES
   /// </summary>
   /// <param name="alarm"></param>
   /// <returns></returns>
   private async Task AlarmEndSendMesAsync(PlcAlarmModel alarm)
   {
      if (alarm.PlcAalrmLevel == PlcAalrmLevelEnum.提示) //提示不上传MES
         return;

      if (alarm.SendMesStatus == SendMesStatusEnum.只上传了报警开始)
      {
            if (!_alarmBizUuidPool.TryGetValue(alarm.Id, out string sn))
            {
                sn = Guid.NewGuid().ToString("D");
                $"报警兜底流水 {sn}".LogProcess(_taskLogHeader, Log4NetLevelEnum.警告);
            }
            var args = new MesRequestBuildNJGX.ArgsPassiveShutdown(
            1,
            sn,
            alarm.MesCode,
            alarm.StartTime,
            alarm.EndTime,
            alarm.AlarmDurationSeconds
         );

         var call = _mesInterfaceParameterConfig.GetApiCall(args);
         if (call != null && call.IsEnable) //有接口并已启用
         {
            var inputResult = await _mesService.SendAsync(call, "", receiveMes => receiveMes.MesCommonParse(_taskLogHeader));
                //if (inputResult.ResultStatus == MesResultStatusEnum.成功)
                //{
                    alarm.SendMesStatus = SendMesStatusEnum.开始及完成都有上传;
                    _alarmBizUuidPool.TryRemove(alarm.Id, out _);
                //}
         }
         //else
         //{
         //    $"未找到MES报警接口或未接口未启用，不上传送报警！".LogProcess(_taskLogHeader);
         //}
      }
   }
   #endregion
}

public class Model
{
    public int Index { get; set; }
    /// <summary>
    /// 类型
    /// </summary>
    public string Trep { get; set; }


    public string Info { get; set; }
}