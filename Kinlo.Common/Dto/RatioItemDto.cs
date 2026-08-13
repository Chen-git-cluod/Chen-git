namespace Kinlo.Common.Dto;

public class RatioItemDto
{
   /// <summary>
   /// 统计项目名称
   /// </summary>
   [JsonInclude]
   public string Process { get; set; } = string.Empty;

   [JsonInclude]
   public int OK { get; private set; }

   [JsonInclude]
   public int NG { get; private set; }

   [JsonInclude]
   public int Count { get; private set; }

   /// <summary>
   /// 合格率
   /// </summary>
   [JsonInclude]
   public double OkRatio { get; private set; }

   /// <summary>
   /// NG占比
   /// </summary>
   [JsonInclude]
   public double NgRatio { get; private set; }

   ///// <summary>
   ///// 当前工序NG比率
   ///// </summary>
   //public double CurrentProcessNgRatio { get; private set; }

   public Visibility Visibility { get; set; } = Visibility.Visible;

   public async Task UpdateAsync(int count, int ok, int ng)
   {
      await UIThreadHelper.InvokeOnUiThreadAsync(() =>
      {
         OK = ok;
         NG = ng;
         Count = ok + ng;
         SetPercentage();
      });
   }

   public async Task UpdateAsync(int ok, int ng)
   {
      await UIThreadHelper.InvokeOnUiThreadAsync(() =>
      {
         OK = ok;
         NG = ng;
         Count = ok + ng;
         SetPercentage();
      });
   }

   public void SetPercentage()
   {
      if (Count == 0)
      {
         OkRatio = 100;
         NgRatio = 0;
         return;
      }

      OkRatio = Math.Round(OK * 100.0 / Count, 2);
      NgRatio = Math.Round(NG * 100.0 / Count, 2);
   }

   public async Task Reset()
   {
      await UIThreadHelper.InvokeOnUiThreadAsync(() =>
      {
         OK = NG = Count = 0;
         OkRatio = 100;
         NgRatio = 0;
      });
   }
}
