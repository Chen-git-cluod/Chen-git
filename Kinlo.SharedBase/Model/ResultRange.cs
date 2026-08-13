namespace Kinlo.SharedBase.Model;

/// <summary>
/// 结果区间
/// </summary>
/// <param name="Min"></param>
/// <param name="Max"></param>
public readonly record struct ResultRange(int Min, int Max)
{
   /// <summary>
   /// 是否单值
   /// </summary>
   public bool IsSingleValue => Min == Max;

   public bool Contains(int value) => value >= Min && value <= Max;

   public bool Contains(ResultRange other) => other.Min >= Min && other.Max <= Max;

   /// <summary>
   /// 是否相交
   /// </summary>
   public bool Intersects(ResultRange other) => Min <= other.Max && Max >= other.Min;

   /// <summary>
   /// 是否相邻
   /// </summary>
   public bool IsAdjacent(ResultRange other) => Max + 1 == other.Min || other.Max + 1 == Min;

   /// <summary>
   /// 是否可以合并
   /// </summary>
   public bool CanMerge(ResultRange other) => Intersects(other) || IsAdjacent(other);

   /// <summary>
   /// 合并区间
   /// </summary>
   public ResultRange Merge(ResultRange other) => new(Math.Min(Min, other.Min), Math.Max(Max, other.Max));
}
