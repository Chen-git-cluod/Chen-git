namespace Kinlo.Common.Models.ConfigModels.MesConfigModels;

public class MqttServiceInfoModel
{
   /// <summary>
   /// 服务器地址
   /// </summary>
   public string BrokerAddress { get; set; } = "10.0.0.2";
   public int Port { get; set; } = 1883;

   /// <summary>
   /// 话题
   /// </summary>
   public string Topic { get; set; } = string.Empty;

   /// <summary>
   /// 间隔时间
   /// </summary>
   public int Interval { get; set; } = 10000;
   public bool Enable { get; set; } = false;
}
