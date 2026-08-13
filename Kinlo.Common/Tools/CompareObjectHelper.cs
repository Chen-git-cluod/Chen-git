namespace Kinlo.Common.Tools;

public static class CompareObjectHelper
{
   /// <summary>
   ///
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="before"></param>
   /// <param name="after"></param>
   /// <param name="differences">差异的集合</param>
   /// <returns></returns>
   public static StringBuilder CompareObject<T>(this T before, T after, Dictionary<string, DifferenceResultDto> differences)
   {
      StringBuilder sb = new StringBuilder();
      try
      {
         if (before == null || after == null)
            return sb;

         var afterProperties = after.GetType().GetProperties();

         foreach (var beforeProp in before.GetType().GetProperties())
         {
            if (beforeProp.GetCustomAttribute<JsonIgnoreAttribute>() != null)
               continue;

            var afterProp = afterProperties.FirstOrDefault(x => x.Name == beforeProp.Name);
            if (afterProp == null)
               continue;

            var beforeValue = beforeProp.GetValue(before);
            var afterValue = afterProp.GetValue(after);

            if (beforeValue == null && afterValue == null)
               continue;

            var description = afterProp.GetCustomAttribute<LanguagesAttribute>()?.Languages?.FirstOrDefault() ?? afterProp.Name;

            sb.CompareValueRecursive(beforeValue, afterValue, description, afterProp.Name, differences);
         }
      }
      catch (Exception ex)
      {
         sb.AppendLine($"异常：{ex.Message}");
      }

      return sb;
   }

   /// <summary>
   /// 比较两个集合的内容，并返回一个 StringBuilder 对象，包含所有不同的项及其值。
   /// </summary>
   /// <param name="sb"></param>
   /// <param name="before"></param>
   /// <param name="after"></param>
   /// <param name="description"></param>
   public static void CompareCollections(
      this StringBuilder sb,
      IEnumerable before,
      IEnumerable after,
      string description,
      string propertyName,
      Dictionary<string, DifferenceResultDto> differences
   )
   {
      if (before == null && after == null)
         return;
      if (before == null || after == null)
      {
         sb.GetSplicingContrast(before, after, description, propertyName, differences);
         return;
      }

      var beforeList = before.Cast<object>().ToList();
      var afterList = after.Cast<object>().ToList();

      if (beforeList.Count != afterList.Count)
      {
         sb.AppendLine($"集合 {description} 项数不同：原 {beforeList.Count} 项 -> 新 {afterList.Count} 项");
      }

      int min = Math.Min(beforeList.Count, afterList.Count);
      for (int i = 0; i < min; i++)
      {
         sb.CompareValueRecursive(beforeList[i], afterList[i], $"{description}[{i}]", $"{propertyName}[{i}]", differences);
      }

      // 如果有多余项
      if (afterList.Count > beforeList.Count)
      {
         for (int i = beforeList.Count; i < afterList.Count; i++)
         {
            sb.AppendLine($"集合 {description} 新增项 [{i}]：{afterList[i]}");
         }
      }
      else if (beforeList.Count > afterList.Count)
      {
         for (int i = afterList.Count; i < beforeList.Count; i++)
         {
            sb.AppendLine($"集合 {description} 删除项 [{i}]：{beforeList[i]}");
         }
      }
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="sb"></param>
   /// <param name="before"></param>
   /// <param name="after"></param>
   /// <param name="description"></param>
   private static void CompareValueRecursive(
      this StringBuilder sb,
      object before,
      object after,
      string description,
      string propertyName,
      Dictionary<string, DifferenceResultDto> differences
   )
   {
      var itemType = before?.GetType() ?? after?.GetType();

      if (itemType == null)
         return;

      if (typeof(IEnumerable).IsAssignableFrom(itemType) && itemType != typeof(string))
      {
         sb.CompareCollections(before as IEnumerable, after as IEnumerable, description, propertyName, differences);
      }
      else if (itemType.IsClass && itemType != typeof(string))
      {
         sb.Append(CompareObject(before, after, differences));
      }
      else if (!Equals(before, after))
      {
         sb.GetSplicingContrast(before, after, description, propertyName, differences);
      }
   }

   private static void GetSplicingContrast(
      this StringBuilder sb,
      object before,
      object after,
      string desc,
      string propertyName,
      Dictionary<string, DifferenceResultDto> differences
   )
   {
      differences[propertyName] = new DifferenceResultDto(before, after);
      // sb.AppendLine($"属性：{desc} == 原值：{before ?? "null"} --> 更改后：{after ?? "null"}");
      sb.AppendLine($"{desc} ==> 修改前：{before ?? "null"} -> 修改后：{after ?? "null"}");
   }
}

public class DifferenceResultDto
{
   public object? BeforeValue { get; set; }
   public object? AfterValue { get; set; }

   /// <summary>
   /// 因为修改参数给MES时，如果是上下限的值，会要一次传一对，如果取了就要打一个标记，表示这个值已经取过了。
   /// </summary>
   public bool IsFinish { get; set; }

   public DifferenceResultDto(object? beforeValue, object? afterValue)
   {
      BeforeValue = beforeValue;
      AfterValue = afterValue;
   }
}
