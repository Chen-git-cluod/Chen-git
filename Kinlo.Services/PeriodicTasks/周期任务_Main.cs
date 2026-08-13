using Kinlo.MESDocking.Ftp;

namespace Kinlo.Services.PeriodicTasks;

/// <summary>
/// 周期任务
/// </summary>
public partial class PeriodicTasksHelper
{
   private IContainer _container;
   private MesService _mesService;
   private MqttHelper _mqttHelper;
   private DisplayDataCollection _displayDataCollection;
   private ParameterConfig _parameterConfig;
   private UsersStatusConfig _usersStatus;
   private GlobalStaticTemporary _globalStaticTemporary;
   private MesInterfaceParameterConfig _mesInterfaceParameterConfig;
   private OtherParameterConfig _otherParameterConfig;
   private AppGlobalConfig _appGlobalConfig;
   private ProcessRatioDisplay _processRatioDisplay;
   private FtpService _ftpService;
   private DbHelper _sugarDB;

   /// <summary>
   /// 周期任务
   /// </summary>
   /// <param name="container"></param>
   public PeriodicTasksHelper(IContainer container)
   {
      _container = container;
      _sugarDB = container.Get<DbHelper>();
      _parameterConfig = container.Get<ParameterConfig>();
      _usersStatus = container.Get<UsersStatusConfig>();
      _globalStaticTemporary = container.Get<GlobalStaticTemporary>();
      _mesInterfaceParameterConfig = _container.Get<MesInterfaceParameterConfig>();
      _otherParameterConfig = _container.Get<OtherParameterConfig>();
      _mqttHelper = container.Get<MqttHelper>();
      _mesService = container.Get<MesService>();
      _ftpService = container.Get<FtpService>();
      _appGlobalConfig = container.Get<AppGlobalConfig>();
      _displayDataCollection = container.Get<DisplayDataCollection>();
      _processRatioDisplay = container.Get<ProcessRatioDisplay>();
   }

   public void Start()
   {
      var snowflake = _container.Get<SnowflakeHelper>();
      //ExecuteWithTracking 任务追踪器，如果执行长时间操作任务需加入任务追踪，防止退出程序时中断未完成任务
      //订阅MES补传
      GlobalClockService.Instance.OnMinute += async t =>
         await GlobalClockService.ExecuteWithTracking(
            snowflake.NextId(),
            "MES补传",
            async () => await PollingResendMes(_container, t, true, 50000, 10)
         );

      //订阅删除MES补传成功并超过一定时间的数据
      GlobalClockService.Instance.OnHour += async t =>
         await GlobalClockService.ExecuteWithTracking(
            snowflake.NextId(),
            "清除MES补传过时数据",
            async () => await _container.Get<DbHelper>().RangeDeleteResendAsync(10)
         );

      //订阅仪表轮询
      GlobalClockService.Instance.OnSecond += async t =>
         await GlobalClockService.ExecuteWithTracking(
            snowflake.NextId(),
            "仪表轮询",
            async () => await PollingElectricMeterData(_container, t)
         );

      GlobalClockService.Instance.OnHour += async t => await SyncPlcTimeAsync(_container); //同步PLC时间
      GlobalClockService.Instance.OnSecond += t => MesHeartbeat(_container); //mes心跳
      GlobalClockService.Instance.OnSecond += async t => await SwitchShiftAsync(t); //切换班次
      GlobalClockService.Instance.OnSecond += async t => await ExportPreShiftDataAsync(t); //根据班次导出数据并上传FTP
      GlobalClockService.Instance.OnSecond += async t => await PlcProductionSyncService(t, _container); //发送生产数据至PLC
      GlobalClockService.Instance.OnSecond += async t => await _usersStatus.AutoLogoutTimerTick(t); //自动退出登陆
      GlobalClockService.Instance.OnSecond += async t => await ParamSendMes(t, _container); //参数定时上传MES
      GlobalClockService.Instance.OnSecond += async t => await PollingProcessRatio(t, _container); //实时统计生产工序数据

      GlobalClockService.Instance.Start(); //全局时钟服务启动
   }

