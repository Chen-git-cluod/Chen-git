namespace Kinlo.GUI.View
{
   /// <summary>
   /// RunLogView.xaml 的交互逻辑
   /// </summary>
   [AddINotifyPropertyChangedInterface]
   public partial class LogRunView : ListView
   {
      public ObservableCollection<RunLogModel> Logs { get; set; }

      public LogRunView(ObservableCollection<RunLogModel> logs)
      {
         InitializeComponent();
         DataContext = this;
         Logs = logs;
      }

      private void CopyLogMessage_Click(object sender, RoutedEventArgs e)
      {
         var element = (FrameworkElement)sender;
         var log = (RunLogModel)element.DataContext;
         Clipboard.SetText(log.Message);
      }
   }
}
