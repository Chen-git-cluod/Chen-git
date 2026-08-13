namespace Kinlo.Common.Dto;

[AddINotifyPropertyChangedInterface]
public class Last24HourOutputValueDto
{
   /// <summary>
   ///  标记当前计数所属的
   /// </summary>
   public int CurrentCountingHour { get; set; }

   /// <summary>
   /// 全天24小时产量分布
   /// </summary>
   public ObservableRangeCollection<KinloControls.HourlyData> HourlyDatas { get; set; }

   /// <summary>
   ///  标记当前计数所属的分钟
   /// </summary>
   public DateTime CurrentCountingMinute { get; set; }

   /// <summary>
   /// 当前分钟产量累计
   /// </summary>
   public double CurrentMinuteCount { get; set; }

   /// <summary>
   /// 上一分钟产值
   /// </summary>
   public double LastMinuteCount { get; set; }

   public Last24HourOutputValueDto()
   {
      if (HourlyDatas == null)
      {
         DateTime now = DateTime.Now;

         HourlyDatas = new();
         for (int i = 0; i < 24; i++)
         {
            DateTime hourTime = new DateTime(now.Year, now.Month, now.Day, i, 0, 0, 0);
            HourlyDatas.Add(
               new HourlyData
               {
                  Time = hourTime,
                  ProductionCount = 0,
                  ValuePrefix = "出站 ",
                  InputCount = 0,
                  InputPrefix = "进站 ",
                  // ValueSuffix = " 颗",
                  IsCurrentHour = true,
                  Subtitle = GenericHelper.Build24HourDataSubTitle(hourTime),
               }
            );
         }
      }
   }
}
