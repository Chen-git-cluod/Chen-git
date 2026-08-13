namespace Kinlo.Services.PeriodicTasks;

/// <summary>
/// 周期任务_实时统计
/// </summary>
public partial class PeriodicTasksHelper
{
   private IBatteryCache? _batteryCache;
   private bool _isFist = true;

   private async Task PollingProcessRatio(DateTime time, IContainer container)
   {
      if (!_isFist && time.Second % 3 != 0) //3秒一次
         return;

      _isFist = false;
      _batteryCache ??= _container.Get<IBatteryCache>();

      await _processRatioDisplay.Refresh(_batteryCache.GetAll());
   }
}
