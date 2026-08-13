namespace Kinlo.Common.Configurations;

public class AppGlobalConfig : ConfigurationBase
{
   public AppGlobalConfig(IContainer container, bool isStartup)
      : base(container, isStartup) { }

   /// <summary>
   /// 班次切换详情
   /// </summary>
   public ShiftSwitchInfoModel ShiftSwitchInfo { get; set; } = new();

   public override void Load()
   {
      try
      {
         var json = FileHelper.LoadToString(this.GetType().Name);
         MapJsonProperties(json);
      }
      catch (Exception ex)
      {
         $"加载配置文件 {this.GetType().Name} 异常: {ex.Message}".LogRun(LogNet.Enums.Log4NetLevelEnum.错误);
      }
   }
}
