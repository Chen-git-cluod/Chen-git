namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 注液表
/// </summary>
[SugarTable($"Batterty_{{year}}{{month}}{{day}}")]
[SugarIndex("Index_Barcode_Battery", nameof(Barcode), OrderByType.Asc)] //索引
[SplitTable(SplitType.Month)] //按月分表 （自带分表支持 年、季、月、周、日）
[AddINotifyPropertyChangedInterface]
[Languages(["完整电芯数据", "完整电芯数据", "Complete battery Data"])]
[BatteryDisplay]
public partial class BatMainModel
{
   private long _id;

   /// <summary>
   /// 电芯ID
   /// </summary>
   [Languages(["ID", "ID", "ID"])]
   [SugarColumn(IsPrimaryKey = true, ColumnDescription = "ID")] //设置主键
   [OrderMarker]
   public long Id
   {
      get { return _id; }
      set
      {
         if (value != _id)
         {
            _id = value;
            CreateTime = SnowflakeHelper.GetDateTimeFromId(value);
         }
      }
   }

   /// <summary>
   /// 分表字段,无需手动赋值
   /// </summary>
   [Languages(["创建时间", "創建時間", "Create time"])]
   [SugarColumn(ColumnDescription = "创建时间")]
   [SplitField] //分表字段
   [BatteryDisplay(IsIgnore = true)]
   public DateTime CreateTime { get; set; }

   /// <summary>
   /// 条码
   /// </summary>
   [Languages(["条码", "條碼", "Barcode"])]
   [SugarColumn(ColumnDescription = "条码", Length = 50)]
   public virtual string Barcode { get; set; } = string.Empty;

   /// <summary>
   /// 设备编号
   /// </summary>
   [SugarColumn(ColumnDescription = "设备编号", Length = 80)]
   [Languages(["设备编号", "設備編號", "Device code"])]
   public string DeviceCode { get; set; } = string.Empty;

   /// <summary>
   /// 生产工单
   /// </summary>
   [Languages(["工单", "工單", "WorkOrder Number"])]
   [SugarColumn(ColumnDescription = "工单", Length = 80)]
   public string WorkOrderNumber { get; set; } = string.Empty;

   /// <summary>
   /// 电解液批次
   /// </summary>
   [Languages(["电解液批次", "電解液批次", "Electrolyte batch"])]
   [SugarColumn(ColumnDescription = "电解液批次")]
   public string ElectrolyteBatch { get; set; } = string.Empty;

   /// <summary>
   /// 胶钉批次
   /// </summary>
   [Languages(["胶钉批次", "膠釘批次", "Glue nail batch"])]
   [SugarColumn(ColumnDescription = "胶钉批次")]
   public string GlueNailBatch { get; set; } = string.Empty;

   ///// <summary>
   ///// 进料框编号
   ///// </summary>
   //[Languages(["进料框编号", "进料框编号", "Entry crate number"])]
   //[SugarColumn(ColumnDescription = "进料框编号", Length = 50)]
   //public string InputCrateNumber { get; set; } = string.Empty;

   ///// <summary>
   ///// 进料框槽位（通道号）
   ///// </summary>
   //[Languages(["进料框槽位", "进料框槽位", "Entry crate slot"])]
   //[SugarColumn(ColumnDescription = "托盘ID")]
   //public int InputCrateSlot { get; set; }

   ///// <summary>
   ///// 出料框编号
   ///// </summary>
   //[Languages(["出料框编号", "出料框编号", "Exit crate number"])]
   //[SugarColumn(ColumnDescription = "托盘ID", Length = 50)]
   //public string OutputCrateNumber { get; set; } = string.Empty;

   ///// <summary>
   ///// 出料框槽位（通道号）
   ///// </summary>
   //[Languages(["出料框槽位", "出料框槽位", "Exit crate slot"])]
   //[SugarColumn(ColumnDescription = "出料框槽位")]
   //public int OutputCrateSlot { get; set; }

   /// <summary>
   /// MES进站状态
   /// </summary>
   [Languages(["MES进站", "MES進站", "MES entry"])]
   [SugarColumn(ColumnDescription = "MES进站")]
   // [BatteryResult(ProcessesTypeEnum.MES)]
   [DynamicClass(IsIgnoreForFinalStatus = true)]
   [ProcessRatio(nameof(CreateTime), DisplayName = "MES进站")]
   public ResultTypeEnum MesInputStatus { get; set; }

