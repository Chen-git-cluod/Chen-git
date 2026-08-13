namespace Kinlo.Common.Models.ConfigModels;

[AddINotifyPropertyChangedInterface]
[Languages]
public class DeviceParameterModel
{
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   [Languages(["软件标题", "Judul perangkat lunak", "Title"])]
   public string Title { get; set; } = "产线采控系统";

   /// <summary>
   /// 白班时间
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)DefaultRoleEnum.设备)]
   [Languages(["白班时间", "Waktu kerja siang hari", "Day shift"])]
   public TimeSpan DayShift { get; set; } = new TimeSpan(7, 30, 0);

   /// <summary>
   /// 夜班时间
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)DefaultRoleEnum.设备)]
   [Languages(["夜班时间", "Waktu Malam", "Night shift"])]
   public TimeSpan NightShift { get; set; } = new TimeSpan(19, 30, 0);

   /// <summary>
   /// 生产数据路径
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)DefaultRoleEnum.设备)]
   [Languages(["生产数据路径", "", "Production data path"])]
   public string ProductionDataPath { get; set; } = "D:\\生产数据";

   /// <summary>
   /// 设备编码
   /// </summary>
   [Languages(["设备编码", "Kode Perangkat", "Device Code"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public string DeviceCode { get; set; } = string.Empty;

   /// <summary>
   /// 设备名称
   /// </summary>
   [Languages(["设备名称", "", "Device name"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public string DeviceName { get; set; } = string.Empty;

   /// <summary>
   ///  产线编号
   /// </summary>
   [Languages(["产线编号", "Nomor jalur produksi", "line Name"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public int LineName { get; set; }

   /// <summary>
   ///  工序编号
   /// </summary>
   [Languages(["工序编号", "Nomor Proses", "Process operation name"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public string ProcessOperationName { get; set; } = string.Empty;

   /// <summary>
   /// 工序名称
   /// </summary>
   [Languages(["工序名称", "Process name", "Process name"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public string ProcessName { get; set; } = string.Empty;

   /// 工步编码
   /// </summary>
   [Languages(["工步编码", "Step code", "Step code"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public string StepCode { get; set; } = string.Empty;

   /// 工步名称
   /// </summary>
   [Languages(["工步名称", "Step name", "Step name"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public string StepNmae { get; set; } = string.Empty;

   /// <summary>
   /// 工作令 或工单编号
   /// </summary>
   [Languages(["默认工单号", "Nomor unit kerja", "Work order number"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public string WorkOrderNo { get; set; } = string.Empty;

   /// <summary>
   /// 电解液编码
   /// </summary>
   [Languages(["电解液编码", "Kode Elektrolit", "Electrolyte code"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦], IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public string ElectrolyteCode { get; set; } = string.Empty;

   /// <summary>
   /// 电解液批次码
   /// </summary>
   [Languages(["电解液批次号", "Nomor batch elektrolit", "Electrolyte lot code"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦], IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public string ElectrolyteLotCode { get; set; } = string.Empty;

   /// <summary>
   /// 电解液数量
   /// </summary>
   [Languages(["电解液数量", "Jumlah elektrolit", "Electrolyte quantity"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦], IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public float ElectrolyteQuantity { get; set; }

   /// <summary>
   /// 胶钉批次号
   /// </summary>
   [Languages(["胶钉批次码", "Batch kode kuku", "Glue nail code"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   public string GlueNailCode { get; set; } = string.Empty;
}
