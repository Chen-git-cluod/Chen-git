namespace Kinlo.Services;

public class ServiceCore
{
   #region field
   private IContainer _container;
   private DevicesConfig _devicesConfig;

   // private IMesService _mesService;
   private UsersStatusConfig _usersStatusConfig;
   private PLCSignalConfig _plcSignalConfig;
   private GlobalStaticTemporary _temporaryData;
   private ParameterConfig _parameterConfig;
   private PlcStatusConfig _plcAlarmConfig;
   private readonly List<HandlerInfoDto> _serviceTaskTypeInfoList;
   private readonly Lazy<ProcessRatioDisplay> _processRatioDisplay;
   private Action? StopAction = null;
   #endregion
   public ServiceCore(IContainer container)
   {
      _container = container;
      //   _mesService = container.Get<IMesService>();
      _devicesConfig = container.Get<DevicesConfig>();

      _temporaryData = container.Get<GlobalStaticTemporary>();
      _parameterConfig = container.Get<ParameterConfig>();
      _usersStatusConfig = container.Get<UsersStatusConfig>();
      _plcSignalConfig = container.Get<PLCSignalConfig>();
      _plcAlarmConfig = container.Get<PlcStatusConfig>();
      _processRatioDisplay = new Lazy<ProcessRatioDisplay>(() => container.Get<ProcessRatioDisplay>());
      _temporaryData.RequestStopAction += Stop;
      _temporaryData.RequestStartFuncAsync += StartAsync;
      var serviceTaskTypes = _container.Get<Assembly>("Services").GetTypes().ToList();
      _serviceTaskTypeInfoList = new();
      foreach (var item in serviceTaskTypes)
      {
         var attributes = item.GetCustomAttributes<DeviceConnecAttribute>();
         if (attributes == null || attributes.Count() == 0)
            continue;

         foreach (var att in attributes)
            _serviceTaskTypeInfoList.Add(new HandlerInfoDto(att.ProcessesTaskType, att.DeviceCommunicationTypes, item));
      }
   }

