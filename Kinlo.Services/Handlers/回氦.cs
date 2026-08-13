namespace Kinlo.Services.Handlers;

/// <summary>
/// 回氦
/// </summary>
[DeviceConnec(ProcessTypeEnum.回氦, [CommunicationEnum.None])] //指定工艺，可指定多个
public class BackHeliumYNHandler : ServiceHandlerBase
{
   /// <summary>
   /// 读取PLC数据的长度，由配置中读取
   /// </summary>
   private readonly int _dataLength = 0;

   public BackHeliumYNHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken)
   {
      var lengthObj = Context.ExtensionProps.FirstOrDefault(x => x.Type == ExtensionType.长度1);
      if (lengthObj != null)
         _dataLength = lengthObj.ValueInt;
      else
      {
         var msg = $"{Context.ProcessesType}数据长度未配置，读取数据会错误，请联系软件工程师！";
         msg.LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
      }
   }

   protected override async Task HandleCore(short plcValue)
   {
      PlcBackHeliumDtu helium = new PlcBackHeliumDtu(_dataLength);
      _plc.ReadLargeClass(Context.DataAddress, helium, _taskLogHeader);
      if (!helium.Actuals.Any(x => x.ID > 0))
      {
         string _msg =
            $"==>>{Context.ProcessesType}<<==\r\n收到启动信号，没收到电池记忆，请联系电气工程师！！！\r\n标签：{JsonSerializer.Serialize(Context.DataAddress)}";

         _ = UIThreadHelper.InvokeOnUiThreadAsync(
            new Action(() => Dialog.Show(new AlarmDialog(new AlarmDialogDto(_msg)), GenericHelper.FullScreenAlarmToken))
         );

         _msg.LogProcess(_taskLogHeader);
         return;
      }
      $"接收到的数据：{JsonSerializer.Serialize(helium, GenericHelper.SerializerOptions)}".LogProcess(_taskLogHeader);
      ResultTypeEnum productResult = ResultTypeEnum.OK;
      ResultTypeEnum mesResult = ResultTypeEnum.OK;
      await Parallel.ForEachAsync(
         helium.Actuals,
         new ParallelOptions { MaxDegreeOfParallelism = Context.DataLength },
         async (actual, _) =>
         {
            if (actual.ID == 0)
               return;
            string logHeader = Context.ToProcessLogHeader(actual.HeliumPosition, actual.ID);
            var mainBattery = await _batteryCache.GetByIdAsync(actual.ID, logHeader); //取缓存
            if (mainBattery == null)
            {
               productResult = ResultTypeEnum.NG;
               _isDeviceAlarm = true;
               return;
            }

            logHeader = Context.ToProcessLogHeader(actual.HeliumPosition, actual.ID, mainBattery.Barcode);
            var backHelium = (IBatBackHeliumModel)mainBattery;
            backHelium.BackHeliumTime = DateTime.Now;
            backHelium.HeliumBeforeVacuumSet1 = helium.Set.HeliumBeforeVacuum1;
            backHelium.HeliumBeforeVacuumSet2 = helium.Set.HeliumBeforeVacuum2;
            backHelium.HeliumAfterVacuumSet = helium.Set.HeliumAfterVacuum;
            backHelium.HeliumBeforeVacuumULSet = helium.Set.HeliumBeforeVacuumUL;
            backHelium.HeliumBeforeVacuumLLSet = helium.Set.HeliumBeforeVacuumLL;
            backHelium.HeliumAfterVacuumULSet = helium.Set.HeliumAfterVacuumUL;
            backHelium.HeliumAfterVacuumLLSet = helium.Set.HeliumAfterVacuumLL;
            backHelium.HeliumBeforeKeepTime1 = helium.Set.HeliumBeforeKeepTime1;
            backHelium.HeliumBeforeKeepTime2 = helium.Set.HeliumBeforeKeepTime2;
            backHelium.HeliumAfterKeepTime = helium.Set.HeliumAfterKeepTime;
            backHelium.VacuumNumberSet = helium.Set.VacuumNumber;
            backHelium.HeliumBeforeVacuumActual1 = actual.HeliumBeforeVacuum1;
            backHelium.HeliumBeforeVacuumActual2 = actual.HeliumBeforeVacuum2;
            backHelium.HeliumAfterVacuumActual = actual.HeliumAfterVacuum;
            backHelium.HeliumTime = actual.HeliumTime;
            backHelium.HeliumStationNo = (byte)actual.HeliumStationNo;
            backHelium.HeliumPosition = (byte)actual.HeliumPosition;

            backHelium.BackHeliumResult = actual.HeliumResult == 1 ? ResultTypeEnum.OK : ResultTypeEnum.回氦NG;

            #region 上传MES   260107去掉NG上传MES
            //if (backHelium.BackHeliumResult != ResultTypeEnum.OK)
            //{
            //    await MesOutput(mainBattery, logHeader);
            //    if ((int)mesResult < 21)
            //        mesResult = mainBattery.MesOutputStatus;
            //}
            #endregion

            if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
            {
               productResult = backHelium.BackHeliumResult = ResultTypeEnum.保存数据库失败;
            }

            //添加百分比显示数据
            AddDisplayData(mainBattery);
         }
      );
      Context.DataAddress.WritePlcResult(productResult, mesResult, _plc, _parameterConfig, _taskLogHeader); //写入PLC结果
   }
}