   #region 同步PLC时间
   /// <summary>
   /// 每小时同步一次PLC时间
   /// </summary>
   /// <param name="plcs"></param>
   /// <param name="tokenSource"></param>
   /// <returns></returns>
   public static async Task SyncPlcTimeAsync(IContainer container)
   {
      try
      {
         var plcSignalConfig = container.Get<PLCSignalConfig>();
         var address = plcSignalConfig.CustomPlcInteractAddresses.FirstOrDefault(x =>
            x.CustomInteractName == CustomInteractNameEnum.PLC同步PC时间
         );
         if (address == null || !address.IsEnable)
            return;

         var devicesConfig = container.Get<DevicesConfig>();

         var plcs = devicesConfig.GetRunDevices(x => x.DeviceInfo.ProcessesType == ProcessTypeEnum.PLC);
         if (plcs != null && plcs.Count > 0)
         {
            foreach (var item in plcs)
            {
               SyncPlcTimeCroe(item, address);
            }
         }
         else
         {
            var clients = devicesConfig.DeviceList.Where(x => x.ProcessesType == ProcessTypeEnum.PLC);
            foreach (var client in clients)
            {
               await client.WithCreatedDeviceAsync(async d => await Task.Run(() => SyncPlcTimeCroe(d, address)));
            }
         }
      }
      catch (Exception ex)
      {
         $"[同步PLC时间]异常：{ex}！".LogRun(Log4NetLevelEnum.警告);
      }
   }

   private static void SyncPlcTimeCroe(IDevice plc, CustomPlcInteractAddressModel customPlcInteractAddress)
   {
      string logHeader = plc.DeviceInfo.ToDeviceLogHeader();
      // var add = new SignalAddressModel("PC_SyncTime");
      var add = customPlcInteractAddress.DataAddress;
      long time = UnixTimeHelper.GetSyncPlcUnixTimeSeconds();
      if (!plc.WriteValue(time, add, logHeader))
      {
         $"[同步PLC时间]失败！".LogProcess(logHeader, Log4NetLevelEnum.警告);
      }
   }
   #endregion

   #region mes心跳
   public static void MesHeartbeat(IContainer container)
   {
      //if (_parameterConfig.AdvancedConfig.MESStatus == MESStatusEnum.关闭)
      //    return Task.CompletedTask;

      //var _mesService = _container.Get<MesService>();
      //var _request = _mesService.GetRequestMessage(MesInterfaceNameEnum.设备状态, []);
      //while (!tokenSource.IsCancellationRequested)
      //{
      //    try
      //    {
      //        var _rss = await _mesService.SendRequestAsync(MesInterfaceNameEnum.设备状态, _request, "设备在线检测");
      //        if (_rss.Status != HttpResultStatusEnum.成功)
      //        {
      //            $"[MES心跳]失败：{_rss.ErrMsg}".LogRun(Log4NetLevelEnum.警告);
      //        }
      //        await Task.Delay(_parameterConfig.RunParameter.MesHeartbeatInterval);
      //    }
      //    catch (Exception ex)
      //    {
      //        $"[MES心跳]异常:{ex}".LogRun(Log4NetLevelEnum.警告);
      //    }
      //}
   }
   #endregion

   #region 设备产能
   private DateTime? _uploadEquipCapacityTime = null;

