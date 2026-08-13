namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 注液
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.注液], DeviceCommunicationType = [CommunicationEnum.None])]
[Languages([nameof(ProcessTypeEnum.注液), "注液站信息", "Inject station"])]
[AddINotifyPropertyChangedInterface]
public partial class BatInjectStationModel
{
   /// <summary>
   /// 打液时间
   /// </summary>
   [Languages(["打液时间", "打液时间", "Thick test time"])]
   [SugarColumn(ColumnDescription = "打液时间")]
   [OrderMarker]
   public DateTime InjectElectrolyteTime { get; set; }

   /// <summary>
   /// 注液泵号
   /// </summary>
   [Languages(["注液泵号", "", "Inject pump No"])]
   [SugarColumn(ColumnDescription = "注液泵号", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   //[Statistics(ProcessTypeEnum.注液, StatisticsFuncEnum.通道)]
   public byte InjectPumpNo { get; set; }

   /// <summary>
   /// 注液站号
   /// </summary>
   [Languages(["注液站号", "", "Inject station No"])]
   [SugarColumn(ColumnDescription = "注液站号", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   //[Statistics(ProcessTypeEnum.注液, StatisticsFuncEnum.通道)]
   public byte InjectStationNo { get; set; }

   /// <summary>
   /// 注液时长
   /// </summary>
   [Languages(["注液时长", "", "Inject Process duration"])]
   [SugarColumn(ColumnDescription = "注液时长")]
   public short InjectedProcessDuration { get; set; }

   /// <summary>
   /// 打液时长
   /// </summary>
   [Languages(["打液时长", "", "Inject duration"])]
   [SugarColumn(ColumnDescription = "打液时长")]
   public short InjectedDuration { get; set; }

   /// <summary>
   /// 注液嘴号
   /// </summary>
   [Languages(["注液嘴号", "註液嘴號", "Inject Nozzle No"])]
   [SugarColumn(ColumnDescription = "注液嘴号", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   //[Statistics(ProcessTypeEnum.注液, StatisticsFuncEnum.通道)]
   public byte InjectNozzleNo { get; set; }

   /// <summary>
   /// 行号
   /// </summary>
   [Languages(["行号", "行号", "Line index"])]
   [SugarColumn(ColumnDescription = "行号", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public byte LineIndex { get; set; }

   /// <summary>
   /// 列号
   /// </summary>
   [Languages(["列号", "列号", "Column index"])]
   [SugarColumn(ColumnDescription = "列号", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public byte ColumnIndex { get; set; }

   /// <summary>
   /// 套杯号
   /// </summary>
   [SugarColumn(ColumnDescription = "套杯号")]
   [Languages(["套杯号", "套杯碼", "Cup code"])]
   public string CupCode { get; set; } = string.Empty;

   /// <summary>
   /// 托盘号
   /// </summary>
   [SugarColumn(ColumnDescription = "托盘号")]
   [Languages(["托盘码", "托盤碼", "Tray Code"])]
   public string TrayCode { get; set; } = string.Empty;

   /// <summary>
   /// 注液量偏移
   /// </summary>
   [Languages("注液量偏移")]
   [SugarColumn(ColumnDescription = "注液量偏移")]
   public float InjectingOffset { get; set; } = -1;

   /// <summary>
   /// 注液泵温度
   /// </summary>
   [Languages(["注液泵温度", "注液泵温度", "Injected Volume"])]
   [SugarColumn(ColumnDescription = "注液泵温度")]
   public float InjectTemperature { get; set; } = -1;

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
