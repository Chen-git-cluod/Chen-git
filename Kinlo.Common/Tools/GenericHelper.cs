using System.Security.Claims;
using System.Windows.Interop;
using Dm.util;
using HandyControl.Controls;

namespace Kinlo.Common.Tools;

public static class GenericHelper
{
   /// <summary>
   /// 全屏报警标记
   /// </summary>
   public const string FullScreenAlarmToken = "FullScreenAlarmToken";

   /// <summary>
   /// JsonOptions
   /// </summary>
   public static JsonSerializerOptions SerializerOptions { get; set; } =
      new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

   /// <summary>
   /// 返回时间String
   /// </summary>
   /// <param name="dateTime"></param>
   /// <param name="dateFormat">时间格式</param>
   /// <returns></returns>
   public static string ToMesDateTime(this DateTime dateTime, string dateFormat = "yyyy-MM-dd HH:mm:ss") =>
      dateTime switch
      {
         var d when IsDefaultTime(d) => "",
         _ => dateTime.ToString(dateFormat),
      };

   /// <summary>
   /// 返回时间String
   /// </summary>
   /// <param name="dateTime"></param>
   /// <param name="dateFormat">时间格式</param>
   /// <returns></returns>
   public static string ToMesDateTime(this DateTime? dateTime, string dateFormat = "yyyy-MM-dd HH:mm:ss") =>
      dateTime switch
      {
         null => "",
         _ => ToMesDateTime(dateTime.Value, dateFormat),
      };

   /// <summary>
   /// 是否是默认的时间
   /// </summary>
   /// <param name="dateTime"></param>
   /// <returns></returns>
   public static bool IsDefaultTime(this DateTime dateTime) =>
      dateTime switch
      {
         var d when d == DateTime.MinValue => true,
         var d when d == default => true,
         var d when d < new DateTime(2000, 1, 1) => true,
         _ => false,
      };

   public static string ToMesString(this object value) =>
      value switch //工艺参数值
      {
         var v when v is string str => str,
         var v when v is float f => f.ToString("0.###"),
         var v when v is double d => d.ToString("0.###"),
         _ => value?.ToString() ?? "",
      };

   public static LoadingCircle CreateLoadingCircle() =>
      new LoadingCircle
      {
         Height = 50,
         Width = 50,
         DotDiameter = 8,
      };

   /// <summary>
   /// 获取enum特性
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="value"></param>
   /// <returns></returns>
   public static T? GetEnumDescription<T>(this Enum value)
      where T : Attribute
   {
      // 获取枚举字段信息
      FieldInfo? field = value.GetType().GetField(value.ToString());
      if (field == null)
         return default;

      // 获取 Description 特性
      T? attribute = field.GetCustomAttribute<T>();
      return attribute;
   }

   #region 一注加压缸数据处理
   public static string PressursToString(this string pressureStr, string funcStr)
   {
      var pressureList = pressureStr.Split(',').ToList();
      var funcList = funcStr.toString().Split(',').ToList();
      int funcIndex = 0;
      return string.Join(
         "，",
         pressureList.Select(
            (x, i) =>
            {
               if (funcIndex >= funcList.Count)
                  funcIndex = 0;

               string fun = funcList[funcIndex].PressursConverter();
               funcIndex++;
               return $"{fun} {x}";
            }
         )
      );
   }

   /// <summary>
   /// //1:真空，2：正压：3：大气
   /// </summary>
   /// <param name="value"></param>
   /// <returns></returns>
   public static string PressursConverter(this string value) =>
      value.Trim() == "1" ? "真空"
      : value.Trim() == "2" ? "正压"
      : value.Trim() == "3" ? "大气"
      : "未定义";

   public static string PressureFuncToString(this string? value) =>
      value == null ? string.Empty : value.Replace("1", "真空").Replace("2", "正压").Replace("3", "大气");

