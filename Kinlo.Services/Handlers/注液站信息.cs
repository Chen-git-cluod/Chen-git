namespace Kinlo.Services.Handlers;

/// <summary>
///  注液
/// </summary>
[DeviceConnec(ProcessTypeEnum.注液, [CommunicationEnum.None])] //指定工艺，可指定多个
public class InjectStationHandler : ServiceHandlerBase
{
   private readonly int _rowCount = 0;
   private readonly int _colCount = 0;

   public InjectStationHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken)
   {
      StringBuilder sb = new StringBuilder();
      var rowObj = Context.ExtensionProps.FirstOrDefault(x => x.Type == ExtensionType.行数);
      var colObj = Context.ExtensionProps.FirstOrDefault(x => x.Type == ExtensionType.列数);

      if (rowObj != null)
         _rowCount = rowObj.ValueInt;
      else
         sb.Append($"[行数]");

      if (colObj != null)
         _colCount = colObj.ValueInt;
      else
         sb.Append($"[列数]");

      if (sb.Length > 0)
         $"{Context.ProcessesType}{sb}未配置，读取数据会错误，请联系软件工程师！".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
   }

   protected override async Task HandleCore(short plcValue)
   {
      ResultTypeEnum pcResult = ResultTypeEnum.OK;
      List<PlcToPcInjectionLineDTU> plcToPcInjectionLines = new();
      for (int i = 0; i < _rowCount; i++)
      {
         PlcToPcInjectionLineDTU line = new PlcToPcInjectionLineDTU(_colCount);
         _plc.ReadLargeClass(new SignalAddressModel($"{Context.DataAddress.Lable}.Line[{i}]", 0), line, _taskLogHeader);
         plcToPcInjectionLines.Add(line);
      }
      string logHeader1 = Context.ToProcessLogHeader();
      $"接收PLC行列数据：{JsonSerializer.Serialize(plcToPcInjectionLines, GenericHelper.SerializerOptions)}".LogProcess(logHeader1);
      if (!plcToPcInjectionLines.Any(x => x.Columns.Any(y => y.ID > 0)))
      {
         string msg =
            $"==>>{Context.ProcessesType}<<==收到启动信号，没收到电池记忆，请联系电气工程师！！！标签：{JsonSerializer.Serialize(Context.DataAddress)}";
         UIThreadHelper.InvokeOnUiThreadAsync(
            new Action(() => Dialog.Show(new AlarmDialog(new AlarmDialogDto(msg)), GenericHelper.FullScreenAlarmToken))
         );
         msg.LogProcess(logHeader1);

         pcResult = ResultTypeEnum.PLC无记忆;
         Context.DataAddress.WritePlcResult(pcResult, ResultTypeEnum._, _plc, _parameterConfig, logHeader1); //写入PLC结果
         return;
      }

      SignalAddressModel[] tags =
      [
         new SignalAddressModel($"{Context.DataAddress.Lable}.TrayCode", 0),
         new SignalAddressModel($"{Context.DataAddress.Lable}.CupCode", 0),
      ];
      var tagData = _plc.ReadObjects(tags, _taskLogHeader);
      if (tagData.Status != DeviceStatus.Success || tagData.Value == null || tagData.Value.Count < 2)
      {
         pcResult = ResultTypeEnum.读取PLC数据失败;
         Context.DataAddress.WritePlcResult(pcResult, ResultTypeEnum._, _plc, _parameterConfig, _taskLogHeader); //写入PLC结果
         $"套杯、托盘读取失败！！！".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
         return;
      }

      string trayCode = tagData.Value[0] as string ?? "";
      string cupCode = tagData.Value[1] as string ?? "";

      await Parallel.ForAsync(
         0,
         plcToPcInjectionLines.Count,
         new ParallelOptions { MaxDegreeOfParallelism = plcToPcInjectionLines.Count },
         async (lineIndex, _) =>
         {
            var line = plcToPcInjectionLines[lineIndex];
            if (line == null)
            {
               $"第{lineIndex}行数据为空！".LogProcess(logHeader1);
               return;
            }

            await Parallel.ForAsync(
               0,
               line.Columns.Length,
               async (columnIndex, _) =>
               {
                  var logHeader = Context.ToProcessLogHeader(lineIndex * columnIndex);
                  var stationData = line.Columns[columnIndex];
                  if (stationData == null)
                  {
                     $"第{lineIndex}行{columnIndex}列数据为空！".LogProcess(logHeader);
                     return;
                  }
                  if (stationData.ID == 0)
                  {
                     $"第{lineIndex}行{columnIndex}列数据为空ID为0！".LogProcess(logHeader);
                     return;
                  }
                  logHeader = Context.ToProcessLogHeader(lineIndex * columnIndex, stationData.ID);
                  var mainBattery = await _batteryCache.GetByIdAsync(stationData.ID, logHeader); //取缓存
                  if (mainBattery == null)
                  {
                     if (pcResult == ResultTypeEnum.OK)
                        pcResult = ResultTypeEnum.数据库找不到电池;
                     return;
                  }
                  logHeader = Context.ToProcessLogHeader(lineIndex * columnIndex, stationData.ID, mainBattery.Barcode);
                  IBatInjectStationModel injectionStation = (IBatInjectStationModel)mainBattery;
                  injectionStation.InjectElectrolyteTime = DateTime.Now;
                  injectionStation.LineIndex = (byte)(lineIndex + 1);
                  injectionStation.ColumnIndex = (byte)(columnIndex + 1);
                  injectionStation.InjectPumpNo = (byte)stationData.InjectionPumpNo;
                  injectionStation.InjectStationNo = (byte)stationData.InjectionStationNo;
                  injectionStation.InjectNozzleNo = (byte)stationData.InjectionNozzle;
                  injectionStation.InjectedProcessDuration = stationData.InjectionTime;
                  injectionStation.InjectedDuration = stationData.InjectionDuration;
                  injectionStation.TrayCode = trayCode;
                  injectionStation.CupCode = cupCode;
                  injectionStation.TestLeakTime = DateTime.Now;
                  injectionStation.BeforeTestLeakVacuum = (float)Math.Round(stationData.LeakBeforeVacuum, 3);
                  injectionStation.AfterTestLeakVacuum = (float)Math.Round(stationData.LeakAfterVauum, 3);
                  injectionStation.TestLeakActualVacuum = (float)Math.Round(stationData.LeakVacuum, 3);
                  injectionStation.TestLeakSetRate = (float)Math.Round(stationData.LeakPressureDifference, 3);
                  injectionStation.TestLeakActualTime = (short)stationData.LeakRetentionTime;
                  injectionStation.TestLeakVacuumHoldTime = (short)stationData.LeakVacuumTime;
                   if (stationData.LeakResult == 1)
                       injectionStation.LeakResult = ResultTypeEnum.OK;
                   else
                       injectionStation.LeakResult = ResultTypeEnum.测漏NG;
                   if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
                  {
                     if (pcResult == ResultTypeEnum.OK)
                        pcResult = ResultTypeEnum.保存数据库失败;
                  }
                  AddDisplayData(mainBattery);
               }
            );
         }
      );
      Context.DataAddress.WritePlcResult(pcResult, ResultTypeEnum._, _plc, _parameterConfig, _taskLogHeader); //写入PLC结果
   }
}
