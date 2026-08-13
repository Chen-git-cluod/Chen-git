namespace Kinlo.Services.Handlers;

/// <summary>
/// 注液量发送
/// </summary>
[DeviceConnec(ProcessTypeEnum.注液量发送, [CommunicationEnum.None])] //指定工艺，可指定多个
[DeviceConnec(ProcessTypeEnum.补液量发送, [CommunicationEnum.None])] //指定工艺，可指定多个
public class LiquidInjectionPumpHandler : ServiceHandlerBase
{
   public LiquidInjectionPumpHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken)
   {
      if (Context.ExtraDataAddress.Address == 0)
      {
         $"接收注液量地址：[{Context.ExtraDataAddress.Address}]；注液泵地址错误，将无法正常工作！".LogProcess(
            _taskLogHeader,
            Log4NetLevelEnum.错误,
            true
         );
      }
   }

   protected override async Task HandleCore(short plcValue)
   {
      if (!TryReadPlcData(out List<ReceivePlcDataModel> plcDatas))
         return;

      string logHeader = Context.ToProcessLogHeader(plcValue);
      var plcData = plcDatas[0];
      var device = _devicesConfig.GetRunDevice(Context, plcData.Index);

      if (device == null)
      {
         _isDeviceAlarm = true;
         $"找不到设备，不给PLC发结果及完成信号，请停机检查！;".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return;
      }

      if (Context.ExtraDataAddress.Address == 0)
      {
         _isDeviceAlarm = true;
         $"接收注液量地址：[{Context.ExtraDataAddress.Address}]；注液泵地址错误，不发送注液量，停机退出！".LogProcess(
            logHeader,
            Log4NetLevelEnum.错误,
            true
         );
         return;
      }

      #region 点检
      if (plcData.ID == -1)
      {
         $"点检注液量：{plcData.PLCData} g".LogProcess(logHeader);
         var testResult = HandlerHelper.SendInj(plcData.PLCData, Context.ExtraDataAddress, device, logHeader)
            ? ResultTypeEnum.OK
            : ResultTypeEnum.注液量发送失败;
         $"点检注液量：[{Context.DeviceStartIndex}]，注液量：[{plcData.PLCData}] 发送结果：{testResult}".LogProcess(logHeader);
         await ReadPumpComp(null, device, true, logHeader);
         plcData.DataAddress.WritePlcResult(testResult, ResultTypeEnum._, _plc, _parameterConfig, logHeader);
         return;
      }
      #endregion

      logHeader = Context.ToProcessLogHeader(plcValue, plcData.ID);
      IBatMainModel? batMain = await _batteryCache.GetByIdAsync(plcData.ID, logHeader); //取缓存
      if (batMain == null)
      {
         plcData.DataAddress.WritePlcResult(ResultTypeEnum.数据库找不到电池, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
         return;
      }
      logHeader = Context.ToProcessLogHeader(plcValue, plcData.ID, batMain.Barcode);

      if (plcData.PLCDataType == 1)
      {
         $"注意：PLC强制不校验注液量；注液量：{plcData.PLCData}；".LogProcess(logHeader);
      }
      else
      {
         var testLeak = (IBatTestLeakModel)batMain;
         var afterWeight = (IBatWeightAfterModel)batMain;

         //是否回流电池
         if (batMain.GetRuntimeState(RuntimeStateType.回流原因, out string reason))
         {
            $"{reason}回流电池不校验注液量；PLC注液量：{plcData.PLCData}；".LogProcess(logHeader);
         }
         else
         {
            float target =
               Context.ProcessesType == ProcessTypeEnum.注液量发送
                  ? (float)((IBatWeightBeforeModel)batMain).TargetInjectionVolume
                  : (float)((IBatWeightAfterModel)batMain).TotalInjectionVolumeDeviation;

            if (plcData.PLCData > target + 0.1 || plcData.PLCData < target - 0.1)
            {
               $"注液量不匹配，PLC注液量：[{plcData.PLCData}] 电池注液量：[{target}]".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               plcData.DataAddress.WritePlcResult(ResultTypeEnum.PLC和PC注液量不对应, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
            }
         }
      }
      var injectRangeResult = (Context.ProcessesType, plcData.PLCData) switch
      {
         var (type, inj) when inj < _parameterConfig.RunParameter.VariableInjectionLower => ResultTypeEnum.目标注液量低于注液泵下限,
         var (type, inj) when type == ProcessTypeEnum.注液量发送 && inj > _parameterConfig.RunParameter.VariableInjectionUpper =>
            ResultTypeEnum.目标注液量超注液泵上限,
         var (type, inj) when type == ProcessTypeEnum.补液量发送 && inj > _parameterConfig.RunParameter.ReplenishInjectionUpper =>
            ResultTypeEnum.目标补液量超补液泵上限,
         _ => ResultTypeEnum.OK,
      };

      if (injectRangeResult != ResultTypeEnum.OK)
      {
         $"{injectRangeResult},将发送0至注液泵；ID:{plcData.ID}，注液量：{plcData.PLCData},注液泵上下限:{_parameterConfig.RunParameter.VariableInjectionUpper}~{_parameterConfig.RunParameter.VariableInjectionLower},补液泵上限:{_parameterConfig.RunParameter.ReplenishInjectionUpper}~{_parameterConfig.RunParameter.VariableInjectionLower};".LogProcess(
            logHeader,
            Log4NetLevelEnum.错误
         );
         plcData.PLCData = 0;
      }
      var sendResult = HandlerHelper.SendInj(plcData.PLCData, Context.ExtraDataAddress, device, logHeader)
         ? ResultTypeEnum.OK
         : ResultTypeEnum.注液量发送失败;
      $"[{Context.DeviceStartIndex}头]，注液量：[{plcData.PLCData}] 发送结果：{sendResult}".LogProcess(logHeader);

      if (_parameterConfig.FunctionEnable.IsEnableReadPumpTemperature) //读取泵补偿相关
      {
         var readResult = await ReadPumpComp(batMain, device, false, logHeader);
      }

      var sendPlc = (injectRangeResult, sendResult) switch
      {
         var r when r.injectRangeResult == ResultTypeEnum.OK && r.sendResult == ResultTypeEnum.OK => ResultTypeEnum.OK,
         var r when r.injectRangeResult != ResultTypeEnum.OK => r.injectRangeResult,
         var r when r.sendResult != ResultTypeEnum.OK => r.sendResult,
         _ => ResultTypeEnum.NG,
      };
      plcData.DataAddress.WritePlcResult(sendPlc, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
   }

   async Task<ResultTypeEnum> ReadPumpComp(IBatMainModel? mainBattery, IDevice device, bool isTest, string logHeader)
   {
      try
      {
         StringBuilder sb = new StringBuilder();
         var temperature = device.ReadValue<float>(new SignalAddressModel("", 1018), logHeader); //读取注液泵温度
         sb.Append($"取注液泵温度{(temperature.Status == DeviceStatus.Success ? $"[{temperature.Value}]" : "失败")}");

         var tempComp = device.ReadValue<float>(new SignalAddressModel("", 1010), logHeader); //读取温度补偿
         sb.Append($"取注液泵温度补偿{(tempComp.Status == DeviceStatus.Success ? $"[{tempComp.Value}]" : "失败")}");

         var processComp = DeviceResult<float>.Success(0.0f); //工艺补偿,后面要改为读取泵
         sb.Append($"取注液泵工艺补偿{(processComp.Status == DeviceStatus.Success ? $"[{processComp.Value}]" : "失败")}");

         var CompMode = DeviceResult<short>.Success(0); //补偿模式，后面要改为读取泵
         sb.Append($"取注液泵补偿模式{(processComp.Status == DeviceStatus.Success ? $"[{CompMode.Value}]" : "失败")}");
         // var inject = device.ReadValue<float>(new SignalAddressModel("", 1004), logHeader); //单脉冲注液量

         sb.ToString().LogProcess(logHeader);

         if (!isTest && mainBattery != null) //非点检时更新数据库
         {
            var injectionData = new InjectionDataModel(); //记录注液泵相关
            injectionData.Id = mainBattery.Id;
            injectionData.Barcode = mainBattery.Barcode;
            injectionData.InjectionTime = DateTime.Now;

            var batInject = (IBatInjectStationModel)mainBattery!;
            if (temperature.Status == DeviceStatus.Success)
               injectionData.Temperature = batInject.InjectTemperature = (float)temperature.Value;
            if (tempComp.Status == DeviceStatus.Success)
               injectionData.TempComp = batInject.InjectingOffset = tempComp.Value;
            if (processComp.Status == DeviceStatus.Success)
               injectionData.ProcessComp = processComp.Value;
            if (CompMode.Status == DeviceStatus.Success)
               injectionData.CompMode = CompMode.Value;

            var upDic = new Dictionary<string, object> //更新指定列
            {
               { nameof(IBatMainModel.Id), mainBattery.Id },
               { nameof(IBatInjectStationModel.InjectTemperature), batInject.InjectTemperature },
               { nameof(IBatInjectStationModel.InjectingOffset), batInject.InjectingOffset },
            };

            if (!await _sugarDB.UpdateColumnsAsync(upDic, mainBattery.Id, mainBattery.Barcode, logHeader))
            {
               $"更新回流次数至数据库失败!".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               return ResultTypeEnum.保存数据库失败;
            }

            //写入注液量表,不等待
            _ = _sugarDB.InsertOrUpdateInjectionAsync(injectionData);
         }

         return ResultTypeEnum.OK;
      }
      catch (Exception e)
      {
         $"取注液泵温度和Offset异常:{e}".LogProcess(logHeader, Log4NetLevelEnum.错误);
         return ResultTypeEnum.异常;
      }
   }
}
