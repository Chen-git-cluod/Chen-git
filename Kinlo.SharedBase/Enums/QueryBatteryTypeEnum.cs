namespace Kinlo.SharedBase.Enums;

//[Languages]
//public enum QueryBatteryTypeEnum
//{
//  [Languages(["全部", "Semua", "All"])]
//  全部,

//  [Languages(["只看合格", "Hanya melihat yang memenuhi syarat", "Only consider qualified"])]
//  只看合格,

//  [Languages(["只看不合格", "Hanya melihat yang tidak memenuhi syarat", "Only consider unqualified"])]
//  只看不合格,
//}

[Languages]
public enum QueryMesResendTypeEnum
{
   [Languages(["全部", "Semua", "All"])]
   全部,

   [Languages(["未上传", "", "Not Uploaded"])]
   未上传,

   [Languages(["已上传", "", "Uploaded"])]
   已上传,
}
