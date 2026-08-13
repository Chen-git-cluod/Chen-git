using HandyControl.Tools.Extension;

namespace Kinlo.Services.Handlers;

/// <summary>
///  静置站新
/// </summary>
[DeviceConnec(ProcessTypeEnum.静置站, [CommunicationEnum.None])] //指定工艺，可指定多个
public class TankHandlerNew : ServiceHandlerBase
{
   private readonly int _rowCount = 0;
   private readonly int _colCount = 0;

   public TankHandlerNew(IContainer container, IDevice plc, PLCInteractAddressModel plcInteractAddress, CancellationTokenSource taskToken)
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

      TankFloorNew.Length = _rowCount; //设定ID行数
      TankLineNew.Length = _colCount; //设定ID列数
   }

   protected override async Task HandleCore(short plcValue)
   {
      ResultTypeEnum pcResult = ResultTypeEnum.OK;

      #region 读取循环及参数长度
      SignalAddressModel[] tags =
      [
         new SignalAddressModel($"{Context.DataAddress.Lable}.CycleTime", 0), //循环时间
         new SignalAddressModel($"{Context.DataAddress.Lable}.CycleNumber", 0), //循环次数
         new SignalAddressModel($"{Context.DataAddress.Lable}.ParameterLength", 0), //参数长度
      ];
      var deviceResult = _plc.ReadObjects(tags, _taskLogHeader);
      if (deviceResult.Status != DeviceStatus.Success || deviceResult.Value == null || deviceResult.Value.Count < 2)
      {
         pcResult = ResultTypeEnum.读取PLC数据失败;
         Context.DataAddress.WritePlcResult(pcResult, ResultTypeEnum._, _plc, _parameterConfig, _taskLogHeader); //写入PLC结果
         $"循环次数、循环时间或参数长度读取失败！！！".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
         return;
      }
      short cycleDuration = (short)deviceResult.Value[0];
      short cycleNumber = (short)deviceResult.Value[1];
      short paramLen = (short)deviceResult.Value[2];
      $"循环次数:[{cycleNumber}],时长：[{cycleDuration}],参数长度：[{paramLen}]".LogProcess(_taskLogHeader);
      #endregion

      #region 读取静置数据
      List<PlcToPcTankActualNewDTU> tankActuals = new List<PlcToPcTankActualNewDTU>(cycleNumber);
      for (int i = 0; i < cycleNumber; i++)
      {
         var actual = _plc.ReadLargeClass(
            new SignalAddressModel($"{Context.DataAddress.Lable}.Actual[{i}]", 0),
            new PlcToPcTankActualNewDTU(paramLen),
            _taskLogHeader
         );

         if (actual.Status != DeviceStatus.Success)
         {
            if (pcResult == ResultTypeEnum.OK)
               pcResult = ResultTypeEnum.读取PLC数据失败;
            $"加压缸实际数据读取失败，索引[{i}] ！！！".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
         }
         else
         {
            tankActuals.Add(actual!.Value!);
         }
      }
      $"读取到加压缸数据：{JsonSerializer.Serialize(tankActuals)}".LogProcess(_taskLogHeader);

      //筛选
      var totalLen = cycleNumber * paramLen; //总长
      List<float> setPressureList = new(totalLen); //设定压力
      List<float> setRetentionTimeList = new(totalLen); //设定保压时间
      List<float> funcList = new(totalLen); //功能
      List<float> actualPressureList = new(totalLen); //实际压力
      List<float> actualRetentionDurationList = new(totalLen); //实际保压时间
      List<float> arrivalDurationList = new(totalLen); //实际到达时间
      tankActuals.ForEach(x =>
      {
         x.Actuals.ForEach(y =>
         {
            if (y.Func > 0)
            {
               setPressureList.Add(y.SetPressure);
               setRetentionTimeList.Add(y.SetRetentionTime);
               funcList.Add(y.Func);
               actualPressureList.Add(y.ActualPressure);
               actualRetentionDurationList.Add(y.ActualRetentionTime);
               arrivalDurationList.Add(y.ArrivalTime);
            }
         });
      });
      #endregion

      #region 读取 ID
      List<long> batIds = new List<long>();
      //如果数据过长可以分层读取
      //for (int idIndex = 0; idIndex < Context.FloorCount; idIndex++)
      //    var idResult = _plc.ReadLargeObjects<TankFloor>(new SignalAddressModel($"{Context.DataAddress.Lable}.Floor[{idIndex}]"), _taskLogHeader);

      //如果 Floor 为值（二维数组也可以）类型可以直接一次读取完
      // var ids = _plc.ReadLargeObjects<long>(new SignalAddressModel($"{Context.DataAddress.Lable}.Floor"), _taskLogHeader);
      var idResult = _plc.ReadLargeObjects<TankFloorNew>(new SignalAddressModel($"{Context.DataAddress.Lable}.Floor"), _taskLogHeader);
      if (idResult.Status == DeviceStatus.Success && idResult.Value != null && idResult.Value.Count > 0)
      {
         foreach (var lines in idResult.Value)
         {
            foreach (var col in lines.Lines)
            {
               foreach (var id in col.Columns)
               {
                  batIds.Add(id.Id);
               }
            }
         }
      }
      $"读取ID：{string.Join(',', batIds)}".LogProcess(_taskLogHeader);
      #endregion

      if (batIds.Any())
      {
         //如果批量更新上1000条，时间太久，此处可以不等待
         await Parallel.ForEachAsync(
            batIds,
            async (id, _) =>
            {
               if (id == 0)
                  return;
               string logHeader = Context.ToProcessLogHeader(id: id);
               IBatMainModel? mainBattery = await _batteryCache.GetByIdAsync(id, logHeader); //取缓存
               if (mainBattery == null)
               {
                  if (pcResult == ResultTypeEnum.OK)
                     pcResult = ResultTypeEnum.数据库找不到电池;
                  return;
               }
               logHeader = Context.ToProcessLogHeader(id: id, barcode: mainBattery.Barcode);
               var tank = (IBatTankModel)mainBattery;
               tank.TankTime = DateTime.Now;
               tank.TankIndex = Context.DeviceStartIndex;
               tank.CycleNumber = (byte)cycleNumber;
               tank.CycleDuration = cycleDuration;
               tank.Func = funcList.ToStringFormArray();
               tank.SetPressure = setPressureList.ToStringFormArray();
               tank.SetHoldPressureDuration = setRetentionTimeList.ToStringFormArray();
               tank.ActualPressure = actualPressureList.ToStringFormArray();
               tank.ActualHoldPressureDuration = actualRetentionDurationList.ToStringFormArray();
               tank.ActualReachingPressureDuration = arrivalDurationList.ToStringFormArray();

               if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
               {
                  if (pcResult == ResultTypeEnum.OK)
                     pcResult = ResultTypeEnum.保存数据库失败;
                  _isDeviceAlarm = true;
               }
               AddDisplayData(mainBattery);
            }
         );
      }
      Context.DataAddress.WritePlcResult(pcResult, ResultTypeEnum._, _plc, _parameterConfig, _taskLogHeader); //写入PLC结果
   }
}
