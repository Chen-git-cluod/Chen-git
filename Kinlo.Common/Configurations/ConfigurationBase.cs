namespace Kinlo.Common.Configurations;

[AddINotifyPropertyChangedInterface]
public abstract class ConfigurationBase : System.ComponentModel.INotifyPropertyChanged
{
   /// <summary>
   /// 导入表格存放目录
   /// </summary>
   protected string ExternalTablesDirectory { get; set; } = $".\\导入表格存放目录";
   protected IContainer _container;

   public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

   /// <summary>
   ///
   /// </summary>
   /// <param name="container"></param>
   /// <param name="isStartup">启动程序时加载保存的设置，后期生产new时可视情况加载</param>
   protected ConfigurationBase(StyletIoC.IContainer container, bool isStartup)
   {
      _container = container;
      if (isStartup)
         Load();
   }

   public abstract void Load();

   /// <summary>
   ///
   /// </summary>
   /// <param name="userName">用户</param>
   /// <param name="revise">修改内容</param>
   /// <param name="isPopup">是否弹窗</param>
   /// <param name="saveName">保存名称</param>
   public virtual void Save(string userName, string revise, bool isPopup = true, bool isPrintLog = true, string saveName = "")
   {
      FileHelper.FileSave(this, userName, revise, isPopup, isPrintLog, saveName);
   }

   /// <summary>
   /// 通用的 JSON 属性解析映射
   /// </summary>
   protected void MapJsonProperties(string json)
   {
      if (string.IsNullOrWhiteSpace(json))
         return;

      using var doc = JsonDocument.Parse(json);
      var root = doc.RootElement;

      var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);

      foreach (var prop in properties)
      {
         if (root.TryGetProperty(prop.Name, out var element))
         {
            try
            {
               var value = element.Deserialize(prop.PropertyType);
               if (value != null)
               {
                  prop.SetValue(this, value);
               }
            }
            catch (Exception ex)
            {
               $"属性 {prop.Name} 解析失败: {ex.Message}".LogRun(Log4NetLevelEnum.警告);
            }
         }
      }
   }
}
