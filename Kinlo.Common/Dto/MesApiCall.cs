namespace Kinlo.Common.Dto;

public class MesApiCall
{
   public MesApiCall(IMesArgs? mesArgs, string interfaceName, string url, string request, int pollingInterval, bool isEnable)
   {
      MesArgs = mesArgs;
      InterfaceName = interfaceName;
      Url = url;
      Request = request;
      PollingIntervalSec = pollingInterval;
      IsEnable = isEnable;
   }

   /// <summary>
   /// 获取MES接口及请求报文对应的参数
   /// </summary>
   public IMesArgs? MesArgs { get; set; }
   public string Request { get; set; } = string.Empty;
   public string InterfaceName { get; set; } = string.Empty;
   public string Url { get; set; } = string.Empty;

   /// <summary>
   /// 频率(秒)
   /// </summary>
   public int PollingIntervalSec { get; set; } = 60;
   public bool IsEnable { get; set; }
}
