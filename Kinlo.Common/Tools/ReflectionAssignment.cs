namespace Kinlo.Common.Tools;

public class ReflectionAssignment
{
   public static void Trans(object fromData, object toData)
   {
      foreach (var pro in fromData.GetType().GetProperties())
      {
         var _dataPropertyInfo = toData.GetType().GetProperty(pro.Name);
         if (_dataPropertyInfo != null)
         {
            _dataPropertyInfo.SetValue(toData, pro.GetValue(fromData));
         }
      }
   }
}
