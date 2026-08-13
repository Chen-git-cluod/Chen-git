using Stylet.Xaml;

namespace Kinlo.GUI.View;

/// <summary>
/// PlcWarningStatisticalView.xaml 的交互逻辑
/// </summary>
public partial class PlcWarningStatisticalView : UserControl
{
   public PlcWarningStatisticalView()
   {
      InitializeComponent();
   }

   private void checkComboBox_Loaded(object sender, RoutedEventArgs e)
   {
      var combo = (HandyControl.Controls.CheckComboBox)sender;
      if (combo.SelectedItems.Count > 0)
         return;
      foreach (var item in combo.Items.OfType<PlcAalrmLevelEnum>())
      {
         if (item is PlcAalrmLevelEnum.报警 or PlcAalrmLevelEnum.警告)
         {
            combo.SelectedItems.Add(item);
         }
      }
   }
}
