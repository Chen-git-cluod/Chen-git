namespace Kinlo.Common.Models.OhtenModels;

/// <summary>
/// 气体浓度
/// </summary>
[SugarTable($"GasConcentration_{{year}}{{month}}{{day}}")]
[SplitTable(SplitType.Month)] //按月分表 （自带分表支持 年、季、月、周、日）
[SugarIndex("index_name", nameof(GasConcentrationModel.Position), OrderByType.Asc)]
[AddINotifyPropertyChangedInterface]
[Languages(["气体浓度", "Konsentrasi gas", "gas concentration"])]
public class GasConcentrationModel : IRownNumber
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
   /// 名称
   /// </summary>
   [Languages(["名称", "Nama", "Name"])]
   [SugarColumn(ColumnDescription = "名称")]
   public ConcentrationPositionEnum Position { get; set; }

   /// <summary>
   /// 浓度
   /// </summary>
   [Languages(["浓度", "Konsentrasi", "Concentration"])]
   [SugarColumn(ColumnDescription = "浓度")]
   public double Concentration { get; set; }

   /// <summary>
   /// 行号
   /// </summary>
   [Languages(["行号", "Nomor baris", "row number"])]
   [SugarColumn(IsIgnore = true)]
   public int RowNumber { get; set; }
}
