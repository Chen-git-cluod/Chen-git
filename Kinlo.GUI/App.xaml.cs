namespace Kinlo.GUI
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      private static Mutex? _mutex;

      protected override void OnStartup(StartupEventArgs e)
      {
         string mutexName = "WpfSingleInstanceApp_Mutex"; // 定义 Mutex 的唯一名称
         // 创建 Mutex
         _mutex = new Mutex(true, mutexName, out bool isNewInstance);

         if (!isNewInstance)
         {
            // 如果不是新的实例，则退出程序
            MessageBox.Show("程序已经运行，请勿重复运行！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            Environment.Exit(0);
         }
         else
         {
            //Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            ThreadPool.SetMinThreads(64, 64);
            UIThreadHelper.Initialize();
            base.OnStartup(e);
         }
      }

      protected override void OnExit(ExitEventArgs e)
      {
         // 释放 Mutex
         _mutex?.ReleaseMutex();
         _mutex = null;

         base.OnExit(e);
      }
   }
}
