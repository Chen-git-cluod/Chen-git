namespace Kinlo.SharedBase.Enums;

public enum ProcessRoleEnum
{
   None,
   进站,
   出站,

   /// <summary>
   /// 如果设备只有一个工序，又是进站又是出站时
   /// </summary>
   进出站,
}
