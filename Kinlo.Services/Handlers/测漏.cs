namespace Kinlo.Services.Handlers;

/// <summary>
///  测漏
/// </summary>
[DeviceConnec(ProcessTypeEnum.测漏, CommunicationEnum.None)] //指定工艺，可指定多个
public class TestLeakHandler : ServiceHandlerBase
{
   private readonly int _rowCount = 0;
   private readonly int _colCount = 0;

   public TestLeakHandler(IContainer container, IDevice plc, PLCInteractAddressModel plcInteractAddress, CancellationTokenSource taskToken)
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
      ResultTypeEnum productResult = ResultTypeEnum.OK;
      ResultTypeEnum mesResult = ResultTypeEnum._;

      await Parallel.ForAsync(
         0,
         _rowCount,
         new ParallelOptions { MaxDegreeOfParallelism = _rowCount },
         (
            async (lineIndex, _) =>
            {
               PlcToPcLeakLineDTU plcToPcLeakLine = new PlcToPcLeakLineDTU(_colCount);
               var resultLines = _plc.ReadLargeClass(
                  new SignalAddressModel($"{Context.DataAddress.Lable}.Line[{lineIndex}]"),
                  plcToPcLeakLine,
                  _taskLogHeader
               );
               if (resultLines.Status != DeviceStatus.Success || resultLines.Value == null)
               {
                  if (productResult == ResultTypeEnum.OK)
                     productResult = ResultTypeEnum.读取PLC数据失败;
                  $"读取PLC数据失败！！！".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
                  Context.DataAddress.WritePlcResult(productResult, mesResult, _plc, _parameterConfig, _taskLogHeader); //写入PLC结果
                  return;
               }
               $"接收PLC第{lineIndex + 1}行数据：{JsonSerializer.Serialize(resultLines.Value, GenericHelper.SerializerOptions)}".LogProcess(
                  _taskLogHeader
               );

               await Parallel.ForAsync(
                  0,
                  resultLines.Value.Columns.Length,
                  (Func<int, CancellationToken, ValueTask>)(
                     async (columnIndex, _) =>
                     {
                        var leakData = resultLines.Value.Columns[columnIndex];
                        if (leakData == null)
                        {
                           $"第{lineIndex}行{columnIndex}列数据为空！".LogProcess(_taskLogHeader);
                           return;
                        }
                        if (leakData.ID == 0)
                        {
                           $"第{lineIndex}行{columnIndex}列数据为空ID为0！".LogProcess(_taskLogHeader);
                           return;
                        }
                        string logHeader = Context.ToProcessLogHeader(id: leakData.ID);
                        var mainBattery = await _batteryCache.GetByIdAsync(leakData.ID, logHeader); //取缓存
                        if (mainBattery == null)
                        {
                           if (productResult == ResultTypeEnum.OK)
                              productResult = ResultTypeEnum.数据库找不到电池;
                           return;
                        }
                        logHeader = Context.ToProcessLogHeader(id: leakData.ID, barcode: mainBattery.Barcode);
                        IBatTestLeakModel batLeak = (IBatTestLeakModel)mainBattery;

                        batLeak.TestLeakTime = DateTime.Now;
                        batLeak.BeforeTestLeakVacuum = (float)Math.Round(leakData.BeforVacuum, 3);
                        batLeak.AfterTestLeakVacuum = (float)Math.Round(leakData.AfterVacuum, 3);
                        batLeak.TestLeakSetVacuum = (float)Math.Round(leakData.SetVacuum, 3);
                        batLeak.TestLeakActualVacuum = (float)Math.Round(leakData.LeakVacuum, 3);
                        batLeak.TestLeakSetRate = (float)Math.Round(leakData.SetLeak, 3);
                        batLeak.TestLeakSetTime = leakData.SetTime;
                        batLeak.TestLeakActualTime = leakData.ActTime;
                        batLeak.TestLeakVacuumHoldTime = leakData.KeepTime;

                        if (leakData.VcheckResult == 1)
                        {
                           batLeak.LeakResult = ResultTypeEnum.OK;
                        }
                        else
                        {
                           batLeak.LeakResult = ResultTypeEnum.测漏NG;
                        }

                        IBatInjectStationModel injectionStation = (IBatInjectStationModel)mainBattery;
                        injectionStation.LineIndex = (byte)(lineIndex + 1);
                        injectionStation.ColumnIndex = (byte)(columnIndex + 1);

                        #region 上传MES   260107去掉NG上传MES
                        //if (batLeak.LeakResult != ResultTypeEnum.OK)
                        //{
                        //    await MesOutput(mainBattery, logHeader);

                        //    if ((int)mesResult < 21)
                        //        mesResult = mainBattery.MesOutputStatus;
                        //}
                        #endregion

                        if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
                        {
                           productResult = batLeak.LeakResult = ResultTypeEnum.保存数据库失败;
                        }

                        base.AddDisplayData(mainBattery);
                     }
                  )
               );
            }
         )
      );
      Context.DataAddress.WritePlcResult(productResult, mesResult, _plc, _parameterConfig, _taskLogHeader); //写入PLC结果
   }
}
