namespace Kinlo.GUI.ViewModel;

public class RealTimeLayoutViewModel : Screen
{
   public object? PanelViewModel { get; set; }
   IContainer _container;
   RealTimeDataViewModel? _realTimeVM;
   EnergyViewModel? _EnergyVM;

   public RealTimeLayoutViewModel(IContainer container)
   {
      _container = container;
      PanelViewModel = _realTimeVM = container.Get<RealTimeDataViewModel>();
      _EnergyVM = container.Get<EnergyViewModel>();
   }

   public void RealPanelCmd() => PanelViewModel = _realTimeVM;

   public void EnergyConsumptionCmd() => PanelViewModel = _EnergyVM;
}
