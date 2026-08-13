namespace Kinlo.Services.PeriodicTasks;

public partial class PeriodicTasksHelper
{
   bool isSync = false;

   public async Task PlcProductionSyncService(DateTime t, IContainer container)
   {
      try
      {
         if (t.Second % 10 != 0 || isSync)
            return;
         isSync = true;
         PLCSignalConfig plcSignalConfig = container.Get<PLCSignalConfig>();
         var plcDataTag = plcSignalConfig.CustomPlcInteractAddresses.FirstOrDefault(x =>
            x.CustomInteractName == CustomInteractNameEnum.PC至PLC生产数据
         );
         if (plcDataTag == null || !plcDataTag.IsEnable)
         {
            //"[同步PLC生产数据]未配置PLC生产数据交互地址！".LogRun(Log4NetLevelEnum.警告);
            return;
         }
         var devicesConfig = container.Get<DevicesConfig>();

         var plcs = devicesConfig.GetRunDevices(x => x.DeviceInfo.ProcessesType == ProcessTypeEnum.PLC);
         if (plcs != null && plcs.Count > 0)
         {
            foreach (var item in plcs)
            {
               PlcProductionSync(container, (IPLC)item, plcDataTag);
            }
         }
         else
         {
            var clients = devicesConfig.DeviceList.Where(x => x.ProcessesType == ProcessTypeEnum.PLC);
            foreach (var client in clients)
            {
               await client.WithCreatedDeviceAsync(async d => await Task.Run(() => PlcProductionSync(container, (IPLC)d, plcDataTag)));
            }
         }
      }
      catch (Exception ex)
      {
         $"[同步PLC生产数据]异常：{ex}！".LogRun(Log4NetLevelEnum.警告);
      }
      finally
      {
         isSync = false;
      }
   }

   /// <summary>
   /// 产量同步至PLC
   /// </summary>
   /// <param name="container"></param>
   /// <param name="plc"></param>
   /// <param name="customPlcInteractAddress"></param>
   public void PlcProductionSync(IContainer container, IPLC plc, CustomPlcInteractAddressModel customPlcInteractAddress)
   {
      string logHeader = $"[同步PLC生产数据]:";
      PlcProductionSyncModel plcProduction = new PlcProductionSyncModel();
      plcProduction.Shift = _appGlobalConfig.ShiftSwitchInfo.Shift == ShiftType.白班 ? (short)1 : (short)2;
      plcProduction.Input = _processRatioDisplay.ProductionCounter.InputCount;
      plcProduction.Output = _processRatioDisplay.ProductionCounter.OutputCount;

      foreach (var item in _processRatioDisplay.Last24HourOutputValue.HourlyDatas)
      {
         if (plcProduction.HourCount.Length >= item.Time.Hour)
            plcProduction.HourCount[item.Time.Hour] = item.ProductionCount;
      }
      foreach (var item in _processRatioDisplay.ProcessRatios)
      {
         int index = item.Process switch
         {
            nameof(ProcessTypeEnum.前扫码) => 0,
            nameof(ProcessTypeEnum.测短路) or nameof(ProcessTypeEnum.测电压) => 1,
            nameof(ProcessTypeEnum.前称重) => 2,
            nameof(ProcessTypeEnum.测漏) => 3,
            nameof(ProcessTypeEnum.注液) or nameof(ProcessTypeEnum.最终注液结果) => 4 ,
            nameof(ProcessTypeEnum.后称重) => 5,
            nameof(ProcessTypeEnum.打钉检测) => 6,
            nameof(ProcessTypeEnum.回氦)=>7,
            _ => -1,
         };
         if (index >= 0 && plcProduction.PlcProcessData.Length > index)
         {
            var data = plcProduction.PlcProcessData[index];
            data.OkCount = item.OkTotal;
            data.Ng1Count = item.NgTotal;
            data.Ng2Count = 0;
            data.PassRate = (float)Math.Round(item.OkRatio, 2);
            data.NgProportion = (float)Math.Round(item.NgRatio, 2);
         }
      }

      if (!plc.WriteClass(plcProduction, new SignalAddressModel(customPlcInteractAddress.DataAddress.Lable), logHeader))
      {
         $"同步生产数据至PLC工失败！".LogRun(Log4NetLevelEnum.错误, true);
      }
   }
}