   /// <summary>
   /// 设备产能,一分钟一次
   /// </summary>
   /// <returns></returns>
   private Task SendUploadEquipCapacity()
   {
      return Task.Run(async () =>
      {
         while (!GlobalStaticTemporary.GlobalToken.IsCancellationRequested)
         {
            try
            {
               //DateTime _currentTime = DateTime.Now;
               //if (_uploadEquipCapacityTime != null && ((DateTime)_uploadEquipCapacityTime).Minute == _currentTime.Minute)
               //    continue;
               //var _startTime = _currentTime.AddMinutes(-1);
               //long _startLong = new DateTime(_startTime.Year, _startTime.Month, _startTime.Day, _startTime.Hour, _startTime.Minute, 0, DateTimeKind.Local).GetIdFromDateTime();
               //long _endLong = new DateTime(_currentTime.Year, _currentTime.Month, _currentTime.Day, _currentTime.Hour, _currentTime.Minute, 0, DateTimeKind.Local).GetIdFromDateTime();
               //var _tableName = _sugarDB.LocalDb.SplitHelper<BatMainModel>().GetTableName(_startTime);//根据时间获取表名,提高效率
               //var _productAmount = await _sugarDB.LocalDb
               //    .Queryable<BatMainModel>()
               //    .AS(_tableName)
               //    .CountAsync(x =>
               //                x.BatteryID >= _startLong &&
               //                x.BatteryID < _endLong &&
               //                x.MesOutboundResult != ResultTypeEnum._);//所有出站的计为生产数，如客户要求OK为合格数可在此加条件
               //await _mesHelper.UploadEquipCapacity(0, _productAmount);
            }
            catch (Exception ex)
            {
               $"设备产能出现异常：{ex}".LogRun(Log4NetLevelEnum.错误);
            }
            finally
            {
               await Task.Delay(5000);
            }
         }
      });
   }
   #endregion

   #region OEE
   /// <summary>
   /// 上次OEE完成时间
   /// </summary>
   DateTime? _oeeFinishTime = null;
   double[] _plcOldTimes = new double[4];
   double[] _plcNewTimes = new double[4];

