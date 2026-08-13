namespace Kinlo.GUI.ViewModel;

public class HelperViewModel : Screen
{
   public OtherParameterConfig OtherParameter { get; set; }
   Lazy<MainViewModel> _mainViewModel;
   Lazy<ConfigurationDeviceViewModel> _configurationDeviceVM;
   ControlInfoModel? _pageInfo = null;
   IContainer _container;
   IWindowManager _windowManager;

   public HelperViewModel(IContainer container, IWindowManager windowManager)
   {
      _container = container;
      _windowManager = windowManager;
      OtherParameter = container.Get<OtherParameterConfig>();
      _mainViewModel = new Lazy<MainViewModel>(() => _container.Get<MainViewModel>());
      _configurationDeviceVM = new Lazy<ConfigurationDeviceViewModel>(() => _container.Get<ConfigurationDeviceViewModel>());
   }

   RemoteAssistantViewModel? _remoteAssistantVM;

   /// <summary>
   /// 打开远程助手
   /// </summary>
   public void OpenRemoteAssistantCmd()
   {
      try
      {
         if (_remoteAssistantVM != null && _remoteAssistantVM.View != null)
         {
            ((RemoteAssistantView)_remoteAssistantVM.View).Activate();
            return;
         }
         _remoteAssistantVM ??= _container.Get<RemoteAssistantViewModel>();
         _windowManager.ShowWindow(_remoteAssistantVM);
      }
      catch (Exception ex)
      {
         $"打开远程助手窗口异常：{ex}".LogRun(Log4NetLevelEnum.警告, true);
      }
   }

   /// <summary>
   /// 打开手册
   /// </summary>
   public void OpenMaualCmd()
   {
      var mainVM = _mainViewModel.Value;
      if (_pageInfo == null)
         _pageInfo = mainVM.Menus.FirstOrDefault(x => x.BindingOrKey == nameof(ConfigurationDeviceViewModel));
      if (_pageInfo == null)
      {
         $"未找到设备页面！".LogRun(Log4NetLevelEnum.警告, true);
         return;
      }
      mainVM.SelectedIndex = mainVM.Menus.IndexOf(_pageInfo);
      _configurationDeviceVM.Value.OpenSelfManual();
   }

   #region 更新模块
   /// <summary>
   /// 更新更哪里
   /// </summary>
   static string toDirectoryPath = Environment.CurrentDirectory;

   /// <summary>
   /// 主程序路径
   /// </summary>
   static string mainProgramFilePath = @$"{toDirectoryPath}\Kinlo.GUI.exe";

   /// <summary>
   /// 更新文件
   /// </summary>
   static string fromFilePath = string.Empty;

   /// <summary>
   /// 备份路径
   /// </summary>
   static string backupsDirectoryPath = @$"{new DirectoryInfo(toDirectoryPath).Parent.FullName}\WeightAppBackups";

   /// <summary>
   /// 备份排除文件,以 -x! 开头，可多个文件或文件夹
   /// </summary>
   static string backupIgnore = @$" -x!MESlog -x!logs -x!Log -x!TableData ";

   /// <summary>
   /// 更新时排除的文件,以 -x! 开头，可多个文件或文件夹
   /// </summary>
   static string updateIgnore = @$" -x!config  -x!appsettings.json ";

   /// <summary>
   /// 运行更新程序位置
   /// </summary>
   static string updateApp = @$"{toDirectoryPath}\Update\Kinlo.Update.exe";

   public void UpdateCMD()
   {
      try
      {
         OpenFileDialog dialog = new OpenFileDialog();
         dialog.Multiselect = false; //该值确定是否可以选择多个文件
         dialog.Title = "请选择文件";
         dialog.Filter = "压缩文件(*.7z;*.zip;*.rar)|*.7z;*.zip;*.rar";
         var v = dialog.ShowDialog();
         if (v == true)
         {
            fromFilePath = dialog.FileName;
            Process.Start(
               updateApp,
               new string[] { "1", mainProgramFilePath, fromFilePath, toDirectoryPath, backupsDirectoryPath, backupIgnore, updateIgnore }
            );
            Environment.Exit(0); //即终止当前进程，应用程序即强制退出，返回exitcode给操作系统。（直接终止进程）
         }
      }
      catch (Exception ex)
      {
         $"[更新程序异常：{ex}]".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   public void BackupCMD()
   {
      try
      {
         Process.Start(
            updateApp,
            new string[] { "2", mainProgramFilePath, fromFilePath, toDirectoryPath, backupsDirectoryPath, backupIgnore, updateIgnore }
         );
      }
      catch (Exception ex)
      {
         $"[备份程序异常：{ex}]".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   public void RevertCMD()
   {
      try
      {
         OpenFileDialog dialog = new OpenFileDialog();
         dialog.InitialDirectory = backupsDirectoryPath;
         dialog.Multiselect = false; //该值确定是否可以选择多个文件
         dialog.Title = "请选择文件";
         dialog.Filter = "压缩文件(*.7z;*.zip;*.rar)|*.7z;*.zip;*.rar";
         var v = dialog.ShowDialog();
         if (v == true)
         {
            fromFilePath = dialog.FileName;
            Process.Start(
               updateApp,
               new string[] { "3", mainProgramFilePath, fromFilePath, toDirectoryPath, backupsDirectoryPath, backupIgnore, updateIgnore }
            );
            Environment.Exit(0); //即终止当前进程，应用程序即强制退出，返回exitcode给操作系统。（直接终止进程）
         }
      }
      catch (Exception ex)
      {
         $"[还原程序异常：{ex}]".LogRun(Log4NetLevelEnum.错误, true);
      }
   }
   #endregion

   #region 语言
   public void SelectedLanguageCMD(LanguageModel language)
   {
      OtherParameter.CurrentLanguage = language;
      OtherParameter.SwitchLanguage();
   }

   #region 切换语言旧方法 弃用
   //public void ConfigLanguage()
   //{

   //    string requestedCulture = $@"Languages\{OtherParameter.OtherParameter.Language.Key}.xaml";
   //    ResourceDictionary? _dictionary =
   //        Application.Current.Resources.MergedDictionaries.FirstOrDefault(d =>
   //        d.Source != null && d.Source.OriginalString.Equals(requestedCulture));
   //    if (_dictionary != null)
   //    {
   //        Application.Current.Resources.MergedDictionaries.Remove(_dictionary);
   //        Application.Current.Resources.MergedDictionaries.Add(_dictionary);

   //        OtherParameter.Save(_usersStatus.LocalLoggedinUser.Name, "自动保存", false);
   //    }
   //}
   #endregion
   #endregion
}
