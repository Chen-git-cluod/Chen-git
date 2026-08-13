using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(IsSingleton = false)]
public class UserRegisterViewModel : Screen
{
   public UserModel User { get; set; } = new UserModel();
   public LoginTypeEnum LoginType { get; set; }
   public UsersStatusConfig UsersStatus { get; set; }
   public RoleConfig Role { get; set; }
   public IEnumerable<string> UserNames
   {
      get
      {
         foreach (var user in UsersStatus.LocalUsers)
            yield return user.Account;
      }
   }

   // UserRegisterView? _usergisterView = null;
   IContainer _container;
   ParameterConfig _parameter;

   /// <summary>
   ///
   /// </summary>
   /// <param name="loginType"></param>
   public UserRegisterViewModel(IContainer container)
   {
      IsInput = false;
      _container = container;
      _parameter = container.Get<ParameterConfig>();
      UsersStatus = container.Get<UsersStatusConfig>();
      Role = container.Get<RoleConfig>();
   }

   protected override void OnViewLoaded()
   {
      base.OnViewLoaded();
      if (this.View is UserRegisterView usergisterView)
      {
         UsersStatus.CardAction = barcode =>
         {
            // $"取到卡号:{barcode}".LogRun(Log4NetLevelEnum.信息);
            UIThreadHelper.InvokeOnUiThreadAsync(() => usergisterView.pwdBox.Password = barcode);
         };

         usergisterView.Closed += (o, e) =>
         {
            UsersStatus.CardAction = null;
            IsInput = false;
         };
      }
   }

   private bool _isInput;

   /// <summary>
   /// 录入指纹
   /// </summary>
   public bool IsInput
   {
      get { return _isInput; }
      set
      {
         _isInput = value;
         if (value)
         {
            if (View == null || !(View is UserRegisterView _usergisterView))
            {
               string _msg = $"{LoginType}丢失View，请联系软件人员！";
               _msg.LogRun(Log4NetLevelEnum.错误, true);
               return;
            }
            _usergisterView.fingerprintInfo.Text = "请按同一根手指3次录入指纹";
            UsersStatus.CurrentFingerpringAction = (msg, device) =>
            {
               UIThreadHelper.Dispatcher.BeginInvoke(() =>
               {
                  #region 显示图片（有问题）
                  //MemoryStream stream = new MemoryStream();
                  //BitmapFormat.GetBitmap(msg, 100, 100, ref stream);
                  //var bitmapImage = new BitmapImage();
                  //bitmapImage.BeginInit();
                  //bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                  //bitmapImage.StreamSource = stream;
                  //bitmapImage.EndInit();
                  //userLoginView.fingerprintImage.Source = bitmapImage;
                  //stream.Dispose();
                  //stream.Close();
                  //User.Fingerprint = msg;
                  //Growl.Success($"录入指纹成功！");
                  #endregion
                  if (device.DBIdentify(msg) > 0)
                  {
                     Growl.Warning("此指纹已被注册！");
                     return;
                  }
                  int _count = Fingerprints.Count;
                  if (_count > 0)
                  {
                     if (!device.DBMatch(msg, Fingerprints[_count - 1]))
                     {
                        Growl.Warning("请按同一根手指！");
                        return;
                     }
                  }
                  Fingerprints.Add(msg);
                  _count = Fingerprints.Count;
                  _usergisterView.fingerprintInfo.Text = $"还需录入{3 - _count}次指纹。";
                  if (_count > 2)
                  {
                     var ret = device.DBMerge(Fingerprints.ToList());
                     if (ret.state)
                     {
                        var _newId = UsersStatus.FingerpringDatas.Count == 0 ? 1 : UsersStatus.FingerpringDatas.Keys.Max() + 1;
                        UsersStatus.FingerpringDatas.Add(_newId, ret.template);
                        device.DBAdd(_newId, ret.template);
                        User.FingerprintID = _newId;
                        _usergisterView.fingerprintInfo.Text = $"录入指纹成功。";
                        $"录入指纹成功！".LogRun(Log4NetLevelEnum.信息, true);
                     }
                     else
                     {
                        _usergisterView.fingerprintInfo.Text = $"合并指纹错误，请查看日志！";
                        $"合并指纹错误，请查看日志！".LogRun(Log4NetLevelEnum.错误, true);
                     }
                     Fingerprints.Clear();
                  }
               });
            };
         }
         else
         {
            Fingerprints.Clear();
            UsersStatus.CurrentFingerpringAction = null;
         }
      }
   }
   public ObservableCollection<byte[]> Fingerprints { get; set; } = new ObservableCollection<byte[]>();

   /// <summary>
   /// 清除指纹
   /// </summary>
   public async void CancelFingerprintCMD()
   {
      if (User.FingerprintID > 0)
      {
         if (UsersStatus.FingerpringDatas.ContainsKey(User.FingerprintID))
         {
            UsersStatus.FingerpringDatas.Remove(User.FingerprintID);
         }
         if (UsersStatus.Live20RFingerprints != null)
         {
            UsersStatus.Live20RFingerprints.DBDel(User.FingerprintID);
         }
         await UIThreadHelper.Dispatcher.BeginInvoke(() =>
         {
            User.FingerprintID = 0;
         });
      }
      UsersStatus.Save(User.Name, "清除指纹完成");
      Growl.Success($"清除指纹完成！");
   }

   public async Task ConfirmCMD()
   {
      if (View == null || !(View is UserRegisterView _usergisterView))
      {
         string _msg = $"{LoginType}丢失View，请联系软件人员！";
         _msg.LogRun(Log4NetLevelEnum.错误, true);
         return;
      }
      if (!string.IsNullOrEmpty(_usergisterView.pwdBox.Password))
         User.Password = _usergisterView.pwdBox.Password;
      //if (!string.IsNullOrEmpty(_usergisterView.mesPwdBox.Password))
      //    User.MESPassword = _usergisterView.mesPwdBox.Password;
      if (string.IsNullOrEmpty(User.Account) || string.IsNullOrEmpty(User.Password))
      {
         Growl.Warning($"[{LoginType}]需要输入帐号及密码！");
         return;
      }

      bool _state = LoginType switch
      {
         LoginTypeEnum.用户注册 => UsersStatus.CreateUser(User),
         _ => UsersStatus.UpdateUser(User),
      };

      if (_state)
      {
         Growl.Success($"[{LoginType}]成功！");
         this.RequestClose();
      }
      else
      {
         Growl.Warning($"[{LoginType}]失败，详情请查看日志！");
      }
      await Task.CompletedTask;
   }
}
