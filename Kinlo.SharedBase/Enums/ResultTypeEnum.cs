using Kinlo.SharedBase.Model;

namespace Kinlo.SharedBase.Enums;

public enum ResultTypeEnum
{
   #region 0~10区间为通用忽略或保留结果
   _ = 0,
   #endregion

   #region 11~20区间为通用OK结果
   OK = 11,
   #endregion

   #region 21~100为通用NG结果
   NG = 21,
   异常 = 22,
   未找到设备 = 23,
   PLC无记忆 = 24,
   读取PLC数据失败 = 25,
   数据库找不到电池 = 26,
   保存数据库失败 = 27,
   上位机处理失败 = 28,
   索引超界 = 29,
   取值失败 = 30,
   通信失败 = 31,
   结果和期望数据类型不同 = 32,
   报文校验不通过 = 33,
   数据类型不支持读取 = 34,
   设备已停机 = 99,
   未知NG原因 = 100,
   #endregion

   #region 101-200为扫码枪NG结果
   扫码失败 = 101,
   扫码成功但规则验证失败 = 102,
   扫码成功但个数不符 = 103,
   条码重复 = 104,
   条码对比NG = 105,
   #endregion

   #region 201-300为电子称结果
   称重偏重 = 201,
   称重偏轻 = 202,
   称重异常 = 203,
   称重不稳定 = 204,
   #endregion

   #region 301-400为注液量结果
   注液量发送失败 = 301,
   注液量偏多 = 302,
   注液量偏少 = 303,
   失液量超标 = 304,
   PLC和PC注液量不对应 = 305,
   目标注液量超注液泵上限 = 306,
   目标注液量低于注液泵下限 = 307,
   目标补液量超补液泵上限 = 308,
   目标补液量低于补液泵下限 = 309,
   少液回流次数超上限 = 310,
   测漏回流次数超上限 = 311,

   /// <summary>
   /// 已有量 + 本次计划注液量 超过了保液量
   /// </summary>
   预注液超保液量 = 312,
   #endregion

   #region 401-500为电池Hipot测试结果
   //艾测3200系列Hipot测试结果
   Hipot_NG = 401,
   E01_开路 = 402,
   E02_未达到设定电压值 = 403,
   E03_电压上升过快_TL报警 = 404,
   E04_电压上升过慢_TH报警 = 405,
   E05_上升阶段VD1超限 = 406,
   E06_电压保持VD2超限 = 407,
   E07_自由放电VD3超限 = 408,
   E08_电阻超下限报警 = 409,
   E09_电阻超上限报警 = 410,
   E10_电容超下限报警 = 411,
   E11_电容超上限报警 = 412,

   阻值超上限 = 413,
   阻值低于下限 = 414,
   Hipot仪器未测试 = 415,

   //锐捷系列
   E01_开路1 = 421,
   E02_严重短路 = 422,

   /// <summary>
   /// E03_电压未在设定时间范围内爬升至置电压
   /// </summary>
   E03_电压欠压 = 423,

   /// <summary>
   /// E04_电压超出范围上限
   /// </summary>
   E04_电压过压 = 424,
   E05_升压阶段Vd1超限 = 425,
   E06_电压保持Vd2超限 = 426,
   E07_TLNg_Tp超下限 = 427,
   E08_THNg_Tp超上限 = 428,
   E09_电阻超下限 = 429,
   E10_电容Ng = 430,
   E11_自由放电Vd3超限 = 431,
   外壳开路 = 432,

   //艾诺
   开路 = 441,
   严重短路 = 442,
   欠压 = 443,
   过压 = 444,
   VD1_NG = 445,
   VD2_NG = 446,
   VD3_NG = 447,
   TL_NG = 448,
   TH_NG = 449,
   电阻测试NG = 450,
   #endregion

   #region 501-600为电池电压测试结果
   测电压仪器未判定 = 501,
   电压超上限 = 502,
   电压低于下限 = 503,
   电压极差NG = 504, //标准差NG
   电压老化压降NG = 505,
   #endregion

   #region 601-700为电池打钉结果
   打钉NG = 601,
   打钉偏高 = 602,
   打钉偏低 = 603,
   拔钉前检测NG = 604,
   预压钉NG = 605,
   全压钉NG = 606,
   胶钉高度差NG = 607,
   钢珠未打 = 608,
   钢珠打偏 = 609,
   注液孔NG = 610,
   #endregion

   #region 701-800 MES相关
   MES判定NG = 701,
   MES异常 = 702,
   MES报文生成失败 = 703,
   MES通讯错误 = 704,
   #endregion

   #region 901-1000为其它NG结果
   回氦NG = 901,
   测漏NG = 902,
   CCD_NG = 903,
   测厚NG = 904,
   未取到进料框槽位信息 = 905,
   进料框槽位不对应 = 906,
   取前工序数据失败 = 907,
   前工序数据不在范围 = 908,
   数据类型不对应 = 909,
   空指针 = 910,
   需停机报警 = 999, //需停机报警
   #endregion
}

/// <summary>
/// 结果分类区域
/// </summary>
public enum ResultArea
{
   /// <summary>
   /// 忽略或未加工
   /// </summary>
   Ignore,
   OK,
   NG,
}

/// <summary>
/// 结果分类帮助
/// </summary>
public static class ResultTypeClassifier
{
   /// <summary>
   /// 获取结果区间
   /// </summary>
   /// <param name="result"></param>
   /// <returns></returns>
   public static ResultArea GetResultArea(this ResultTypeEnum result)
   {
      int v = (int)result;

      if (v <= ResultArea.Ignore.GetResultAreaBounds().Max)
         return ResultArea.Ignore;
      if (v <= ResultArea.OK.GetResultAreaBounds().Max)
         return ResultArea.OK;

      return ResultArea.NG;
   }

   /// <summary>
   /// 获取结果区域边界（左右值都是包含的）
   /// </summary>
   /// <param name="area"></param>
   /// <returns></returns>
   public static ResultRange GetResultAreaBounds(this ResultArea area) =>
      area switch
      {
         ResultArea.Ignore => new(0, 10),
         ResultArea.OK => new(11, 20),
         _ => new(21, int.MaxValue),
      };
}
