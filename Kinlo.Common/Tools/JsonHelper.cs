namespace Kinlo.Common.Tools;

public static class JsonHelper
{
   /// <summary>
   /// 解析JSON字符串为指定类型的对象
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="json"></param>
   /// <param name="extractor">提取器</param>
   /// <returns></returns>
   public static T ParseJson<T>(this string json, Func<JsonElement, T> extractor)
   {
      using var doc = JsonDocument.Parse(json);
      return extractor(doc.RootElement);
   }
}
