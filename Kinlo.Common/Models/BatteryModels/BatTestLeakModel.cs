namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 测漏
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.测漏], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.测漏), "", "Leak hunting"])]
[AddINotifyPropertyChangedInterface]
public partial class BatTestLeakModel
{
   /// <summary>
   /// 测漏时间
   /// </summary>
   [Languages(["测漏时间", "测漏时间", "Thick test time"])]
   [SugarColumn(ColumnDescription = "测漏时间")]
   [OrderMarker]
   public DateTime TestLeakTime { get; set; }

   /// <summary>
   /// 测漏前压力
   /// </summary>
   [SugarColumn(ColumnDescription = "测漏前压力")]
   [Languages(["测漏前压力(Kpa)", "", "Before testLeak vacuum(Kpa)"])]
   public float BeforeTestLeakVacuum { get; set; }

   /// <summary>
   /// 测漏后压力(测漏实际压力)
   /// </summary>
   [SugarColumn(ColumnDescription = "测漏后压力")]
   [Languages(["测漏后压力(Kpa)", "", "After testLeak vacuum(Kpa)"])]
   public float AfterTestLeakVacuum { get; set; }

   /// <summary>
   /// 测漏设定压力
   /// </summary>
   [SugarColumn(ColumnDescription = "测漏设定压力")]
   [Languages(["测漏设定压力(Kpa)", "", "Test leak set vacuum(Kpa)"])]
   public float TestLeakSetVacuum { get; set; }

   /// <summary>
   /// 测漏实际漏率
   /// </summary>
   [SugarColumn(ColumnDescription = "测漏实际漏率")]
   [Languages(["测漏实际漏率(Kpa)", "", "Test leak actual vacuum(Kpa)"])]
   public float TestLeakActualVacuum { get; set; }

   /// <summary>
   /// 测漏设定漏率
   /// </summary>
   [SugarColumn(ColumnDescription = "测漏设定漏率")]
   [Languages(["测漏设定漏率", "", "Test leak set rate"])]
   public float TestLeakSetRate { get; set; }

   /// <summary>
   /// 测漏设定时间
   /// </summary>
   [SugarColumn(ColumnDescription = "测漏设定时长")]
   [Languages(["测漏设定时长", "", "Test leak set time"])]
   public short TestLeakSetTime { get; set; }

   /// <summary>
   /// 测漏实际时间
   /// </summary>
   [SugarColumn(ColumnDescription = "测漏实际时长")]
   [Languages(["测漏实际时长", "", "Test leak actual time"])]
   public short TestLeakActualTime { get; set; }

   /// <summary>
   /// 首次验漏保压时间
   /// </summary>
   [SugarColumn(ColumnDescription = "首次验漏保压时间")]
   [Languages(["首次验漏保压时间", "", "Test leak vacuum hold time"])]
   public short TestLeakVacuumHoldTime { get; set; }

   /// <summary>
   /// 测漏结果
   /// </summary>
   [Languages(["测漏结果", "", "Test thick reslut"])]
   [SugarColumn(ColumnDescription = "测漏结果")]
   [DynamicClass(Process = ProcessTypeEnum.测漏)]
   [ProcessRatio(nameof(TestLeakTime))]
   public ResultTypeEnum LeakResult { get; set; }
}
