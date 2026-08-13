namespace Kinlo.Services.Handlers;

/// <summary>
/// 回氦打钢珠
/// </summary>
[DeviceConnec(ProcessTypeEnum.回氦打钢珠, [CommunicationEnum.None])] //指定工艺，可指定多个
public class BackHeliumAndBallHandler : ServiceHandlerBase
{
   private readonly int _dataLength = 0;

   public BackHeliumAndBallHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken)
   {
      /// <summary>
      /// 读取PLC数据的长度，由配置中读取
      /// </summary>
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

         UIThreadHelper.InvokeOnUiThreadAsync(
            new Action(() => Dialog.Show(new AlarmDialog(new AlarmDialogDto(_msg)), GenericHelper.FullScreenAlarmToken))
         );
         _msg.LogProcess(_taskLogHeader);
         return;
      }
      $"接收到回氦数据：{JsonSerializer.Serialize(helium, GenericHelper.SerializerOptions)}".LogProcess(_taskLogHeader);

      PlcBallDtu plcBall = new PlcBallDtu(_dataLength);
      _plc.ReadLargeClass(Context.ExtraDataAddress, plcBall, _taskLogHeader);
      $"接收到打钢珠数据：{JsonSerializer.Serialize(plcBall, GenericHelper.SerializerOptions)}".LogProcess(_taskLogHeader);

      ResultTypeEnum productResult = ResultTypeEnum.OK;
      ResultTypeEnum mesResult = ResultTypeEnum.OK;
      await Parallel.ForAsync(
         0,
         helium.Actuals.Length,
         new ParallelOptions { MaxDegreeOfParallelism = Context.DataLength },
         async (dtIndex, _) =>
         // await Parallel.ForEachAsync(helium.Actuals, new ParallelOptions { MaxDegreeOfParallelism = Context.DataLength }, async (actual, _) =>
         {
            var actual = helium.Actuals[dtIndex];
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
            var backHelium = (IBatBackHeliumAndBallModel)mainBattery;
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

            if (plcBall.Actuals.Length > dtIndex)
            {
               var ballActual = plcBall.Actuals[dtIndex];
               backHelium.BallHorizontalOffset = ballActual.deviant1;
               backHelium.BallVerticalOffset = ballActual.deviant2;
               backHelium.BallHorizontalOffsetReslut = ballActual.Results1 switch
               {
                  1 => ResultTypeEnum.OK,
                  2 => ResultTypeEnum.钢珠打偏,
                  4 => ResultTypeEnum.注液孔NG,
                  _ => ResultTypeEnum.钢珠未打,
               };

               if (backHelium.BallHorizontalOffsetReslut != ResultTypeEnum.注液孔NG)
               {
                  backHelium.BackHeliumResult = actual.HeliumResult == 1 ? ResultTypeEnum.OK : ResultTypeEnum.回氦NG;
                  backHelium.BallVerticalOffsetReslut = ballActual.Results2 == 1 ? ResultTypeEnum.OK : ResultTypeEnum.NG;
               }

               var ballReulst =
                  backHelium.BallHorizontalOffsetReslut == ResultTypeEnum.OK && backHelium.BallVerticalOffsetReslut == ResultTypeEnum.OK
                     ? ResultTypeEnum.OK
                     : ResultTypeEnum.NG;
            }
            else
            {
               $"位置[{dtIndex + 1}]有回氦信息，但无打钢珠信息;".LogProcess(_taskLogHeader);
            }

            #region 上传MES
            if (
               backHelium.BackHeliumResult != ResultTypeEnum.OK
               || backHelium.BallHorizontalOffsetReslut != ResultTypeEnum.OK
               || backHelium.BallVerticalOffsetReslut != ResultTypeEnum.OK
            )
            {
               await MesOutput(mainBattery, logHeader);
               if ((int)mesResult < 21)
                  mesResult = mainBattery.MesOutputStatus;
            }
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