   /// <summary>
   /// 发送OEE,每小时发一次
   /// </summary>
   /// <returns></returns>
   private Task UploadOeeData()
   {
      return Task.Run(async () => {
         //while (!_taskToken.IsCancellationRequested)
         //{
         //    try
         //    {
         //        DateTime _currentTime = DateTime.Now;
         //        if (_oeeFinishTime != null && ((DateTime)_oeeFinishTime).Hour == _currentTime.Hour)
         //            continue;
         //        var _oee = _plc?.ReadObjects<PlcOeeDTU>(new SignalAddressModel("OEE", 0), 1);
         //        if (_oee != null)
         //        {
         //            _plcNewTimes[0] = _oee[0].Hour * 60 + _oee[0].min + _oee[0].Second / 60;//停机时间(分钟)
         //            _plcNewTimes[1] = _oee[1].Hour * 60 + _oee[1].min + _oee[1].Second / 60;//运行时间(分钟)
         //            _plcNewTimes[2] = _oee[2].Hour * 60 + _oee[2].min + _oee[2].Second / 60;//待料时间累计值(分钟)
         //            _plcNewTimes[3] = _oee[3].Hour * 60 + _oee[3].min + _oee[3].Second / 60;//待料时间累计值(分钟)
         //        }
         //        else
         //        {
         //            $"[OEE]取PLC为空".LogRun(Log4NetLevelEnum.错误);
         //        }

         //        #region 从数据库统计数量
         //        DateTime _startTime = _currentTime.AddHours(-1);
         //        long _startLong = new DateTime(_startTime.Year, _startTime.Month, _startTime.Day, _startTime.Hour, 0, 0, DateTimeKind.Local).GetIdFromDateTime();
         //        long _endLong = new DateTime(_currentTime.Year, _currentTime.Month, _currentTime.Day, _currentTime.Hour, 0, 0, DateTimeKind.Local).GetIdFromDateTime();
         //        var _tableName = _sugarDB.LocalDb.SplitHelper<BatMainModel>().GetTableName(_startTime);//根据时间获取表名,提高效率
         //        var _productAmount = await _sugarDB.LocalDb
         //            .Queryable<BatMainModel>()
         //            .AS(_tableName)
         //            .CountAsync(x =>
         //                        x.BatteryID >= _startLong &&
         //                        x.BatteryID < _endLong &&
         //                        x.MesOutboundResult != ResultTypeEnum._);//所有出站的计为生产数，如客户要求OK为合格数可在此加条件

         //        var _ngAmount = await _sugarDB.LocalDb
         //            .Queryable<BatMainModel>()
         //            .AS(_tableName)
         //            .CountAsync(x =>
         //                        x.BatteryID >= _startLong &&
         //                        x.BatteryID < _endLong &&
         //                        x.Decide == ResultTypeEnum.不合格 &&
         //                        x.MesOutboundResult != ResultTypeEnum._);
         //        #endregion

         //        List<Dictionary<string, object>> _oeeAlarms = new List<Dictionary<string, object>>();
         //        for (int i = _globalStaticTemporary.OeeHourAlarm.Count - 1; i > -1; i--)
         //        {
         //            var _alarm = _globalStaticTemporary.OeeHourAlarm[i];
         //            if (_alarm.EndTime != null)//已结束的报警
         //            {
         //                double _warningTime = 60.0;
         //                if (((DateTime)_alarm.StartTime) < _startTime)
         //                {
         //                    _warningTime = ((DateTime)_alarm.EndTime).Subtract(_startTime).TotalMinutes;
         //                }
         //                else
         //                {
         //                    _warningTime = ((DateTime)_alarm.EndTime).Subtract((DateTime)_alarm.StartTime).TotalMinutes;
         //                }
         //                _oeeAlarms.Add(new Dictionary<string, object> { { "warningCode", _alarm.Code } });
         //                _oeeAlarms.Add(new Dictionary<string, object> { { "warningTime", _warningTime } });
         //                _globalStaticTemporary.OeeHourAlarm.RemoveAt(i);
         //            }
         //            else
         //            {
         //                double _warningTime = 60.0;
         //                if (((DateTime)_alarm.StartTime) >= _startTime)
         //                {
         //                    _warningTime = _currentTime.Subtract((DateTime)_alarm.StartTime).TotalMinutes;
         //                }
         //                _oeeAlarms.Add(new Dictionary<string, object> { { "warningCode", _alarm.Code } });
         //                _oeeAlarms.Add(new Dictionary<string, object> { { "warningTime", _warningTime } });
         //            }
         //        }
         //        var _dic = new Dictionary<string, object>
         //        {
         //            {"waitMaterialTime",_plcNewTimes[2] -_plcOldTimes[2] },//待料时间(分钟)
         //            {"blockTime",_plcNewTimes[3] -_plcOldTimes[3] },//堵料时间(分钟)
         //            {"equipFaultValue",0.0 },//设备故障累计值(分钟)
         //            {"stopValue",_plcNewTimes[0] },//非故障停机累计值(分钟)
         //            {"runTimeValue",_plcNewTimes[1] },//运行时间累计值(分钟)
         //            {"warningValue",0.0 },//报警时间累计值(分钟)
         //            {"waitMaterialValue",_plcNewTimes[2] },//待料时间累计值(分钟)
         //            {"blockValue",_plcNewTimes[3]},//堵料时间累计值(分钟)
         //            {"energyConsume",_globalStaticTemporary.ZTDTSU666Result.ImpEp },//设备电能消耗(kWh)
         //            {"productAmount",_productAmount },//生产数量
         //            {"productAmountUnit",_parameterConfig.DeviceParameter.Unit },//單位
         //            {"ngAmount",_ngAmount},//不良数量
         //            {"warningDetail",_oeeAlarms},//报警
         //        };

         //        if (await _mesHelper.UploadOeeData(_dic))
         //        {
         //            _oeeFinishTime = _currentTime;
         //            for (int i = 0; i < _plcNewTimes.Length; i++)
         //            {
         //                _plcOldTimes[i] = _plcNewTimes[i];
         //            }
         //        }
         //    }
         //    catch (Exception ex)
         //    {
         //        $"[OEE]出现异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         //    }
         //    finally
         //    {
         //        await Task.Delay(1000 * 60);
         //    }
         //}
      });
   }
   #endregion

