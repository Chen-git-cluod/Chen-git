using HandyControl.Controls;

namespace Kinlo.Common.Configurations;

public partial class UsersStatusConfig : ConfigurationBase
{
   /// <summary>
   /// 最后登陆或最后键鼠活动时间
   /// </summary>
   [JsonIgnore]
   public DateTime LastActivityTime { get; set; }
   private ParameterConfig _parameterConfig;
   private Lazy<RoleConfig> _roleConfigLazy;
   private Lazy<DevicesConfig> _devicesConfigLazy;
   private Lazy<PLCSignalConfig> _signalLazy;

   /// <summary>
   /// 已登陆用户类型
   /// </summary>
   [JsonIgnore]
   public LoggedInTypeEnum LoggedInUserType { get; set; }

   [JsonIgnore]
   private UserModel _localLoggedinUser = new();

   /// <summary>
   /// 本地登陆用户
   /// </summary>
   [JsonIgnore]
   public UserModel LocalLoggedinUser
   {
      get { return _localLoggedinUser; }
      set
      {
         if (_localLoggedinUser != value)
         {
            _localLoggedinUser = value;
            RefreshAutoLogoutTimer();
         }
      }
   }

   /// <summary>
   /// 用户列表
   /// </summary>
   public ObservableCollection<UserModel> LocalUsers { get; set; } = new();

   /// <summary>
   /// 如果在用户配置界面，不登陆
   /// true 登陆，false 不登陆
   /// </summary>
   [JsonIgnore]
   public bool IsLogin { get; set; } = true;

   private LoginModeEnum _loginMode;

   /// <summary>
   /// 登陆模式
   /// </summary>
   public LoginModeEnum LoginMode
   {
      get { return _loginMode; }
      set
      {
         if (_loginMode != value)
         {
            _loginMode = value;
            try
            {
               this.Save(LocalLoggedinUser.Name, "修改登陆模式", false);
            }
            catch (Exception ex)
            {
               $"修改登陆模式异常:{ex}".LogSetting(Log4NetLevelEnum.错误, true);
            }
         }
      }
   }

   public UsersStatusConfig(StyletIoC.IContainer container, bool isStartup)
      : base(container, isStartup)
   {
      _parameterConfig = container.Get<ParameterConfig>();
      _roleConfigLazy = new Lazy<RoleConfig>(() => _container.Get<RoleConfig>());
      _devicesConfigLazy = new Lazy<DevicesConfig>(() => _container.Get<DevicesConfig>());
      _signalLazy = new Lazy<PLCSignalConfig>(() => _container.Get<PLCSignalConfig>());
      RefreshAutoLogoutTimer();
   }

   public override void Load()
   {
      LoadUserData();

      var _devicesConfig = _container.Get<DevicesConfig>();
      InitializeLive20RFingerprintDevice(_devicesConfig);
      InitializeHX540CardReader(_devicesConfig);
      InitializeGenericCardReaders(_devicesConfig);
      InitializeLibCardFeeReader(_devicesConfig);
   }

