namespace Kinlo.GUI.ViewModel;

[Languages(["图表统计", "Chart statistics", "Chart statistics"], IsScanProperty = false)]
[UIDisplay(true, 4, ulong.MaxValue, true, "\xe632")]
public class DataStatisticalChartsViewModel : Screen, IMenu
{
   public List<string> StatisticalTypes { get; set; } = ["统计工序数据", "按托盘统计注液", "注液CPK", "注液泵信息", "PLC报警"];
   public object? ContentView { get; set; }
   private int _selectIndex = -1;

   public int SelectIndex
   {
      get { return _selectIndex; }
      set
      {
         if (_selectIndex != value)
         {
            _selectIndex = value;
            ContentView = value switch
            {
               0 => _container.Get<ProcessPanelViewModel>(),
               1 => _container.Get<TrayStatisticsViewModel>(),
               2 => _container.Get<InjectCPKChartViewModel>(),
               3 => _container.Get<InjectionPumpViewModel>(),
               4 => _container.Get<PlcWarningStatisticalViewModel>(),
               _ => null,
            };
         }
      }
   }

   IContainer _container;

   public DataStatisticalChartsViewModel(IContainer container)
   {
      _container = container;
   }

   public void Load() { }

   public bool Unload()
   {
      return true;
   }
}
