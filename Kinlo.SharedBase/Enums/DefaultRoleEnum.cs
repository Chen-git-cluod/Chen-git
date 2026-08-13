namespace Kinlo.SharedBase.Enums;

public enum DefaultRoleEnum : ulong
{
   [Languages("生产", "Produksi", "Production")]
   生产 = 1, //顺序不能随便改

   [Languages("工艺", "Kerajinan", "Craft")]
   工艺 = 2, //顺序不能随便改

   [Languages("设备", "Perangkat", "Equipment")]
   设备 = 4, //顺序不能随便改

   [Languages("管理员", "Administrator", "Administrator")]
   管理员 = ulong.MaxValue >> 1, //顺序不能随便改

   [Languages("管理员", "Super Administrator", "Super Administrator")]
   超级管理员 = ulong.MaxValue, //顺序不能随便改
}
