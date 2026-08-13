namespace Kinlo.Common.Models.OhtenModels;

/// <summary>
/// 物流托盘模型
/// </summary>
//public class LogisticsCrateModel
//{
//    public LogisticsCrateModel()
//    {

//    }
//    public LogisticsCrateModel(long id, string crateCode, List<LogisticsBatteryModel> batteries)
//    {
//        Id = id;
//        CrateCode = crateCode;
//        Batteries = batteries ?? new List<LogisticsBatteryModel>();
//    }
//    public long Id  { get; set; }
//    /// <summary>
//    /// 托盘条码
//    /// </summary>
//    public string CrateCode { get; set; } = string.Empty;
//    /// <summary>
//    /// 电芯列表
//    /// </summary>
//    public List<LogisticsBatteryModel> Batteries { get; set; } = new List<LogisticsBatteryModel>();
//}
/// <summary>
///
/// </summary>
public class LogisticsBatteryModel
{
   /// <summary>
   /// 电芯条码
   /// </summary>
   public string BatteryCode { get; set; } = string.Empty;

   /// <summary>
   /// 通道号（1-24）
   /// </summary>
   public string PositionNo { get; set; } = string.Empty;

   /// <summary>
   /// 电芯状态（0-无电芯；1-OK；2-NG；3-假电池）
   /// </summary>
   public string BatteryStatus { get; set; } = string.Empty;
}

public class LogisticsBatteryLocalModel
{
   /// <summary>
   /// 电芯条码
   /// </summary>
   public string BatteryCode { get; set; } = string.Empty;

   /// <summary>
   /// 通道号（1-24）
   /// </summary>
   public string PositionNo { get; set; } = string.Empty;

   /// <summary>
   /// 托盘码
   /// </summary>
   public string CrateCode { get; set; } = string.Empty;

   /// <summary>
   /// 托盘ID
   /// </summary>
   public long CrateId { get; set; }
}
