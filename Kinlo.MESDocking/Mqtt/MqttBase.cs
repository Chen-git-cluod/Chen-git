using MQTTnet;
using MQTTnet.Protocol;

namespace Kinlo.MESDocking;

public abstract class MqttBase
{
   IContainer _container;
   protected MesInterfaceParameterConfig _mesConfig;
   protected ParameterConfig _parameterConfig;
   protected IMqttClient? _mqttClient;

   public MqttBase(IContainer container)
   {
      _container = container;
      _mesConfig = container.Get<MesInterfaceParameterConfig>();
      _parameterConfig = container.Get<ParameterConfig>();
   }

   #region 异步

   /// <summary>
   /// 连接到MQTT服务器的方法
   /// </summary>
   /// <returns></returns>
   public async Task ConnectToMqttServerAsync()
   {
      try
      {
         var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_mesConfig.MqttServiceInfo.BrokerAddress, _mesConfig.MqttServiceInfo.Port)
            .Build();
         _mqttClient = new MqttClientFactory().CreateMqttClient();
         // 连接到MQTT服务器
         await _mqttClient.ConnectAsync(options);
      }
      catch (Exception ex)
      {
         $"[Mqtt连接]异常,地址：{_mesConfig.MqttServiceInfo.BrokerAddress},端口：{_mesConfig.MqttServiceInfo.Port},详情：{ex}".LogRun(
            Log4NetLevelEnum.错误,
            true
         );
      }
   }

   /// <summary>
   /// 发送消息到指定主题的方法
   /// </summary>
   /// <param name="topic">主题</param>
   /// <param name="message"></param>
   /// <returns></returns>
   public async Task SendMessageAsync(string topic, string message)
   {
      try
      {
         $"[Mqtt发送]主题：{topic}，信息：{message}".LogRun();
         if (_mqttClient == null || !_mqttClient.IsConnected)
         {
            await ConnectToMqttServerAsync();
         }

         if (_mqttClient == null || !_mqttClient.IsConnected)
         {
            $"[Mqtt发送]主题：{topic}，无法实例化！".LogRun(Log4NetLevelEnum.错误, true);
            return;
         }

         var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(message)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

         // 发布消息
         await _mqttClient.PublishAsync(mqttMessage);
         $"[Mqtt发送]主题：{topic},完成".LogRun();
      }
      catch (Exception ex)
      {
         $"[Mqtt发送]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }
   #endregion
}
