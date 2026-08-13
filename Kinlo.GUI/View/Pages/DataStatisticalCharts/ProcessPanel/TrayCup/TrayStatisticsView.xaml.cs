namespace Kinlo.GUI.View
{
   /// <summary>
   /// ProcessDataStatisticsView.xaml 的交互逻辑
   /// </summary>
   public partial class TrayStatisticsView : UserControl
   {
      public TrayStatisticsView()
      {
         InitializeComponent();
      }

      private void UpdateClip(Grid grid)
      {
         grid.Clip = new RectangleGeometry(new Rect(0, 0, grid.ActualWidth, grid.ActualHeight), 8, 8);
      }

      private void Root_Loaded(object sender, RoutedEventArgs e)
      {
         if (sender is Grid grid)
            UpdateClip(grid);
      }

      private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         if (sender is Grid grid)
            UpdateClip(grid);
      }
   }
}
