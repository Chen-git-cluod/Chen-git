namespace Kinlo.SharedBase.Attributes;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public class MesInterfaceInfoAttribute : Attribute
{
   public string Url { get; set; } = string.Empty;

   /// <summary>
   /// 频率(秒)（间隔时间）
   /// </summary>
   public int PollingInterval { get; set; }

   public MesInterfaceInfoAttribute(string url)
   {
      Url = url;
   }

   public MesInterfaceInfoAttribute(string url, int frequency)
   {
      Url = url;
      PollingInterval = frequency;
   }
}
//[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
//public class WebApiInterfaceInfoAttribute : Attribute
//{
//    public string Route { get; set; } = string.Empty;

//   // public string Name { get; set; }
//    public WebApiInterfaceInfoAttribute(string route)
//    {
//       // Name = name;
//        Route = route;
//    }
//}
