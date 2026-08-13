using HandyControl.Controls;

namespace Kinlo.Common;

/// <summary>
/// 全局临时文件
/// </summary>
public class GlobalStaticTemporary : ConfigurationBase
{
   [JsonIgnore]
   private static readonly CancellationTokenSource _cts = new();

   /// <summary>
   /// 全局 Token ，用于关闭一些随系统启动的任务
   /// </summary>
   [JsonIgnore]
   public static CancellationToken GlobalToken => _cts.Token;

   /// <summary>
   /// 全局 CancellationTokenSource ，用于关闭一些随系统启动的任务，注意：一般传CancellationToken，实在需要才传送CancellationTokenSource
   /// </summary>
   [JsonIgnore]
   public static CancellationTokenSource GlobalCancellationTokenSource => _cts;

   /// <summary>
   /// 取消全局 Token
   /// </summary>
   public static void CancelGlobalToken()
   {
      _cts?.Cancel();
      _cts?.Dispose();
   }

   #region 设备运行相关
   /// <summary>
   /// 设备运行状态
   /// </summary>
   [JsonIgnore]
   // public bool IsRunning { get; set; }
   public bool IsRunning { get; set; }

   /// <summary>
   /// 是否加载完成
   /// </summary>
   [JsonIgnore]
   public bool IsLoadFun { get; set; } = false;

   /// <summary>
   /// 委托停机事件
   /// </summary>
   public event Action? RequestStopAction;

   /// <summary>
   /// 委托开机事件
   /// </summary>
   public event Func<Task<bool>>? RequestStartFuncAsync;

   /// <summary>
   /// 禁止开机原因，如果没有就可以开机
   /// </summary>
   [JsonIgnore]
   private Dictionary<ShutdownType, ShutdownReason> StartupInterlock { get; set; } = new();

   /// <summary>
   /// 请求开机
   /// </summary>
   public async Task<bool> RequestStartAsync()
   {
      if (IsRunning)
         return IsRunning;

      bool strartRes = false;
      if (StartupInterlock.Any())
      {
         string msg =
            "当前无法开机，详情如下：\r\n\r\n"
            + string.Join(
               '；',
               StartupInterlock.Select(x =>
                  $"错误类别：{x.Value.Category}\r\n\r\n原因：{x.Value.Message}\r\n\r\n解决办法：{x.Value.Solution}"
               )
            );
         msg.LogRun(Log4NetLevelEnum.错误, true);
         // UIThreadHelper.InvokeOnUiThread(() => Growl.Warning(msg, "无法开机错误"));
         strartRes = false;
      }
      else
      {
         if (RequestStartFuncAsync != null)
         {
            strartRes = await RequestStartFuncAsync.Invoke();
         }
         else
         {
            strartRes = false;
         }
      }

      await UIThreadHelper.InvokeOnUiThreadAsync(() => IsRunning = strartRes);
      return IsRunning;
   }

   /// <summary>
   /// 请求停机
   /// </summary>
   public void RequestStop(ShutdownReason reason)
   {
      StartupInterlock[reason.Category] = reason;
      if (IsRunning)
         RequestStopAction?.Invoke();
   }

   /// <summary>
   /// 请求停机 无参
   /// </summary>
   public void RequestStop() => RequestStopAction?.Invoke();

   /// <summary>
   /// 删除开机原因
   /// </summary>
   /// <param name="reasons"></param>
   public void DeleteStopReason(params ShutdownType[] reasons)
   {
      foreach (var item in reasons)
      {
         StartupInterlock.Remove(item);
      }
   }

   #endregion

   /// <summary>
   /// 电能表
   /// </summary>
   [JsonIgnore]
   public ZTDTSU666ResultModel ZTDTSU666Result { get; set; } = new ZTDTSU666ResultModel();

   /// <summary>
   /// 浮子流量计，贺德克(HDK-LDZ-DN50)
   /// </summary>
   [JsonIgnore]
   public HDK_LDZ_DN50DTO HDK_LDZ_DN50 { get; set; } = new HDK_LDZ_DN50DTO();

   /// <summary>
   /// 前扫码正在使用的托盘电池
   /// </summary>
   public ConcurrentDictionary<string, LogisticsBatteryLocalModel> CurrentCrates { get; set; } = new();

   /// <summary>
   /// 上一次生产数据导出时间
   /// </summary>
   public DateTime LastProductionDataExportTime { get; set; } = DateTime.MinValue;
   private Lazy<UsersStatusConfig> _usersStatusLazy;

   public GlobalStaticTemporary(StyletIoC.IContainer container, bool isStartup)
      : base(container, isStartup)
   {
      _usersStatusLazy = new Lazy<UsersStatusConfig>(() => container.Get<UsersStatusConfig>());
   }

   public override void Load()
   {
      var json = FileHelper.LoadToString(this.GetType().Name);
      if (!string.IsNullOrEmpty(json))
      {
         json.ParseJson(root =>
         {
            JsonElement element;
            if (
               root.TryGetProperty(nameof(LastProductionDataExportTime), out element)
               && element.TryGetDateTime(out var lastProductionDataExportTime)
            )
               LastProductionDataExportTime = lastProductionDataExportTime;

            if (root.TryGetProperty(nameof(CurrentCrates), out element))
               CurrentCrates = element.Deserialize<ConcurrentDictionary<string, LogisticsBatteryLocalModel>>();
            return true;
         });
      }
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="IsRunEdit">在运行时是否可编辑</param>
   /// <param name="Level">使用此功能需要的等级</param>
   public record Permission(bool IsRunEdit, ulong Level);

   /// <summary>
   /// 权限验证
   /// </summary>
   /// <param name="permission"></param>
   /// <returns></returns>
   public bool PermissionVerification(Permission permission, out string msg)
   {
      var userState = _usersStatusLazy.Value;
      if (!permission.IsRunEdit && IsRunning)
      {
         msg = "设备正在运行，无法使用此功能！";
         return false;
      }

      if ((userState.LocalLoggedinUser.Level & permission.Level) <= 0)
      {
         msg = "您无此权限！";
         return false;
      }
      msg = "";
      return true;
   }
}
