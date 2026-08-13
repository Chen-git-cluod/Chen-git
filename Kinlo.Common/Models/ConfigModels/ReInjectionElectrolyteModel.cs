namespace Kinlo.Common.Models.ConfigModels;

/// <summary>
/// 手动补液设置项
/// </summary>
[AddINotifyPropertyChangedInterface]
public class ReInjectionElectrolyteModel
{
   /// <summary>
   /// 是否有自动补液称
   /// </summary>
   public bool IsHasReInjectionScale { get; set; }

   /// <summary>
   /// 是否要和PLC交互
   /// </summary>
   public bool IsHasPlc { get; set; }

   /// <summary>
   /// 是否有补液泵
   /// </summary>
   public bool IsHasPump { get; set; }

   /// <summary>
   /// 扫码时条码的长度
   /// </summary>
   public int BarcodeLength { get; set; } = 24;

   /// <summary>
   /// 数据清除时间（清除到上一个补液信息处）
   /// </summary>
   public int ClearTime { get; set; } = 5;
}
