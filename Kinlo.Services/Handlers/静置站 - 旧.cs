namespace Kinlo.Services.Handlers;

/// <summary>
///  静置站
/// </summary>
//[DeviceConnec(ProcessTypeEnum.静置站, [CommunicationEnum.None])] //指定工艺，可指定多个
public class TankHandler : ServiceHandlerBase
{
   public TankHandler(IContainer container, IDevice plc, PLCInteractAddressModel plcInteractAddress, CancellationTokenSource taskToken)
      : base(container, plc, plcInteractAddress, taskToken) { }

   protected override async Task HandleCore(short plcValue)
   {
      ResultTypeEnum pcResult = ResultTypeEnum.OK;
      SignalAddressModel[] tags =
      [
         new SignalAddressModel($"{Context.DataAddress.Lable}.CycleTime", 0),
         new SignalAddressModel($"{Context.DataAddress.Lable}.CycleNumber", 0),
      ];
      var deviceResult = _plc.ReadObjects(tags, _taskLogHeader);
      if (deviceResult.Status != DeviceStatus.Success || deviceResult.Value == null || deviceResult.Value.Count < 2)
      {
         pcResult = ResultTypeEnum.读取PLC数据失败;
         Context.DataAddress.WritePlcResult(pcResult, ResultTypeEnum._, _plc, _parameterConfig, _taskLogHeader); //写入PLC结果
         $"加压缸ID、循环次数或循环时间读取失败！！！".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
         return;
      }
      short _cycleDuration = (short)deviceResult.Value[0];
      short cycleNumber = (short)deviceResult.Value[1];
      $"循环次数:[{cycleNumber}],时间：[{_cycleDuration}]".LogProcess(_taskLogHeader);

      PlcToPcTankSettingDTU settings = new PlcToPcTankSettingDTU();
      if (
         _plc.ReadClass(new SignalAddressModel($"{Context.DataAddress.Lable}.Setting", 0), settings, _taskLogHeader).Status
         != DeviceStatus.Success
      )
      {
         pcResult = ResultTypeEnum.读取PLC数据失败;
         Context.DataAddress.WritePlcResult(pcResult, ResultTypeEnum._, _plc, _parameterConfig, _taskLogHeader); //写入PLC结果
         $"加压缸设定数据读取失败！！！".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
         return;
      }
      $"读取加压缸设定值:[{JsonSerializer.Serialize(settings)}]".LogProcess(_taskLogHeader, Log4NetLevelEnum.信息);
      //筛选
      var siftDatas = settings.Datas.Where(x => x.Func > 0); //1:真空，2：正压：3：大气
      var siftSettingPressure = string.Join(",", siftDatas.Select(x => x.SetPressure));
      var siftSeeingRetentionTime = string.Join(",", siftDatas.Select(x => x.SetRetentionTime));
      var siftFunc = string.Join(",", siftDatas.Select(x => x.Func));
      $"筛选后,设定压力值：[{siftSettingPressure}]，设定保压时间：[{siftSeeingRetentionTime}]，功能项：[{siftFunc}]".LogProcess(
         _taskLogHeader,
         Log4NetLevelEnum.信息
      );

      List<PlcToPcTankActualDTU> tankActuals = new List<PlcToPcTankActualDTU>();
      for (int i = 0; i < cycleNumber; i++)
      {
         var actual = _plc.ReadClass<PlcToPcTankActualDTU>(
            new SignalAddressModel($"{Context.DataAddress.Lable}.Actual[{i}]", 0),
            null,
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
            tankActuals.Add(actual.Value);
         }
      }
      //筛选
      List<float> actualPressureList = new();
      List<short> actualRetentionDurationList = new();
      List<short> arrivalDurationList = new();
      tankActuals.ForEach(x =>
      {
         actualPressureList.AddRange(x.Actuals.Where((_, i) => settings.Datas[i].Func > 0).Select(y => y.ActualPressure));
         actualRetentionDurationList.AddRange(x.Actuals.Where((_, i) => settings.Datas[i].Func > 0).Select(y => y.ActualRetentionTime));
         arrivalDurationList.AddRange(x.Actuals.Where((_, i) => settings.Datas[i].Func > 0).Select(y => y.ArrivalTime));
      });
      string actualPressure = string.Join(",", actualPressureList);
      string actualRetentionDuration = string.Join(",", actualRetentionDurationList);
      string arrivalDuration = string.Join(",", arrivalDurationList);
      $"筛选后,实际压力值：[{actualPressure}]，实际保压时间：[{actualRetentionDuration}]，实际到达时间：[{arrivalDuration}]".LogProcess(
         _taskLogHeader,
         Log4NetLevelEnum.信息
      );

      List<long> batIds = new List<long>();
      //如果数据过长可以分层读取
      //for (int idIndex = 0; idIndex < Context.FloorCount; idIndex++)
      //    var idResult = _plc.ReadLargeObjects<TankFloor>(new SignalAddressModel($"{Context.DataAddress.Lable}.Floor[{idIndex}]"), _taskLogHeader);

      //如果 Floor 为值（二维数组也可以）类型可以直接一次读取完
      // var ids = _plc.ReadLargeObjects<long>(new SignalAddressModel($"{Context.DataAddress.Lable}.Floor"), _taskLogHeader);

      var idResult = _plc.ReadLargeObjects<TankFloor>(new SignalAddressModel($"{Context.DataAddress.Lable}.Floor"), _taskLogHeader);
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
               tank.CycleDuration = _cycleDuration;
               tank.Func = siftFunc;
               tank.SetPressure = siftSettingPressure;
               tank.SetHoldPressureDuration = siftSeeingRetentionTime;
               tank.ActualPressure = actualPressure;
               tank.ActualHoldPressureDuration = actualRetentionDuration;
               tank.ActualReachingPressureDuration = arrivalDuration;

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
