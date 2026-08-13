namespace Kinlo.Common.Dialogs;

/// <summary>
/// AlarmDialog.xaml 的交互逻辑
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class AlarmDialog : Border
{
   public AlarmDialog(AlarmDialogDto dialogDto)
   {
      InitializeComponent();
      this.DataContext = dialogDto;
   }
}
