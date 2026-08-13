namespace Kinlo.GUI.ViewModel;

[Languages(["主頁", "Laman Utama", "Home"], IsScanProperty = false)]
[UIDisplayAttribute(true, 1, ulong.MaxValue, true, "\xe623")]
public class HomeViewModel : Screen, IMenu
{
   public EntityPropertyVisibleViewModel EntityPropertyVisibleVM { get; set; }
   public RealTimeLayoutViewModel RealTimeLayoutVM { get; set; }
   public DisplayDataCollection DisplayData { get; set; }
   public DeviceStatusViewModel DeviceStatusVM { get; set; }

   //public UsersStatusConfig UsersStatus { get; set; }
   IContainer _container;

   public HomeViewModel(IContainer container)
   {
      _container = container;
      DisplayData = container.Get<DisplayDataCollection>();
      RealTimeLayoutVM = container.Get<RealTimeLayoutViewModel>();
      EntityPropertyVisibleVM = container.Get<EntityPropertyVisibleViewModel>();
      DeviceStatusVM = container.Get<DeviceStatusViewModel>();
      //UsersStatus = container.Get<UsersStatusConfig>();
      // var id =  SnowflakeHelper.NextId();
   }

   /// <summary>
   /// 调整
   /// </summary>
   /// <param name="processesType"></param>
   public void AdjustCmd(ProcessTypeEnum processesType)
   {
      if (processesType == ProcessTypeEnum._)
      {
         EntityPropertyVisibleVM.DisplayData = DisplayData.CompleteBatteryDatas;
      }
      else
      {
         EntityPropertyVisibleVM.DisplayData = DisplayData.ProcessesDatas.FirstOrDefault(x => x.Processes == processesType)!;
      }

      if (EntityPropertyVisibleVM.DisplayData.PropertyBindings != null)
      {
         EntityPropertyVisibleVM.DisplayData.PropertyBindings.ForEach(propertyBinding =>
         {
            propertyBinding.IsSelected = false;
         });
      }
   }

   public void Load() { }

   public bool Unload() => true;
}
