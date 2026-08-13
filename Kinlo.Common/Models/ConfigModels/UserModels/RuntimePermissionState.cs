using Stylet;

namespace Kinlo.Common.Models.ConfigModels.UserModels;

/// <summary>
/// 除本地权限外的第三方（如mes）的权限管控
/// </summary>
public class RuntimeInterlockState : PropertyChangedBase
{
   private int _version;

   /// <summary>
   /// 刷新版本号
   /// 用于通知UI重新执行Binding/Converter
   /// </summary>
   public int Version
   {
      get => _version;
      set => SetAndNotify(ref _version, value);
   }

   /// <summary>
   /// 被锁定的属性，ignore 大小写
   /// key：属性名
   /// </summary>
   public HashSet<string> LockedProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

   /// <summary>
   /// 锁定属性
   /// </summary>
   public void Lock(params string[] propertyNames)
   {
      bool isChanged = false;

      foreach (var propertyName in propertyNames)
      {
         if (string.IsNullOrWhiteSpace(propertyName))
            continue;

         if (LockedProperties.Add(propertyName))
            isChanged = true;
      }

      if (isChanged)
         Version++;
   }

   /// <summary>
   /// 解锁属性
   /// </summary>
   public void Unlock(params string[] propertyNames)
   {
      bool isChanged = false;

      foreach (var propertyName in propertyNames)
      {
         if (string.IsNullOrWhiteSpace(propertyName))
            continue;

         if (LockedProperties.Remove(propertyName))
            isChanged = true;
      }

      if (isChanged)
         Version++;
   }

   /// <summary>
   /// 清空所有锁定
   /// </summary>
   public void Clear()
   {
      if (LockedProperties.Count == 0)
         return;

      LockedProperties.Clear();
      Version++;
   }

   /// <summary>
   /// 替换全部锁定项
   /// </summary>
   public void Replace(IEnumerable<string> propertyNames)
   {
      LockedProperties.Clear();

      foreach (var item in propertyNames)
      {
         if (!string.IsNullOrWhiteSpace(item))
         {
            LockedProperties.Add(item);
         }
      }

      Version++;
   }

   /// <summary>
   /// 是否锁定
   /// </summary>
   public bool IsLocked(string propertyName)
   {
      if (string.IsNullOrWhiteSpace(propertyName))
         return false;

      return LockedProperties.Contains(propertyName);
   }
}
