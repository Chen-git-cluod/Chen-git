using System.ComponentModel;
using HandyControl.Controls;

namespace Kinlo.GUI.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView : System.Windows.Window
{
   protected override void OnClosed(EventArgs e)
   {
      try
      {
         HandyControl.Controls.Growl.Clear();
         KeyboardHook.Hook.Stop();
         GlobalClockService.Instance.Stop(); //全局时钟服务关闭
         GlobalStaticTemporary.CancelGlobalToken();
      }
      finally
      {
         base.OnClosed(e);
         //  Environment.Exit(0);
      }
   }

   protected override void OnClosing(CancelEventArgs e)
   {
      try
      {
         if (this.DataContext is MainViewModel viewModel)
         {
            if (viewModel.Temporary.IsRunning)
            {
               Growl.Warning("请先停止运行后再关闭!");
               e.Cancel = true;
               return;
            }
         }
         if (GlobalClockService.ActiveTasks.Count > 0) //检查周期任务是否完成
         {
            string msg = string.Join(',', GlobalClockService.ActiveTasks.Select(x => x.Value));
            Growl.Warning($"有周期任务 [{msg}] 未完成，请稍候再关闭");
            e.Cancel = true;
            return;
         }
         if (
            HandyControl.Controls.MessageBox.Show(
               "确认要关闭软件",
               "警告",
               System.Windows.MessageBoxButton.YesNo,
               System.Windows.MessageBoxImage.Warning
            ) == System.Windows.MessageBoxResult.No
         )
         {
            e.Cancel = true;
         }
         else
         {
            base.OnClosing(e);
         }
      }
      catch (Exception ex)
      {
         $"关闭程序异常: {ex}".LogRun(Log4NetLevelEnum.错误);
         base.OnClosing(e);
      }
   }

   public MainView()
   {
      InitializeComponent();
   }

   /// <summary>
   /// 监听键盘
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   private void OnUserActivity(object sender, System.Windows.Input.KeyEventArgs e)
   {
      if (this.DataContext is MainViewModel viewModel)
      {
         viewModel.UsersStatus.RefreshAutoLogoutTimer();
      }
   }

   /// <summary>
   /// 监听鼠标
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   private void OnUserActivity(object sender, System.Windows.Input.MouseEventArgs e)
   {
      if (this.DataContext is MainViewModel viewModel)
      {
         viewModel.UsersStatus.RefreshAutoLogoutTimer();
      }
   }

   private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
   {
      Growl.Info("您无此权限  或  当前设备正在运行而此页面不支持运行中编辑！");
   }
}
