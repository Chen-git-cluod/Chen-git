using System.Threading.Tasks;
using Kinlo.Common.Models.OhtenModels;

namespace Kinlo.GUI.View
{
   /// <summary>
   /// RunLogView.xaml 的交互逻辑
   /// </summary>
   [AddINotifyPropertyChangedInterface]
   public partial class LogPlcAlarmView : Grid
   {
      public ObservableRangeCollection<PlcAlarmModel> CurrentAlarms { get; set; } = new();
      public ObservableRangeCollection<PlcAlarmModel> ClearedAlarms { get; set; } = new();

      public LogPlcAlarmView(IContainer container)
      {
         InitializeComponent();
         DataContext = this;
         container.Get<PlcStatusConfig>().DisplayPlcAlarmsCallback += AddLog;
      }

      private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

      /// <summary>
      ///  为null时代表设备运行了清除所有报警，
      /// </summary>
      /// <param name="plcAlarm"></param>
      /// <returns></returns>
      public async Task<bool> AddLog(PlcAlarmModel? plcAlarm)
      {
         try
         {
            await _semaphore.WaitAsync();
            if (plcAlarm == null) { }
            else if (plcAlarm.EndTime == null) //结束 为null时，为新报警
            {
               await UIThreadHelper.InvokeOnUiThreadAsync(
                  new Action(() =>
                  {
                     CurrentAlarms.Insert(0, plcAlarm);
                     if (CurrentAlarms.Count > 800)
                        CurrentAlarms.RemoveAt(800);
                  })
               );
            }
            else
            {
               await ClearAlarm(plcAlarm);
            }
         }
         finally
         {
            _semaphore.Release();
         }
         return true;
      }

      /// <summary>
      /// 为null时代表设备运行了清除所有报警，
      /// </summary>
      /// <param name="plcAlarm"></param>
      /// <returns></returns>
      private async Task ClearAlarm(PlcAlarmModel? plcAlarm)
      {
         if (plcAlarm != null)
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(
               new Action(() =>
               {
                  var cleared = CurrentAlarms.FirstOrDefault(x => x.Id == plcAlarm.Id);
                  if (cleared != null)
                  {
                     CurrentAlarms.Remove(cleared);
                  }

                  ClearedAlarms.Insert(0, plcAlarm);

                  if (ClearedAlarms.Count > 800)
                     ClearedAlarms.RemoveAt(800);
               })
            );
         }
         else
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(
               new Action(() =>
               {
                  foreach (var cleared in CurrentAlarms)
                  {
                     CurrentAlarms.Remove(cleared);
                     ClearedAlarms.Insert(0, cleared);
                     if (ClearedAlarms.Count > 800)
                        ClearedAlarms.RemoveAt(800);
                  }
               })
            );
         }
      }

      private void CopyLogMessage_Click(object sender, RoutedEventArgs e)
      {
         var element = (FrameworkElement)sender;
         var log = (RunLogModel)element.DataContext;
         Clipboard.SetText(log.Message);
      }
   }
}
