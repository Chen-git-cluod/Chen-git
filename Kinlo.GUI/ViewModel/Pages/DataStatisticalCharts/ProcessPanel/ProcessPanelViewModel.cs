namespace Kinlo.GUI.ViewModel;

[UIDisplay(true)]
public class ProcessPanelViewModel : Screen
{
   public ProcessStatisticsViewModel? ProcessStatisticsVM { get; set; }

   public CustomStatisticsViewModel? CustomStatisticsVM { get; set; }
   public PlcStatusStatisticalViewModel? PlcStatusStatisticalVM { get; set; }

   private DisplayDataCollection? _displayData;
   private IContainer _container;

   public ProcessPanelViewModel(IContainer container)
   {
      _container = container;
      _displayData = container.Get<DisplayDataCollection>();
      ProcessStatisticsVM = container.Get<ProcessStatisticsViewModel>();
      CustomStatisticsVM = container.Get<CustomStatisticsViewModel>();
      PlcStatusStatisticalVM = container.Get<PlcStatusStatisticalViewModel>();
      var value = InitProcessesType();
      if (ProcessStatisticsVM != null)
         ProcessStatisticsVM.ResultFields = value.results;
      if (CustomStatisticsVM != null)
      {
         CustomStatisticsVM.ResultFields = value.results.ToObservableCollection();
         CustomStatisticsVM.IndexFields = value.indexs.ToObservableCollection();
         //if (value.results.Count > 0)
         //    CustomStatisticsVM.SelectResultField = value.results[0];
         //if (value.indexs.Count > 0)
         //    CustomStatisticsVM.SelectIndexField = value.indexs[0];
      }
   }

   /// <summary>
   /// 初始化主页工序显示
   /// </summary>
   public (List<FieldInfoModel> results, List<FieldInfoModel> indexs) InitProcessesType()
   {
      var resultFields = new List<FieldInfoModel>();
      var indexFields = new List<FieldInfoModel>();
      try
      {
         foreach (var item in _displayData.CompleteBatteryDatas.RuntimeBatteryType.GetProperties())
         {
            var language = item.GetCustomAttribute<LanguagesAttribute>();
            if (language == null)
               continue;
            if (item.PropertyType == typeof(ResultTypeEnum) && item.Name != nameof(BatMainModel.FinalStatus))
            {
               resultFields.Add(new FieldInfoModel { Display = language.Languages[0], FieldName = item.Name });
            }
            if (
               item.PropertyType == typeof(byte)
               || item.Name == nameof(BatInjectStationModel.TrayCode)
               || item.Name == nameof(BatInjectStationModel.CupCode)
            )
            {
               indexFields.Add(new FieldInfoModel { Display = language.Languages[0], FieldName = item.Name });
            }
         }
      }
      catch (Exception ex)
      {
         $"初始化统计显示异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
      return (resultFields, indexFields);
   }
}
