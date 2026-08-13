namespace Kinlo.Common.Models.ConfigModels;

[AddINotifyPropertyChangedInterface]
public class ControlInfoModel
{
   /// <summary>
   /// 排序
   /// </summary>
   [JsonIgnore]
   public int Index { get; set; }

   /// <summary>
   /// 控件图标
   /// </summary>
   [JsonIgnore]
   public string Icon { get; set; } = string.Empty;

   /// <summary>
   /// 控件显示名
   /// </summary>
   public string DisplayName { get; set; } = string.Empty;

   /// <summary>
   /// 绑定或Key
   /// </summary>
   public string BindingOrKey { get; set; } = string.Empty;

   /// <summary>
   /// 可显示的工序，用',' 号分隔
   /// </summary>
   [JsonIgnore]
   public string ProductVisibility { get; set; } = string.Empty;

   [JsonIgnore]
   public Type? Type { get; set; }

   /// <summary>
   /// 运行时是否可编辑
   /// </summary>
   [JsonIgnore]
   public bool IsRunEdit { get; set; } = true;

   /// <summary>
   /// 编辑权限
   /// </summary>
   public ulong EditLevel { get; set; }

   /// <summary>
   ///
   /// </summary>
   [JsonIgnore]
   public bool IsSelected { get; set; } = true;

   /// <summary>
   /// 指定Margin
   /// </summary>
   [JsonIgnore]
   public double[]? Margin { get; set; }

   public void SetSelect(RoleModel role)
   {
      if (role == null)
         return;
      var _displayLevel = (role.Level & EditLevel) > 0 || (role.Level & EditLevel) == role.Level;
      IsSelected = _displayLevel;
   }

   public void SetLevel(RoleModel role, bool b)
   {
      if (b)
      {
         EditLevel = EditLevel | role.Level;
      }
      else
      {
         EditLevel = EditLevel & ~role.Level;
      }
   }

   public int CompareTo(ControlInfoModel p)
   {
      int result = 0;
      if (Index == p.Index)
         result = 0;
      if (Index > p.Index)
         result = 1;
      if (Index < p.Index)
         result = -1;
      return result;
   }
}
