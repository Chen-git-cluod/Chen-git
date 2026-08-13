namespace Kinlo.GUI.ViewModel;

[AddINotifyPropertyChangedInterface]
[UIDisplayAttribute(true)]
public class ThemeViewModel : Screen
{
   public SolidColorBrush MaimBrush { get; set; }
   public SolidColorBrush TextBrush { get; set; }
   public bool IsMaimBrush { get; set; } = true;
   public bool IsTextBrush { get; set; }
   IContainer _container;

   public ThemeViewModel(IContainer container)
   {
      _container = container;
      ThemeManager.Current.SystemThemeChanged += Current_SystemThemeChanged;
      PresetManager.Current.ColorPreset = new PresetManager.Preset { ColorPreset = @"Style\Preset\Default", AssemblyName = "Kinlo.Common" };
      // PresetManager.Current.ColorPreset = new PresetManager.Preset { ColorPreset = @"Style\Preset\Default", AssemblyName = Assembly.GetExecutingAssembly().GetName().Name };
   }

   private void Current_SystemThemeChanged(object? sender, HandyControl.Data.FunctionEventArgs<ThemeManager.SystemTheme> e)
   {
      if (!IsSystemTheme)
         return;

      SelectedTheme = (LocalThemeEnum)(int)e.Info.CurrentTheme;
   }

   private SolidColorBrush _selectedBrush;

   public SolidColorBrush SelectedBrush
   {
      get { return _selectedBrush; }
      set
      {

         if (value != _selectedBrush)
         {
            _selectedBrush = value;
            if (IsMaimBrush)
            {
               MaimBrush = value;
            }
            else
            {
               TextBrush = value;
            }
         }
      }
   }

   /// <summary>
   /// 更改主色
   /// </summary>
   public void SelectedColorChangedCMD()
   {
      ThemeManager.Current.AccentColor = SelectedBrush;
   }

   /// <summary>
   /// 还原主色
   /// </summary>
   public void CanceledCMD()
   {
      ThemeManager.Current.AccentColor = null;
   }

   /// <summary>
   /// 打开系统主题面板
   /// </summary>
   public async Task OpenSystemThemeCMD()
   {
      var p = new Process();
      p.StartInfo.FileName = "cmd.exe";
      p.StartInfo.UseShellExecute = false;
      p.StartInfo.RedirectStandardInput = true;
      p.StartInfo.RedirectStandardOutput = true;
      p.StartInfo.RedirectStandardError = true;
      p.StartInfo.CreateNoWindow = true;
      //p.OutputDataReceived += SortOutputHandler;
      p.Start(); //启动程序
      p.StandardInput.AutoFlush = true;
      p.StandardInput.WriteLine($"control color");
      await Task.Delay(500);
      p.Close();
   }

   private bool _isSystemTheme;

   public bool IsSystemTheme
   {
      get { return _isSystemTheme; }
      set
      {
         if (_isSystemTheme != value)
         {
            ThemeManager.Current.UsingSystemTheme = _isSystemTheme = value;
         }
         if (value)
         {
            SelectedTheme = (LocalThemeEnum)(int)ThemeManager.Current.ActualApplicationTheme;
         }
      }
   }

   private LocalThemeEnum _selectedTheme;

   public LocalThemeEnum SelectedTheme
   {
      get { return _selectedTheme; }
      set
      {
         _selectedTheme = value;
         ThemeManager.Current.ApplicationTheme = (ApplicationTheme)(int)value;
         _container.Get<MainViewModel>().IsDark = value == LocalThemeEnum.深色 ? true : false;
      }
   }
}
