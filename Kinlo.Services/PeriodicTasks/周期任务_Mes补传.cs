namespace Kinlo.Services.PeriodicTasks;

public partial class PeriodicTasksHelper
{
   private static bool _resendMesRuning = false;
   static readonly object _lock = new object();
   public static bool IsStopResend { get; set; } = false;

   //private readonly Channel<MesDto> _channel =
   //Channel.CreateUnbounded<MesDto>();
   /// <summary>
   /// 补传MES
   /// </summary>
   /// <param name="container"></param>
   /// <param name="time"></param>
   /// <param name="isAuto"></param>
   /// <param name="sendCount">一个周期内最多传多少数据</param>
   /// <param name="intervalTime">间隔时间</param>
   /// <returns></returns>
   public static async Task PollingResendMes(IContainer container, DateTime time, bool isAuto, int sendCount, int intervalTime)
   {
      var parameterConfig = container.Get<ParameterConfig>();
      IsStopResend = false;
      if (isAuto)
      {
         if (!parameterConfig.FunctionEnable.IsEnableMesResend)
            return;
         if (time.Minute <= 10)
            return; //每小时的第10分钟开始补传，避开整点，整点在执行删除补传表作业
      }
      string logHeader = $"[{(isAuto ? "自动" : "手动")}MES补传]";
      lock (_lock)
      {
         if (_resendMesRuning)
         {
            $"{logHeader} 当前上一次补传任务未结束;".LogRun();
            return;
         } //上一个任务还未完成
         _resendMesRuning = true;
      }

      $"{logHeader} 进入...".LogRun();

      try
      {
         int count = 0;
         int totalCount = 0;
         while (!GlobalStaticTemporary.GlobalToken.IsCancellationRequested && !IsStopResend)
         {
            ++count;
            $"MES补传第[{count}]次开始;".LogRun();
            var sugarDB = container.Get<DbHelper>();
            var list = await sugarDB.QueryMesResendListAsync(200, logHeader);
            if (list == null || list.Count == 0)
            {
               $"MES补传第[{count}]次,总上传数量 [{totalCount}]; 已无数据，结束;".LogRun();
               return;
            }
            var reslut = await MesResend(container, intervalTime, logHeader, list.ToArray());
            if (reslut != ResendResultEnum.成功)
            {
               $"MES补传第[{count}]次,结果 [{reslut}]，本次结束;".LogRun();
               return;
            }
            totalCount += list.Count;
            if (totalCount > sendCount)
            {
               $"MES补传第[{count}]次,总上传数量 [{totalCount}] 超出一次性补传数量上限 [{sendCount}]，本次结束;".LogRun();
               return;
            }
            $"MES补传第[{count}]次,总上传数量 [{totalCount}];".LogRun();
         }
      }
      catch (Exception ex)
      {
         $"MES补传异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
      finally
      {
         lock (_lock)
         {
            _resendMesRuning = false;
         }
      }
   }

   /// <summary>
   /// 补传MES
   /// </summary>
   /// <param name="container"></param>
   /// <param name="logHeader"></param>
   /// <param name="resends"></param>
   /// <returns></returns>
   private static async Task<ResendResultEnum> MesResend(
      IContainer container,
      int intervalTime,
      string logHeader,
      params MesResendModel[] resends
   )
   {
      MesService mesService = container.Get<MesService>();
      DbHelper db = container.Get<DbHelper>();
      var batteryCache = container.Get<IBatteryCache>();

      var batterys = await db.GetBatteryListByIdsAsync(logHeader, resends.Select(x => x.Id).ToArray());
      var needUpdatedMainList = new List<IBatMainModel>(); //主表需更新的电池列表
      var needUpdatedResendList = new List<MesResendModel>(); //补传表需更新列表
      int consecutiveFailures = 0; //连续失败次数.
      foreach (var bat in batterys)
      {
         if (bat.MesOutputStatus == ResultTypeEnum.OK)
         {
            var resend = resends.FirstOrDefault(x => x.Id == bat.Id);
            if (resend != null)
            {
               resend.LastResult = bat.MesOutputStatus;
               resend.LastUpdateTime = bat.OutputTime;
               resend.ResendStatus = ResendStatusEnum.上传成功;
               needUpdatedResendList.Add(resend);
            }
         }
         else
         {
            var sendResult = await MesOutboundHelper.MesOutput(container, mesService, bat, "MES自动补传");
            if (sendResult == OutputStatus.未上传)
            {
               return ResendResultEnum.接口未开启;
            }
            else
            {
               if (sendResult != OutputStatus.成功)
                  ++consecutiveFailures;
               else
                  consecutiveFailures = 0;

               needUpdatedMainList.Add(bat);
               if (sendResult is OutputStatus.成功 or OutputStatus.MES判定NG) //只有成功或失败才需更新补传表
               {
                  var resend = resends.FirstOrDefault(x => x.Id == bat.Id);
                  if (resend != null)
                  {
                     resend.ResendCount++;
                     resend.LastResult = bat.MesOutputStatus;
                     resend.LastUpdateTime = bat.OutputTime;
                     resend.ResendStatus = sendResult switch
                     {
                        OutputStatus.成功 => ResendStatusEnum.上传成功,
                        _ => ResendStatusEnum.上传失败,
                     };
                     needUpdatedResendList.Add(resend);
                  }
               }
            }
            if (consecutiveFailures >= 10)
               return ResendResultEnum.失败次数超上限;

            await Task.Delay(intervalTime);
         }
      }
      return await UpdateTran(db, needUpdatedMainList, needUpdatedResendList, batteryCache, logHeader);
   }

   /// <summary>
   /// 更新主表，补传及缓存
   /// </summary>
   /// <param name="db"></param>
   /// <param name="needUpdatedMainList"></param>
   /// <param name="needUpdatedResendList"></param>
   /// <param name="batteryCache"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static async Task<ResendResultEnum> UpdateTran(
      DbHelper db,
      List<IBatMainModel> needUpdatedMainList,
      List<MesResendModel> needUpdatedResendList,
      IBatteryCache batteryCache,
      string logHeader
   )
   {
      var dbResult = await db.UpdateMesResendAndMainBattery(needUpdatedResendList, needUpdatedMainList, logHeader);

      if (dbResult) //事务成功后更新缓存
      {
         foreach (var item in needUpdatedMainList) //更新缓存
         {
            var bat = await batteryCache.GetByIdAsync(item.Id, logHeader, useCacheOnly: true);
            if (bat != null)
            {
               bat.OutputTime = item.OutputTime;
               bat.MesOutputStatus = item.MesOutputStatus;
            }
         }
         return ResendResultEnum.成功;
      }
      else
      {
         $"更新补传数据事务失败！".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return ResendResultEnum.更新数据事务失败;
      }
   }

   public enum ResendResultEnum
   {
      成功,
      更新数据事务失败,
      接口未开启,
      失败次数超上限,
   }
}
