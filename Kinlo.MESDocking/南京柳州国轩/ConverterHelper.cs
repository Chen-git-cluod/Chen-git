namespace Kinlo.MESDocking;

public static class ConverterHelper
{
   public static string GetMesHipotResult(this object value, params ResultTypeEnum[] resultTypes)
   {
      if (value is ResultTypeEnum result)
      {
         var resultInt = (int)result;
         if (resultInt < 11)
            return "未加工";
         if (resultInt < 21)
            return "OK";
         foreach (var item in resultTypes)
         {
            if (result == item)
               return "NG";
         }
         return "OK";
      }
      return "";
   }
}
