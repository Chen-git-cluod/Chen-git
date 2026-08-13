namespace Kinlo.Common.Models.ConfigModels.UserModels;

[AddINotifyPropertyChangedInterface]
//[SugarTable("User_n")]
[Languages(["用户管理", "Pengelolaan Pengguna", "User management"])]
public class UserModel
{
   /// <summary>
   /// 帐号
   /// </summary>
   [Languages(["帐号", "Akun", "Account"])]
   public string Account { get; set; } = "notLoggedIn";

   /// <summary>
   /// 姓名
   /// </summary>
   [Languages(["姓名", "Nama", "Name"])]
   public string Name { get; set; } = "未定义";

   /// <summary>
   /// 密码
   /// </summary>
   [Languages(["密码", "Kata sandi", "Password"])]
   public string Password { get; set; } = string.Empty;

   /// <summary>
   /// 指纹ID
   /// </summary>
   [Languages(["指纹ID", "ID Sidik Jari", "Fingerprint ID"])]
   public int FingerprintID { get; set; } = 0;

   /// <summary>
   /// 部门ID
   /// </summary>
   public ulong Level { get; set; }

   /// <summary>
   /// 电话
   /// </summary>
   [Languages(["电话", "Telepon", "Phone"])]
   public string Tel { get; set; } = string.Empty;

   /// <summary>
   /// 注册时间
   /// </summary>
   [Languages(["注册时间", "Waktu pendaftaran", "Register time"])]
   public DateTime RegisterTime { get; set; }

   /// <summary>
   /// 最后一次登录时间
   /// </summary>
   [Languages(["上次登陆时间", "Waktu pendaratan terakhir", "Last login time"])]
   public DateTime LoginTime { get; set; }

   /// <summary>
   /// 修改时间
   /// </summary>
   [Languages(["修改时间", "Modifikasi waktu", "Update time"])]
   public DateTime UpdateTime { get; set; }
}
