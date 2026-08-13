namespace Kinlo.Common.Tools;

/// <summary>
/// 电池运行时状态管理
/// </summary>
public static class BatteryRuntimeStateManager
{
   /// <summary>
   /// Json 序列化配置
   /// </summary>
   private static readonly JsonSerializerOptions JsonOptions = new() { Converters = { new JsonStringEnumConverter() } };

   /// <summary>
   /// 添加或更新运行时电池状态，记得添加后要做持久化保存
   /// </summary>
   public static bool AddOrUpdateRuntimeState(this IBatMainModel battery, RuntimeStateType key, string value)
   {
      try
      {
         Dictionary<RuntimeStateType, string> dic = [];

         // 已存在运行时状态时先反序列化
         if (!string.IsNullOrWhiteSpace(battery.RuntimeStateJson))
         {
            dic = JsonSerializer.Deserialize<Dictionary<RuntimeStateType, string>>(battery.RuntimeStateJson, JsonOptions);

            // Json存在但反序列化为空
            if (dic == null)
            {
               $"[添加或更新运行时电池状态]工序运行时状态数据[{battery.RuntimeStateJson}]反序列化为空!".LogRun(Log4NetLevelEnum.错误, true);

               return false;
            }
         }

         // 添加或覆盖状态
         dic[key] = value;

         // 回写 Json
         battery.RuntimeStateJson = JsonSerializer.Serialize(dic, JsonOptions);

         return true;
      }
      catch (Exception ex)
      {
         $"[添加或更新运行时电池状态]工序运行时状态处理失败：{ex}".LogRun(Log4NetLevelEnum.错误, true);

         return false;
      }
   }

   /// <summary>
   /// 获取电池运行时状态
   /// </summary>
   public static bool GetRuntimeState(this IBatMainModel battery, RuntimeStateType key, out string value)
   {
      value = string.Empty;

      try
      {
         // 无运行时状态数据
         if (string.IsNullOrWhiteSpace(battery.RuntimeStateJson))
         {
            return false;
         }

         var dic = JsonSerializer.Deserialize<Dictionary<RuntimeStateType, string>>(battery.RuntimeStateJson, JsonOptions);

         // Json存在但反序列化为空
         if (dic == null)
         {
            $"[获取电池运行时状态]工序运行时状态数据[{battery.RuntimeStateJson}]反序列化为空!".LogRun(Log4NetLevelEnum.错误, true);

            return false;
         }

         // 获取状态失败
         if (!dic.TryGetValue(key, out string? dicVal))
         {
            return false;
         }

         // 状态值为空
         if (string.IsNullOrWhiteSpace(dicVal))
         {
            return false;
         }

         value = dicVal;

         return true;
      }
      catch (Exception ex)
      {
         $"[获取电池运行时状态]工序运行时状态处理失败：{ex}".LogRun(Log4NetLevelEnum.错误, true);

         return false;
      }
   }

   /// <summary>
   /// 删除运行时电池状态，记得添加后要做持久化保存
   /// </summary>
   public static bool RemoveRuntimeState(this IBatMainModel battery, RuntimeStateType key)
   {
      try
      {
         // 无运行时状态数据
         if (string.IsNullOrWhiteSpace(battery.RuntimeStateJson))
         {
            return true;
         }

         var dic = JsonSerializer.Deserialize<Dictionary<RuntimeStateType, string>>(battery.RuntimeStateJson, JsonOptions);

         // Json存在但反序列化为空
         if (dic == null)
         {
            $"[删除运行时电池状态]工序运行时状态数据[{battery.RuntimeStateJson}]反序列化为空!".LogRun(Log4NetLevelEnum.错误, true);

            return false;
         }

         // 不存在指定 Key
         if (!dic.Remove(key))
         {
            return false;
         }

         // 删除后无数据则置空
         if (dic.Count == 0)
         {
            battery.RuntimeStateJson = null;
         }
         else
         {
            battery.RuntimeStateJson = JsonSerializer.Serialize(dic, JsonOptions);
         }

         return true;
      }
      catch (Exception ex)
      {
         $"[删除运行时电池状态]工序运行时状态处理失败：{ex}".LogRun(Log4NetLevelEnum.错误, true);

         return false;
      }
   }

   /// <summary>
   /// 判断是否存在运行时状态
   /// </summary>
   public static bool ContainsRuntimeState(this IBatMainModel battery, RuntimeStateType key)
   {
      try
      {
         // 无运行时状态数据
         if (string.IsNullOrWhiteSpace(battery.RuntimeStateJson))
         {
            return false;
         }

         var dic = JsonSerializer.Deserialize<Dictionary<RuntimeStateType, string>>(battery.RuntimeStateJson, JsonOptions);

         // Json存在但反序列化为空
         if (dic == null)
         {
            $"[判断运行时电池状态]工序运行时状态数据[{battery.RuntimeStateJson}]反序列化为空!".LogRun(Log4NetLevelEnum.错误, true);

            return false;
         }

         return dic.ContainsKey(key);
      }
      catch (Exception ex)
      {
         $"[判断运行时电池状态]工序运行时状态处理失败：{ex}".LogRun(Log4NetLevelEnum.错误, true);

         return false;
      }
   }

   /// <summary>
   /// 清空运行时状态，记得添加后要做持久化保存
   /// </summary>
   public static void ClearRuntimeState(this IBatMainModel battery)
   {
      battery.RuntimeStateJson = null;
   }
}
