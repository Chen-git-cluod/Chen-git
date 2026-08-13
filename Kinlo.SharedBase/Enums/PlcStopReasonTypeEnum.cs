namespace Kinlo.SharedBase.Enums;

public enum PlcStopTypeEnum
{
   主动停机,
   被动停机,
}

public enum PlcStopReasonTypeEnum
{
   吃饭_休息 = 1,
   清洁_5S = 2,
   验证测试 = 3,
   设备清洁 = 4,
   设备点检 = 5,
   设备维保 = 6,
   换型 = 7,
   治具更换 = 8,
   物料更换 = 9,
   缺料停机 = 10,
   动力异常 = 11,
   参数调整 = 12,
   返工作业 = 13,
   其它停机 = 14,
   设备故障 = 15,
}
