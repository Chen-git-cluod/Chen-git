namespace Kinlo.GUI.ViewModel.Parts.FileDisplay;

/// <summary>
/// 打开设备文档说明书
/// </summary>
public static class DeviceManualHelper
{
   private static readonly string basePath;

   static DeviceManualHelper()
   {
      // 获取当前基目录的上一级
      string parentDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
      basePath = Path.Combine(parentDir, "说明书");
   }

   public static FileNode GetFilePath(ProcessTypeEnum processType, CommunicationEnum communication) =>
      OnGetFilePath(processType, communication);

   public static FileNode GetFilePath(ProcessTypeEnum? processType, CommunicationEnum communication) =>
      OnGetFilePath(processType, communication);

   public static FileNode OnGetFilePath(ProcessTypeEnum? processType, CommunicationEnum communication)
   {
      var deviceType = processType.ToManualType();
      var subPath = communication.ToManulaSubPath();

      var fullPaht = Path.Combine(basePath, deviceType.ToString());
      if (!string.IsNullOrWhiteSpace(subPath))
         fullPaht = Path.Combine(fullPaht, subPath);

      return new FileNode(fullPaht, true, basePath);
   }

   /// <summary>
   /// 转手册子目录
   /// </summary>
   /// <param name="communication"></param>
   /// <returns></returns>
   public static string ToManulaSubPath(this CommunicationEnum communication) =>
      communication switch
      {
         CommunicationEnum.None
         or CommunicationEnum.Modbus_TCP_ABCD
         or CommunicationEnum.Modbus_TCP_BADC
         or CommunicationEnum.Modbus_TCP_DCBA
         or CommunicationEnum.Modbus_TCP_CDAB
         or CommunicationEnum.FinsUdpShortConn
         or CommunicationEnum.FinsUdpLongConn => "",
         CommunicationEnum.CipInovance => "汇川PLC",
         CommunicationEnum.CipOrmonPlc or CommunicationEnum.CipOrmonPlcLight => "欧姆龙PLC",
         CommunicationEnum.S712_PLC or CommunicationEnum.S715_PLC => "基恩士PLC",
         _ => communication.ToString(),
      };

   /// <summary>
   /// 转手册类型
   /// </summary>
   /// <param name="processType"></param>
   /// <returns></returns>
   public static DeviceManualType ToManualType(this ProcessTypeEnum? processType) =>
      processType switch
      {
         null => DeviceManualType.本软件说明书,
         ProcessTypeEnum.PLC => DeviceManualType.PLC,
         ProcessTypeEnum.测短路 => DeviceManualType.短路测试仪,
         ProcessTypeEnum.测电压 or ProcessTypeEnum.计算电压极差 => DeviceManualType.电压测试仪,
         ProcessTypeEnum.托盘扫码 or ProcessTypeEnum.套杯扫码 => DeviceManualType.RFID读码,

         ProcessTypeEnum.前扫码
         or ProcessTypeEnum.后扫码
         or ProcessTypeEnum.装盘扫码
         or ProcessTypeEnum.拆盘扫码
         or ProcessTypeEnum.回流扫码
         or ProcessTypeEnum.下料扫码 => DeviceManualType.常规扫码枪,
         ProcessTypeEnum.前称重
         or ProcessTypeEnum.前称重清零
         or ProcessTypeEnum.后称重
         or ProcessTypeEnum.后称重清零
         or ProcessTypeEnum.下料称重
         or ProcessTypeEnum.下料称重清零
         or ProcessTypeEnum.回氦称重
         or ProcessTypeEnum.回氦称重清零
         or ProcessTypeEnum.手动补液
         or ProcessTypeEnum.补液称清零 => DeviceManualType.称重,
         ProcessTypeEnum.注液站浓度 or ProcessTypeEnum.储液柜浓度 or ProcessTypeEnum.补液站浓度 => DeviceManualType.其它仪表,
         _ => DeviceManualType.其它,
      };

   public static bool CreateManualDir(this DeviceManualType type)
   {
      try
      {
         string path = type == DeviceManualType.其它 ? basePath : Path.Combine(basePath, type.ToString());
         if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
         return true;
      }
      catch (Exception ex)
      {
         $"创建外设异常：{ex}".LogRun(Log4NetLevelEnum.警告);
      }

      return false;
   }
}
