namespace Kinlo.Services.Handlers;

[DeviceConnec(ProcessTypeEnum.前称重, [CommunicationEnum.None])]
public class WeightBeforeHandler : ServiceHandlerBase
{
   ConcurrentBag<int> _alarmDevice = new ConcurrentBag<int>();

   public WeightBeforeHandler(
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
         $"{string.Join(',', _alarmDevice.Select(x => $"{x}号"))} 称重异常！".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
         // _isDeviceAlarm = true;
         //  WritePlcSingle(1, _plcSignalConfig.PLCAlarmAddresses.Alarm_BeforeWeighing);
      }
   }

   private async ValueTask HandleBatteryAsync(ReceivePlcDataModel plcData, CancellationToken token)
   {
      //  TaskLog($"处理器ID:{Thread.GetCurrentProcessorId()}-线程ID:{Thread.CurrentThread.ManagedThreadId}", 0);
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
      var beforeWeight = (IBatWeightBeforeModel)mainBattery;
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
            beforeWeight.BeforeWeightResult = ResultTypeEnum.未找到设备;

            //更新界面显示
            AddDisplayData(mainBattery);
            _isDeviceAlarm = true;
            $"找不到设备，不给PLC发结果及完成信号，请停机检查！;".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
            return;
         }

         deviceResult = HandlerHelper.GetWeiging(device, logHeader);
      }

      #region 点检
      if (isSpotCheck)
      {
         float checkValue = deviceResult.Value == null ? 0 : (float)deviceResult.Value;
         var inspectionResult = Context.ProcessesType.RangeInspection(
            plcData.Index,
            _inspectionConfig,
            deviceResult.Status == DeviceStatus.Success,
            checkValue
         );
         OnSupplementaryElectrolyteToPlc(plcData, checkValue, logHeader, "[点检发送前称重量]");
         plcData.DataAddress.WritePlcResult(inspectionResult, ResultTypeEnum._, _plc, _parameterConfig, logHeader);
         return;
      }
      #endregion

      var beforeScan = (IBatScanBeforeModel)mainBattery;

      mainBattery.SetBatteryRange(_parameterConfig); //写入电池范围值
      beforeWeight.BeforeWeightTime = DateTime.Now;
      beforeWeight.BeforeWeightIndex = plcData.Index;
      beforeWeight.BeforeWeightResult = deviceResult.Status.ToProductResult();

      if (beforeWeight.BeforeWeightResult == ResultTypeEnum.OK)
      {
         beforeWeight.BeforeWeight = (float)deviceResult.Value;
         beforeWeight.LossOfFluid =
            beforeScan.PreProcessWeight > 0 ? Math.Round((float)(beforeScan.PreProcessWeight - beforeWeight.BeforeWeight), 3) : 0f;

         beforeWeight.BeforeWeightResult =
            mainBattery.ReproductionCount > 0 //复投不进行前重检测
               ? ResultTypeEnum.OK
               : BatteryWeightValidator.IncomingWeightRangeCheck(
                  beforeWeight.BeforeWeight,
                  beforeWeight.BeforWeightRange,
                  _parameterConfig,
                  logHeader
               );

         if (beforeScan.NetWeight == 0) //电芯干重
         {
            beforeScan.NetWeight = beforeWeight.BeforeWeight;

            //发送干重给分容
            _ = SendWeightToMes(beforeScan, mainBattery.Barcode, logHeader);
         }
      }
      if (beforeWeight.BeforeWeight < _weightWarningValue)
         _alarmDevice.Add(plcData.Index); //小于20g报警
      //计算失液量及发给PLC的注液量
      float sedInjection = CalculateLossAndInjection(beforeWeight, beforeScan, logHeader);

      //发送注液量等到PLC
      var injectResult = OnSupplementaryElectrolyteToPlc(plcData, sedInjection, logHeader, "注液量");
      if (beforeWeight.BeforeWeightResult == ResultTypeEnum.OK)
         beforeWeight.BeforeWeightResult = injectResult;

      #region 上传MES
      if (beforeWeight.BeforeWeightResult != ResultTypeEnum.OK)
      {
         await MesOutput(mainBattery, logHeader);
      }
      #endregion

      //保存本工序数据
      if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
      {
         beforeWeight.BeforeWeightResult = ResultTypeEnum.保存数据库失败;
      }

      AddDisplayData(mainBattery);
      plcData.DataAddress.WritePlcResult(beforeWeight.BeforeWeightResult, mainBattery.MesOutputStatus, _plc, _parameterConfig, logHeader); //写入PLC结果
   }

   /// <summary>
   /// 发送干重给分容
   /// </summary>
   /// <param name="beforeScan"></param>
   /// <param name="barcode"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public async Task SendWeightToMes(IBatScanBeforeModel beforeScan, string barcode, string logHeader)
   {
      var call = _mesInterfaceParameterConfig.GetApiCall(new MesRequestBuildNJGX.ArgsSendNetWeight(barcode, beforeScan.NetWeight));
      if (call != null && call.IsEnable)
      {
         //发送干重给分容
         var sendNetWeightRs = await _mesService.SendAsync(call, barcode, receiveMesJson => receiveMesJson.SendDryWeight());
         $"发送分容干重：条码 [{barcode}] 干重 [{beforeScan.NetWeight}] {sendNetWeightRs.ResultStatus}".LogProcess(logHeader);
      }
   }

   /// <summary>
   /// 计算失液量及发给PLC的注液量
   /// </summary>
   /// <param name="beforeWeight"></param>
   /// <param name="logHeader"></param>
   /// <returns>失败发 0 重置注液泵上面的注液量</returns>
   private float CalculateLossAndInjection(IBatWeightBeforeModel beforeWeight, IBatScanBeforeModel beforeScan, string logHeader)
   {
      //电池内已有电解液量
      var existingVolume = beforeWeight.BeforeWeight - beforeScan.NetWeight;

      beforeWeight.TargetInjectionVolume = _parameterConfig.FunctionEnable.IsEnableVariableInjection
         ? _parameterConfig.RunParameter.InjectionStandard - existingVolume
         : _parameterConfig.RunParameter.InjectionStandard;

      if (beforeWeight.BeforeWeightResult != ResultTypeEnum.OK)
         return 0;

      //判断失液量
      if (beforeWeight.LossOfFluid > _parameterConfig.RunParameter.LossOfFluidUpper)
      {
         beforeWeight.BeforeWeightResult = ResultTypeEnum.失液量超标;
         $"失液量：{beforeWeight.LossOfFluid}; 超出失液量上限:{_parameterConfig.RunParameter.LossOfFluidUpper}，注液量发送零！".LogProcess(
            logHeader
         );
         return 0;
      }

      //目标注液量低于下限
      if (beforeWeight.TargetInjectionVolume <= 0)
      {
         beforeWeight.BeforeWeightResult = ResultTypeEnum.目标注液量低于注液泵下限;
         $"目标注液量：{beforeWeight.TargetInjectionVolume} 少于或等于零，注液量发送零！".LogProcess(logHeader);
         return 0;
      }

      //预注液超保液量
      if (beforeWeight.TargetInjectionVolume + existingVolume > _parameterConfig.RunParameter.InjectionUpper)
      {
         beforeWeight.BeforeWeightResult = ResultTypeEnum.预注液超保液量;
         $"目标注液量：{beforeWeight.TargetInjectionVolume} 加已注液量 {existingVolume} 超出保量液上限 {_parameterConfig.RunParameter.InjectionUpper}，注液量发送零！".LogProcess(
            logHeader
         );
         return 0;
      }

      if (beforeWeight.TargetInjectionVolume > _parameterConfig.RunParameter.VariableInjectionUpper)
      {
         beforeWeight.BeforeWeightResult = ResultTypeEnum.目标注液量超注液泵上限;
         $"目标注液量：{beforeWeight.TargetInjectionVolume}; 超出注液泵上限:{_parameterConfig.RunParameter.VariableInjectionUpper}，注液量发送零！".LogProcess(
            logHeader
         );
         return 0;
      }
      return (float)beforeWeight.TargetInjectionVolume;
   }
}
