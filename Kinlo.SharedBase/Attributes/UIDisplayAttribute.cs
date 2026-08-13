namespace Kinlo.SharedBase.Attributes;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public class UIDisplayAttribute : Attribute
{
   /// <summary>
   /// 是否单例
   /// </summary>
   public bool IsSingleton { get; set; }

   /// <summary>
   /// 排序
   /// </summary>
   public int Index { get; set; }

   /// <summary>
   /// 视图图标
   /// </summary>
   public string Icon { get; set; } = string.Empty;

   /// <summary>
   /// 运行时是否可以编辑
   /// </summary>
   public bool IsRunEdit { get; set; }

   /// <summary>
   /// 可编辑权限
   /// </summary>
   public ulong EditLevel { get; set; }

   /// <summary>
   /// 当外部导入dll时，在此标注功能
   /// </summary>
   public string FuncName { get; set; } = string.Empty;

   /// <summary>
   /// 针对生产机台隐藏
   /// </summary>
   public ProductionTypeEnum[] Hiddens { get; set; } = [];

   ///// <summary>
   ///// 哪些机台可以
   ///// </summary>
   //public ProductionTypeEnum[] VisibleMachines { get; set; } = [];
   /// <summary>
   /// 指定Margin
   /// </summary>
   public double[]? Margin { get; set; }

   ///// <summary>
   ///// 提示,暂未使用
   ///// </summary>
   //public LanguagesAttribute? Tip { get; set; }
   public UIDisplayAttribute() { }

   public UIDisplayAttribute(bool isSingleton)
      : this()
   {
      IsSingleton = isSingleton;
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="isSingleton">是否单例</param>
   /// <param name="displayName">名称</param>
   /// <param name="index">排序</param>
   public UIDisplayAttribute(bool isSingleton, int index)
      : this(isSingleton)
   {
      Index = index;
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="isSingleton">是否单例</param>
   /// <param name="index">排序</param>
   /// <param name="editLevel">编辑所需权限</param>
   /// <param name="isRunEdit">运行进是否可编辑</param>
   /// <param name="menuIcon">图标</param>
   public UIDisplayAttribute(bool isSingleton, int index, ulong editLevel, bool isRunEdit, string menuIcon)
      : this(isSingleton, index)
   {
      EditLevel = editLevel;
      IsRunEdit = isRunEdit;
      Icon = menuIcon;
   }
}

/// <summary>
/// 在方法上权限验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class MethodPermissionAttribute : Attribute
{
   /// <summary>
   /// 运行时是否可以编辑
   /// </summary>
   public bool IsRunEdit { get; set; }

   /// <summary>
   /// 可编辑权限
   /// </summary>
   public ulong EditLevel { get; set; }

   public MethodPermissionAttribute(bool isRunEdit, ulong editLevel)
   {
      IsRunEdit = isRunEdit;
      EditLevel = editLevel;
   }
}
