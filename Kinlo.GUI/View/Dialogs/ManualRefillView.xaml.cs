namespace Kinlo.GUI.View
{
   /// <summary>
   /// UpdateReplenishVolumeView.xaml 的交互逻辑
   /// </summary>
   public partial class ManualRefillView : Window
   {
      public ManualRefillView()
      {
         InitializeComponent();
      }

      protected override void OnClosed(EventArgs e)
      {
         ((ManualRefillViewModel)this.DataContext).CancelCMD();
         base.OnClosed(e);
      }
   }
}
