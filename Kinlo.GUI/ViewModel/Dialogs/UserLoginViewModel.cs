using HandyControl.Controls;
using SqlSugar;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(IsSingleton = false)]
public class UserLoginViewModel : Screen
{
   public UserModel User { get; set; } = new UserModel();
   public UsersStatusConfig UsersStatus { get; set; }
   public IEnumerable<string> UserNames
   {
      get
      {
         foreach (var user in UsersStatus.LocalUsers)
            yield return user.Account;
      }
   }

   IContainer _container;
   ParameterConfig _parameter;

   /// <summary>
   ///
   /// </summary>
   /// <param name="container"></param>
   public UserLoginViewModel(IContainer container)
   {
      _container = container;
      _parameter = container.Get<ParameterConfig>();
      UsersStatus = container.Get<UsersStatusConfig>();
   }

   public async Task ConfirmCMD()
   {
      if (View == null || !(View is UserLoginView userLoginView))
      {
         string _msg = $"登陆丢失View，请联系软件人员！";
         _msg.LogRun(Log4NetLevelEnum.错误, true);
         return;
      }

      if (!string.IsNullOrEmpty(userLoginView.pwdBox.Password))
         User.Password = userLoginView.pwdBox.Password;
      var loginStatus = await UsersStatus.LoginAsync(User.Account, User.Password, string.Empty, 0, LoginAccountTypeEnum.常规登陆);

      if (loginStatus)
      {
         Growl.Success($"登陆成功！");
         this.RequestClose();
      }
      else
      {
         Growl.Warning($"登陆失败，详情请查看日志！");
      }
      await Task.CompletedTask;
   }
}
