namespace Kinlo.Common.Tools;

public static class ProductionStatisticsUpdaterHelper
{
   /// <summary>
   /// 更新注液统计数量
   /// </summary>
   /// <param name="ok"></param>
   /// <param name="low"></param>
   /// <param name="high"></param>
   /// <param name="oldResult"></param>
   /// <param name="newResult"></param>
   public static void UpdateInjectCount(ref int ok, ref int low, ref int high, ResultTypeEnum oldResult, ResultTypeEnum newResult)
   {
      if (oldResult == newResult)
         return;
      switch (newResult)
      {
         case ResultTypeEnum.OK:
            Interlocked.Increment(ref ok);
            if (oldResult == ResultTypeEnum.注液量偏少)
               Interlocked.Decrement(ref low);
            else if (oldResult == ResultTypeEnum.注液量偏多)
               Interlocked.Decrement(ref high);
            break;
         case ResultTypeEnum.注液量偏少:
            Interlocked.Increment(ref low);
            if (oldResult is ResultTypeEnum.OK)
               Interlocked.Decrement(ref ok);
            else if (oldResult == ResultTypeEnum.注液量偏多)
               Interlocked.Decrement(ref high);
            break;
         case ResultTypeEnum.注液量偏多:
            Interlocked.Increment(ref high);
            if (oldResult is ResultTypeEnum.OK)
               Interlocked.Decrement(ref ok);
            else if (oldResult == ResultTypeEnum.注液量偏少)
               Interlocked.Decrement(ref low);
            break;
         default:
            break;
      }
   }

   /// <summary>
   /// 更新统计数量（防重及支持回流）
   /// </summary>
   /// <param name="ok"></param>
   /// <param name="ng"></param>
   /// <param name="oldResult"></param>
   /// <param name="newResult"></param>
   public static void UpdateProcessCount(ref int ok, ref int ng, ResultTypeEnum oldResult, ResultTypeEnum newResult)
   {
      if (oldResult == newResult)
         return;
      ResultArea oldArea = oldResult.GetResultArea();
      ResultArea newArea = newResult.GetResultArea();

      if (oldArea == newArea) // 同一区域，不影响统计
         return;

      if (newArea == ResultArea.OK) // 新结果为 OK
      {
         Interlocked.Increment(ref ok);
         if (oldArea == ResultArea.NG)
            Interlocked.Decrement(ref ng);
      }
      else if (newArea == ResultArea.NG) // 新结果为 NG
      {
         Interlocked.Increment(ref ng);

         if (oldArea == ResultArea.OK)
            Interlocked.Decrement(ref ok);
      }
   }

   /// <summary>
   /// 更新统计数量（防重及支持回流）
   /// </summary>
   /// <param name="ok"></param>
   /// <param name="ng"></param>
   /// <param name="oldResult"></param>
   /// <param name="newResult"></param>
   public static void UpdateOutputCount(ref int ok, ref int ng, ResultTypeEnum oldResult, ResultTypeEnum newResult)
   {
      if (oldResult == newResult)
         return;

      ResultArea oldArea = oldResult.GetResultArea();
      ResultArea newArea = newResult.GetResultArea();

      if (oldArea == newArea) // 同一区域，不影响统计
         return;

      if (newArea == ResultArea.OK) // 新结果为 OK
      {
         Interlocked.Increment(ref ok);
         if (oldArea == ResultArea.NG)
            Interlocked.Decrement(ref ng);
      }
      else if (newArea == ResultArea.NG) // 新结果为 NG
      {
         Interlocked.Increment(ref ng);

         if (oldArea == ResultArea.OK)
            Interlocked.Decrement(ref ok);
      }
   }
}
