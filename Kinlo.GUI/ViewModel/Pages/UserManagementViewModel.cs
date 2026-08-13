namespace Kinlo.GUI.ViewModel;

[Languages(["用户管理", "Pengelolaan Pengguna", "User management"], IsScanProperty = false)]
[UIDisplayAttribute(true, 21, ((ulong)1) << 62, isRunEdit: true, "\xe660")]
public class UserManagementViewModel : Screen, IMenu
{
   IContainer _container;
   IWindowManager _windowManager;
   public UsersStatusConfig UsersStatus { get; set; }
   public Lazy<RoleConfig> RoleConfigLazy { get; set; }

   public UserManagementViewModel(IContainer container, IWindowManager windowManager)
   {
      _container = container;
      _windowManager = windowManager;
      UsersStatus = container.Get<UsersStatusConfig>();
      RoleConfigLazy = new Lazy<RoleConfig>(() => container.Get<RoleConfig>());
   }

   /// <summary>
   /// 新增用户
   /// </summary>
   public void CreateUserCMD()
   {
      UserRegisterViewModel _userLoginVM = _container.Get<UserRegisterViewModel>();
      _userLoginVM.User = new UserModel { Account = "", Name = "" };
      _userLoginVM.LoginType = LoginTypeEnum.用户注册;
      _windowManager.ShowDialog(_userLoginVM);
   }

   /// <summary>
   ///  修改用户
   /// </summary>
   public void UpeateUserCMD(UserModel selectUser)
   {
      UserModel user = new UserModel
      {
         Account = selectUser.Account,
         Name = selectUser.Name,
         Password = selectUser.Password,
         FingerprintID = selectUser.FingerprintID,
         //MESPassword = user.MESPassword,
         //MESAccount = user.MESAccount,
         Level = selectUser.Level,
         LoginTime = selectUser.LoginTime,
         RegisterTime = selectUser.RegisterTime,
         Tel = selectUser.Tel,
         UpdateTime = selectUser.UpdateTime,
      };
      UserRegisterViewModel _userLoginVM = _container.Get<UserRegisterViewModel>();
      _userLoginVM.User = user;
      _userLoginVM.LoginType = LoginTypeEnum.修改用户;
      _windowManager.ShowDialog(_userLoginVM);
   }

   /// <summary>
   ///  删除用户
   /// </summary>
   public void DeleteUserCMD(UserModel user)
   {
      var _dialog = MessageBox.Show($"确定删除用户:[{user.Account}]?", "警告:", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
      if (_dialog != MessageBoxResult.OK)
         return;
      ((UserManagementView)this.View).dataGrid.UnselectAll();
      UsersStatus.DeleteUser(user);
   }

   public void Load()
   {
      UsersStatus.IsLogin = false;
   }

   public bool Unload() => UsersStatus.IsLogin = true;
}
