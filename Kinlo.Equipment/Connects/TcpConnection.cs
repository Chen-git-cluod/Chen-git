namespace Kinlo.Equipment.Connects;

public class TcpConnection : SocketConnection
{
   public TcpConnection(DeviceInfoModel info)
      : base(info) { }
}
