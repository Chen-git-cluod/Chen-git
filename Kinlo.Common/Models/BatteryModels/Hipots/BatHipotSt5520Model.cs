namespace Kinlo.Common.Models.BatteryModels;

/// <summary>
/// 测短路 ST5520
/// </summary>
[BatteryDisplay(DisplayProcesses = [ProcessTypeEnum.测短路], DeviceCommunicationType = [CommunicationEnum.ShortCircuit_ST5520])]
[Languages(["测短路", "测短路", "Short circuit test"])]
[AddINotifyPropertyChangedInterface]
public partial class BatHipotSt5520Model
{
   /// <summary>
   /// 测短路时间
   /// </summary>
   [Languages(["测短路时间", "测短路时间", "Hipot time"])]
   [SugarColumn(ColumnDescription = "测短路时间")]
   [OrderMarker]
   public DateTime HipotTime { get; set; }

   /// <summary>
   /// 测短路通道
   /// </summary>
   [Languages(["测短路通道", "测短路通道", "Hipot index"])]
   [SugarColumn(ColumnDescription = "测短路通道", ColumnDataType = "tinyint UNSIGNED")] //UNSIGNED 无符号
   //[Statistics(ProcessTypeEnum.测短路, StatisticsFuncEnum.通道)]
   public byte HipotIndex { get; set; }

   /// <summary>
   /// 测试电压(V)
   /// </summary>
   [Languages(["测试电压(V)", "电压测试电压(V)", "Voltage(V)"])]
   [SugarColumn(ColumnDescription = "电压测试电压(V)")]
   public double HipotTestVoltageValue { get; set; }

   /// <summary>
   /// 电阻值(MΩ)
   /// </summary>
   [Languages(["电阻值(MΩ)", "电阻值(MΩ)", "Resistance value(MΩ)"])]
   [SugarColumn(ColumnDescription = "电阻值(MΩ)")]
   public double ResistanceValue { get; set; }

   /// <summary>
   /// 测短路结果
   /// </summary>
   [Languages(["Hipot结果", "Hipot结果", "Hipot reslut"])]
   [SugarColumn(ColumnDescription = "Hipot结果")]
   [DynamicClass(Process = ProcessTypeEnum.测短路)]
   [ProcessRatio(nameof(HipotTime))]
   public ResultTypeEnum HipotResult { get; set; }
}
