namespace Kinlo.Services.Handlers;

[DeviceConnec(ProcessTypeEnum.测短路, [CommunicationEnum.ShortCircuit_RJ6903GX])] //指定工艺，可指定多个
public class HipotRJ6903GXHandler : ServiceHandlerBase
{
   public HipotRJ6903GXHandler(
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

            var scTest = (IBatHipotRJ6903GXModel)mainBattery!;
            if (device == null)
            {
               scTest.HiptionOverallResult = ResultTypeEnum.未找到设备;
               AddDisplayData(mainBattery); //更新界面显示
               _isDeviceAlarm = true;
               $"找不到设备，不给PLC发结果及完成信号，请停机检查！;".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
               return;
            }

            var deviceResult = device.ReadClass<RJ6903GXHipotResultModel>(null!, null!, logHeader, null)!;

            if (deviceResult.Status != DeviceStatus.Success)
               scTest.HiptionOverallResult = deviceResult.Status.ToProductResult();
            else
            {
               var result = deviceResult.Value!;
               scTest.HipotTime = DateTime.Now;
               scTest.HipotIndex = (byte)plcValue;
               scTest.HiptionOverallResult = result.OverallResult;
               //正负极
               scTest.PtoNVd1 = result.PositiveToNegative.Vd1;
               scTest.PtoNVd2 = result.PositiveToNegative.Vd2;
               scTest.PtoNVd3 = result.PositiveToNegative.Vd3;
               scTest.PtoNVpVoltage = result.PositiveToNegative.VpVoltage;
               scTest.PtoNTpTime = result.PositiveToNegative.TpTime;
               scTest.PtoNInsulation = result.PositiveToNegative.Insulation;
               scTest.PtoNResult = result.PositiveToNegative.ChannelResult;
               //正极壳
               scTest.PtoCVd1 = result.PositiveToCase.Vd1;
               scTest.PtoCVd2 = result.PositiveToCase.Vd2;
               scTest.PtoCVd3 = result.PositiveToCase.Vd3;
               scTest.PtoCVpVoltage = result.PositiveToCase.VpVoltage;
               scTest.PtoCTpTime = result.PositiveToCase.TpTime;
               scTest.PtoCInsulation = result.PositiveToCase.Insulation;
               scTest.PtoCResult = result.PositiveToCase.ChannelResult;
               //负极壳
               scTest.NtoCVd1 = result.NegativeToCase.Vd1;
               scTest.NtoCVd2 = result.NegativeToCase.Vd2;
               scTest.NtoCVd3 = result.NegativeToCase.Vd3;
               scTest.NtoCVpVoltage = result.NegativeToCase.VpVoltage;
               scTest.NtoCTpTime = result.NegativeToCase.TpTime;
               scTest.NtoCInsulation = result.NegativeToCase.Insulation;
               scTest.NtoCResult = result.NegativeToCase.ChannelResult;
               scTest.CurvePoint = result.CurvePoint;
            }
            if (isSpotCheck) //点检
            {
               AddDisplayData(mainBattery); //更新界面显示
               plcData.DataAddress.WritePlcResult(scTest.HiptionOverallResult, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
               return;
            }

            #region 上传MES
            if (scTest.HiptionOverallResult != ResultTypeEnum.OK)
            {
               // mainBattery.MesOutputTime = DateTime.Now;
               await MesOutput(mainBattery, logHeader);
            }
            #endregion

            if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
            {
               scTest.HiptionOverallResult = ResultTypeEnum.保存数据库失败;
            }

            AddDisplayData(mainBattery); //更新界面显示
            plcData.DataAddress.WritePlcResult(scTest.HiptionOverallResult, mainBattery.MesOutputStatus, _plc, _parameterConfig, logHeader);
         }
      );
   }
}
