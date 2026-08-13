namespace Kinlo.Common.Models.ConfigModels;

[Languages]
[AddINotifyPropertyChangedInterface]
public class AdvancedConfigModel
{
   /// <summary>
   /// 生产类型
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)DefaultRoleEnum.设备)]
   [Languages(["生产类型", "Jenis Produksi", "Production Type"])]
   public ProductionTypeEnum ProductionType { get; set; } = ProductionTypeEnum.一次注液;

   /// <summary>
   /// 电芯条码验证规则
   /// </summary>
   [Languages(["电芯条码验证规则", "Aturan verifikasi barcode baterai", "Cell Barcode Rules"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   public string BatteryBarcodeValidationRule { get; set; } = "^[a-zA-Z0-9]{10,24}$";

   /// <summary>
   /// 注液托盘码验证规则
   /// </summary>
   [Languages(["注液托盘码验证规则", "", "Injection Tray Code Rule"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   public string InjectionTrayCodeValidationRule { get; set; } = "^[a-zA-Z0-9]{10,24}$";

   /// <summary>
   /// 物流托盘码验证规则
   /// </summary>
   [Languages(["物流托盘码验证规则", "", "Logistics Tray Code Rule"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   public string LogisticsTrayCodeValidationRule { get; set; } = "^[a-zA-Z0-9]{10,24}$";

   /// <summary>
   /// 自动退出超级管理员时间(秒)
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["自动返回主界面(秒)"])]
   public int AutoExitSuperAdminTime { get; set; } = 180;

   ///// <summary>
   ///// 取托盘号数字正则式，【 @"(?<=^.{5}).{1,3}"】，(?<=^.{n})中的N代表跳过几位；.{n,m-n}这部分就是匹配任意字符最少匹配 n 次且最多匹配 m 次，也就是如果数字可能是1-2位的放就是.{1,2}。
   ///// </summary>
   //[Languages(["取托盘数字正则式", "", ""])]
   //[UIDisplay(IsRunEdit = true, EditLevel = (uint)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺))]
   //public string TrayExtractValidationRule { get; set; } = @"(?<=^.{5}).{1,2}";

   /// <summary>
   /// 本地数据库连接1
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["本地数据库连接1", "Koneksi database lokal 2", "Local connection 1"])]
   public string LocalConnectionString { get; set; } =
      "server=127.0.0.1;port=3306;user=root;password=admin999; database=weightdb;CharSet=utf8mb4;Allow User Variables=True;";

   /// <summary>
   /// 本地数据库连接2
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["本地数据库连接2", "Koneksi database lokal 2", "Local connection 2"])]
   public string LocalConnectionString2 { get; set; } = string.Empty;

   /// <summary>
   /// 远程数据库1
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["远程数据库连接1", "Koneksi database lainnya", "Remote connection 1"])]
   public string OtherConnectionString { get; set; } =
      "server=127.0.0.1;port=3306;user=root;password=admin999; database=weightdb2;CharSet=utf8mb4;Allow User Variables=True;";

   /// <summary>
   /// 远程数据库2
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["远程数据库连接2", "Koneksi database lainnya", "Remote connection 2"])]
   public string OtherConnectionString2 { get; set; } = string.Empty;

   /// <summary>
   /// 执行SQL超时时间(秒)
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["执行SQL超时时间(秒)", "sql execution timeout(s)", "sql execution timeout(s)"])]
   public int SqlExecutionTimeout { get; set; } = 30;

   /// <summary>
   /// SQL打印日志阈值(秒) [当执行sql超过此阈值时打印sql至日志，如果设为0就打印所有日志]
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages([
      "SQL打印日志阈值(秒) [当执行sql超过此阈值时打印sql至日志，如果设为0就打印所有日志]",
      "",
      "SQL print log threshold (s) [When executing SQL exceeds this threshold, print the SQL to the log. If set to 0, print all logs]",
   ])]
   public float SqlLogThreshold { get; set; } = 0.6f;

   /// <summary>
   /// 从设备获取电池干重 (如一注或本机)
   /// </summary>
   [UIDisplay(
      Hiddens = [ProductionTypeEnum.一次注液, ProductionTypeEnum.清洗机],
      IsRunEdit = true,
      EditLevel = (ulong)DefaultRoleEnum.工艺
   )]
   [Languages(["从其它设备取前工序数据  [优先级:1]", "Ambil data dari perangkat proses sebelumnya", "Retrieve data from other devices"])]
   public bool IsEnableNetWeightFromDevice { get; set; } = false;

   /// <summary>
   /// 启用MES取前工序数据 [优先级:1，需启用MES]
   /// </summary>
   [UIDisplay(Margin = [5, 10, 5, 5], Hiddens = [ProductionTypeEnum.一次注液], IsRunEdit = true, EditLevel = (ulong)DefaultRoleEnum.工艺)]
   [Languages([
      "MES取前工序数据  [优先级:2，需启用MES接口]",
      "Mengaktifkan MES untuk mengambil data proses awal",
      "Enable obtaining pre process data from MES [priority: 1,MES needs to be enabled]",
   ])]
   public bool IsEnablePreProcessDataFromMES { get; set; } = true;

   /// <summary>
   /// 禁用数据库自动删除列
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["禁用数据库自动删除列", "Is Disabled Delete", "Is Disabled Delete"])]
   public bool IsDisabledDelete { get; set; } = true;

   ///// <summary>
   ///// 打印所有SQL
   ///// </summary>
   //[UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   //[Languages(["日志打印所有SQL", "Log mencetak semua SQL", "Print all SQL for logs"])]
   //public bool PrintAllSql { get; set; } = false;

   /// <summary>
   /// 打印动态类
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["打印动态类", "Cetak kelas dinamis", "Print dynamic class"])]
   public bool PrintDynamicClass { get; set; } = false;

   /// <summary>
   /// 设备状态防抖间隔
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["设备状态防抖间隔(毫秒)", "设备状态防抖间隔(毫秒)", "Device Status Span(ms)"])]
   public int DeviceStatusSpanMillisecond { get; set; } = 1000;

   ///// <summary>
   ///// 启用MES
   ///// </summary>
   //[UIDisplay(IsRunEdit = true, EditLevel = (ulong)DefaultRoleEnum.工艺)]
   //[Languages(["启用MES", "Mengaktifkan MES", "MES Status"])]
   //public MESStatusEnum MESStatus { get; set; } = MESStatusEnum.关闭;
}
