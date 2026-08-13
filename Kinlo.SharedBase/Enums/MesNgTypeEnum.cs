namespace Kinlo.SharedBase.Enums;

/// <summary>
/// 一注ngtype
/// </summary>
public enum MesNgTypeEnum
{
   None = 0,

   /// <summary>
   /// 一注短路測試NG
   /// </summary>
   ZY001_shortCutTest = 1,

   /// <summary>
   /// 一注前称重NG
   /// </summary>
   ZY001_preWeight = 2,

   /// <summary>
   /// 一注測漏NG
   /// </summary>
   ZY001_sideLeak = 3,

   /// <summary>
   /// 一注注液NG
   /// </summary>
   ZY001_injection = 4,

   /// <summary>
   /// 一注後称重NG
   /// </summary>
   ZY001_postWeight = 5,

   /// <summary>
   /// 一注膠釘NG
   /// </summary>
   ZY001_plasticStud = 6,

   /// <summary>
   /// 二注取一注數據NG
   /// </summary>
   ZY002_dataFromFirstInjection = 101,

   /// <summary>
   /// 二注前称重NG
   /// </summary>
   ZY002_preWeight = 102,

   /// <summary>
   /// 二注測漏NG
   /// </summary>
   ZY002_sideLeak = 103,

   /// <summary>
   /// 二注注液NG
   /// </summary>
   ZY002_injection = 104,

   /// <summary>
   /// 二注後称重NG
   /// </summary>
   ZY002_postWeight = 105,

   /// <summary>
   /// 二注回氦NG
   /// </summary>
   ZY002_heLeakingTest = 106,

   /// <summary>
   /// 二注膠釘NG
   /// </summary>
   ZY002_plasticStud = 107,
}
