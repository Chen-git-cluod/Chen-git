using System.Collections.Specialized;
using System.ComponentModel;

namespace Kinlo.Common.Tools;

/// <summary>
/// 高性能实时变更追踪器。
/// 支持深度嵌套对象及集合（ObservableCollection）内部项的属性追踪。
/// </summary>
/// <typeparam name="T">根对象类型，必须实现 INotifyPropertyChanged</typeparam>
public class ChangeTracker<T> : IDisposable
   where T : class, INotifyPropertyChanged
{
   private readonly T _original; // 原始快照对象（基准）
   private readonly T _working; // UI 绑定的工作对象（副本）

   // 存储变更记录：Key = 属性全路径（如 WebApiRoutes[GetOrder].IsEnable）
   private readonly Dictionary<string, ChangeEntry> _changes = new();

   // 事件订阅记录表，用于在 Dispose 时精准解绑，防止内存泄漏
   // 元组内容：(事件源对象, 处理委托, 是否为集合事件)
   private readonly List<(object Target, Delegate Handler, bool IsCollection)> _subscriptions = new();

   /// <summary>
   /// 获取当前的变更项列表
   /// </summary>
   public IReadOnlyDictionary<string, ChangeEntry> GetChanges => _changes;

   /// <summary>
   /// 是否存在变更
   /// </summary>
   public bool HasChanges => _changes.Count > 0;

   public ChangeTracker(T original, T working)
   {
      _original = original ?? throw new ArgumentNullException(nameof(original));
      _working = working ?? throw new ArgumentNullException(nameof(working));

      // 从根对象开始启动递归订阅
      SubscribeRecursively(_working, _original, "");
   }

   /// <summary>
   /// 核心递归订阅方法
   /// </summary>
   /// <param name="workingObj">当前工作的对象层级</param>
   /// <param name="originalObj">对应的原始对象层级</param>
   /// <param name="prefix">路径前缀</param>
   private void SubscribeRecursively(object workingObj, object originalObj, string prefix)
   {
      if (workingObj == null || originalObj == null)
         return;

      // --- 情况 1：处理集合类型 (如 ObservableCollection<T>) ---
      if (workingObj is IList workingList && originalObj is IList originalList)
      {
         // 初始扫描：对集合内现有的每一项进行深度追踪
         for (int i = 0; i < workingList.Count; i++)
         {
            // 尝试匹配原始集合中相同位置的对象
            if (i < originalList.Count)
            {
               string itemPath = BuildItemPath(prefix, workingList[i], i);
               SubscribeRecursively(workingList[i]!, originalList[i]!, itemPath);
            }
         }

         //  监听集合本身的增删改动作
         if (workingObj is INotifyCollectionChanged occ)
         {
            NotifyCollectionChangedEventHandler colHandler = (s, e) =>
            {
               // 当集合结构变动时，记录一条概要变更
               _changes[prefix] = new ChangeEntry
               {
                  Path = prefix,
                  Description = "集合数量或顺序已变更",
                  OriginalValue = $"Count: {originalList.Count}",
                  CurrentValue = $"Count: {workingList.Count}",
               };

               // 注意：如果需要追踪新加入对象的属性，需在此处重新调用 SubscribeRecursively
            };
            occ.CollectionChanged += colHandler;
            _subscriptions.Add((occ, colHandler, true));
         }
         return;
      }

      // --- 情况 2：处理普通对象属性 ---
      if (workingObj is INotifyPropertyChanged npc)
      {
         foreach (var prop in workingObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
         {
            // 排除只读属性和索引器
            if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
               continue;

            string fullPath = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

            if (IsSimpleType(prop.PropertyType))
            {
               // 简单类型：挂载属性变更监听
               PropertyChangedEventHandler handler = (s, e) =>
               {
                  if (e.PropertyName != prop.Name)
                     return;

                  var newVal = prop.GetValue(s);
                  var oldVal = prop.GetValue(originalObj); // 与原始快照对比

                  if (Equals(newVal, oldVal))
                  {
                     _changes.Remove(fullPath);
                  }
                  else
                  {
                     _changes[fullPath] = new ChangeEntry
                     {
                        Path = fullPath,
                        Description = GetDisplayName(prop),
                        OriginalValue = oldVal ?? "null",
                        CurrentValue = newVal ?? "null",
                     };
                  }
               };
               npc.PropertyChanged += handler;
               _subscriptions.Add((npc, handler, false));
            }
            else // 复杂类型：继续向下递归
            {
               var wValue = prop.GetValue(workingObj);
               var oValue = prop.GetValue(originalObj);

               if (wValue != null && oValue != null)
                  SubscribeRecursively(wValue, oValue, fullPath);
            }
         }
      }
   }

   /// <summary>
   /// 构建集合项的路径。优先使用 Name 属性作为标识，否则使用索引。
   /// 示例：WebApiRoutes[GetOrder] 或 WebApiRoutes[0]
   /// </summary>
   private string BuildItemPath(string prefix, object item, int index)
   {
      var nameProp = item.GetType().GetProperty("Name");
      var nameVal = nameProp?.GetValue(item)?.ToString();
      string identifier = !string.IsNullOrEmpty(nameVal) ? nameVal : index.ToString();
      return $"{prefix}[{identifier}]";
   }

   /// <summary>
   /// 判断是否为简单可比较类型
   /// </summary>
   private static bool IsSimpleType(Type type)
   {
      Type actualType = Nullable.GetUnderlyingType(type) ?? type;
      return Type.GetTypeCode(actualType) != TypeCode.Object
         || actualType == typeof(Guid)
         || actualType == typeof(TimeSpan)
         || actualType.IsEnum;
   }

   /// <summary>
   /// 获取属性的显示名称
   /// </summary>
   private string GetDisplayName(PropertyInfo prop)
   {
      var attr = prop.GetCustomAttribute<LanguagesAttribute>();
      return attr?.Languages?.FirstOrDefault() ?? prop.Name;
   }

   /// <summary>
   /// 清除所有变更记录（通常在成功执行 Save 动作后调用）
   /// </summary>
   public void ClearChanges() => _changes.Clear();

   /// <summary>
   /// 彻底释放追踪器：解绑所有事件订阅。
   /// 必须调用，否则会导致 ViewModel 和 Model 无法被垃圾回收。
   /// </summary>
   public void Dispose()
   {
      foreach (var sub in _subscriptions)
      {
         if (sub.IsCollection)
            ((INotifyCollectionChanged)sub.Target).CollectionChanged -= (NotifyCollectionChangedEventHandler)sub.Handler;
         else
            ((INotifyPropertyChanged)sub.Target).PropertyChanged -= (PropertyChangedEventHandler)sub.Handler;
      }
      _subscriptions.Clear();
      _changes.Clear();
   }
}

/// <summary>
/// 变更分量记录项
/// </summary>
public class ChangeEntry
{
   public string Path { get; set; } = string.Empty; // 路径：WebApiRoutes[GetOrder].Route
   public string Description { get; set; } = string.Empty; // 描述
   public object OriginalValue { get; set; } = null!; // 旧值
   public object CurrentValue { get; set; } = null!; // 新值

   public override string ToString() => $"{Description}（{Path}）==> 修改前：{OriginalValue ?? ""} -> 修改后：{CurrentValue ?? ""}   ";
   // public override string ToString() => $"[{Path}] ({Description}): {OriginalValue} -> {CurrentValue}";
}
