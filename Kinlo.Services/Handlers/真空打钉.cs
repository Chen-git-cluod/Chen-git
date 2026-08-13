namespace Kinlo.Services.Handlers;

/// <summary>
///
/// </summary>
[DeviceConnec(ProcessTypeEnum.真空打钉, [CommunicationEnum.None])] //指定工艺，可指定多个
public class NailVacuumHandler : ServiceHandlerBase
{
   public NailVacuumHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken) { }

   protected override async Task HandleCore(short plcValue)
   {
      PlcToPcVacuumNailDtu plcToPcVacuumNail = new PlcToPcVacuumNailDtu();
      _plc.ReadClass(Context.DataAddress, plcToPcVacuumNail, _taskLogHeader);

      $"记忆ID：{JsonSerializer.Serialize(plcToPcVacuumNail)}".LogProcess(_taskLogHeader);
      await Parallel.ForAsync(
         0,
         plcToPcVacuumNail.Id.Length,
         async (i, _) =>
         //  await Parallel.ForEachAsync(plcToPcVacuumNail.Id, new ParallelOptions { MaxDegreeOfParallelism = Context.DataLength }, async (plcData, _) =>
         {
            long id = plcToPcVacuumNail.Id[i];
            if (id == 0)
               return;
            string logHeader = Context.ToProcessLogHeader(plcValue + i, id);
            var mainBattery = await _batteryCache.GetByIdAsync(id, logHeader); //取缓存
            if (mainBattery == null)
            {
               Context.DataAddress.WritePlcResult(ResultTypeEnum.数据库找不到电池, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
               return;
            }

            logHeader = Context.ToProcessLogHeader(plcValue + i, id, mainBattery.Barcode);
            ResultTypeEnum result = ResultTypeEnum.OK;
            var nail = (IBatVacuumNailModel)mainBattery;
            nail.VacuumNailTime = DateTime.Now;
            nail.SetVaccumValue = (float)Math.Round(plcToPcVacuumNail.SetVaccumValue, 3);
            nail.KeepPressureTime = (float)Math.Round(plcToPcVacuumNail.KeepPressureTime, 3);
            nail.BeforeKeepPressureVacuumValue = (float)Math.Round(plcToPcVacuumNail.BeforeKeepPressureVacuumValue[i], 3);
            nail.AfterKeepPressureVacuumValue = (float)Math.Round(plcToPcVacuumNail.AfterKeepPressureVacuumValue[i], 3);

            result = ResultTypeEnum.OK;
            if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
            {
               result = ResultTypeEnum.保存数据库失败;
            }

            AddDisplayData(mainBattery); //更新界面显示
            Context.DataAddress.WritePlcResult(result, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
         }
      );
   }
}
