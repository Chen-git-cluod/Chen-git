namespace Kinlo.SharedBase.Enums;

[Languages]
public enum QueryBatteryMESStatusEnum
{
   [Languages(["全部", "Semua", "All"])]
   全部,

   [Languages(["未进站", "Belum diunggah", "Not entered"])]
   未进站,

   [Languages(["未出站", "Belum diunggah", "Not exited"])]
   未出站,

   [Languages(["进站失败", "Kegagalan", "Enter failed"])]
   进站失败,

   [Languages(["出站失败", "Kegagalan", "Exite failed"])]
   出站失败,

   [Languages(["全部失败", "Kegagalan", "All failed"])]
   进站或出站失败,
   // [Languages(["发送测试", "", "Send test"])] 发送测试,
}
