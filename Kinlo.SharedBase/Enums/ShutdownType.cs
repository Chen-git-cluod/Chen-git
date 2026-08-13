namespace Kinlo.SharedBase.Enums;

/// <summary>
/// 开机锁类型
/// </summary>
public enum ShutdownType
{
   加载缓存超上限,
}

public class ShutdownReason
{
   /// <summary>
   /// 分类
   /// </summary>
   public ShutdownType Category { get; set; }

   /// <summary>
   /// 联锁描述
   /// </summary>
   public string Message { get; set; } = string.Empty;

   /// <summary>
   /// 创建时间
   /// </summary>
   public DateTime CreateTime { get; set; }

   /// <summary>
   /// 解决方法
   /// </summary>
   public string Solution { get; set; } = string.Empty;
   /// <summary>
   /// 是否已恢复
   /// </summary>
   //public bool IsRecovered { get; set; }
}
