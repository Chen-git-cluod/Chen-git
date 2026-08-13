namespace Kinlo.Common.Configurations;

[Languages]
public class ParameterConfig : ConfigurationBase
{
   /// <summary>
   /// 当前配方名
   /// </summary>
   public string RecipeName { get; private set; } = string.Empty;

   [Languages(["基本参数", "Parameter dasar", "Basic parameters"])]
   public DeviceParameterModel DeviceParameter { get; set; } = new();

   [Languages(["功能启用", "Fungsi diaktifkan", "Function enabled"])]
   public FunctionEnableModel FunctionEnable { get; set; } = new();

   [Languages(["运行参数", "Parameter operasi", "Operating parameters"])]
   public RunParameterModel RunParameter { get; set; } = new();

   [Languages(["高级设置", "Pengaturan Lanjutan", "Advanced setting"])]
   public AdvancedConfigModel AdvancedConfig { get; set; } = new();

   public ParameterConfig(StyletIoC.IContainer container, bool isStartup)
      : base(container, isStartup) { }

   public override void Load()
   {
      try
      {
         var oteherConfig = _container.Get<OtherParameterConfig>();
         var val = oteherConfig.OtherParameter.GetParameterConfig();
         Copy(val, this);
      }
      catch (Exception ex)
      {
         $"[ParameterConfig从配方恢复文件]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   public static void Copy(ParameterConfig source, ParameterConfig target)
   {
      target.SetRecipeName(source.RecipeName);
      ExpressionAssignmentMapper<RunParameterModel, RunParameterModel>.Trans(source.RunParameter, target.RunParameter);
      ExpressionAssignmentMapper<DeviceParameterModel, DeviceParameterModel>.Trans(source.DeviceParameter, target.DeviceParameter);
      ExpressionAssignmentMapper<FunctionEnableModel, FunctionEnableModel>.Trans(source.FunctionEnable, target.FunctionEnable);
      ExpressionAssignmentMapper<AdvancedConfigModel, AdvancedConfigModel>.Trans(source.AdvancedConfig, target.AdvancedConfig);
   }

   public override void Save(string userName, string revise, bool isPopup = true, bool isPrintLog = true, string saveName = "")
   {
      base.Save(userName, revise, isPopup, isPrintLog, $"{FileHelper.ParameterSaveFolder}\\{RecipeName}");
   }

   public void SetRecipeName(string name) => RecipeName = name;

   public string GetRecipeName() => RecipeName;

   #region 用于不是在ConfigurationParameterViewModel类中修改参数时统一调用，加锁多线程安全
   /// <summary>
   /// 取得UI显示的副本及监控器
   /// </summary>
   [JsonIgnore]
   public Func<(ParameterConfig? uiCopy, ChangeTracker<ParameterConfig>? changeTracker)>? GetUIParameter { get; set; }
   private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

   /// <summary>
   /// 注意 UIParameter 可能为null,(如果从来没进入过设置页面，此处为null，赋值时需为null检查)
   /// </summary>
   /// <param name="updateAction"></param>
   /// <returns></returns>
   public async Task UpdateParameterAsync(Action<ParameterConfig, ParameterConfig?, ChangeTracker<ParameterConfig>?> updateAction)
   {
      try
      {
         await _semaphore.WaitAsync();
         var uiInfo = GetUIParameter?.Invoke();

         await UIThreadHelper.InvokeOnUiThreadAsync(() => updateAction(this, uiInfo?.uiCopy, uiInfo?.changeTracker));
      }
      finally
      {
         _semaphore.Release();
      }
   }
   #endregion
}
