using KinloControls;

namespace Kinlo.GUI.ViewModel;

[UIDisplayAttribute(true)]
public class DeviceStatusViewModel : Screen
{
   public DevicesConfig Devices { get; set; }
   public ObservableCollection<PlcStatusDisplayModel> CopyPlcStatusDisplays { get; set; } =
      new ObservableCollection<PlcStatusDisplayModel>();
   public PlcStatusConfig PlcStatus { get; set; }

   [Inject]
   public UsersStatusConfig UsersStatus { get; set; }
   public ObservableCollection<TimelineItem> Last24HoursPlcStatus { get; }
   private PlcStatusDisplayModel _plcStatusDisplay;

   public DeviceStatusViewModel(IContainer container)
   {
      Devices = container.Get<DevicesConfig>();
      PlcStatus = container.Get<PlcStatusConfig>();
      Last24HoursPlcStatus = PlcStatus.Last24HoursPlcStatus;

      //Task.Run(async () =>
      //{
      //    var handle = new PLcStatusAndAlarmHandler(container, null, new PLCInteractAddressModel(), new CancellationTokenSource());
      //    Random random = new Random();
      //    for (int i = 0, a = 1; i < 100; i++, a++)
      //    {
      //        var b = random.Next(1, 6);
      //        if (i > 3)
      //            b = 1;
      //         await handle.Handle((short)b);
      //        var t = random.Next(1000, 5000);
      //        await Task.Delay(t);
      //    }
      //});

      //Task.Run(async () =>
      //{
      //    DateTime startTime = DateTime.Now;
      //    Random random = new Random();
      //    for (int i = 0; i < 100; i++)
      //    {
      //        var status = random.Next(1, 6);
      //        if (plcStatus.PlcStatusDic.TryGetValue(status, out var value))
      //        {
      //            DateTime endTime = startTime.AddMilliseconds(random.Next(100, 2000));
      //            TimelineItem timelineItem = new TimelineItem
      //            {
      //                Id=0,
      //                Value =1,
      //                StartTime = startTime,
      //                EndTime = endTime,
      //                Label = value.status,
      //                Color = value.color
      //            };
      //            startTime = endTime;
      //            await UIThreadHelper.InvokeIfRequiredAsync(() => Last24HoursPlcStatus.Add(timelineItem));
      //            await Task.Delay(1000);
      //        }

      //    }
      //});
   }

   /// <summary>
   /// 修改plc状态配置
   /// </summary>
   public void EditPlcStatusCmd()
   {
      //CopyPlcStatusDisplays.Clear();
      //foreach (var item in PlcStatus.PlcStatusDisplays)
      //{
      //    CopyPlcStatusDisplays.Add(new PlcStatusDisplayModel
      //    {
      //        Code = item.Code,
      //        Color = item.Color,
      //        ColorIndex = item.ColorIndex,
      //        Description = item.Description,
      //    });
      //}
   }

   public void CreateStatusCmd()
   {
      CopyPlcStatusDisplays.Add(new PlcStatusDisplayModel { Color = Brushes.DarkGray, Description = "未定义" });
   }

   public void SavePlcStatusCmd()
   {
      //PlcStatus.PlcStatusDisplays.Clear();
      //foreach (var item in CopyPlcStatusDisplays)
      //{
      //    PlcStatus.PlcStatusDisplays.Add(new PlcStatusDisplayModel
      //    {
      //        Code = item.Code,
      //        Color = item.Color,
      //        ColorIndex= item.ColorIndex,
      //        Description = item.Description,
      //    });
      //}
      //PlcStatus.Save("", "修改PLC状态");
   }

   public void SelectPlcStatusCmd(PlcStatusDisplayModel plcStatusDisplay) => _plcStatusDisplay = plcStatusDisplay;

   public void DeletePlcStatusCmd(PlcStatusDisplayModel plcStatusDisplay) => CopyPlcStatusDisplays.Remove(plcStatusDisplay);

   public void SelectColorCmd(int colorIndex)
   {
      //if (_plcStatusDisplay != null)
      //{
      //    _plcStatusDisplay.ColorIndex = colorIndex;
      //    _plcStatusDisplay.Color = PlcStatusConfig.PlcStatusColors[colorIndex];
      //}
   }
}
