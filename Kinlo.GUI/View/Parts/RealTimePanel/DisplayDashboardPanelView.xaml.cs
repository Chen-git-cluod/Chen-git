namespace Kinlo.GUI.View;

/// <summary>
/// DisplayDashboardPanelView.xaml 的交互逻辑
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class DisplayDashboardPanelView : Border
{
   IContainer _container;

   public GlobalStaticTemporary Temporary { get; set; }
   public double Value { get; set; }
   public double Value1 { get; set; }
   public double Value2 { get; set; }
   public double Value3 { get; set; }

   public DisplayDashboardPanelView(IContainer container)
   {
      InitializeComponent();
      _container = container;
      this.DataContext = this;

      Temporary = container.Get<GlobalStaticTemporary>();

      _ = Task.Run(async () =>
      {
         Random random = new Random();
         await UIThreadHelper.InvokeOnUiThreadAsync(async () =>
         {
            while (true)
            {
               Value = random.Next(300, 1000) / 10;
               Value1 = random.Next(300, 1000) / 10;
               Value2 = random.Next(300, 1000) / 10;
               Value3 = random.Next(300, 1000) / 10;
               await Task.Delay(1000);
            }
         });
      });
   }
}
