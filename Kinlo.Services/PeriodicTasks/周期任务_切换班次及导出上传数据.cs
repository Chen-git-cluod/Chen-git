using System.Dynamic;
using System.IO;
using Kinlo.Common.Models.ConfigModels.MesConfigModels;
using Kinlo.MESDocking.Ftp;
using static Kinlo.Common.Tools.ShiftHelper;

namespace Kinlo.Services.PeriodicTasks;

public partial class PeriodicTasksHelper
{
   /// <summary>
   /// 切换班次及时间
   /// </summary>
   public async Task SwitchShiftAsync(DateTime t)
   {
      if (t.Second % 5 != 0)
         return; //5秒一次
      await UpdateShiftAsync(t, _parameterConfig, _appGlobalConfig);
   }

   public static async Task UpdateShiftAsync(DateTime dateTime, ParameterConfig parameterConfig, AppGlobalConfig appGlobalConfig)
   {
      var currentShift = dateTime.GetShiftInfoByTime(parameterConfig);
      await UIThreadHelper.InvokeOnUiThreadAsync(() =>
      {
         if (appGlobalConfig.ShiftSwitchInfo.Shift != currentShift.shift)
            appGlobalConfig.ShiftSwitchInfo.Shift = currentShift.shift;

         if (appGlobalConfig.ShiftSwitchInfo.LastResetTime < currentShift.startTime)
            appGlobalConfig.ShiftSwitchInfo.LastResetTime = currentShift.startTime;
      });
   }

   private int _lockShift = 0;

   /// <summary>
   /// 导出上一班数据
   /// </summary>
   /// <param name="t"></param>
   /// <returns></returns>
   public async Task ExportPreShiftDataAsync(DateTime t)
   {
      if (t.Second % 5 != 0)
         return; //5秒一次

      if (Interlocked.Exchange(ref _lockShift, 1) == 0) //导出上一班的数据并上传FTP
      {
         try
         {
            if (!_globalStaticTemporary.LastProductionDataExportTime.IsTimeInShift(t, _parameterConfig)) //如果时间不是当次班次范围
            {
               await ExportProductedDataAsync(
                  t,
                  _ftpService,
                  _mesInterfaceParameterConfig,
                  _parameterConfig,
                  _sugarDB,
                  _otherParameterConfig,
                  _displayDataCollection
               );
               await ExportPlcAlarmDataAsync(t, _ftpService, _mesInterfaceParameterConfig, _parameterConfig, _sugarDB);

               _globalStaticTemporary.LastProductionDataExportTime = t;
               _globalStaticTemporary.Save("系统自动保存", $"[自动导出上一班数据]完成", false);
            }
         }
         finally
         {
            _lockShift = 0;
         }
      }
   }

   #region 按班次导出生产数据
   /// <summary>
   /// 导出上一班的生产数据
   /// </summary>
   public static async Task<bool> ExportProductedDataAsync(
      DateTime t,
      FtpService ftpService,
      MesInterfaceParameterConfig mesInterfaceParameterConfig,
      ParameterConfig parameterConfig,
      DbHelper dbHelper,
      OtherParameterConfig otherParameterConfig,
      DisplayDataCollection displayData
   )
   {
      //导出上一班的生产数据
      var shiftTimeRange = t.GetPreShiftInfoByTime(parameterConfig);
      var exportResult = await ExportProductedDataByShiftAsync(
         shiftTimeRange,
         parameterConfig,
         dbHelper,
         otherParameterConfig,
         displayData
      );
        if (exportResult.state)
        {
            if (mesInterfaceParameterConfig.FtpServiceInfo.Enable && !string.IsNullOrEmpty(exportResult.filePath))
            {
                await UploadFtpAsync(
                   shiftTimeRange,
                   exportResult.filePath,
                   ftpService,
                   mesInterfaceParameterConfig.FtpServiceInfo,
                   parameterConfig,
                   SendFtpType.生产数据
                );
            }
        } 
        return exportResult.state;
   }