   /// <summary>
   /// MES出站结果
   /// </summary>
   [Languages(["MES出站结果", "MES出站结果", "MES exit result"])]
   [SugarColumn(ColumnDescription = "MES出站结果")]
   // [BatteryResult(ProcessesTypeEnum.MES)]
   [DynamicClass(IsIgnoreForFinalStatus = true)]
   [ProcessRatio(nameof(OutputTime), DisplayName = "MES出站")]
   public ResultTypeEnum MesOutputStatus { get; set; }

   /// <summary>
   /// 出站时间，以此字段来判断电池是否生产完成了,不一定要MES出站，只要排出产线就算出站
   /// </summary>
   [Languages(["出站时间", "出站时间", "Mes exit time"])]
   [SugarColumn(ColumnDescription = "出站时间")]
   // [BatteryDisplay(IsIgnore = true)]
   public DateTime OutputTime { get; set; }

   /// <summary>
   /// 生产时对电池判定,不允许手动更新, 会自动更新
   /// 某些工位结果不想参数判断的,可以用特性DynamicClassAttribute.IsIgnoreForFinalStatus=true忽略
   /// </summary>
   [Languages(["生产状态", "生产状态", "Production status"])]
   [SugarColumn(ColumnDescription = "生产状态")]
   public ResultTypeEnum FinalStatus { get; set; } = ResultTypeEnum.OK;

   /// <summary>
   /// NG工序,不允许手动更新
   /// </summary>
   [Languages(["NG工序", "NG工序", "NG processes"])]
   [SugarColumn(ColumnDescription = "NG工序")]
   public ProcessTypeEnum NgProcesses { get; set; } = ProcessTypeEnum._;

   /// <summary>
   /// 少液回流次数
   /// </summary>
   [Languages(["少液回流次数", "少液回流次数", "Inject Less reinject count"])]
   [SugarColumn(ColumnDescription = "少液NG次数")]
   public int LowElectorlyteNgCount { get; set; }

   /// <summary>
   /// 测漏回流次数
   /// </summary>
   [Languages(["测漏回流次数", "测漏回流次数", "Leak reinject count"])]
   [SugarColumn(ColumnDescription = "测漏NG次数")]
   public int LeakTestNgCount { get; set; }

   /// <summary>
   /// 复投次数
   /// </summary>
   [Languages(["复投次数", "复投次数", "rework count"])]
   [SugarColumn(ColumnDescription = "复投次数")]
   public byte ReproductionCount { get; set; }

   /// <summary>
   /// 设备扩展字段
   /// 用于存储不同设备或项目中的扩展业务数据。
   /// 部分字段仅在特定设备、工艺或客户项目中使用，
   /// 为避免主表字段过度冗余，可将低频、非通用的扩展数据以 Json 形式存储于此。
   ///
   /// 适用于：
   /// 1. 特定设备独有参数
   /// 2. 低频使用的业务字段
   /// 3. 后期新增但暂未标准化的扩展数据
   ///
   /// 当前项目暂未启用，预留用于后续项目兼容扩展。
   /// </summary>
   //[SugarColumn(ColumnDataType = "LONGTEXT")]
   //public string? ExtraJson { get; set; }

   /// <summary>
   /// 工序运行时状态数据
   /// 用于在电池流转过程中临时保存工艺状态、中间结果或上下文信息。
   ///
   /// 该数据主要用于工序间状态传递，
   /// 不属于长期业务数据，通常会在后续工序消费后清除或失效。
   ///
   /// 例如：
   /// 1. 回流原因（少液、测漏等）
   /// 2. 工序临时判定结果
   /// 3. 中间流程标记
   /// 4. 特殊流程上下文状态
   ///
   /// 数据以 Json 形式存储，序列化后为字典结构。。
   /// </summary>
   [SugarColumn(ColumnDataType = "LONGTEXT")]
   public string? RuntimeStateJson { get; set; }

   #region 处理方法
   /// <summary>
   /// 更新判定,在动态类中自动实现
   /// </summary>
   public void UpdateFinalResult(string propertyName, ResultTypeEnum propertyValue) { }
   #endregion
}
