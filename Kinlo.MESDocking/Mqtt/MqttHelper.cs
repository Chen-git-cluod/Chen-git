namespace Kinlo.MESDocking;

public class MqttHelper : MqttBase
{
   public MqttHelper(IContainer container)
      : base(container) { }

   public async Task SendProcessData(string msg)
   {
      //TOPIC：/aiipc/{工序编码}/{设备编码}
      //string _topic = $"/aiipc/{_parameterConfig.DeviceParameter.ProcessesCode}/{_parameterConfig.DeviceParameter.DeviceCode}";
      string _topic = _mesConfig.MqttServiceInfo.Topic;
      await SendMessageAsync(_topic, msg);
   }
}
