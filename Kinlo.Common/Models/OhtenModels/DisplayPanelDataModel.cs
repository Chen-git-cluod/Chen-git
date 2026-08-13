namespace Kinlo.Common.Models.OhtenModels;

public class DisplayPanelDataModel : IEntity
{
   [SugarColumn(IsPrimaryKey = true, ColumnDescription = "ID")] //设置主键
   public long Id { get; set; }

   [SugarColumn(ColumnDescription = "创建时间")]
   public DateTime CreateTime { get; set; }

   [SugarColumn(ColumnDescription = "班次", Length = 8)]
   public string Shift { get; set; } = string.Empty;

   [SugarColumn(ColumnDescription = "工序", Length = 8)]
   public string Process { get; set; } = string.Empty;

   [SugarColumn(ColumnDescription = "设备", Length = 96)]
   public string Device { get; set; } = string.Empty;

   [SugarColumn(ColumnDescription = "预留")]
   public string Reserve01 { get; set; } = string.Empty;

   [SugarColumn(ColumnDescription = "进站数量")]
   public int BatteryInCount { get; set; }

   [SugarColumn(ColumnDescription = "出站数量")]
   public int BatteryOutCount { get; set; }

   [SugarColumn(ColumnDescription = "注液合格")]
   public int InjectionOkCount { get; set; }

   [SugarColumn(ColumnDescription = "注液过多")]
   public int InjectionMuchCount { get; set; }

   [SugarColumn(ColumnDescription = "注液过少")]
   public int InjectionLessCount { get; set; }

   [SugarColumn(ColumnDescription = "前扫码OK")]
   public int FrontScanOkCount { get; set; }

   [SugarColumn(ColumnDescription = "前扫码NG")]
   public int FrontScanNgCount { get; set; }

   [SugarColumn(ColumnDescription = "Hipot OK")]
   public int ShortCircuitOkCount { get; set; }

   [SugarColumn(ColumnDescription = "Hipot NG")]
   public int ShortCircuitNgCount { get; set; }

   [SugarColumn(ColumnDescription = "前称重OK")]
   public int FrontWeightOkCount { get; set; }

   [SugarColumn(ColumnDescription = "前称重NG")]
   public int FrontWeightNgCount { get; set; }

   [SugarColumn(ColumnDescription = "测漏OK")]
   public int LeakOkCount { get; set; }

   [SugarColumn(ColumnDescription = "测漏NG")]
   public int LeakNgCount { get; set; }

   [SugarColumn(ColumnDescription = "后称重OK")]
   public int RearWeightOkCount { get; set; }

   [SugarColumn(ColumnDescription = "后称重NG")]
   public int RearWeightNgCount { get; set; }

   [SugarColumn(ColumnDescription = "测高OK")]
   public int HeightOkCount { get; set; }

   [SugarColumn(ColumnDescription = "测高NG")]
   public int HeightNgCount { get; set; }

   [SugarColumn(ColumnDescription = "回氦OK")]
   public int HeliumOkCount { get; set; }

   [SugarColumn(ColumnDescription = "回氦NG")]
   public int HeliumNgCount { get; set; }
   public int Hour0Count { get; set; }
   public int Hour1Count { get; set; }
   public int Hour2Count { get; set; }
   public int Hour3Count { get; set; }
   public int Hour4Count { get; set; }
   public int Hour5Count { get; set; }
   public int Hour6Count { get; set; }
   public int Hour7Count { get; set; }
   public int Hour8Count { get; set; }
   public int Hour9Count { get; set; }
   public int Hour10Count { get; set; }
   public int Hour11Count { get; set; }
   public int Hour12Count { get; set; }
   public int Hour13Count { get; set; }
   public int Hour14Count { get; set; }
   public int Hour15Count { get; set; }
   public int Hour16Count { get; set; }
   public int Hour17Count { get; set; }
   public int Hour18Count { get; set; }
   public int Hour19Count { get; set; }
   public int Hour20Count { get; set; }
   public int Hour21Count { get; set; }
   public int Hour22Count { get; set; }
   public int Hour23Count { get; set; }
}
