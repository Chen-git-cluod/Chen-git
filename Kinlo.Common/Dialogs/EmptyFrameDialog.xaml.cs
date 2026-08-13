using System.Windows.Controls;

namespace Kinlo.Common.Dialogs;

/// <summary>
/// AlarmDialog.xaml 的交互逻辑
/// </summary>
public partial class EmptyFrameDialog : Border
{
   public object UserControlVM { get; set; }

   public EmptyFrameDialog(object userControl)
   {
      InitializeComponent();
      DataContext = this;
      UserControlVM = userControl;
   }
}
