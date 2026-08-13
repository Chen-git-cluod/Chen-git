namespace Kinlo.Common.Models.OhtenModels;

[AddINotifyPropertyChangedInterface]
[SugarIndex("index_barcode", nameof(MesResendModel.Barcode), OrderByType.Asc)]
public class MesResendModel : IEntity
{
   [SugarColumn(IsPrimaryKey = true, ColumnDescription = "ID")] //设置主键
   public long Id { get; set; }

   [SugarColumn(ColumnDescription = "条码")]
   public string Barcode { get; set; } = string.Empty;

   [SugarColumn(ColumnDescription = "创建时间")]
   public DateTime CreateTime { get; set; }

   /// <summary>
   /// 最多上传10次
   /// </summary>
   [SugarColumn(ColumnDescription = "上传次数")]
   public byte ResendCount { get; set; }

   [SugarColumn(ColumnDescription = "最后更新时间")]
   public DateTime LastUpdateTime { get; set; }

   [SugarColumn(ColumnDescription = "状态")]
   public ResendStatusEnum ResendStatus { get; set; }

   [SugarColumn(ColumnDescription = "最后一次结果")]
   public ResultTypeEnum LastResult { get; set; }
}

public enum ResendStatusEnum
{
   未上传,
   上传成功,
   上传失败,
}