   /// <summary>
   /// 按班次导出生产数据
   /// </summary>
   /// <param name="shiftTimeRange"></param>
   /// <param name="parameterConfig"></param>
   /// <param name="dbHelper"></param>
   /// <param name="otherParameterConfig"></param>
   /// <param name="displayData"></param>
   /// <returns>返回当前文件路径</returns>
   public static async Task<(bool state, string filePath)> ExportProductedDataByShiftAsync(
      ShiftInfo shiftTimeRange,
      ParameterConfig parameterConfig,
      DbHelper dbHelper,
      OtherParameterConfig otherParameterConfig,
      DisplayDataCollection displayData
   )
   {
      try
      {
         var info = GenericHelper.GetOutputTimeMap();
         var queryTime = new TimeFilterDto
         {
            PropertyName = info.PropertyName,
            IsQueryById = info.IsQueryById,
            StartTime = shiftTimeRange.startTime,
            EndTime = shiftTimeRange.endTime,
         };
         var queryRes = new ResultFilterDto(ResultFilterMode.All);
         QueryFilterDto option = new QueryFilterDto(queryTime, queryRes);
         var data = await dbHelper.GetBatterysByOutputTimeRangeAsync<ExpandoObject>(option);
         $"[按班次导出生产数据]导出班次：{shiftTimeRange.shift}，开始时间：{shiftTimeRange.startTime}，结束时间：{shiftTimeRange.endTime}；共[{(data == null ? 0 : data.Count)}]条数据！".LogRun();
         if (data == null || !data.Any())
         {
            $"[按班次导出生产数据]导出班次：{shiftTimeRange.shift}，开始时间：{shiftTimeRange.startTime}，结束时间：{shiftTimeRange.endTime}；无数据！".LogRun();
            return (true, string.Empty);
         }
         if (!parameterConfig.DeviceParameter.ProductionDataPath.IsValidPath())
            parameterConfig.DeviceParameter.ProductionDataPath = @"C:\生产数据";

         string filePath = Path.Combine(
            parameterConfig.DeviceParameter.ProductionDataPath,
            SendFtpType.生产数据.ToString(),
            $"{shiftTimeRange.startTime.Year}年",
            $"{shiftTimeRange.startTime.Month}月",
            $"{shiftTimeRange.shift}_{shiftTimeRange.startTime:yyyy年MM月dd日HH时mm分ss秒}~{shiftTimeRange.endTime:yyyy年MM月dd日HH时mm分ss秒}.xlsx"
         );
         var exportResult = await Task.Run(() =>
            data.ExportBattery(filePath, otherParameterConfig, displayData.CompleteBatteryDatas.PropertyBindings, isOpen: false)
         );
         $"[按班次导出数据]{(exportResult ? "成功！" : "失败")}；".LogRun();
         return (exportResult, filePath);
      }
      catch (Exception ex)
      {
         $"[按班次导出数据]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
         return (false, string.Empty);
      }
   }

   #endregion

   #region 按班次导出PLC报警数据
   public static async Task ExportPlcAlarmDataAsync(
      DateTime t,
      FtpService ftpService,
      MesInterfaceParameterConfig mesInterfaceParameterConfig,
      ParameterConfig parameterConfig,
      DbHelper dbHelper
   )
   {
      //导出上一班的数据
      var shiftTimeRange = t.GetPreShiftInfoByTime(parameterConfig);
      var exportResult = await ExportPlcAlarmDataByShiftAsync(shiftTimeRange, parameterConfig, dbHelper);
      if (exportResult.state)
      {
         if (mesInterfaceParameterConfig.FtpServiceInfo.Enable && !string.IsNullOrEmpty(exportResult.filePath))
         {
            await UploadFtpAsync(
               shiftTimeRange,
               exportResult.filePath,
               ftpService,
               mesInterfaceParameterConfig.FtpServiceInfo,
               parameterConfig,
               SendFtpType.报警数据
            );
         }
      }
   }

