namespace Kinlo.Common.Tools;

public static class OEEHelper
{
   /// <summary>
   /// 计算可用性[运行时间 / 计划时间]
   /// </summary>
   /// <param name="plannedProductionTime">计划运行时间</param>
   /// <param name="downtime">意外停机时间</param>
   /// <returns></returns>
   public static double CalculateAvailability(this double plannedProductionTime, double downtime) =>
      plannedProductionTime <= 0 ? 0 : (plannedProductionTime - downtime) / plannedProductionTime;

   /// <summary>
   /// 计算性能 [实际产量 / (运行时间 * 理论节拍)]
   /// </summary>
   /// <param name="TPM">每分钟节拍</param>
   /// <param name="totalPieces">实际生产总数（包括NG）</param>
   /// <param name="plannedProductionTime">计划运行时间</param>
   /// <param name="downtime">意外停机时间</param>
   /// <returns></returns>
   public static double CalculatePerformance(this double TPM, int totalPieces, double plannedProductionTime, double downtime)
   {
      var runTime = Math.Max(0, plannedProductionTime - downtime); //实际运行时间
      var TheoreticalCount = runTime * TPM;
      if (TheoreticalCount <= 0)
         return 0;
      return totalPieces * 1.0 / TheoreticalCount;
   }

   /// <summary>
   /// 计算质量 [OK / 总数]
   /// </summary>
   /// <param name="okPieces">合格数</param>
   /// <param name="totalPieces">生产总数</param>
   /// <returns></returns>
   public static double CalculateQuality(this int okPieces, int totalPieces) => totalPieces <= 0 ? 0 : okPieces * 1.0 / totalPieces;

   /// <summary>
   /// 计算  oee
   /// </summary>
   /// <param name="availability">可用性</param>
   /// <param name="performance">性能</param>
   /// <param name="quality">质量</param>
   /// <returns></returns>
   public static double CalculateOEE(this double availability, double performance, double quality)
   {
      var oee = availability * performance * quality;
      return Math.Clamp(oee, 0, 1); // OEE值应该在0到1之间
   }

   /// <summary>
   /// 计算重叠时间区间的总时长
   /// </summary>
   /// <param name="timeRanges"></param>
   /// <returns></returns>
   public static TimeSpan CalculateTotalDuration(this List<(DateTime Start, DateTime End)> timeRanges)
   {
      if (timeRanges == null || timeRanges.Count == 0)
         return TimeSpan.Zero;

      // 1. 按照开始时间进行排序
      var sortedIntervals = timeRanges.OrderBy(i => i.Start).ToList();

      // 2. 合并重叠区间
      var merged = new List<(DateTime Start, DateTime End)>();
      var current = sortedIntervals[0];

      for (int i = 1; i < sortedIntervals.Count; i++)
      {
         var next = sortedIntervals[i];

         if (next.Start <= current.End)
         {
            // 有重叠，更新当前区间的结束时间（取两者中的最大值）
            if (next.End > current.End)
            {
               current.End = next.End;
            }
         }
         else
         {
            // 无重叠，保存当前区间，开始处理下一个
            merged.Add(current);
            current = next;
         }
      }
      // 添加最后一个区间
      merged.Add(current);

      // 3. 计算总时长
      TimeSpan totalDuration = TimeSpan.Zero;
      foreach (var interval in merged)
      {
         totalDuration += (interval.End - interval.Start);
      }

      return totalDuration;
   }
}
