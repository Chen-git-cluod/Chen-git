namespace Kinlo.GUI.View.Parts
{
   /// <summary>
   /// LogMesLogView.xaml 的交互逻辑
   /// </summary>
   [AddINotifyPropertyChangedInterface]
   public partial class LogMesLogView : ListView
   {
      public ObservableCollection<MesLogModel> Logs { get; set; }

      public LogMesLogView(ObservableCollection<MesLogModel> logs)
      {
         InitializeComponent();
         DataContext = this;
         Logs = logs;
      }
   }
}
