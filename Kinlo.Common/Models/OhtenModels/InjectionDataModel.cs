namespace Kinlo.Common.Models.OhtenModels;

/// <summary>
/// 注液CPK表
/// </summary>
[SugarTable($"Injection_{{year}}{{month}}{{day}}")]
[SplitTable(SplitType.Month)] //按月分表 （自带分表支持 年、季、月、周、日）
[SugarIndex("index_injectiontime", nameof(InjectionDataModel.InjectionTime), OrderByType.Asc)]
[AddINotifyPropertyChangedInterface]
[Languages(["注液CPK", "CPK injeksi cair", "Injection cpk"])]
public class InjectionDataModel
{
   private long _id;

   /// <summary>
   /// ID
   /// </summary>
   [Languages(["ID", "ID", "ID"])]
   [SugarColumn(IsPrimaryKey = true, ColumnDescription = "ID")] //设置主键
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
   /// 分表字段 勿手动赋值
   /// </summary>
   [Languages(["创建时间", "Waktu pembuatan", "Create time"])]
   [SugarColumn(ColumnDescription = "创建时间")]
   [SplitField] //分表字段
   public DateTime CreateTime { get; set; }

   /// <summary>
   /// 条码
   /// </summary>
   [Languages(["条码", "Kode batang", "Barcode"])]
   [SugarColumn(ColumnDescription = "条码")]
   public string Barcode { get; set; } = string.Empty;

   [Languages(["注液时间", "Waktu injeksi", "Inection time"])]
   [SugarColumn(ColumnDescription = "注液时间")]
   public DateTime InjectionTime { get; set; }

   /// <summary>
   /// 注液泵号
   /// </summary>
   [Languages(["注液泵号", "Pump injeksi", "Pump number"])]
   [SugarColumn(ColumnDescription = "注液泵号", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public byte StationNo { get; set; }

   /// <summary>
   /// 注液针号
   /// </summary>
   [Languages(["注液针号", "Nomor jarum", "Needle number"])]
   [SugarColumn(ColumnDescription = "注液针号", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public byte NeedleNo { get; set; }

   /// <summary>
   /// 目标注液量
   /// </summary>
   [Languages(["目标注液量", "Volume injeksi", "Injection value"])]
   [SugarColumn(ColumnDescription = "目标注液量")]
   public double TargetInjectionVolume { get; set; }

   /// <summary>
   /// 实际注液量
   /// </summary>
   [Languages(["注液量", "Volume injeksi", "Injection value"])]
   [SugarColumn(ColumnDescription = "注液量")]
   public double InjectionValue { get; set; }

   /// <summary>
   /// 当前温度
   /// </summary>
   [Languages(["温度", "Temp", "temperature"])]
   [SugarColumn(ColumnDescription = "温度")]
   public double Temperature { get; set; }

   /// <summary>
   /// 温度补偿量
   /// </summary>
   [Languages(["温度补偿量", "temperature Compensation", "temperature Compensation"])]
   [SugarColumn(ColumnDescription = "温度补偿量")]
   public double TempComp { get; set; }

   /// <summary>
   /// 工艺补偿量
   /// </summary>
   [Languages(["工艺补偿量", "process Compensation", "process Compensation"])]
   [SugarColumn(ColumnDescription = "工艺补偿量")]
   public double ProcessComp { get; set; }

   /// <summary>
   /// 补偿模式
   /// </summary>
   [Languages(["补偿模式", "Compensation mode", "Compensation mode"])]
   [SugarColumn(ColumnDescription = "补偿模式")]
   public int CompMode { get; set; }
}
