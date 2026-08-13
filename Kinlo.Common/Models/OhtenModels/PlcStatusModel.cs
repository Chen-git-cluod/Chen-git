namespace Kinlo.Common.Models.OhtenModels;

[SugarTable($"PlcStatus_{{year}}{{month}}{{day}}")]
[SplitTable(SplitType.Year)] //按年分表 （自带分表支持 年、季、月、周、日）
[AddINotifyPropertyChangedInterface]
[Languages(["PLC状态列表", "", "PLC status table"])]
public class PlcStatusModel
{
   private long _id;

   /// <summary>
   /// 电芯ID
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

   [SugarColumn(ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public DeviceStateEnum Status { get; set; }

   /// <summary>
   /// 班次
   /// </summary>
   [SugarColumn(ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public ShiftType? Shift { get; set; }

   /// <summary>
   /// 一般用来写停机原因
   /// </summary>
   [SugarColumn(ColumnDataType = "text")]
   public string Msg { get; set; } = string.Empty;

   [SplitField] //分表字段 在插入的时候会根据这个字段插入哪个表，在更新删除的时候用这个字段找出相关表
   public DateTime CreateTime { get; set; }
   public DateTime StartTime { get; set; }

   public DateTime? EndTime { get; set; }
}
