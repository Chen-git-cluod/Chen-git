namespace Kinlo.Common.Models.OhtenModels;

/// <summary>
/// 一般存数据库实体接口
/// </summary>
public interface IRownNumber
{
   int RowNumber { get; set; }
   long Id { get; set; }
   DateTime CreateTime { get; set; }
}

/// <summary>
/// 一般存数据库实体接口
/// </summary>
public interface IEntity
{
   long Id { get; set; }
   DateTime CreateTime { get; set; }
}