   private void LoadUserData()
   {
      try
      {
         var _dic = FileHelper.LoadToDictionary(this.GetType().Name);
         if (_dic != null && _dic.TryGetValue(nameof(LoginMode), out object LoginModeObj) && LoginModeObj != null)
         {
            if (Enum.TryParse<LoginModeEnum>(LoginModeObj.ToString(), out var mode))
               LoginMode = mode;
         }
         if (FingerpringDatas == null)
            FingerpringDatas = new Dictionary<int, byte[]>();
         if (_dic != null && _dic.TryGetValue(nameof(FingerpringDatas), out object fingers) && fingers != null)
         {
            FingerpringDatas = JsonSerializer.Deserialize<Dictionary<int, byte[]>>(fingers.ToString());
         }
         if (FingerpringDatas == null)
            FingerpringDatas = new Dictionary<int, byte[]>();

         if (_dic != null && _dic.TryGetValue(nameof(LocalUsers), out object value) && value != null)
         {
            LocalUsers = JsonSerializer.Deserialize<ObservableCollection<UserModel>>(value.ToString());
         }
         if (LocalUsers == null)
            LocalUsers = new ObservableCollection<UserModel>();

         if (!LocalUsers.Any(x => x.Level == (ulong)DefaultRoleEnum.管理员))
         {
            LocalUsers.Add(
               new UserModel
               {
                  Account = "admin",
                  Password = "admin",
                  Name = "默认管理员",
                  RegisterTime = DateTime.Now,
                  Level = (long)DefaultRoleEnum.管理员,
               }
            );
         }
      }
      catch (Exception ex)
      {
         $"[初始化UsersStatusConfig]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   public bool CreateUser(UserModel user)
   {
      try
      {
         if (LocalUsers.Any(x => x.Account == user.Account))
         {
            Growl.Warning($"[新增用户]:用户名 [{user.Account}] 重复,请重新填写!");
            return false;
         }
         user.RegisterTime = DateTime.Now;
         LocalUsers.Add(user);
         this.Save(LocalLoggedinUser.Account, $"[新增用户]:[{user.Account}]");
         return true;
      }
      catch (Exception ex)
      {
         $"[新增用户]异常:\r\n{ex}".LogSetting(Log4NetLevelEnum.错误, true);
         return false;
      }
   }

   public bool UpdateUser(UserModel user)
   {
      try
      {
         StringBuilder sb = new StringBuilder();

         UserModel _user = LocalUsers.First(x => x.Account == user.Account);

         if (_user.Password != user.Password)
         {
            _user.Password = user.Password;
            sb.Append($" [修改了密码] \r\n");
         }
         if (_user.Name != user.Name)
         {
            sb.Append($" [修改了姓名 {_user.Name}==>{user.Name}] \r\n");
            _user.Name = user.Name;
         }
         if (_user.FingerprintID != user.FingerprintID)
         {
            sb.Append($" [修改了指纹 {_user.FingerprintID}==>{user.FingerprintID}] \r\n");
            _user.FingerprintID = user.FingerprintID;
         }
         if (_user.Level != user.Level)
         {
            sb.Append($" [修改了权限 {_user.Level}==>{user.Level}] \r\n");
            _user.Level = user.Level;
         }
         if (_user.Tel != user.Tel)
         {
            sb.Append($" [{_user.Tel}==>{user.Tel}] \r\n");
            _user.Tel = user.Tel;
         }
         if (sb.Length > 0)
         {
            _user.UpdateTime = DateTime.Now;
            this.Save(LocalLoggedinUser.Account, $"[修改用户]:\r\n{sb}");
         }
         return true;
      }
      catch (Exception ex)
      {
         $"[修改用户]异常:\r\n{ex}".LogSetting(Log4NetLevelEnum.错误, true);
         return false;
      }
   }

   public bool DeleteUser(UserModel user)
   {
      try
      {
         LocalUsers.Remove(user);
         this.Save(LocalLoggedinUser.Account, $"[删除用户]:[{user.Account}]", true);
         return true;
      }
      catch (Exception ex)
      {
         $"[删除用户]异常:\r\n{ex}".LogSetting(Log4NetLevelEnum.错误, true);
         return false;
      }
   }

   #region 用户超时自动退出
   /// <summary>
   /// 用户活动时调用
   /// </summary>
   public void RefreshAutoLogoutTimer()
   {
      LastActivityTime = DateTime.Now; // 仅更新时间戳
   }

   public event Action? BackHome = null;

   public async Task AutoLogoutTimerTick(DateTime time)
   {
      if (_parameterConfig.AdvancedConfig.AutoExitSuperAdminTime <= 0) //0秒为不退出
         return;
      // 检查是否超时
      if ((time - LastActivityTime).TotalSeconds >= _parameterConfig.AdvancedConfig.AutoExitSuperAdminTime)
      {
         await UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            if (LocalLoggedinUser.Level == (ulong)DefaultRoleEnum.超级管理员) //当登陆的为超级管理员时自动退出
            {
               LocalLoggedinUser = new UserModel();
               LoggedInUserType = LoggedInTypeEnum.未登陆;
               Growl.Info("长时间无操作，已自动退出登录。");
            }
            BackHome?.Invoke();
         });
      }
   }
   #endregion
}

[Languages]
public enum LoginModeEnum
{
   [Languages("用户登陆本地")]
   用户登陆本地,

   [Languages("用户登陆MES")]
   用户登陆MES,

   [Languages("优先MES登陆")]
   优先MES登陆,
}
