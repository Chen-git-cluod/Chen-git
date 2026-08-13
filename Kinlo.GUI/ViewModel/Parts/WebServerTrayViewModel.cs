using System.Windows.Media.Imaging;
using HandyControl.Controls;
using Kinlo.WebApi;

namespace Kinlo.GUI.ViewModel
{
   [UIDisplay(IsSingleton = true)]
   public class WebServerTrayViewModel : Screen
   {
      NotifyIcon? _notifyIcon;
      IContainer _container;
      Lazy<MainViewModel> _mainViewModel;
      Lazy<IApiService> _apiService;

      public WebServerTrayViewModel(IContainer container)
      {
         _container = container;
         _mainViewModel = new Lazy<MainViewModel>(() => _container.Get<MainViewModel>());
         _apiService = new Lazy<IApiService>(() => _container.Get<IApiService>());
      }

      protected override void OnViewLoaded()
      {
         if (this.View is WebServerTrayView v)
         {
            _notifyIcon = v.notifuIco;
         }

         base.OnViewLoaded();
      }

      protected override void OnClose()
      {
         _notifyIcon?.Dispose();
         base.OnClose();
      }

      public async Task SetRunStatusAsync(bool status)
      {
         await UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            if (_notifyIcon == null)
            {
               $"未找到托盘控件！".LogRun(Log4NetLevelEnum.警告);
               return;
            }
            _notifyIcon.Icon = status switch
            {
               true => new BitmapImage(new Uri("pack://application:,,,/Resources/ServiceRun.png")),
               _ => new BitmapImage(new Uri("pack://application:,,,/Resources/ServiceStop1.png")),
            };
         });
      }

      public async Task StartServerCmd()
      {
         await _apiService.Value.StartApiAsync();
      }

      public async Task StopServerCmd()
      {
         await _apiService.Value.StopApiAsync();
      }

      ControlInfoModel? _webPage = null;

      public void SettingCmd()
      {
         var mainVM = _mainViewModel.Value;
         if (_webPage == null)
            _webPage = mainVM.Menus.FirstOrDefault(x => x.BindingOrKey == nameof(ConfigurationWebViewModel));
         if (_webPage == null)
         {
            $"未找到Web服务页面！".LogRun(Log4NetLevelEnum.警告, true);
            return;
         }
         mainVM.SelectedIndex = mainVM.Menus.IndexOf(_webPage);
      }
   }
}
