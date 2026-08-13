namespace Kinlo.Services.PeriodicTasks;

/// <summary>
/// 轮询仪表
/// </summary>
public partial class PeriodicTasksHelper
{
   private static readonly SemaphoreSlim _semaphore = new(1, 1); //初始可用许可证数量：1;最大许可证数量：1；

   /// <summary>
   /// 仪表 ZTDTSU666电能表 HDK_LDZ_DN50流量计
   /// </summary>
   /// <param name="container"></param>
   /// <returns></returns>
   private static async Task PollingElectricMeterData(IContainer container, DateTime time)
   {
      if (time.Second % 5 != 0) //5秒一次
         return;
      if (await _semaphore.WaitAsync(0)) // 尝试立即获取许可证，0ms 超时
      {
         try
         {
            await OnPollingElectricMeterData(container);
         }
         finally
         {
            _semaphore.Release();
         }
      }
   }

   private static Task OnPollingElectricMeterData(IContainer container)
   {
      return Task.Run(async () =>
      {
         var temporary = container.Get<GlobalStaticTemporary>();
         var devicesConfig = container.Get<DevicesConfig>();
         var ztdTSU666 = devicesConfig.GetRunDevice(x => x?.DeviceInfo.Communication == CommunicationEnum.ZTDTSU666电能表);
         if (ztdTSU666 != null)
         {
            await ReadZTDTSU666(ztdTSU666, container, temporary);
         }
         else
         {
            var client = devicesConfig.DeviceList.FirstOrDefault(x =>
               x.Communication == CommunicationEnum.ZTDTSU666电能表 && x.IsEnable && x.IsOnline == 1
            );
            if (client != null)
            {
               await client.WithCreatedDeviceAsync(async d => await ReadZTDTSU666(d, container, temporary));
            }
            else
            {
               await UIThreadHelper.Dispatcher.BeginInvoke(() => temporary.ZTDTSU666Result = new ZTDTSU666ResultModel());
            }
         }

         var _HDK_LDZ_DN50 = devicesConfig.GetRunDevice(x => x?.DeviceInfo.Communication == CommunicationEnum.HDK_LDZ_DN50流量计);
         if (_HDK_LDZ_DN50 != null)
         {
            await ReadHDK_LDZ_DN50(_HDK_LDZ_DN50, container, temporary);
         }
         else
         {
            var client = devicesConfig.DeviceList.FirstOrDefault(x =>
               x.Communication == CommunicationEnum.HDK_LDZ_DN50流量计 && x.IsEnable && x.IsOnline == 1
            );
            if (client != null)
            {
               await client.WithCreatedDeviceAsync(async d => await ReadHDK_LDZ_DN50(d, container, temporary));
            }
            else
            {
               await UIThreadHelper.Dispatcher.BeginInvoke(() => temporary.HDK_LDZ_DN50 = new HDK_LDZ_DN50DTO());
            }
         }
      });
   }

   static async Task ReadZTDTSU666(IDevice device, IContainer container, GlobalStaticTemporary temporary)
   {
      try
      {
         var _result = device.ReadClass<ZTDTSU666ResultModel>(null, null, "");
         if (_result.Status == DeviceStatus.Success)
            await UIThreadHelper.Dispatcher.BeginInvoke(() => temporary.ZTDTSU666Result = _result.Value!);
         else
            $"电能表取值失败！".LogRun(Log4NetLevelEnum.警告);
      }
      catch (Exception ex)
      {
         await UIThreadHelper.Dispatcher.BeginInvoke(() => temporary.ZTDTSU666Result = new ZTDTSU666ResultModel());
         $"电能表异常:{ex}".LogRun(Log4NetLevelEnum.警告);
      }
   }

   static async Task ReadHDK_LDZ_DN50(IDevice device, IContainer container, GlobalStaticTemporary temporary)
   {
      try
      {
         var _result = device.ReadClass<HDK_LDZ_DN50DTO>(null, null, "");
         if (_result.Status == DeviceStatus.Success)
            await UIThreadHelper.Dispatcher.BeginInvoke(() => temporary.HDK_LDZ_DN50 = _result.Value!);
         else
            $"HDK_LDZ_DN50流量计取值失败！".LogRun(Log4NetLevelEnum.警告);
      }
      catch (Exception ex)
      {
         await UIThreadHelper.Dispatcher.BeginInvoke(() => temporary.HDK_LDZ_DN50 = new HDK_LDZ_DN50DTO());
         $"HDK_LDZ_DN50流量计异常:{ex}".LogRun(Log4NetLevelEnum.警告);
      }
   }
}
