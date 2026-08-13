namespace Kinlo.Common.Models.OhtenModels;

/// <summary>
/// PLC报警表列表
/// </summary>
[SugarTable($"PlcAlarm_{{year}}{{month}}{{day}}")]
[SplitTable(SplitType.Month)] //按月分表 （自带分表支持 年、季、月、周、日）
[AddINotifyPropertyChangedInterface]
[Languages(["PLC报警表列表", "", "PLC alarm table"])]
public class PlcAlarmModel
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
            StartTime = SnowflakeHelper.GetDateTimeFromId(value);
         }
      }
   }

   /// <summary>
   /// 工序类型
   /// </summary>
   public string ProcessType { get; set; } = string.Empty;

   /// <summary>
   /// 报警等级
   /// </summary>
   public PlcAalrmLevelEnum PlcAalrmLevel { get; set; }

   /// <summary>
   /// 读取PLC报警时的Tag
   /// </summary>
   public string PlcTag { get; set; } = string.Empty;

   /// <summary>
   /// MES代码
   /// </summary>
   public string MesCode { get; set; } = string.Empty;

   /// <summary>
   /// PLC报警信息
   /// </summary>
   public string AlarmMessage { get; set; } = string.Empty;

   /// <summary>
   /// 本地报警代码，主要用于查询
   /// </summary>
   public int AlarmCode { get; set; }

   /// <summary>
   ///上传MES状态
   /// </summary>
   [SugarColumn(ColumnDescription = "上传MES状态", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   public SendMesStatusEnum SendMesStatus { get; set; }

   [SplitField] //分表字段 在插入的时候会根据这个字段插入哪个表，在更新删除的时候用这个字段找出相关表
   public DateTime StartTime { get; set; }

   private DateTime? _endTime;

   public DateTime? EndTime
   {
      get { return _endTime; }
      set
      {
         if (value != _endTime)
         {
            _endTime = value;
            if (value is DateTime t)
            {
               var span = t - StartTime;
               AlarmDurationSeconds = (int)span.TotalSeconds;
            }
         }
      }
   }

   /// <summary>
   /// 报警时长（秒）
   /// </summary>
   public int AlarmDurationSeconds { get; set; }
}

[Flags]
public enum SendMesStatusEnum : byte
{
   未上传,
   只上传了报警开始,
   开始及完成都有上传,
}
