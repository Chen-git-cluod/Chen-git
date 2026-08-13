using System;
using System.Xml.Linq;

namespace Kinlo.Common.Models.OhtenModels;

/// <summary>
/// 同步PLC生产数据模型
/// </summary>
public class PlcProductionSyncModel
{
   /// <summary>
   /// 1白班，2夜班
   /// </summary>
   public short Shift { get; set; }
   public short MyProperty { get; set; }

   public int Input { get; set; }
   public int Output { get; set; }
   public int[] HourCount { get; set; } = new int[24];
   public PlcProcessDataModel[] PlcProcessData { get; set; } = new PlcProcessDataModel[10];

   /// <summary>
   /// 待机时间
   /// </summary>
   public TimeModel StandByTime { get; set; } = new();

   /// <summary>
   /// 待料时间
   /// </summary>
   public TimeModel WaitingTime { get; set; } = new();

   /// <summary>
   /// 堵料时间
   /// </summary>
   public TimeModel MateridlBlockingTime { get; set; } = new();

   /// <summary>
   /// 报警时间
   /// </summary>
   public TimeModel AlarmTime { get; set; } = new();

   /// <summary>
   /// 运行时间
   /// </summary>
   public TimeModel RunningTime { get; set; } = new();
   public short MyProperty12 { get; set; }

   public PlcProductionSyncModel()
   {
      HourCount = Enumerable.Repeat(1, 24).ToArray();
      PlcProcessData = new PlcProcessDataModel[10];
      for (int i = 0; i < PlcProcessData.Length; i++)
      {
         PlcProcessData[i] = new PlcProcessDataModel();
      }
   }
}

public class PlcProcessDataModel
{
   public int OkCount { get; set; }
   public int Ng1Count { get; set; }
   public int Ng2Count { get; set; }
   public float PassRate { get; set; }
   public float NgProportion { get; set; }
}

public class TimeModel
{
   public short Hour { get; set; }
   public short Min { get; set; }
   public short Second { get; set; }
}
