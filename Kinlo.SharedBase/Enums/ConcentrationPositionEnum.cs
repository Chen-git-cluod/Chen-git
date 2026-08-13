namespace Kinlo.SharedBase.Enums;

[Languages(IsScanProperty = true)]
public enum ConcentrationPositionEnum
{
   [Languages("全部", "", "")]
   全部,

   [Languages("注液站", "", "")]
   注液站,

   [Languages("储液柜", "", "")]
   储液柜,

   [Languages("补液站", "", "")]
   补液站,
}
