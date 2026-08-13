namespace Kinlo.GUI.ViewModel
{
   public class QueryBatteryViewModel : Screen
   {
      [Inject]
      public DbHelper? SugarDb { get; set; }
      public Action<IBatMainModel>? GetBatteryAction { get; set; }
      public Action<string>? GetBarcode { get; set; }
      public int TypeIndex { get; set; }
      public string Barcode { get; set; } = string.Empty;
      public ObservableCollection<IBatMainModel> Batterys { get; set; } = new();

      /// <summary>
      /// 手动查询电池
      /// </summary>
      public async void ManuallyQueryCMD()
      {
         if (TypeIndex == 0)
            Batterys = await SugarDb.GetProcessByBarcodeFuzzyAsync(Barcode, "[手动查询电池]");
         else
         {
            GetBarcode?.Invoke(Barcode);
            this.RequestClose();
         }
      }

      /// <summary>
      /// 手动选择电池
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void SelectionCMD(IBatMainModel batt)
      {
         GetBatteryAction?.Invoke(batt);
         this.RequestClose();
      }
   }
}
