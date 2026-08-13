using System.Windows.Controls;

namespace Kinlo.Common.Dialogs;

/// <summary>
/// AlarmDialog.xaml 的交互逻辑
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class AlarmNotification : Border
{
   public AlarmNotification(AlarmDialogDto dialogDto)
   {
      InitializeComponent();
      this.DataContext = dialogDto;
   }
}
