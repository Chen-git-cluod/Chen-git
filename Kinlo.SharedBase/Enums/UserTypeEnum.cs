namespace Kinlo.SharedBase.Enums;

/// <summary>
/// 用户登陆类型
/// </summary>
[Languages]
public enum LoggedInTypeEnum
{
   [Languages(["未登陆", "未登陆", "Not logged in"])]
   未登陆,

   [Languages(["本地登陆", "Masuk Lokal", "Log on Locally"])]
   本地登陆,

   [Languages(["MES登陆", "MES login", "MES login"])]
   MES登陆,
}
