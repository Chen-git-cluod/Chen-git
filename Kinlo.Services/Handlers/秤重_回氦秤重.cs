namespace Kinlo.Services.Handlers;

[DeviceConnec(ProcessTypeEnum.回氦称重, CommunicationEnum.None)]
public class WeightHeliumHandler : ServiceHandlerBase
{
   ConcurrentBag<int> _alarmDevice = new ConcurrentBag<int>();

   public WeightHeliumHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken) { }

   protected override async Task HandleCore(short plcValue)
   {
      _alarmDevice.Clear();
      if (!TryReadPlcData(out List<ReceivePlcDataModel> plcDatas))
         return;
      await Parallel.ForEachAsync(plcDatas, new ParallelOptions { MaxDegreeOfParallelism = Context.DataLength }, HandleBatteryAsync);

      if (_alarmDevice.Any())
      {
         $"{string.Join(',', _alarmDevice.Select(x => $"{x}号"))} 称重不稳定！".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
         // _isDeviceAlarm = true;
         //WritePlcSingle(1, _plcSignalConfig.PLCAlarmAddresses.Alarm_AfterWeighing);
      }
   }

   protected async ValueTask HandleBatteryAsync(ReceivePlcDataModel plcData, CancellationToken token)
   {
      //TaskLog($"任务ID:{Task.CurrentId}-处理器ID:{Thread.GetCurrentProcessorId()}-线程ID:{Thread.CurrentThread.ManagedThreadId}", 0);
      bool isSpotCheck = plcData.ID == -1; //是否为点检
      string logHeader = Context.ToProcessLogHeader(plcData.Index, plcData.ID);
      IBatMainModel? mainBattery = isSpotCheck
         ? Activator.CreateInstance(_displayDataCollection.CompleteBatteryDatas.RuntimeBatteryType) as IBatMainModel //点检
         : await _batteryCache.GetByIdAsync(plcData.ID, logHeader); //取缓存

      if (mainBattery == null)
      {
         if (isSpotCheck)
            "创建点检电池对象失败；".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         plcData.DataAddress.WritePlcResult(ResultTypeEnum.数据库找不到电池, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
         return;
      }
      logHeader = Context.ToProcessLogHeader(plcData.Index, plcData.ID, mainBattery.Barcode);
      IBatWeightHeliumModel batHeliumWeight = (IBatWeightHeliumModel)mainBattery;
      var deviceResult = DeviceResult<double>.Failure("默认");

      if (_parameterConfig.FunctionEnable.IsEmptyLoadMode) //空跑则不去获取设备数据
      {
         $"开启空载模式，随机生成重量;".LogProcess(logHeader, Log4NetLevelEnum.警告);
         var weiging = new Random().Next(
            (int)(_parameterConfig.RunParameter.IncomingWeightLower + _parameterConfig.RunParameter.InjectionStandard),
            (int)(_parameterConfig.RunParameter.IncomingWeightUpper + _parameterConfig.RunParameter.InjectionStandard)
         );
         deviceResult = DeviceResult<double>.Success(weiging);
      }
      else
      {
         var device = _devicesConfig.GetRunDevice(Context, plcData.Index);
         if (device == null)
         {
            batHeliumWeight.HeliumWeightResult = ResultTypeEnum.未找到设备;
            AddDisplayData(mainBattery);
            //更新界面显示
            _isDeviceAlarm = true;
            $"找不到设备，不给PLC发结果及完成信号，请停机检查！;".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
            return;
         }
         deviceResult = HandlerHelper.GetWeiging(device, logHeader);
      }
      #region 点检
      if (isSpotCheck)
      {
         ResultTypeEnum checkResult = deviceResult switch
         {
            var r when r.Status == DeviceStatus.Success => ResultTypeEnum.NG,
            _ => BatteryWeightValidator.IncomingWeightRangeCheck(
               deviceResult.Value,
               BatteryWeightValidator.GetBeforWeightRange(_parameterConfig),
               _parameterConfig,
               logHeader
            ),
         };
         plcData.DataAddress.WritePlcResult(checkResult, ResultTypeEnum._, _plc, _parameterConfig, logHeader);
         return;
      }
      #endregion
      IBatScanBeforeModel batBefScan = (IBatScanBeforeModel)mainBattery;

      batHeliumWeight.HeliumWeightResult = deviceResult.Status.ToProductResult();
      batHeliumWeight.InjectionVolumeRange =
         $"{_parameterConfig.RunParameter.InjectionUpper}~{_parameterConfig.RunParameter.InjectionStandard}~{_parameterConfig.RunParameter.InjectionLower}";
      batHeliumWeight.IncomingWeightRange =
         $"{_parameterConfig.RunParameter.IncomingWeightUpper}~{_parameterConfig.RunParameter.IncomingWeightLower}";
      batHeliumWeight.HeliumWeightTime = DateTime.Now;
      batHeliumWeight.HeliumWeightIndex = plcData.Index;

      if (batHeliumWeight.HeliumWeightResult == ResultTypeEnum.OK)
      {
         batHeliumWeight.HeliumWeight = (float)deviceResult.Value;
         batHeliumWeight.TotalInjectionVolume = Math.Round(batHeliumWeight.HeliumWeight - batBefScan.NetWeight, 3);

         batHeliumWeight.HeliumLossOfFluid =
            batBefScan.PreProcessWeight > 0 ? Math.Round((float)(batBefScan.PreProcessWeight - batHeliumWeight.HeliumWeight), 3) : 0f;

         batHeliumWeight.TotalInjectionVolumeDeviation = Math.Round(
            batHeliumWeight.TotalInjectionVolume - _parameterConfig.RunParameter.InjectionStandard,
            3
         );

         batHeliumWeight.HeliumWeightResult = BatteryWeightValidator.IncomingWeightRangeCheck(
            batHeliumWeight.HeliumWeight,
            BatteryWeightValidator.GetInjectionRange(_parameterConfig),
            _parameterConfig,
            logHeader
         );

         if (batHeliumWeight.HeliumLossOfFluid > _parameterConfig.RunParameter.LossOfFluidUpper)
         {
            batHeliumWeight.HeliumWeightResult = ResultTypeEnum.失液量超标;
            $"失液量：{batHeliumWeight.HeliumLossOfFluid}; 超出失液量上限:{_parameterConfig.RunParameter.LossOfFluidUpper}!".LogProcess(
               logHeader
            );
         }
         else
         {
            batHeliumWeight.HeliumWeightResult = BatteryWeightValidator.InjectInjectCheck(
               batHeliumWeight.TotalInjectionVolume,
               BatteryWeightValidator.GetInjectionRange(_parameterConfig),
               _parameterConfig,
               logHeader
            );
         }
      }
      if (batHeliumWeight.HeliumWeight < _weightWarningValue)
         _alarmDevice.Add(plcData.Index); //小于20g报警
      #region 上传MES
      if (batHeliumWeight.HeliumWeightResult != ResultTypeEnum.OK)
      {
         await MesOutput(mainBattery, logHeader);
      }
      #endregion

      //保存本工序数据
      if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
      {
         batHeliumWeight.HeliumWeightResult = ResultTypeEnum.保存数据库失败;
      }

      plcData.DataAddress.WritePlcResult(
         batHeliumWeight.HeliumWeightResult,
         mainBattery.MesOutputStatus,
         _plc,
         _parameterConfig,
         logHeader
      ); //写入PLC结果
      AddDisplayData(mainBattery);
   }
}