   /// <summary>
   /// 是否为加压缸相关属性
   /// </summary>
   /// <param name="value"></param>
   /// <returns></returns>
   public static bool IsPressureValid(this string? value)
   {
      if (value == null)
         return false;
      if (
         value
         is nameof(BatTankModel.SetPressure)
            or nameof(BatTankModel.ActualPressure)
            or nameof(BatTankModel.SetHoldPressureDuration)
            or nameof(BatTankModel.ActualHoldPressureDuration)
            //   or nameof(BatTankModel.SetReachingPressureDuration)
            or nameof(BatTankModel.ActualReachingPressureDuration)
      )
         // or nameof(BatTankModel.lineUpTime))
         return true;
      return false;
   }
   #endregion
   /// <summary>
   ///计算标准偏差
   /// </summary>
   /// <param name="arrData">只有合格的电芯数据参与计算</param>
   /// <param name="value">这个固定的值，要开放出来填写</param>
   /// <returns>西格玛下限,西格玛上限,极差值,极差上限,平均值</returns>
   public static (float lower, float upper, float diffUpper, float diff, float average) CalculateStandardDeviation(
      this List<float> arrData,
      float value
   ) //计算标准偏差
   {
      float xAvg = arrData.Average(); //平均值
      float sSum = 0F;
      int arrNum = arrData.Count;

      for (int j = 0; j < arrNum; j++)
      {
         sSum += ((arrData[j] - xAvg) * (arrData[j] - xAvg));
      }
      var stdDiff = Convert.ToSingle(Math.Sqrt((sSum / (arrNum - 1))).ToString()); //标准差 极差
      var upper = xAvg + 3 * stdDiff; //西格玛上限
      var diffUpper = arrData.Min() + value; //极差上限
      return (0, upper, stdDiff, diffUpper, xAvg);
   }

   /// <summary>
   /// 转为生产日志Header
   /// </summary>
   /// <param name="plcInteractAddress"></param>
   /// <param name="lane"></param>
   /// <param name="id"></param>
   /// <param name="barcode"></param>
   /// <returns></returns>
   public static string ToProcessLogHeader(
      this PLCInteractAddressModel plcInteractAddress,
      int lane = 0,
      long id = 0,
      string barcode = ""
   ) =>
      plcInteractAddress == null
         ? "[警告：日志头文件为空]"
         : $"[{plcInteractAddress.ServiceName}-{plcInteractAddress.ProcessesType}-{plcInteractAddress.DeviceCommunicationType}-{(lane < 1 ? "" : $"{lane}通道")}-{(id < 1 ? "" : $"{id}")}-{barcode}]";

   /// <summary>
   /// 转为生产日志Header
   /// </summary>
   /// <param name="rocessesType"></param>
   /// <param name="serviceName"></param>
   /// <param name="communication"></param>
   /// <param name="lane"></param>
   /// <param name="id"></param>
   /// <param name="barcode"></param>
   /// <returns></returns>
   public static string ToProcessLogHeader(
      this ProcessTypeEnum rocessesType,
      string serviceName,
      CommunicationEnum communication,
      int lane = 0,
      long id = 0,
      string barcode = ""
   ) => $"[{serviceName}-{rocessesType}-{communication}-{(lane < 1 ? "" : $"{lane}通道")}-{(id < 1 ? "" : $"{id}")}-{barcode}]";

   /// <summary>
   /// mes结果转生产结果
   /// </summary>
   /// <param name="mesResult"></param>
   /// <returns></returns>
   public static ResultTypeEnum ToResult(this MesResultStatusEnum mesResult) =>
      mesResult switch
      {
         MesResultStatusEnum.成功 => ResultTypeEnum.OK,
         MesResultStatusEnum.生成报文失败 => ResultTypeEnum.MES报文生成失败,
         MesResultStatusEnum.通讯错误 => ResultTypeEnum.MES通讯错误,
         _ => ResultTypeEnum.MES判定NG,
      };

   public static string TimeSpanToString(this TimeSpan duration)
   {
      int days = duration.Days; // 总天数（不包括小时部分计算的天数）
      int hours = duration.Hours; // 剩余的小时数 (0-23)
      int minutes = duration.Minutes; // 剩余的分钟数 (0-59)
      int seconds = duration.Seconds; // 剩余的秒数 (0-59)

      var str =
         $"{(days > 0 ? $"{days}天" : "")}{(hours > 0 ? $"{hours}小时" : "")}{(minutes > 0 ? $"{minutes}分" : "")}{(seconds > 0 ? $"{seconds}秒" : "")}";
      return str.IsNullOrEmpty() ? "0秒" : str;
   }

