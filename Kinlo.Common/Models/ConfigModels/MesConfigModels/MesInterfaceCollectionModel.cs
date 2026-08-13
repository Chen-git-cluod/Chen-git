namespace Kinlo.Common.Models.ConfigModels.MesConfigModels;

[AddINotifyPropertyChangedInterface]
public class MesInterfaceCollectionModel
{
   /// <summary>
   /// HttpClient 基地址
   /// </summary>
   public string BaseAddress { get; set; } = string.Empty;

   /// <summary>
   /// HttpClient 基地址2
   /// </summary>
   public string BaseAddress2 { get; set; } = string.Empty;

   /// <summary>
   /// MES本地网口IP
   /// </summary>
   public string LocalMesIP { get; set; } = string.Empty;

   /// <summary>
   /// MES总超时,单位为毫秒
   /// 包含了从建立连接到读取完响应的所有时间，也就是发起请求到读取结束的总时间
   /// </summary>
   public int Timeout { get; set; } = 5000;
   public int RetryCount { get; set; } = 3;

   public ObservableCollection<MesInterfaceInfoModel> MesParameterItems { get; set; } = new ObservableCollection<MesInterfaceInfoModel>();
}
