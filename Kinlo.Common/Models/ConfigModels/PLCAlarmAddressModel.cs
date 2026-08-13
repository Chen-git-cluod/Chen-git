namespace Kinlo.Common.Models.ConfigModels;

public class PLCAlarmAddressModel
{
   #region 记忆跟踪异常
   public Dictionary<ProcessTypeEnum, SignalAddressModel> PLCMemoryLossAlarms = new Dictionary<ProcessTypeEnum, SignalAddressModel>
   {
      { ProcessTypeEnum.前称重, new SignalAddressModel("PC_Alarm.NoMemory_BeforeWeighing", 0) },
      { ProcessTypeEnum.测短路, new SignalAddressModel("PC_Alarm.NoMemory_ShortCircuitTester", 0) },
      { ProcessTypeEnum.后称重, new SignalAddressModel("PC_Alarm.NoMemory_AfterWeighing", 0) },
      { ProcessTypeEnum.下料称重, new SignalAddressModel("PC_Alarm.NoMemory_DownWeighing", 0) },
      { ProcessTypeEnum.后扫码, new SignalAddressModel("PC_Alarm.NoMemory_AfterScanCode", 0) },
      { ProcessTypeEnum.注液量发送, new SignalAddressModel("PC_Alarm.NoMemory_LiquidInjectionPump", 0) },
      { ProcessTypeEnum.补液量发送, new SignalAddressModel("PC_Alarm.Reserve[0]", 0) },
      { ProcessTypeEnum.回氦, new SignalAddressModel("PC_Alarm.NoMemory_HeliumReturn", 0) },
      { ProcessTypeEnum.打钉检测, new SignalAddressModel("PC_Alarm.NoMemory_AdhesiveNailHeight", 0) },
   };
   #endregion

   #region 报警
   /// <summary>
   /// 前称重不稳定报警
   /// </summary>
   public SignalAddressModel Alarm_BeforeWeighing { get; set; } = new SignalAddressModel("PC_Alarm.Unstable_BeforeWeighing", 0);

   /// <summary>
   /// 后称重不稳定报警
   /// </summary>
   public SignalAddressModel Alarm_AfterWeighing { get; set; } = new SignalAddressModel("PC_Alarm.Unstable_AfterWeighing", 0);

   /// <summary>
   /// 补液称不稳定报警
   /// </summary>
   public SignalAddressModel Alarm_RefillWeighing { get; set; } = new SignalAddressModel("PC_Alarm.Unstable_RehydrationWeighing", 0);

   /// <summary>
   /// 露点报警
   /// </summary>
   public SignalAddressModel Alarm_DewPoint { get; set; } = new SignalAddressModel("", 0);
   #endregion

   #region 电子称清零异常
   /// <summary>
   /// 前称重清零异常
   /// </summary>
   public SignalAddressModel Alarm_Zeroing_BeforeWeighing { get; set; } = new SignalAddressModel("PC_Alarm.Zeroing_BeforeWeighing", 0);

   /// <summary>
   /// 后称重清零异常
   /// </summary>
   public SignalAddressModel Alarm_Zeroing_AfterWeighing { get; set; } = new SignalAddressModel("PC_Alarm.Zeroing_AfterWeighing", 0);

   /// <summary>
   /// 下料称重清零异常
   /// </summary>
   public SignalAddressModel Alarm_Zeroing_DownWeighing { get; set; } = new SignalAddressModel("PC_Alarm.Zeroing_DownWeighing", 0);

   /// <summary>
   /// 补液称清零异常
   /// </summary>
   public SignalAddressModel Alarm_Zeroing_RefillWeighing { get; set; } = new SignalAddressModel("PC_Alarm.Zeroing_RehydrationWeighing", 0);
   #endregion
}
