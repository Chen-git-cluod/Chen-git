namespace Kinlo.Services.Handlers;

[DeviceConnec(ProcessTypeEnum.后称重, CommunicationEnum.None)]
[DeviceConnec(ProcessTypeEnum.回流补液, CommunicationEnum.None)]
public partial class WeightAfterHandler : ServiceHandlerBase
{
   ConcurrentBag<int> _alarmDevice = new ConcurrentBag<int>();

   public WeightAfterHandler(
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
         $"{string.Join(',', _alarmDevice.Select(x => $"{x}号"))} 称重重量异常！".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
         // WritePlcSingle(1, _plcSignalConfig.PLCAlarmAddresses.Alarm_AfterWeighing);//去掉报警
      }
   }

   private async ValueTask HandleBatteryAsync(ReceivePlcDataModel plcData, CancellationToken token)
   {
      //TaskLog($"任务ID:{Task.CurrentId}-处理器ID:{Thread.GetCurrentProcessorId()}-线程ID:{Thread.CurrentThread.ManagedThreadId}", 0);

      bool isSpotCheck = plcData.ID == -1; //是否为点检
      string logHeader = Context.ToProcessLogHeader(plcData.Index, plcData.ID);

      IBatMainModel? mainBattery = isSpotCheck switch
      {
         true => Activator.CreateInstance(_displayDataCollection.CompleteBatteryDatas.RuntimeBatteryType) as IBatMainModel, //点检
         _ => await _batteryCache.GetByIdAsync(plcData.ID, logHeader), //取缓存
      };

      if (mainBattery == null)
      {
         if (isSpotCheck)
            "创建点检电池对象失败；".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         plcData.DataAddress.WritePlcResult(ResultTypeEnum.数据库找不到电池, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
         return;
      }

      IBatWeightAfterModel batAftWeight = (IBatWeightAfterModel)mainBattery;
      //当前是否为回流自动补液
      var isReinject = Context.ProcessesType == ProcessTypeEnum.回流补液 || batAftWeight.ActualInjectionVolume > 5;

      //生成日志头
      logHeader = isReinject
         ? ProcessTypeEnum.回流补液.ToProcessLogHeader(
            Context.ServiceName,
            Context.DeviceCommunicationType,
            plcData.Index,
            plcData.ID,
            mainBattery.Barcode
         )
         : Context.ToProcessLogHeader(plcData.Index, plcData.ID, mainBattery.Barcode);

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
            batAftWeight.FinalInjectResult = ResultTypeEnum.未找到设备;
            AddDisplayData(mainBattery);
            ; //更新界面显示
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
         OnSupplementaryElectrolyteToPlc(plcData, checkValue, logHeader, "[点检发送后称重量]");
         plcData.DataAddress.WritePlcResult(inspectionResult, ResultTypeEnum._, _plc, _parameterConfig, logHeader);
         return;
      }
      #endregion

      if (_parameterConfig.FunctionEnable.IsEnableCurrentRange) //是否使用实时范围值
      {
         batAftWeight.InjectionVolumeRange = _parameterConfig.GetInjectionRange(); //注液范围
         batAftWeight.AfterWeighingRange = _parameterConfig.GetFinalWeightRange(); //后称范围
      }
      if (deviceResult.Value < _weightWarningValue)
         _alarmDevice.Add(plcData.Index); //小于20g报警
      #region 首注或少液回流分叉（如果测漏回流电池走后称正常流程）
      //计算回流次数
      CalculateReinjectCount(mainBattery, logHeader);

      if (isReinject) //处理少液回流电池
      {
         await RefillHandle(mainBattery, deviceResult, plcData, logHeader);
      }
      else
      {
         await AfterWeightHanler(mainBattery, deviceResult, plcData, logHeader);
      }
      #endregion
   }

   /// <summary>
   /// 发送补液量至PLC
   /// </summary>
   /// <param name="injectionVolumeDeviation">注液量偏差</param>
   /// <param name="plcData"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   private ResultTypeEnum SupplementaryElectrolyteToPlc(double injectionVolumeDeviation, ReceivePlcDataModel plcData, string logHeader)
   {
      float volume = 0;
      ResultTypeEnum injectVolumResult = ResultTypeEnum.OK;
      //负数才是少液
      if (injectionVolumeDeviation < 0)
         volume = (float)Math.Abs(injectionVolumeDeviation);

      //判断补液量
      if (volume > _parameterConfig.RunParameter.ReplenishInjectionUpper)
      {
         injectVolumResult = ResultTypeEnum.目标补液量超补液泵上限;
         $"补液量：{volume}; 超出补液泵上限:{_parameterConfig.RunParameter.ReplenishInjectionUpper}".LogProcess(logHeader);
         volume = 0;
      }

      //任何情况都给PLC发补液量
      var toPlcInjectResult = OnSupplementaryElectrolyteToPlc(plcData, volume, logHeader, "补液量");

      if (injectVolumResult != ResultTypeEnum.OK)
         return injectVolumResult;

      return toPlcInjectResult;
   }

   /// <summary>
   /// 检查回流次数
   /// </summary>
   /// <param name="mainBattery"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   protected ResultTypeEnum CheckReworkCount(IBatMainModel mainBattery, string logHeader)
   {
      var aftWeight = (IBatWeightAfterModel)mainBattery;
      var leak = (IBatTestLeakModel)mainBattery;

      if (mainBattery.LeakTestNgCount >= _parameterConfig.RunParameter.TestLeakReworkCount)
      {
         $"当前电池注液过少，但[{ResultTypeEnum.测漏回流次数超上限}]，拒绝回流！".LogProcess(logHeader);
         return ResultTypeEnum.测漏回流次数超上限;
      }

      if (mainBattery.LowElectorlyteNgCount >= _parameterConfig.RunParameter.InjectLessReworkCount)
      {
         $"当前电池注液过少，但[{ResultTypeEnum.少液回流次数超上限}]，拒绝回流！".LogProcess(logHeader);
         return ResultTypeEnum.少液回流次数超上限;
      }

      return ResultTypeEnum.OK;
   }

   /// <summary>
   /// 单纯获取发给PLC结果，此过程不会给电池赋值
   /// </summary>
   /// <param name="mainBattery"></param>
   /// <param name="supplementaryRes"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   // protected ResultTypeEnum BuildPlcResult(IBatMainModel mainBattery, ResultTypeEnum supplementaryRes, string logHeader)
   protected ResultTypeEnum ClaculatePlcResult(
      ResultTypeEnum injectRes,
      ResultTypeEnum weightRes,
      ResultTypeEnum supplementaryRes,
      IBatMainModel mainBattery,
      string logHeader
   )
   {
      if (supplementaryRes != ResultTypeEnum.OK)
      {
         $"发送注液量至PLC失败，直接给PLC发结果NG！".LogProcess(logHeader);
         return supplementaryRes;
      }

      // 注液偏少需要优先处理回流
      if (injectRes == ResultTypeEnum.注液量偏少)
      {
         //检查回流次数
         var reworkRes = CheckReworkCount(mainBattery, logHeader);
         if (reworkRes != ResultTypeEnum.OK)
            return reworkRes;
         else
            return injectRes;
      }

      // 后称失败优先返回
      if (weightRes != ResultTypeEnum.OK)
         return weightRes;

      // 注液失败
      if (injectRes != ResultTypeEnum.OK)
         return injectRes;

      return ResultTypeEnum.OK;
   }

   /// <summary>
   /// 更新回流原因
   /// </summary>
   /// <param name="battery"></param>
   protected void AddOrUpdateReworkReason(IBatMainModel battery)
   {
      string reason =
         battery is IBatTestLeakModel testLeak && testLeak.LeakResult.GetResultArea() is ResultArea.NG
            ? ResultTypeEnum.测漏NG.ToString()
            : ResultTypeEnum.注液量偏少.ToString();

      battery.AddOrUpdateRuntimeState(RuntimeStateType.回流原因, reason);
   }

   /// <summary>
   /// 计算回流次数
   /// </summary>
   /// <param name="battery"></param>
   protected void CalculateReinjectCount(IBatMainModel battery, string logHeader)
   {
      //不是回流电池
      if (!battery.GetRuntimeState(RuntimeStateType.回流原因, out string reason))
         return;
      //不是回流电池
      if (!Enum.TryParse<ResultTypeEnum>(reason, true, out var reasonEnum))
      {
         $"当前是回流电池但解析原因失败；{reason};".LogProcess(logHeader);
         return;
      }

      bool hasReason = false;
      switch (reasonEnum)
      {
         case ResultTypeEnum.测漏NG:
            hasReason = true;
            battery.LeakTestNgCount++;
            $"当前电池为测漏NG回流电池，测漏NG回流次数 [{battery.LeakTestNgCount}];".LogProcess(logHeader);
            break;

         case ResultTypeEnum.注液量偏少:
            hasReason = true;
            battery.LowElectorlyteNgCount++;
            $"当前电池为少液回流电池，少液回流次数 [{battery.LowElectorlyteNgCount}];".LogProcess(logHeader);
            break;
      }
      //计算完回流次数后删除原因
      if (hasReason)
         battery.RemoveRuntimeState(RuntimeStateType.回流原因);
   }
}
