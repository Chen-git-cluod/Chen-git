namespace Kinlo.Services.Handlers;

[DeviceConnec(
   ProcessTypeEnum.测短路,
   [CommunicationEnum.ShortCircuit_AC3200, CommunicationEnum.ShortCircuit_Ainuo_ANBTS7201]
) //临时使用
] //指定工艺，可指定多个
public class ACShortCircuitTesterHandler : ServiceHandlerBase
{
   public ACShortCircuitTesterHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken) { }

   protected override async Task HandleCore(short plcValue)
   {
      if (!TryReadPlcData(out List<ReceivePlcDataModel> plcDatas))
         return;

      await Parallel.ForEachAsync(
         plcDatas,
         new ParallelOptions { MaxDegreeOfParallelism = Context.DataLength },
         async (plcData, _) =>
         {
            bool isSpotCheck = plcData.ID == -1; //是否为点检
            string logHeader = Context.ToProcessLogHeader(plcValue, plcData.ID);
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
            logHeader = Context.ToProcessLogHeader(plcValue, plcData.ID, mainBattery.Barcode);
            var device = _devicesConfig.GetRunDevice(Context, plcData.Index);

            var shortCircuitTestAC = (IBatHipotAc3200Model)mainBattery!;
            if (device == null)
            {
               shortCircuitTestAC.HipotResult = ResultTypeEnum.未找到设备;
               AddDisplayData(mainBattery); //更新界面显示
               _isDeviceAlarm = true;
               $"找不到设备，不给PLC发结果及完成信号，请停机检查！;".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               ;
               return;
            }

            int optionType = 0;
            var deviceResult = device.ReadClass<Ac3200HipotResultModel>(
               null,
               null,
               logHeader,
               new DeviceOperationOptions { OperationType = optionType }
            )!;

            if (deviceResult.Status != DeviceStatus.Success)
               shortCircuitTestAC.HipotResult = deviceResult.Status.ToProductResult();
            else
            {
               var result = deviceResult.Value!;
               shortCircuitTestAC.HipotTime = DateTime.Now;
               shortCircuitTestAC.HipotIndex = (byte)plcValue;
               shortCircuitTestAC.HipotPulseResult = result.HipotPulseResult;
               shortCircuitTestAC.HipotVpVoltage = result.HipotVpVoltage;
               shortCircuitTestAC.HipotFallOne = result.HipotFallOne;
               shortCircuitTestAC.HipotFallTwo = result.HipotFallTwo;
               shortCircuitTestAC.HipotFallThree = result.HipotFallThree;
               shortCircuitTestAC.HipotPulseTp = result.HipotPulseTp;
               shortCircuitTestAC.ResistanceTestResult = result.ResistanceTestResult;
               shortCircuitTestAC.InsulationTestValue = result.InsulationTestValue;
               shortCircuitTestAC.CapacitorsResult = result.CapacitorsResult;
               shortCircuitTestAC.Capacitors = result.Capacitors;
               shortCircuitTestAC.CurvePoint = result.CurvePoint;
               shortCircuitTestAC.HipotResult = result.HipotResult;
            }
            if (isSpotCheck) //点检
            {
               AddDisplayData(mainBattery); //更新界面显示
               plcData.DataAddress.WritePlcResult(shortCircuitTestAC.HipotResult, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
               return;
            }

            #region 上传MES
            if (shortCircuitTestAC.HipotResult != ResultTypeEnum.OK)
            {
               // mainBattery.MesOutputTime = DateTime.Now;
               await MesOutput(mainBattery, logHeader);
            }
            #endregion

            if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
            {
               shortCircuitTestAC.HipotResult = ResultTypeEnum.保存数据库失败;
            }

            AddDisplayData(mainBattery); //更新界面显示
            plcData.DataAddress.WritePlcResult(
               shortCircuitTestAC.HipotResult,
               mainBattery.MesOutputStatus,
               _plc,
               _parameterConfig,
               logHeader
            );
         }
      );
   }
}