   /// <summary>
   /// 开始运行
   /// </summary>
   /// <returns></returns>
   private async Task<bool> StartAsync()
   {
      Stop();
      var runningServiceTasks = new CancellationTokenSource();
      try
      {
         //停止运行的委托
         StopAction = () =>
         {
            if (runningServiceTasks != null && !runningServiceTasks.IsCancellationRequested)
               runningServiceTasks.Cancel();
         };
         //开机前检查
         if (!await CheckCanRunAsync(runningServiceTasks))
         {
            runningServiceTasks?.Cancel();
            return false;
         }
         //初始化设备仪器
         if (!await _devicesConfig.InitDeviceLink(runningServiceTasks))
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(() =>
               Dialog.Show(new AlarmDialog(new AlarmDialogDto("设备连接失败!!!前往日志系统查看详细设备!")))
            );
            runningServiceTasks?.Cancel();
            return false;
         }

         ConcurrentDictionary<int, string> taskLock = new ConcurrentDictionary<int, string>();
         runningServiceTasks.Token.Register(
            async state =>
            {
               var tuple = (ValueTuple<ConcurrentDictionary<int, string>, CancellationTokenSource?>)state!;
               await CleanupAsync(tuple.Item1, tuple.Item2);
            },
            (taskLock, runningServiceTasks)
         );
         //启动所有后台任务
         StartAllBackgroundTasks(runningServiceTasks, taskLock);
         return true;
      }
      catch (Exception ex)
      {
         runningServiceTasks?.Cancel();
         $"启动任务异常：{ex}".LogRun(Log4NetLevelEnum.错误);
      }
      return false;
   }

   /// <summary>
   /// 开机前检查
   /// </summary>
   /// <returns></returns>
   private async Task<bool> CheckCanRunAsync(CancellationTokenSource runningServiceTasks)
   {
      //if (_parameterConfig.AdvancedConfig.MESStatus == MESStatusEnum.启用)
      //{
      //    if (_usersStatusConfig.LoggedInUserType != LoggedInTypeEnum.MES登陆)
      //    {
      //        await UIThreadHelper.InvokeOnUiThreadAsync(() => Dialog.Show(new AlarmDialog(new AlarmDialogDto("MES开启，请先登陆MES再运行！!"))));
      //        return false;
      //    }
      //}

      //if (_parameterConfig.FunctionEnable.IsEnableMes && !await _mesHelper.UploadControlParametersOnDeviceStartup())
      //{
      //    await UIThreadHelper.InvokeOnUiThreadAsync(() => Dialog.Show(new AlarmDialog(new AlarmDialogDto("上传开机参数失败;请查看MES日志！"))));
      //    return false;
      //}
      await Task.Delay(1);
      return true;
   }

   /// <summary>
   /// 启动所有后台任务
   /// </summary>
   /// <returns></returns>
   private void StartAllBackgroundTasks(CancellationTokenSource runningServiceTasks, ConcurrentDictionary<int, string> taskLock)
   {
      // List<IPLC> plcs = new List<IPLC>();
      foreach (var plcScanSignal in _plcSignalConfig.PLCScanSignals)
      {
         string logHeader = ProcessTypeEnum.PLC.ToProcessLogHeader(
            plcScanSignal.ServiceName,
            plcScanSignal.DeviceCommunicationType,
            barcode: "扫描线程"
         );

         foreach (var item in plcScanSignal.PLCResections)
            item.IsExcision = false;
         if (plcScanSignal.DeviceCommunicationType == CommunicationEnum.None)
            continue;

         var plc =
            _devicesConfig.GetRunDevice(t =>
               t.DeviceInfo.Communication == plcScanSignal.DeviceCommunicationType && t.DeviceInfo.ServiceName == plcScanSignal.ServiceName
            ) as IPLC;

         if (plc == null)
         {
            var msg = $"无法运行！扫描线程 [{plcScanSignal.ServiceName}] 未找到PLC,退出扫描；";
            msg.LogProcess(logHeader, Log4NetLevelEnum.警告);
            HandyControl.Controls.MessageBox.Show(msg, "未找到PLC", MessageBoxButton.OK, MessageBoxImage.Warning);
            runningServiceTasks.Cancel();
            return;
         }

         // plcs.Add(plc);

         _ = PlcScanTaskAsync(plc, plcScanSignal, runningServiceTasks, taskLock, logHeader); //PLC扫描
         _ = PlcHeartbeatAsync(plc, plcScanSignal, runningServiceTasks, logHeader); //PLC心跳
      }
   }

   private async Task TestPlc(IPLC plc)
   {
      return;

      await Task.Run(() =>
      {
         var nameLen = 16;
         var sendName = "刘锦政".PadRight(nameLen, ' ');
         var sendRss = plc.WriteValue(sendName, new SignalAddressModel("PC_UserID"), "[PLC用户名]写入名字", encoding: Encoding.UTF8);
         int i = 0;
         // List<long> ids = new List<long>();
         //for (int i = 0; i < 30; i++)
         //{

         //    var id2s = plc.ReadLargeObjects<PlcTestlDTU>(new SignalAddressModel($"PC_UpData[0]") { Length = 12 }, "");
         //    var id2s2 = plc.ReadLargeObjects<PlcTestlDTU>(new SignalAddressModel($"PC_UpData[12]") { Length = 12 }, "");
         //    // var id2s22 = plc.ReadClass(new SignalAddressModel($"PC_UpData"), helium, "");
         //    //var id2s2 = plc.ReadClasses<PlcTestlDTU>([new SignalAddressModel($"PC_UpData[0]"), new SignalAddressModel($"PC_UpData[1]")], "");

         //    Thread.Sleep(10);
         //}
         //JsonSerializer.Serialize(ids).LogRun();
      });
   }

   /// <summary>
   /// PLC扫描
   /// </summary>
   /// <param name="plc"></param>
   /// <param name="signal"></param>
   /// <param name="runningServiceTasks"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   private async Task PlcScanTaskAsync(
      IPLC plc,
      PLCScanSignalModel signal,
      CancellationTokenSource runningServiceTasks,
      ConcurrentDictionary<int, string> taskLock,
      string logHeader
   )
   {
      try
      {
         await Task.Run(
               async () =>
               {
                  await TestPlc(plc);

                  ConcurrentDictionary<int, IServiceHandler> tasks = BuildServiceTasks(plc, signal, runningServiceTasks); //当前PLC的所有任务集合
                  var plcScanValue = new PlcScanSignalDTU(signal.LengthSignal, signal.LengthResection);

                  using var scanTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(20));
                  int exceptionCount = 0; //异常次数

                  while (await scanTimer.WaitForNextTickAsync(runningServiceTasks.Token).ConfigureAwait(false))
                  {
                     exceptionCount = 0;
                     try
                     {
                        if (plc.Scan(signal.AddressStart, plcScanValue, logHeader).Status == DeviceStatus.Success)
                        {
                           foreach (var keyValue in tasks) //启动信号
                           {
                              IServiceHandler processTask = keyValue.Value;
                              short plcValue = (short)plcScanValue.Cmd[keyValue.Key];
                              if (keyValue.Key != 0 && plcValue <= 0)
                                 continue;
                              if (
                                 taskLock.ContainsKey(processTask.Context.Key)
                                 || !taskLock.TryAdd(
                                    processTask.Context.Key,
                                    $"{processTask.Context.ProcessesType}-{processTask.Context.DeviceStartIndex}"
                                 )
                              )
                                 continue;
                              _ = Task.Run(
                                    async () =>
                                    {
                                       try
                                       {
                                          await processTask.Handle(plcValue).ConfigureAwait(false);
                                       }
                                       catch (Exception ex)
                                       {
                                          exceptionCount++;
                                          $"启动信号异常：{ex}".LogProcess(processTask.Context.ToProcessLogHeader(), Log4NetLevelEnum.错误);
                                       }
                                       finally
                                       {
                                          taskLock.TryRemove(processTask.Context.Key, out _);
                                       }
                                    },
                                    runningServiceTasks.Token
                                 )
                                 .ConfigureAwait(false);
                           }
                           UpdateResections(signal, plcScanValue, logHeader); //切除信号
                        }
                        else
                        {
                           exceptionCount++;
                           await Task.Delay(500, runningServiceTasks.Token);
                        }
                     }
                     catch (Exception ex)
                     {
                        exceptionCount++;
                        if (runningServiceTasks?.Token != null && !runningServiceTasks.Token.IsCancellationRequested)
                        {
                           $"信号扫描线程异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
                           await Task.Delay(500, runningServiceTasks.Token);
                        }
                     }

                     if (exceptionCount >= 2)
                     {
                        if (runningServiceTasks?.Token != null && !runningServiceTasks.Token.IsCancellationRequested)
                        {
                           $"[{signal.ServiceName}] 连续扫描线程异常，退出任务！！！".LogProcess(logHeader, Log4NetLevelEnum.错误);
                        }
                        break;
                     }
                  }
               },
               runningServiceTasks.Token
            )
            .ConfigureAwait(false);
      }
      catch (OperationCanceledException) { } // _taskToken 取消时正常退出
      catch (Exception ex)
      {
         $"扫描线程执行异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }
      finally
      {
         runningServiceTasks?.Cancel();
      }
   }

   /// <summary>
   /// 当前PLC的所有任务集合
   /// </summary>
   /// <param name="plc"></param>
   /// <param name="signal"></param>
   /// <param name="runningServiceTasks"></param>
   /// <returns></returns>
   private ConcurrentDictionary<int, IServiceHandler> BuildServiceTasks(
      IPLC plc,
      PLCScanSignalModel signal,
      CancellationTokenSource runningServiceTasks
   )
   {
      var dic = new ConcurrentDictionary<int, IServiceHandler>();
      foreach (var addr in _plcSignalConfig.PLCInteractAddresses.Where(x => x.ServiceName == signal.ServiceName))
      {
         var task = GetServiceTasks(addr, plc, runningServiceTasks);
         if (task != null)
            dic.TryAdd(addr.StartCommand.Index, task);
      }
      return dic;
   }

   /// <summary>
   /// 更新切除信号
   /// </summary>
   /// <param name="signal"></param>
   /// <param name="plcScanValue"></param>
   /// <param name="logHeader"></param>
   private void UpdateResections(PLCScanSignalModel signal, PlcScanSignalDTU plcScanValue, string logHeader)
   {
      foreach (var resection in signal.PLCResections) //切除信号
      {
         if (!resection.IsEnabled)
            continue;
         int _index = resection.Index;
         if (plcScanValue.Resection[_index] != resection.IsExcision)
         {
            if (plcScanValue.Resection[_index] && !resection.IsExcision)
               $"[{signal.ServiceName}] [{resection.Tag}] 功能已启用！！！".LogProcess(logHeader, Log4NetLevelEnum.成功);
            else if (!plcScanValue.Resection[_index] && resection.IsExcision)
               $"[{signal.ServiceName}] 注意：[{resection.Tag}] 功能已关闭！！！".LogProcess(logHeader, Log4NetLevelEnum.警告);

            UIThreadHelper.InvokeOnUiThreadAsync(() => resection.IsExcision = plcScanValue.Resection[_index]);
         }
      }
   }

   /// <summary>
   /// PLC心跳
   /// </summary>
   /// <param name="plc"></param>
   /// <param name="plcScanSignal"></param>
   private async Task PlcHeartbeatAsync(
      IPLC plc,
      PLCScanSignalModel plcScanSignal,
      CancellationTokenSource runningServiceTasks,
      string logHeader
   )
   {
      try
      {
         await Task.Run(async () =>
            {
               int errCount = 0;
               using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(plcScanSignal.HeartbeatIntervalMs));
               while (await timer.WaitForNextTickAsync(runningServiceTasks.Token).ConfigureAwait(false))
               {
                  try
                  {
                     if (plc.WriteValue((short)1, plcScanSignal.Heartbeat, "心跳写入"))
                     {
                        errCount = 0;
                     }
                     else if (runningServiceTasks?.Token != null && !runningServiceTasks.Token.IsCancellationRequested)
                     {
                        errCount++;
                        $"[{plcScanSignal.ServiceName}] 写入心跳失败;".LogProcess(logHeader, Log4NetLevelEnum.错误);
                        if (errCount >= 10)
                        {
                           runningServiceTasks?.Cancel();
                           return;
                        }
                     }
                  }
                  catch (Exception ex)
                  {
                     $"[{plcScanSignal.ServiceName}] 写入心跳异常: {ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
                  }
               }
            })
            .ConfigureAwait(false);
      }
      catch (OperationCanceledException) { } // 外部取消时正常退出
   }

   #region 关机操作
   /// <summary>
   /// 停止
   /// </summary>
   private void Stop()
   {
      StopAction?.Invoke();
      StopAction = null;
   }

   /// <summary>
   /// 清理(停止前清理)
   /// </summary>
   /// <param name="taskLock"></param>
   /// <param name="runningServiceTasks"></param>
   private Task CleanupAsync(ConcurrentDictionary<int, string> taskLock, CancellationTokenSource? runningServiceTasks)
   {
      return Task.Run(async () =>
      {
         string msg = "停机自动保存";
         try
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(() =>
            {
               _temporaryData.IsLoadFun = false; //禁止点开始按钮
               _temporaryData.IsRunning = false;
            });

            await CloseTaskAsync(taskLock);
            _temporaryData.Save(msg, msg, false);
            _plcAlarmConfig.Save(msg, msg, false);
            _parameterConfig.Save(msg, msg, false);
            _processRatioDisplay.Value.Save(msg, msg, false);

            var statusTasks = _plcAlarmConfig.PlcStatusPendingSaveTasks.Select(x => x.Value("上位机主动停机")).ToArray(); //设备状态退出时保存
            await Task.WhenAll(statusTasks);
            _plcAlarmConfig.PlcStatusPendingSaveTasks.Clear();

            var alarmTasks = _plcAlarmConfig.PlcCurrentAlarmTasks.Select(x => x.Value.func()).ToArray(); //设备报警退出时保存
            await Task.WhenAll(alarmTasks);
            _plcAlarmConfig.PlcCurrentAlarmTasks.Clear();
         }
         catch (Exception ex)
         {
            $"{msg}异常:{ex}".LogRun(Log4NetLevelEnum.警告);
         }
         finally
         {
            runningServiceTasks?.Dispose();
            runningServiceTasks = null;
            await UIThreadHelper.InvokeOnUiThreadAsync(() => _temporaryData.IsLoadFun = true);
         }
      });
   }

   /// <summary>
   /// 清理生产任务
   /// </summary>
   private async Task CloseTaskAsync(ConcurrentDictionary<int, string> taskLock)
   {
      try
      {
         int _awaitCount = 30;
         while (taskLock.Count != 0)
         {
            await Task.Delay(500);
            if (_awaitCount < 1)
            {
               taskLock.Clear(); //如果超时直接清理
            }
            string _msg = $"等待任务 [{string.Join(",", taskLock.Select(x => x.Value))}] 退出，倒计时：{--_awaitCount}.....";
            await UIThreadHelper.InvokeOnUiThreadAsync(
               new Action(() => Dialog.Show(new AlarmDialog(new AlarmDialogDto(_msg)), GenericHelper.FullScreenAlarmToken))
            );
            _msg.LogRun(Log4NetLevelEnum.警告);
         }
         //释放所有运行设备
         _devicesConfig.ClearRunDevice();
      }
      catch (Exception ex)
      {
         $"等待或清理任务异常：{ex}".LogRun(Log4NetLevelEnum.错误);
      }
      finally
      {
         await UIThreadHelper.InvokeOnUiThreadAsync(() => Dialog.Close(GenericHelper.FullScreenAlarmToken));
      }
   }
   #endregion

   record HandlerInfoDto(ProcessTypeEnum prcessType, CommunicationEnum[] commTypes, Type type);

   /// <summary>
   /// 获取工序操作任务
   /// </summary>
   /// <param name="plcInteract"></param>
   /// <param name="plc"></param>
   /// <returns></returns>
   private IServiceHandler? GetServiceTasks(PLCInteractAddressModel plcInteract, IDevice plc, CancellationTokenSource runningServiceTasks)
   {
      string logHeader = plcInteract.ToProcessLogHeader();
      try
      {
         $"[ServiceTask初始化]工序：[{plcInteract.ProcessesType}]开始！".LogProcess(logHeader);
         plcInteract.CreateKey();

         var handlerInfo = _serviceTaskTypeInfoList.FirstOrDefault(x =>
            x.prcessType == plcInteract.ProcessesType && x.commTypes.Contains(plcInteract.DeviceCommunicationType)
         ); //先精确匹配
         if (handlerInfo == null)
            handlerInfo = _serviceTaskTypeInfoList.FirstOrDefault(x =>
               x.prcessType == plcInteract.ProcessesType && x.commTypes.Contains(CommunicationEnum.None)
            ); //先精确匹配
         if (handlerInfo == null)
         {
            $"[ServiceTask初始化]工序：[{plcInteract.ProcessesType}]未找到实现Handle类！".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return null;
         }
         var serviceTaskType = handlerInfo.type;

         var parameterInfos = serviceTaskType.GetConstructors()[^1].GetParameters();
         var parameters = new object[parameterInfos.Length];
         for (int i = 0; i < parameters.Length; i++)
         {
            parameters[i] = parameterInfos[i].ParameterType switch
            {
               var _p when _p == typeof(IContainer) => _container,
               var _p when _p == typeof(PLCInteractAddressModel) => plcInteract,
               var _p when _p == typeof(IDevice) => plc,
               var _p when _p == typeof(CancellationTokenSource) => runningServiceTasks,
               _ => null!,
            };
         }

         var serviceTask = Activator.CreateInstance(serviceTaskType, parameters) as IServiceHandler;
         if (serviceTask != null)
         {
            $"[ServiceTask初始化]工序：[{plcInteract.ProcessesType}]成功！".LogProcess(logHeader, Log4NetLevelEnum.成功);
            return serviceTask;
         }
         else
         {
            $"[ServiceTask初始化]工序：[{plcInteract.ProcessesType}]创建Handle类失败！".LogProcess(logHeader, Log4NetLevelEnum.成功);
         }
      }
      catch (Exception ex)
      {
         $"[ServiceTask初始化]工序：[{plcInteract.ProcessesType}]异常：{ex}！".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }
      return null;
   }
}

public class PlcTestlDTU
{
   public long ID { get; set; }
   public short State { get; set; }
   public short PLCDataType { get; set; }
   public float[] PLCData { get; set; }
   public int MyProperty { get; set; }

   public PlcTestlDTU()
   {
      PLCData = new float[20];
   }
}
