namespace Kinlo.Equipment.Devices.CIP;

/// <summary>
/// 常规Omron Cip 如NJ500,NX500 支持连接数较多系列
/// </summary>
[DeviceConnec([CommunicationEnum.CipOrmonPlc])]
public class OmronCip : CipBase
{
   public OmronCip(DeviceInfoModel info)
      : base(info) { }

   public override bool Open()
   {
      Close();
      string logHeader = DeviceInfo.ToDeviceLogHeader();
      for (int i = 0; i < 6; i++)
      {
         if (i == 0)
         {
            var clinet = this.BuildCip(CipMode.有连接模式, i + 1, logHeader);
            if (clinet == null)
            {
               Close();
               return false;
            }

            ScanConnected = clinet;
         }
         else if (i < 3) //有连接模式
         {
            var clinet = this.BuildCip(CipMode.有连接模式, i + 1, logHeader);
            if (clinet == null)
            {
               Close();
               return false;
            }
            Connected.Add(clinet);
         }
         else //无连接模式
         {
            var clinet = this.BuildCip(CipMode.无连接模式, i + 1, logHeader);
            if (clinet == null)
            {
               Close();
               return false;
            }
            Unconnected.Add(clinet);
         }
      }
      return true;
   }

   public override void Close()
   {
      string logHeader = DeviceInfo.ToDeviceLogHeader();
      while (Connected.TryTake(out var connected))
      {
         connected.Close(logHeader + $"-[{connected.Index}]");
      }
      while (Unconnected.TryTake(out var unconnected))
      {
         unconnected.Close(logHeader + $"-[{unconnected.Index}]");
      }
      ScanConnected.Close(logHeader + $"{(ScanConnected != null ? $"- [{ScanConnected.Index}]" : "")}");
   }
}
