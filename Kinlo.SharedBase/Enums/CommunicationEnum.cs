namespace Kinlo.SharedBase.Enums;

public enum CommunicationEnum
{
   None = 0,
   CipOrmonPlc = 1,

   /// <summary>
   /// 轻量版（只有一个连接）
   /// </summary>
   CipOrmonPlcLight = 2,

   /// <summary>
   /// 汇川 PLC
   /// </summary>
   CipInovance = 3,

   /// <summary>
   /// udp短连接(每次交互都新建连接)
   /// </summary>
   FinsUdpShortConn = 11,

   /// <summary>
   /// udp长连接
   /// </summary>
   FinsUdpLongConn = 12,
   FinsTcp = 13,
   S715_PLC = 14,
   S712_PLC = 15,
   Modbus_TCP_ABCD = 21,
   Modbus_TCP_BADC = 22,
   Modbus_TCP_DCBA = 23,
   Modbus_TCP_CDAB = 24,
   ScanCode_SR700 = 41,
   ScanCode_SR1000 = 42,
   RFID_RD900M = 46,

   /// <summary>
   ///
   /// </summary>
   RFID_BIS00EJ = 47,

   /// <summary>
   /// 泰合森
   /// </summary>
   RFID_TAIHESEN = 48,
   RFID_TAIHESEN_RJ45 = 49,
   Scale_Pris_TC06 = 61,
   Scale_AND_4531B = 62,

   /// <summary>
   /// 可竹电子称
   /// </summary>
   Scale_KZ313_RTU = 63,
   Scale_KZ313_TCP = 64,

   /// <summary>
   /// 赛多利斯电子称
   /// </summary>
   Scale_Sartorius赛多利斯 = 65,
   Scale_鸿伟城 = 66,
   Scale_XJC_C103 = 67,
   Scale_科迪手工称_5100 = 68,
   Scale_ZH8010_TCP = 69,
   Scale_TSL_Count=80,
   ShortCircuit_ST5520 = 81,
   ShortCircuit_RJ6902R = 82,
   ShortCircuit_RJ6902CAGX = 83,
   ShortCircuit_RJ6901A = 84,
   ShortCircuit_AC3200 = 85,
   ShortCircuit_AC1100T = 86,
   ShortCircuit_RJ6903GX = 87,
   ShortCircuit_Ainuo_ANBTS7201 = 88,

   /// <summary>
   /// 日志电压测试
   /// </summary>
   VoltogeTest_HIOKI_DM7275 = 91,
   VoltogeTest_Keysight = 92,
   VoltogeTest_AC35 = 93,
   ZTDTSU666电能表 = 101, //电能表
   DewPoint零点仪 = 102, //露点测试仪
   HDK_LDZ_DN50流量计 = 103, //浮子流量计，贺德克(HDK-LDZ-DN50)
   Live20R指纹器,
   HX540_H_E刷卡器,
   通用串口刷卡器,
   LibCardFee_一卡通读卡器,
}