   #region MQTT
   /// <summary>
   /// 发送MQTT
   /// </summary>
   /// <returns></returns>
   private Task SendMQTT(IContainer container)
   {
      return Task.Run(async () =>
      {
         var devicesConfig = container.Get<DevicesConfig>();
         var plcs = devicesConfig.GetRunDevices(x => x.DeviceInfo.ProcessesType == ProcessTypeEnum.PLC);
         if (plcs.Count < 1)
            return;
         if (!(plcs[0] is IPLC plc))
            return;

         string logHeader = plc.DeviceInfo.ToDeviceLogHeader();
         var _tags = new SignalAddressModel[12];
         for (int i = 0; i < _tags.Length; i++)
            _tags[i] = new SignalAddressModel($"PC_Pressure[{i}]", 0);
         while (!GlobalStaticTemporary.GlobalToken.IsCancellationRequested)
         {
            try
            {
               var _parmDic = new Dictionary<string, object>();
               var pressures = plc?.ReadObjects(_tags, logHeader);
               if (pressures == null && pressures.Status != DeviceStatus.Success)
               {
                  $"MQTT未取到值！".LogRun(Log4NetLevelEnum.警告);
                  continue;
               }
               if (_parameterConfig.AdvancedConfig.ProductionType == ProductionTypeEnum.一次注液)
               {
                  _parmDic.Add(
                     "JCZY_20001",
                     $"{_parameterConfig.RunParameter.IncomingWeightLower}-{_parameterConfig.RunParameter.IncomingWeightUpper}"
                  );
                  _parmDic.Add(
                     "JCZY_20002",
                     $"{_parameterConfig.RunParameter.InjectionLower}-{_parameterConfig.RunParameter.InjectionStandard}-{_parameterConfig.RunParameter.InjectionUpper}"
                  );
                  _parmDic.Add("JCZY_20025", $"{_usersStatus.LocalLoggedinUser.Account}");
                  // _parmDic.Add("JCZY_20024", _displayPercentageCollection.DisplayPercentages.Sum(x => x.NGCount));
                  for (int i = 0; i < pressures.Value?.Count; i++)
                     _parmDic.Add($"JCZY_{20004 + i}", pressures.Value[i]);
               }
               else
               {
                  _parmDic.Add(
                     "ECZY_20001",
                     $"{_parameterConfig.RunParameter.IncomingWeightLower}-{_parameterConfig.RunParameter.IncomingWeightUpper}"
                  );
                  _parmDic.Add(
                     "ECZY_20002",
                     $"{_parameterConfig.RunParameter.InjectionLower}-{_parameterConfig.RunParameter.InjectionStandard}-{_parameterConfig.RunParameter.InjectionUpper}"
                  );
                  _parmDic.Add(
                     "ECZY_20004",
                     $"{_parameterConfig.RunParameter.InjectionLower}-{_parameterConfig.RunParameter.InjectionStandard}-{_parameterConfig.RunParameter.InjectionUpper}"
                  );
                  _parmDic.Add("JCZY_20025", $"{_usersStatus.LocalLoggedinUser.Account}");
                  //_parmDic.Add("ECZY_20021", _displayPercentageCollection.DisplayPercentages.Sum(x => x.NGCount));
                  for (int i = 0; i < pressures.Value.Count; i++)
                     _parmDic.Add($"ECZY_{20005 + i}", pressures.Value[i]);
               }

               var _dic = new Dictionary<string, object>
               {
                  {
                     "b",
                     new Dictionary<string, object> { { "rf", 5 }, { "dl", _parmDic } }
                  },
                  { "ts", DateTime.Now.ToMesDateTime() },
               };
               await _mqttHelper.SendProcessData(JsonSerializer.Serialize(_dic));
            }
            catch (Exception ex)
            {
               $"MQTT出现异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
            }
            finally
            {
               Thread.Sleep(_mesInterfaceParameterConfig.MqttServiceInfo.Interval);
            }
         }
      });
   }
   #endregion
}
