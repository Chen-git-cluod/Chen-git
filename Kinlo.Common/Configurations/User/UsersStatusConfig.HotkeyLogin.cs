namespace Kinlo.Common.Configurations;

/// <summary>
/// 热键登陆
/// </summary>
public partial class UsersStatusConfig
{
   /// <summary>
   /// 注册热键
   /// </summary>
   public void RegisterHotkey()
   {
      KeyboardHook.Hook.RegistrationListening(
         new ShortcutKeyModel
         {
            IsCrtl = true,
            IsAlt = true,
            Key = 'K',
            Action = () => LoginWithHotkey(1),
         }
      );
      KeyboardHook.Hook.RegistrationListening(
         new ShortcutKeyModel
         {
            IsCrtl = true,
            Key = 'W',
            Action = () => LoginWithHotkey(2),
         }
      );
      KeyboardHook.Hook.RegistrationListening(
         new ShortcutKeyModel
         {
            IsCrtl = true,
            Key = 'G',
            Action = () => LoginWithHotkey(3),
         }
      );
      KeyboardHook.Hook.RegistrationListening(
         new ShortcutKeyModel
         {
            IsCrtl = true,
            Key = 'B',
            Action = () => LoginWithHotkey(4),
         }
      );
      KeyboardHook.Hook.RegistrationListening(
         new ShortcutKeyModel
         {
            IsCrtl = true,
            Key = 'B',
            Action = () => LoginWithHotkey(4),
         }
      );
   }

   /// <summary>
   /// 热键登陆
   /// </summary>
   /// <param name="key"></param>
   /// <returns></returns>
   public bool LoginWithHotkey(int key)
   {
      ObservableCollection<RoleModel> _roles = _container.Get<RoleConfig>().Roles;
      UserModel? loginUser = key switch
      {
         1 => new UserModel { Account = "超级用户", Level = (ulong)DefaultRoleEnum.超级管理员 },
         2 => new UserModel { Account = "调试管理员", Level = (ulong)DefaultRoleEnum.管理员 },
         3 => new UserModel { Account = "调试工艺", Level = (ulong)DefaultRoleEnum.工艺 },
         4 => new UserModel { Account = "调试设备", Level = (ulong)DefaultRoleEnum.设备 },
         _ => null,
      };
      if (loginUser == null)
         return false;

      LoggedInUserType = LoggedInTypeEnum.本地登陆;
      loginUser.LoginTime = DateTime.Now;
      LocalLoggedinUser = loginUser;
      return true;
   }
}
