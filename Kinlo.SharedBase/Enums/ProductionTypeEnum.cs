namespace Kinlo.SharedBase.Enums;

[Languages]
public enum ProductionTypeEnum
{
   [Languages("一次注液", "Satu kali penyuntikan", "Single injection")]
   一次注液 = 0,

   [Languages("二次注液", "Injeksi kedua", "Second injection")]
   二次注液 = 1,

   [Languages("三次注液", "", "tertiary injection")]
   三次注液 = 2,

   [Languages("回氦", "Kembali ke Helium", "Helium")]
   回氦 = 11,

   [Languages("清洗机")]
   清洗机 = 12,
}
//public enum GetNetweightPositionEnum
//{
//    关闭,
//    Mes,
//    其它机台,
//    本机,
//}
