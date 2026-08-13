using Kinlo.Common.Configurations;

namespace Kinlo.Services.Handlers;

//[DeviceConnec(ProcessTypeEnum.手动补液, CommunicationEnum.Scale_Pris_TC06, CommunicationEnum.Scale_KZ313_RTU)]
public class WeightReplenishHandler : ServiceHandlerBase
{
   List<int> _alarmDevice = new List<int>();

   public WeightReplenishHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken) { }

   //弃用，直接使用后称重模块
   protected override async Task HandleCore(short plcValue)
   {
      //_alarmDevice.Clear();
      //if (!TryReadPlcData(out List<ReceivePlcDataModel> plcDatas))
      //    return;

      //await Parallel.ForEachAsync(plcDatas, new ParallelOptions { MaxDegreeOfParallelism = Context.DataLength }, async (plcData, _) =>
      //{
      //    bool isSpotCheck = plcData.ID == -1;//是否为点检
      //    string logHeader = Context.ToProcessLogHeader(plcData.Index, plcData.ID);
      //    IBatMainModel? mainBattery = isSpotCheck switch
      //    {
      //        true => Activator.CreateInstance(_displayDataCollection.CompleteBatteryDatas.RuntimeBatteryModel) as IBatMainModel,//点检
      //        _ => await _batteryCache.GetByIdAsync(plcData.ID, logHeader)//取缓存
      //    };
      //    if (mainBattery == null)
      //    {
      //        if (isSpotCheck) "创建点检电池对象失败；".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      //        plcData.DataAddress.WritePlcResult(ResultTypeEnum.数据库找不到电池, ResultTypeEnum._, _plc, _parameterConfig, logHeader);//写入PLC结果
      //        return;
      //    }
      //    logHeader = Context.ToProcessLogHeader(plcData.Index, plcData.ID, mainBattery.Barcode);
      //    var aftWeight = (IBatWeightAfterModel)mainBattery;
      //    OperationResult<double> deviceResult = OperationResult<double>.Failure("默认");
      //    if (_parameterConfig.FunctionEnable.IsEmptyLoadMode)  //空跑则不去获取设备数据
      //    {
      //        $"开启空载模式，随机生成重量;".LogProcess(logHeader, Log4NetLevelEnum.警告);
      //        var weiging = new Random().Next((int)(_parameterConfig.RunParameter.IncomingWeightLower + _parameterConfig.RunParameter.InjectionStandard), (int)(_parameterConfig.RunParameter.IncomingWeightUpper + _parameterConfig.RunParameter.InjectionStandard));
      //        deviceResult = OperationResult<double>.Success(weiging);
      //    }
      //    else
      //    {
      //        var device = _devicesConfig.GetRunDevice(Context, plcData.Index);
      //        if (device == null)
      //        {
      //            aftWeight.InjectResult = ResultTypeEnum.未找到设备;
      //            AddDisplayData(mainBattery); ;//更新界面显示
      //            _isDeviceAlarm = true;
      //            $"找不到设备，不给PLC发结果及完成信号，请停机检查！;".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      //            return;
      //        }
      //        deviceResult = HandlerHelper.GetWeiging(device, _parameterConfig, logHeader);
      //    }

      //    #region 点检
      //    if (isSpotCheck)
      //    {
      //        ResultTypeEnum _checkResult = deviceResult switch
      //        {
      //            var r when !r.IsSuccess => ResultTypeEnum.NG,
      //            _ => BatteryWeightValidator.IncomingWeightRangeCheck(deviceResult.Value, BatteryWeightValidator.GetBeforWeightRange(_parameterConfig), _parameterConfig, logHeader)
      //        };
      //        plcData.DataAddress.WritePlcResult(_checkResult, ResultTypeEnum._, _plc, _parameterConfig, logHeader);
      //        return;
      //    }
      //    #endregion
      //    if (deviceResult.Value < _weightWarningValue)
      //        _alarmDevice.Add(plcData.Index);//小于20g报警
      //    if (_parameterConfig.FunctionEnable.IsEnableCurrentRange)
      //    {
      //        aftWeight.InjectionVolumeRange = _parameterConfig.GetInjectionRange();//注液范围
      //        aftWeight.AfterWeighingRange = _parameterConfig.GetFinalWeightRange();//最终重量范围
      //    }

      //    await RefillHandle(mainBattery, deviceResult, plcData, logHeader);

      //});
      //if (_alarmDevice.Any())
      //{
      //    $"{string.Join(',', _alarmDevice.Select(x => $"{x}号"))} 称重不稳定！".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
      //    //_isDeviceAlarm = true;
      //    //WritePlcSingle(1, _plcSignalConfig.PLCAlarmAddresses.Alarm_RefillWeighing, _taskLogHeader); 去掉报警
      //}
   }
}