   /// <summary>
   /// 按班次导出报警数据
   /// </summary>
   /// <param name="shiftTimeRange"></param>
   /// <param name="parameterConfig"></param>
   /// <param name="dbHelper"></param>
   /// <returns>返回当前文件路径</returns>
   public static async Task<(bool state, string filePath)> ExportPlcAlarmDataByShiftAsync(
      ShiftInfo shiftTimeRange,
      ParameterConfig parameterConfig,
      DbHelper dbHelper
   )
   {
      try
      {
         var data = await dbHelper.GetTimeRangePlcAlarmsAsync(
            shiftTimeRange.startTime,
            shiftTimeRange.endTime,
            [PlcAalrmLevelEnum.报警, PlcAalrmLevelEnum.警告]
         );
         $"[按班次导出报警数据]导出班次：{shiftTimeRange.shift}，开始时间：{shiftTimeRange.startTime}，结束时间：{shiftTimeRange.endTime}；共[{(data == null ? 0 : data.Count)}]条数据！".LogRun();
         if (data == null || !data.Any())
         {
            $"[按班次导出报警数据]导出班次：{shiftTimeRange.shift}，开始时间：{shiftTimeRange.startTime}，结束时间：{shiftTimeRange.endTime}；无数据！".LogRun();
            return (true, string.Empty);
         }

         List<UploadFtpPlcAlarmDto> upload = new List<UploadFtpPlcAlarmDto>();
         foreach (var item in data)
         {
            upload.Add(
               new UploadFtpPlcAlarmDto
               {
                  Id = item.Id,
                  Level = item.PlcAalrmLevel.ToString(),
                  MesCode = item.MesCode,
                  AlarmMessage = item.AlarmMessage,
                  StartTime = item.StartTime,
                  EndTime = item.EndTime,
               }
            );
         }

         if (!parameterConfig.DeviceParameter.ProductionDataPath.IsValidPath())
            parameterConfig.DeviceParameter.ProductionDataPath = @"C:\生产数据";

         string filePath = Path.Combine(
            parameterConfig.DeviceParameter.ProductionDataPath,
            $"{SendFtpType.报警数据}",
            $"{shiftTimeRange.startTime.Year}年",
            $"{shiftTimeRange.startTime.Month}月",
            $"{shiftTimeRange.shift}_{shiftTimeRange.startTime:yyyy年MM月dd日HH时mm分ss秒}~{shiftTimeRange.endTime:yyyy年MM月dd日HH时mm分ss秒}.xlsx"
         );
         var exportResult = await Task.Run(() => upload.ExportExcel(filePath, false));
         $"[按班次导出报警数据]{(exportResult ? "成功！" : "失败")}；".LogRun();
         return (exportResult, filePath);
      }
      catch (Exception ex)
      {
         $"[按班次导出报警数据]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
         return (false, string.Empty);
      }
   }

   public class UploadFtpPlcAlarmDto
   {
      [Languages("Id")]
      public long Id { get; set; }

      /// <summary>
      /// 报警等级
      /// </summary>
      [Languages("报警等级")]
      public string Level { get; set; } = string.Empty;

      /// <summary>
      /// MES代码
      /// </summary>
      [Languages("MES代码")]
      public string MesCode { get; set; } = string.Empty;

      /// <summary>
      /// PLC报警信息
      /// </summary>
      [Languages("报警信息")]
      public string AlarmMessage { get; set; } = string.Empty;

      [Languages("开始时间")]
      public DateTime StartTime { get; set; }

      [Languages("结束时间")]
      public DateTime? EndTime { get; set; } = null;
   }
   #endregion

   #region 辅助方法


   /// <summary>
   /// 上传FTP
   /// </summary>
   /// <param name="shiftTimeRanget"></param>
   /// <param name="ftpService"></param>
   /// <param name="localFilePath"></param>
   /// <param name="ftpServiceInfo"></param>
   /// <param name="parameterConfig"></param>
   /// <param name="sendFtpType"></param>
   /// <returns></returns>
   public static async Task<bool> UploadFtpAsync(
      ShiftInfo shiftTimeRanget,
      string localFilePath,
      FtpService ftpService,
      FtpServiceInfoModel ftpServiceInfo,
      ParameterConfig parameterConfig,
      SendFtpType sendFtpType
   )
   {
      string remoteDirectory = sendFtpType switch
      {
         SendFtpType.生产数据 => $"{ftpServiceInfo.RemoteDirectory.TrimEnd('/')}/生产数据",
         _ => $"{ftpServiceInfo.RemoteDirectory.TrimEnd('/')}/报警数据",
      }; //远程目录
        remoteDirectory = $"{remoteDirectory}/{shiftTimeRanget.startTime:yyyy}/{shiftTimeRanget.startTime:MM}/{shiftTimeRanget.startTime:dd}/{shiftTimeRanget.shift}";//芜湖追加
        var sendTimeRange =
         $"({shiftTimeRanget.startTime.Hour}.{shiftTimeRanget.startTime.Minute:D2}-{shiftTimeRanget.endTime.Hour}.{shiftTimeRanget.endTime.Minute:D2})";
      string remoteFileName =
         $"{shiftTimeRanget.startTime:yyyyMMdd}_{parameterConfig.DeviceParameter.DeviceName}_{shiftTimeRanget.shift}{sendTimeRange}.xlsx"; //远程文件名

      //上传到FTP
      var uploadResult = await ftpService.UploadFileWithNewConnectionAsync(localFilePath, remoteDirectory, remoteFileName);
      if (uploadResult)
      {
         $"[自动导出上一班数据]文件上传FTP成功；".LogRun();
      }
      else
      {
         $"[自动导出上一班数据]文件上传FTP失败；".LogRun(Log4NetLevelEnum.错误, true);
      }
      return uploadResult;
   }

   public enum SendFtpType
   {
      生产数据,
      报警数据,
   }
   #endregion
}
