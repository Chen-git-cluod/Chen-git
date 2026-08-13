using Kinlo.SharedBase.Model;

namespace Kinlo.GUI.ViewModel;

[UIDisplayAttribute(true)]
public class UserStatusViewModel : Screen
{
   public List<LoginModeEnum> LoginTypes { get; set; } = Enum.GetValues<LoginModeEnum>().ToList();
   public bool LoginTypeIsChecked { get; set; }
   public UsersStatusConfig UsersStatus { get; set; }
   public Lazy<RoleConfig> RoleConfigLazy { get; set; }

   MesInterfaceParameterConfig _mesInterfaceParameterConfig;
   IContainer _container;
   Stylet.IWindowManager _windowManager;

   public UserStatusViewModel(IContainer container, Stylet.IWindowManager windowManager)
   {
      _container = container;
      _windowManager = windowManager;
      UsersStatus = container.Get<UsersStatusConfig>();
      RoleConfigLazy = new Lazy<RoleConfig>(() => container.Get<RoleConfig>());
      _mesInterfaceParameterConfig = container.Get<MesInterfaceParameterConfig>();

      //注册热键登陆
      UsersStatus.RegisterHotkey();
      //注册 USB刷卡器 刷卡登陆
      UsersStatus.RegisterUsbCardReader();
      //注册MES登陆
      UsersStatus.OnMesLoginAsync = MesLoginAsync;
   }

   public void SelectedCmd()
   {
      LoginTypeIsChecked = false;
   }

   public void LocalLoginCMD()
   {
      UserLoginViewModel _userLoginVM = _container.Get<UserLoginViewModel>();
      _windowManager.ShowDialog(_userLoginVM);
   }

   public void LocalLogOutCMD()
   {
      UsersStatus.LocalLoggedinUser = new();
      UsersStatus.LoggedInUserType = LoggedInTypeEnum.未登陆;
   }

   /// <summary>
   /// MES登陆
   /// </summary>
   /// <param name="account">如果快捷登陆启用，1:超级管理员;2:调试员</param>
   /// <param name="password"></param>
   /// <param name="cardNumber">卡号</param>
   /// <param name="fingerprintId">指纹ID</param>
   /// <param name="loginType">登陆类型</param>
   /// <returns></returns>
   private async Task<MesResultModel<UserModel>> MesLoginAsync(
      string account,
      string password,
      string cardNumber,
      int fingerprintId,
      LoginAccountTypeEnum loginType
   )
   {
      MesService mesService = _container.Get<MesService>();
      if (loginType == LoginAccountTypeEnum.指纹登陆)
      {
         return MesResultModel<UserModel>.RequestBuildError("指纹登陆未实现");
      }
      else //if (loginType == LoginAccountTypeEnum.刷卡登陆)//常规及刷卡登陆
      {
         // string pwd = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));//转base64

         var call = _mesInterfaceParameterConfig.GetApiCall(new MesRequestBuildNJGX.ArgsMesLogin(account, password));
         if (call == null || !call.IsEnable)
         {
            string error = "[MES登陆]接口未启用或未找到接口信息！";
            return MesResultModel<UserModel>.RequestBuildError(error);
         }
         return await mesService.SendAsync(call, "MES登录", receiveMes => receiveMes.MesCommonParse("MES登陆").MesLoginParse());
      }
   }
}
