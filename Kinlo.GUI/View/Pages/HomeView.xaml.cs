namespace Kinlo.GUI.View;

/// <summary>
/// HomeView.xaml 的交互逻辑
/// </summary>
public partial class HomeView : UserControl
{
   public HomeView()
   {
      InitializeComponent();
   }

   private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
   {
      HandyControl.Controls.Growl.WarningGlobal("ddff");
   }
}
