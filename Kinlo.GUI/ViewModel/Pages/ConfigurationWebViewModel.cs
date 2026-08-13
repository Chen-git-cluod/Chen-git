using HandyControl.Controls;
using Kinlo.WebApi;

namespace Kinlo.GUI.ViewModel;

[Languages(["Web服务", "Web Service", "Web Service"], IsScanProperty = false)]
[UIDisplayAttribute(isSingleton: true, 41, (ulong)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺), isRunEdit: true, "\xe63a")]
public class ConfigurationWebViewModel : Screen, IMenu
{
   IContainer _container;
   private WebApiConfig WebApi { get; set; }
   public WebApiConfig WebApiCpoy { get; set; }
   public Lazy<IApiService> _apiService;
   public Lazy<UsersStatusConfig> _usersStatus;
   ChangeTracker<WebApiConfig> _webCfgTracker;

   public ConfigurationWebViewModel(IContainer container, WebApiConfig webApiConfig)
   {
      _container = container;
      _apiService = new Lazy<IApiService>(() => container.Get<IApiService>());
      _usersStatus = new Lazy<UsersStatusConfig>(() => container.Get<UsersStatusConfig>());
      WebApi = webApiConfig;
      WebApiCpoy = new WebApiConfig(_container, false);
      WebApiConfig.Copy(WebApi, WebApiCpoy);
      _webCfgTracker = new ChangeTracker<WebApiConfig>(WebApi, WebApiCpoy);
   }

   /// <summary>
   /// 全选
   /// </summary>
   public void SelectAllCmd() => WebApiCpoy.WebApiRoutes.ForEach(x => x.IsEnable = true);

   /// <summary>
   /// 全不选
   /// </summary>
   public void DeselectAllCmd() => WebApiCpoy.WebApiRoutes.ForEach(x => x.IsEnable = false);

   /// <summary>
   /// 反选
   /// </summary>
   public void InvertSelectionCmd() => WebApiCpoy.WebApiRoutes.ForEach(x => x.IsEnable = !x.IsEnable);

   public async Task StartWebCmd()
   {
      await _apiService.Value.StartApiAsync();
   }

   public async Task StopWebCmd()
   {
      await _apiService.Value.StopApiAsync();
   }

   public void SaveCmd()
   {
      if (!_webCfgTracker.HasChanges)
      {
         Growl.Info($"文件未修改！");
         return;
      }
      var rs = HandyControl.Controls.MessageBox.Show("保存将重启Web服务，是否保存？", "信息", MessageBoxButton.YesNo);
      if (rs != MessageBoxResult.Yes)
         return;

      var chages = _webCfgTracker.GetChanges;
      var msg = string.Join(',', chages.Values.Select(x => x.ToString()));
      try
      {
         WebApiConfig.Copy(WebApiCpoy, WebApi);
         WebApi.Save(_usersStatus.Value.LocalLoggedinUser.Account, msg);
         _webCfgTracker.Dispose();
         _webCfgTracker = new ChangeTracker<WebApiConfig>(WebApi, WebApiCpoy);

         Task.Run(async () => //避免UI线程和后台线程相互等待死锁
            {
               await _apiService.Value.StopApiAsync();
               await _apiService.Value.StartApiAsync();
            })
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
      }
      catch (Exception ex)
      {
         HandyControl.Controls.MessageBox.Show($"保存失败：{ex.Message}", "错误");
      }
   }

   public void Load() { }

   public bool Unload()
   {
      if (_webCfgTracker.HasChanges)
      {
         var rs = System.Windows.MessageBox.Show("有修改未保存，是否保存？", "提示", MessageBoxButton.YesNoCancel);
         if (rs == MessageBoxResult.Yes)
         {
            SaveCmd();
            return true;
         }
         else if (rs == MessageBoxResult.No)
         {
            WebApiConfig.Copy(WebApi, WebApiCpoy);
            _webCfgTracker.Dispose();
            _webCfgTracker = new ChangeTracker<WebApiConfig>(WebApi, WebApiCpoy);
            return true;
         }
         else
         {
            return false;
         }
      }
      return true;
   }
}
