namespace Kinlo.GUI.View.Parts
{
   /// <summary>
   /// LogMesLogView.xaml 的交互逻辑
   /// </summary>
   [AddINotifyPropertyChangedInterface]
   public partial class LogWebLogView : ListView
   {
      public ObservableCollection<WebLogModel> Logs { get; set; }

      public LogWebLogView(ObservableCollection<WebLogModel> logs)
      {
         InitializeComponent();
         DataContext = this;
         Logs = logs;
      }
   }
}
