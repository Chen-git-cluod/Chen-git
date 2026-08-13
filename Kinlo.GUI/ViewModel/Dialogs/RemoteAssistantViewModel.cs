using System.Windows.Media.Imaging;

namespace Kinlo.GUI.ViewModel;

/// <summary>
//
/// </summary>
public class RemoteStepItem
{
   public string StepLabel { get; set; } // "STEP 1"
   public string Title { get; set; } // 绑定 DynamicResource 后的字符串
   public string Description { get; set; }
   public string Emoji { get; set; } //
   public string Icon { get; set; } // Segoe MDL2 背景图标字符
   public string ImagePath { get; set; } // 可选：实际步骤截图路径
}

/// <summary>
/// 步骤进度点
/// </summary>
public class StepDotItem
{
   public double Size { get; set; }
   public Brush Fill { get; set; }
}

/// <summary>
/// 远程助手 ViewModel
/// </summary>
public class RemoteAssistantViewModel : Screen
{
   // ── 向日葵常见安装路径 ──────────────────────────────────────────
   //private static readonly string[] SunLoginPaths =
   //{
   //   @"C:\Program Files\Oray\SunLogin\SunloginClient\SunloginClient.exe",
   //   @"C:\Program Files (x86)\Oray\SunLogin\SunloginClient\SunloginClient.exe",
   //   @"C:\Program Files\Oray\SunloginClient\SunloginClient.exe",
   //};

   // ── 外网检测地址 ────────────────────────────────────────────────
   private const string PingHost = "www.baidu.com";
   private const int PingTimeout = 3000;

   // ══════════════════════════════════════════════════════════════════
   //  可绑定属性
   // ══════════════════════════════════════════════════════════════════

   private bool _isAndroid = true;
   public bool IsAndroid
   {
      get => _isAndroid;
      set
      {
         SetAndNotify(ref _isAndroid, value);
         RebuildSteps();
      }
   }

   private int _stepIndex;
   public int StepIndex
   {
      get => _stepIndex;
      set
      {
         SetAndNotify(ref _stepIndex, value);
         NotifyOfPropertyChange(nameof(CurrentStep));
         NotifyOfPropertyChange(nameof(CanGoPrev));
         NotifyOfPropertyChange(nameof(CanGoNext));
         RebuildDots();
      }
   }

   private List<RemoteStepItem> _steps = new();
   public List<RemoteStepItem> Steps
   {
      get => _steps;
      set
      {
         SetAndNotify(ref _steps, value);
         NotifyOfPropertyChange(nameof(CurrentStep));
      }
   }

   public RemoteStepItem CurrentStep => Steps.Count > 0 && Steps.Count > StepIndex ? Steps[StepIndex] : null;
   public bool CanGoPrev => StepIndex > 0;
   public bool CanGoNext => StepIndex < Steps.Count - 1;

   private BindableCollection<StepDotItem> _stepDots = new();
   public BindableCollection<StepDotItem> StepDots
   {
      get => _stepDots;
      set => SetAndNotify(ref _stepDots, value);
   }

   // ── 网络状态 ────────────────────────────────────────────────────
   private bool? _isNetworkConnected = null;
   public bool IsNetworkConnected => _isNetworkConnected == true;

   private bool _isChecking;
   public bool IsNotChecking => !_isChecking;

   private string _checkNetworkBtnText;
   public string CheckNetworkBtnText
   {
      get => _checkNetworkBtnText;
      set => SetAndNotify(ref _checkNetworkBtnText, value);
   }

   public string NetworkStatusText
   {
      get
      {
         if (_isChecking)
            return GetRes("网络检测中");
         if (_isNetworkConnected == null)
            return GetRes("网络未知");
         return _isNetworkConnected == true ? GetRes("网络已连接") : GetRes("网络未连接");
      }
   }

   public Brush NetworkStatusBackground =>
      _isNetworkConnected switch
      {
         true => new SolidColorBrush(Color.FromRgb(0xE6, 0xF9, 0xEE)),
         false => new SolidColorBrush(Color.FromRgb(0xFF, 0xEE, 0xEE)),
         _ => new SolidColorBrush(Color.FromRgb(0xF2, 0xF2, 0xF2)),
      };

