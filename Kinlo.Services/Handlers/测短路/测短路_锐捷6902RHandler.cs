namespace Kinlo.Services.Handlers;

[DeviceConnec(ProcessTypeEnum.测短路, [CommunicationEnum.ShortCircuit_RJ6902R])] //指定工艺，可指定多个
public class RJ6902RHandler : ServiceHandlerBase
{
   public RJ6902RHandler(IContainer container, IDevice plc, PLCInteractAddressModel plcInteractAddress, CancellationTokenSource taskToken)
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

            IBatShortCircuitRj6902rModel shortCircuitTestRJ = (IBatShortCircuitRj6902rModel)mainBattery!;
            if (device == null)
            {
               shortCircuitTestRJ.ShortCircuitResult = ResultTypeEnum.未找到设备;
               AddDisplayData(mainBattery);
               ; //更新界面显示
               _isDeviceAlarm = true;
               $"找不到设备，不给PLC发结果及完成信号，请停机检查！;".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               return;
            }
            DeviceResult<RJ6902RResultModel> deviceResult = DeviceResult<RJ6902RResultModel>.Failure("默认");
            try
            {
               $"{(plcData.PLCDataType == 3 ? "负极外壳测试" : plcData.PLCData == 2 ? "正极外壳测试" : "正负极测试")}开始检测".LogProcess(
                  logHeader
               );
               deviceResult = device.ReadClass<RJ6902RResultModel>(null, null, logHeader);
            }
            catch (Exception ex)
            {
               deviceResult = DeviceResult<RJ6902RResultModel>.Failure(DeviceStatus.异常, "默认");
               $"获取结果时发生异常：{ex}".LogProcess(logHeader);
            }

            shortCircuitTestRJ.ShortCircuitTestRJTime = DateTime.Now;
            shortCircuitTestRJ.ShortCircuitTestRJIndex = (byte)plcValue;
            switch (plcData.PLCData)
            {
               case 3: //负极外壳测试
                  break;
               case 2: //正极外壳测试
                  break;
               default: //正负极测试
                  if (deviceResult.Status != DeviceStatus.Success)
                  {
                     shortCircuitTestRJ.ShortCircuitResult = deviceResult.Status.ToProductResult();
                  }
                  else
                  {
                     var rj6902RResult = deviceResult.Value!;
                     $"短路测试总结果:[{rj6902RResult.总结果}];获取结果原始值：[{deviceResult.Value?.TestMsg}]".LogProcess(logHeader);

                     shortCircuitTestRJ.ResistanceTestValue = rj6902RResult.电阻测试数据;
                     shortCircuitTestRJ.BoostTime = rj6902RResult.升压时间 / 10.00;
                     shortCircuitTestRJ.FallOne = rj6902RResult.跌落1;
                     shortCircuitTestRJ.FallTwo = rj6902RResult.跌落2;
                     shortCircuitTestRJ.ShellShortCircuitVoltage = rj6902RResult.VP电压;
                     shortCircuitTestRJ.ShellShortCircuitOpenCircuitResult = rj6902RResult.开路结果;
                     shortCircuitTestRJ.ShellShortCircuitFallOneResult = rj6902RResult.跌落1结果;
                     shortCircuitTestRJ.ShellShortCircuitFallTwoResult = rj6902RResult.跌落2结果;
                     shortCircuitTestRJ.ShellShortCircuitTLResult = rj6902RResult.TL结果;
                     shortCircuitTestRJ.ShellShortCircuitTHResult = rj6902RResult.TH结果;
                     shortCircuitTestRJ.ShellShortCircuitResistanceTestResult = rj6902RResult.电阻测试结果;
                     //如果要测外壳，此处重写
                     shortCircuitTestRJ.ShortCircuitResult = rj6902RResult.ConverterRJ6902R();
                  }
                  break;
            }

            if (isSpotCheck) //点检
            {
               AddDisplayData(mainBattery); //更新界面显示
               plcData.DataAddress.WritePlcResult(
                  shortCircuitTestRJ.ShortCircuitResult,
                  ResultTypeEnum._,
                  _plc,
                  _parameterConfig,
                  logHeader
               ); //写入PLC结果
               return;
            }

            #region 上传MES
            if (shortCircuitTestRJ.ShortCircuitResult != ResultTypeEnum.OK)
            {
               // mainBattery.MesOutputTime = DateTime.Now;
               await MesOutput(mainBattery, logHeader);
            }
            #endregion

            if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
            {
               shortCircuitTestRJ.ShortCircuitResult = ResultTypeEnum.保存数据库失败;
            }

            AddDisplayData(mainBattery);
            plcData.DataAddress.WritePlcResult(
               shortCircuitTestRJ.ShortCircuitResult,
               mainBattery.MesOutputStatus,
               _plc,
               _parameterConfig,
               logHeader
            ); //写入PLC结果
         }
      );
   }
}