   /// <summary>
   /// 计算月份差
   /// </summary>
   /// <param name="start"></param>
   /// <param name="end"></param>
   /// <returns></returns>
   public static int GetMonthCount(this DateTime start, DateTime end)
   {
      if (end < start)
         return 0;

      return (end.Year - start.Year) * 12 + (end.Month - start.Month) + 1;
   }

   public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);

   public static bool IsNullOrWhiteSpace(this string text) => string.IsNullOrWhiteSpace(text);

   /// <summary>
   /// 判断是否包含了这个状态
   /// </summary>
   /// <param name="value"></param>
   /// <param name="deviceState"></param>
   /// <returns></returns>
   public static bool IsIncludeDeviceState(this short value, DeviceStateEnum deviceState) => (value & (short)deviceState) > 0;

   /// <summary>
   /// 返回包含的所有状态
   /// </summary>
   /// <param name="value"></param>
   /// <returns></returns>
   public static IReadOnlyList<DeviceStateEnum> ToDeviceState(this short value)
   {
      return Enum.GetValues<DeviceStateEnum>().Where(ds => value.IsIncludeDeviceState(ds)).ToList().AsReadOnly();
   }

    /// <summary>
    /// 取得某个状态的值，返回true或false
    /// </summary>
    /// <param name="value"></param>
    /// <param name="deviceState"></param>
    /// <returns></returns>
    public static bool GetPlcStatus(this short value, DeviceStateEnum deviceState)
    {
        return value == (short)deviceState;
    }

    /// <summary>
    /// 取得状态的索引
    /// </summary>
    /// <param name="deviceState"></param>
    /// <returns></returns>
    public static ushort GetDeviceStateIndex(this DeviceStateEnum deviceState) =>
      (ushort)System.Numerics.BitOperations.TrailingZeroCount((int)deviceState);

   /// <summary>
   /// 只保留最优状态，优先级：运行>报警>待料>堵料>待机
   /// </summary>
   /// <param name="plcValue"></param>
   /// <returns></returns>
   public static DeviceStateEnum KeepBest(this short plcValue)
   {
      if (plcValue.IsIncludeDeviceState(DeviceStateEnum.运行))
         return DeviceStateEnum.运行;
      if (plcValue.IsIncludeDeviceState(DeviceStateEnum.报警))
         return DeviceStateEnum.报警;
      if (plcValue.IsIncludeDeviceState(DeviceStateEnum.待料))
         return DeviceStateEnum.待料;
      if (plcValue.IsIncludeDeviceState(DeviceStateEnum.堵料))
         return DeviceStateEnum.堵料;
      return DeviceStateEnum.待机;
   }

   public static string ToStringFormArray<T>(this IEnumerable<T> array) => string.Join(",", array);

   /// <summary>
   /// 创建进站查询映射
   /// </summary>
   /// <returns></returns>
   public static TimePropertyDisplayMapModel BuildInputTimeMap() =>
      new TimePropertyDisplayMapModel
      {
         IsQueryById = true,
         Display = "进站时间",
         PropertyName = nameof(BatMainModel.Id),
      };

   /// <summary>
   /// 创建出站查询映射
   /// </summary>
   /// <returns></returns>
   public static TimePropertyDisplayMapModel GetOutputTimeMap() =>
      new TimePropertyDisplayMapModel
      {
         IsQueryById = false,
         Display = "出站时间",
         PropertyName = nameof(BatMainModel.OutputTime),
      };

   public static string Build24HourDataSubTitle(DateTime startTime, DateTime? endTiem = null) =>
      endTiem == null ? $"{startTime:dd日 HH时}~{startTime.AddHours(1):HH时}" : $"{startTime:dd日 HH时}~{endTiem.Value:HH时}";
}