   public Brush NetworkStatusForeground =>
      _isNetworkConnected switch
      {
         true => new SolidColorBrush(Color.FromRgb(0x1A, 0x85, 0x44)),
         false => new SolidColorBrush(Color.FromRgb(0xC0, 0x21, 0x21)),
         _ => new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)),
      };

   // ── 网速（轻量：仅在连接成功后展示） ───────────
   private string _networkSpeedText = "";
   public string NetworkSpeedText
   {
      get => _networkSpeedText;
      set => SetAndNotify(ref _networkSpeedText, value);
   }

   // ══════════════════════════════════════════════════════════════════
   //  构造 & 初始化
   // ══════════════════════════════════════════════════════════════════

   public RemoteAssistantViewModel()
   {
      CheckNetworkBtnText = GetRes("检测网络");
      RebuildSteps();
   }

   private void RebuildSteps()
   {
      Steps = IsAndroid ? BuildAndroidSteps() : BuildIosSteps();
      StepIndex = 0;
      RebuildDots();
   }

   readonly string _souceBasePaht = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "外部资源", "手机开远程教程图片");

   private List<RemoteStepItem> BuildAndroidSteps()
   {
      string imgDir = Path.Combine(_souceBasePaht, "Android");

      if (!Directory.Exists(imgDir))
         Directory.CreateDirectory(imgDir);
      return new List<RemoteStepItem>
      {
         new()
         {
            StepLabel = "STEP 1",
            Title = GetRes("手机USB连接主机"),
            Description = GetRes("确保手机通过USB连接上主机USB接口"),
            Emoji = "🔌",
            ImagePath = Path.Combine(imgDir, "AndroidStep1.jpeg"),
         },
         new()
         {
            StepLabel = "STEP 2",
            Title = GetRes("手机设置页面"),
            Description = GetRes("进入手机设置页面，找到个人热点或者网络和互联网"),
            Emoji = "📶",
            ImagePath = Path.Combine(imgDir, "AndroidStep2.png"),
         },
         new()
         {
            StepLabel = "STEP 3",
            Title = GetRes(" 开启热点，找到【USB网络共享】选项"),
            Description = GetRes("找到【USB网络共享】选项选择打开，如果是灰色的打不开，则是USB线接手机没接好"),
            Emoji = "💻",
            ImagePath = Path.Combine(imgDir, "AndroidStep3.png"),
         },
         //new()
         //{
         //    StepLabel   = "STEP 4",
         //    Title       = GetRes("安卓_步骤4_标题"),
         //    Description = GetRes("安卓_步骤4_描述"),
         //    Emoji       = "✅",
         //    ImagePath   = Path.Combine(imgDir, "AndroidStep4.jpg"),
         //},
      };
   }

   private List<RemoteStepItem> BuildIosSteps()
   {
      string imgDir = Path.Combine(_souceBasePaht, "ios");
      if (!Directory.Exists(imgDir))
         Directory.CreateDirectory(imgDir);
      return new List<RemoteStepItem>
      {
         new()
         {
            StepLabel = "STEP 1",
            Title = GetRes("手机USB连接主机"),
            Description = GetRes("确保手机通过USB连接上主机USB接口"),
            Emoji = "🔌",
            ImagePath = Path.Combine(imgDir, "iosStep1.jpeg"),
         },
         new()
         {
            StepLabel = "STEP 2",
            Title = GetRes("手机桌面找到设置"),
            Description = GetRes("找到设置，点击进入设置"),
            Emoji = "🍎",
            ImagePath = Path.Combine(imgDir, "iosStep2.jpeg"),
         },
         new()
         {
            StepLabel = "STEP 3",
            Title = GetRes("进入个人热点"),
            Description = GetRes("进入个人热点"),
            Emoji = "💻",
            ImagePath = Path.Combine(imgDir, "iosStep3.jpeg"),
         },
         new()
         {
            StepLabel = "STEP 4",
            Title = GetRes("打开usb共享"),
            Description = GetRes("打开usb共享"),
            Emoji = "✅",
            ImagePath = Path.Combine(imgDir, "iosStep4.jpeg"),
         },
      };
   }

   private void RebuildDots()
   {
      var primary = Application.Current.TryFindResource("PrimaryBrush") as Brush ?? Brushes.DodgerBlue;
      var muted = Application.Current.TryFindResource("BorderBrush") as Brush ?? Brushes.LightGray;

      StepDots = new BindableCollection<StepDotItem>(
         Steps.Select((_, i) => new StepDotItem { Size = i == StepIndex ? 10 : 7, Fill = i == StepIndex ? primary : muted })
      );
   }

   public void PreviewImageCMD(string imagePath)
   {
      if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
         return;

      var win = new Window
      {
         Title = "",
         WindowStyle = WindowStyle.None,
         ResizeMode = ResizeMode.NoResize,
         WindowStartupLocation = WindowStartupLocation.CenterScreen,
         Background = System.Windows.Media.Brushes.Black,
         Width = 900,
         Height = 700,
         Topmost = true,
      };

      // 点击任意位置关闭
      win.MouseLeftButtonDown += (_, _) => win.Close();

      // 按 Esc 关闭
      win.KeyDown += (_, e) =>
      {
         if (e.Key == System.Windows.Input.Key.Escape)
            win.Close();
      };

      var img = new System.Windows.Controls.Image
      {
         Source = new BitmapImage(new Uri(imagePath)),
         Stretch = Stretch.Uniform,
         Margin = new Thickness(16),
      };

      // 附加属性单独设置
      RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);

      win.Content = img;
      win.ShowDialog();
   }

   // ══════════════════════════════════════════════════════════════════
   //  Commands（Stylet Action 绑定）
   // ══════════════════════════════════════════════════════════════════

   public void SwitchToAndroidCMD() => IsAndroid = true;

   public void SwitchToIosCMD() => IsAndroid = false;

   public void PrevStepCMD()
   {
      if (CanGoPrev)
         StepIndex--;
   }

   public void NextStepCMD()
   {
      if (CanGoNext)
         StepIndex++;
   }

   public async void CheckNetworkCMD()
   {
      if (_isChecking)
         return;
      _isChecking = true;
      CheckNetworkBtnText = GetRes("网络检测中");
      NotifyNetworkStatus();

      var (success, rtt) = await PingAsync(PingHost, PingTimeout);

      _isNetworkConnected = success;
      _isChecking = false;
      CheckNetworkBtnText = GetRes("检测网络");

      // 轻量网速：用 RTT 给一个估算
      if (success)
         NetworkSpeedText =
            rtt < 100 ? $"{rtt} ms ▲▲▲"
            : rtt < 300 ? $"{rtt} ms ▲▲"
            : $"{rtt} ms ▲";

      NotifyNetworkStatus();
   }

   readonly Dictionary<string, string> RemoteDic = new();

   /// <summary>
   /// 打开远程助手
   /// </summary>
   /// <param name="type"></param>
   public void OpenRemoteCMD(string type)
   {
      string path = string.Empty;
      string msg = string.Empty;
      bool issucces = false;
      string name = type switch
      {
         "1" => "UU远程",
         "2" => "ToDesk",
         _ => "向日葵远程控制",
      };
      try
      {
         if (RemoteDic.TryGetValue(name, out path!) && path.StartApp(out msg))
         {
            issucces = true;
            return;
         }

         if (name.StartFromStartMenu(out path, out msg))
         {
            issucces = true;
            RemoteDic[name] = path;
         }
      }
      catch (Exception ex)
      {
         issucces = false;
         msg = ex.ToString();
      }
      finally
      {
         if (!issucces)
         {
            HandyControl.Controls.MessageBox.Show($"{name}启动失败：{msg}", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
         }
      }
   }

   public const string DialogToken = "RemoteAssistant";

   public void CloseCMD()
   {
      this.RequestClose();
   }

   // ══════════════════════════════════════════════════════════════════
   //  辅助方法
   // ══════════════════════════════════════════════════════════════════

   private static async Task<(bool success, long rtt)> PingAsync(string host, int timeout)
   {
      try
      {
         using var ping = new Ping();
         var reply = await ping.SendPingAsync(host, timeout);
         return (reply.Status == IPStatus.Success, reply.RoundtripTime);
      }
      catch
      {
         return (false, 0);
      }
   }

   private void NotifyNetworkStatus()
   {
      NotifyOfPropertyChange(nameof(IsNotChecking));
      NotifyOfPropertyChange(nameof(IsNetworkConnected));
      NotifyOfPropertyChange(nameof(NetworkStatusText));
      NotifyOfPropertyChange(nameof(NetworkStatusBackground));
      NotifyOfPropertyChange(nameof(NetworkStatusForeground));
   }

   /// <summary>从 App 资源字典读取字符串，找不到则返回 key 本身</summary>
   private static string GetRes(string key) => Application.Current.TryFindResource(key) as string ?? key;
}
