using HandyControl.Controls;
using Kinlo.SharedBase.Model;

namespace Kinlo.Common.Configurations;

/// <summary>
/// 用户登陆
/// </summary>
public partial class UsersStatusConfig
{
   /// <summary>
   /// 登陆入口
   /// </summary>
   /// <param name="account">如果快捷登陆启用，1:超级管理员;2:调试员</param>
   /// <param name="password"></param>
   /// <param name="cardNumber">卡号</param>
   /// <param name="fingerprintId">指纹ID</param>
   /// <param name="loginType">登陆类型</param>
   /// <returns></returns>
   public async Task<bool> LoginAsync(string account, string password, string cardNumber, int fingerprintId, LoginAccountTypeEnum loginType)
   {
      try
      {
         if (LoginMode is LoginModeEnum.用户登陆MES or LoginModeEnum.优先MES登陆) //如果开启MES登陆，优先MES登陆
         {
            if (await MesLoginAsync(account, password, cardNumber, fingerprintId, loginType))
            {
               return true;
            }
            //只从MES登陆
            if (LoginMode is LoginModeEnum.用户登陆MES)
               return false;
         }

         return await LocalLoginAsync(account, password, cardNumber, fingerprintId, loginType);
      }
      catch (Exception ex)
      {
         Growl.Warning($"登陆异常：{ex}");
         return false;
      }
   }

   /// <summary>
   /// 本地登陆
   /// </summary>
   /// <param name="account">如果快捷登陆启用，1:超级管理员;2:调试员</param>
   /// <param name="password"></param>
   /// <param name="cardNumber">卡号</param>
   /// <param name="fingerprintId">指纹ID</param>
   /// <param name="loginType">登陆类型</param>
   /// <returns></returns>
   private async Task<bool> LocalLoginAsync(
      string account,
      string password,
      string cardNumber,
      int fingerprintId,
      LoginAccountTypeEnum loginType
   )
   {
      var loginUser = loginType switch //本地登陆
      {
         LoginAccountTypeEnum.刷卡登陆 => LocalUsers.FirstOrDefault(x => x.Password == cardNumber),
         var t when t == LoginAccountTypeEnum.指纹登陆 && fingerprintId > 0 => LocalUsers.FirstOrDefault(x =>
            x.FingerprintID == fingerprintId
         ),
         _ => LocalUsers.FirstOrDefault(x => x.Account == account && x.Password == password),
      };
      if (loginUser != null)
      {
         LoggedInUserType = LoggedInTypeEnum.本地登陆;
         loginUser.LoginTime = DateTime.Now;
         LocalLoggedinUser = loginUser;
         await SyncPLC(LocalLoggedinUser); //同步PLC
         return true;
      }

      $"登陆失败,帐号或密码错误!".LogRun(Log4NetLevelEnum.错误, true);
      return false;
   }

   public Func<string, string, string, int, LoginAccountTypeEnum, Task<MesResultModel<UserModel>>> OnMesLoginAsync;

   /// <summary>
   /// MES登陆
   /// </summary>
   /// <param name="account">如果快捷登陆启用，1:超级管理员;2:调试员</param>
   /// <param name="password"></param>
   /// <param name="cardNumber">卡号</param>
   /// <param name="fingerprintId">指纹ID</param>
   /// <param name="loginType">登陆类型</param>
   /// <returns></returns>
   private async Task<bool> MesLoginAsync(
      string account,
      string password,
      string cardNumber,
      int fingerprintId,
      LoginAccountTypeEnum loginType
   )
   {
      if (OnMesLoginAsync == null)
         return false;

      var mesRes = await OnMesLoginAsync.Invoke(account, password, cardNumber, fingerprintId, loginType);
      if (mesRes.ResultStatus == MesResultStatusEnum.成功)
      {
         LoggedInUserType = LoggedInTypeEnum.MES登陆;
         LocalLoggedinUser = mesRes.Data;
         await SyncPLC(LocalLoggedinUser); //同步PLC
         return true;
      }
      else
      {
         Growl.Warning(mesRes.ErrMsg);
      }
      return false;
   }

   #region 同步PLC
   /// <summary>
   /// 同步PLC
   /// </summary>
   /// <returns></returns>
   public async Task SyncPLC(UserModel user)
   {
      var role = _roleConfigLazy.Value.Roles.FirstOrDefault(x => x.Level == user.Level);
      if (role == null)
      {
         HandyControl.Controls.Growl.Warning($"未找到[{user.Account}]的对应权限[{user.Level}]，不发PLC");
         return;
      }
      if (
         _parameterConfig.FunctionEnable.IsEnableSyncPLCInquire
         && HandyControl.Controls.MessageBox.Show(
            $"要同步[{role.Name}权限至PLC吗?",
            "提示：",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning
         ) != MessageBoxResult.OK
      )
      {
         return;
      }

      var plc = _devicesConfigLazy.Value.GetRunDevice(x => x.DeviceInfo.ProcessesType == ProcessTypeEnum.PLC);
      if (plc != null)
      {
         Send(plc, user, role);
      }
      else
      {
         var device = _devicesConfigLazy.Value.DeviceList.FirstOrDefault(x => x.ProcessesType == ProcessTypeEnum.PLC);
         if (device == null)
         {
            Growl.Warning($"未找到PLC配置文件！");
            return;
         }
         await device.WithCreatedDeviceAsync(async plc => await Task.Run(() => Send(plc, user, role)));
      }
   }

   private void Send(IDevice plc, UserModel user, RoleModel role)
   {
      //写入权限
      var custom = _signalLazy.Value.CustomPlcInteractAddresses.FirstOrDefault(x =>
         x.CustomInteractName == CustomInteractNameEnum.PC至PLC用户权限
      );
      if (custom != null && custom.IsEnable && !string.IsNullOrEmpty(custom.DataAddress.Lable))
      {
         var levelResult = plc.WriteValue(role.PlcLevel, custom.DataAddress, "[PLC权限]写入权限");
         $"[PLC权限] 权限 [{role.PlcLevel}],写入 [{JsonSerializer.Serialize(custom.DataAddress)}] {(levelResult ? "成功" : "失败")}".LogRun(
            levelResult ? Log4NetLevelEnum.成功 : Log4NetLevelEnum.警告
         );
      }
      else
      {
         $"[PLC权限] 未设定相关地址或未启用，不写入PLC;".LogRun(Log4NetLevelEnum.警告);
      }

      //写入名字
      custom = _signalLazy.Value.CustomPlcInteractAddresses.FirstOrDefault(x =>
         x.CustomInteractName == CustomInteractNameEnum.PC至PLC用户名
      );
      if (custom != null && custom.IsEnable && !string.IsNullOrEmpty(custom.DataAddress.Lable))
      {
         var nameLen = custom.DataAddress.Lable.Length < 2 ? 16 : custom.DataAddress.Lable.Length;
         var sendName = user.Name.PadRight(nameLen, ' ');
         var sendRss = plc.WriteValue(sendName, custom.DataAddress, "[PLC用户名]写入名字", encoding: Encoding.UTF8);
         $"[PLC权限] 用户名 [{user.Name}],写入 [{JsonSerializer.Serialize(custom.DataAddress)}] {(sendRss ? "成功" : "失败")}".LogRun(
            sendRss ? Log4NetLevelEnum.成功 : Log4NetLevelEnum.警告
         );
      }
      else
      {
         $"[PLC用户名] 未设定相关地址或未启用，不写入PLC;".LogRun(Log4NetLevelEnum.警告);
      }
   }
   #endregion
}
