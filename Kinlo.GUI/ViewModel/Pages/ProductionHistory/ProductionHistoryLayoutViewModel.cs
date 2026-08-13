using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[Languages(["生产历史", "Sejarah Produksi", "Production history"], IsScanProperty = false)]
[UIDisplayAttribute(true, 2, ulong.MaxValue, true, "\xe74e")]
public class ProductionHistoryLayoutViewModel : Screen, IMenu
{
   public ObservableCollection<DisplayItemDto> DisplayItems { get; set; } = new();
   public ProcessRatioDisplay ProcessRatio { get; set; }
   public static string DialogToke { get; set; } = "DialogQuery";

   private readonly Lazy<IBatteryCache> _cacheLazy;
   private readonly Lazy<OtherParameterConfig> _otherParameterLazy;
   private readonly Lazy<DisplayDataCollection> _displayDataLazy;
   private readonly Lazy<UsersStatusConfig> _usersStatusConfig;

   public ProductionHistoryLayoutViewModel(IContainer container)
   {
      var productionHistoryVM = container.Get<ProductionHistoryViewModel>();
      var resendVM = container.Get<MesResendViewModel>();
      _cacheLazy = new Lazy<IBatteryCache>(() => container.Get<IBatteryCache>());
      _otherParameterLazy = new Lazy<OtherParameterConfig>(() => container.Get<OtherParameterConfig>());
      _displayDataLazy = new Lazy<DisplayDataCollection>(() => container.Get<DisplayDataCollection>());
      _usersStatusConfig = new Lazy<UsersStatusConfig>(() => container.Get<UsersStatusConfig>());
      ProcessRatio = container.Get<ProcessRatioDisplay>();
      DisplayItems.Add(
         new DisplayItemDto
         {
            Content = productionHistoryVM,
            Name = "生产历史",
            Index = 0,
         }
      );
      DisplayItems.Add(
         new DisplayItemDto
         {
            Content = resendVM,
            Name = "MES重传",
            Index = 1,
         }
      );
   }

   public async Task ExportCacheCmd()
   {
      if (_usersStatusConfig.Value.LocalLoggedinUser.Level != (ulong)DefaultRoleEnum.超级管理员)
      {
         Growl.Warning("您无权限导出！");
         return;
      }
      var data = _cacheLazy.Value.GetAll();
      if (data != null)
      {
         try
         {
            SaveFileDialog _dialog = new SaveFileDialog();
            _dialog.Filter = "Excel 工作簿(*.xlsx)|*.xlsx";
            _dialog.FileName = DateTime.Now.ToString("缓存yyyy-MM-dd HH点mm分ss秒");
            if (_dialog.ShowDialog() != true)
            {
               return;
            }
            await Task.Run(() => ExcelHelper.ExportExcel(data, _dialog.FileName, true));
         }
         catch (Exception ex)
         {
            Growl.Warning(ex.ToString());
         }
      }
   }

   public void Load() { }

   public bool Unload() => true;
}
