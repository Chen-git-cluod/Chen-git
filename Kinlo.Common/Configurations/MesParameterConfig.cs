using System.ComponentModel;

namespace Kinlo.Common.Configurations;

/// <summary>
/// MES参数
/// </summary>
[Languages]
public class MesParameterConfig : ConfigurationBase
{
   public MesParameterConfig(StyletIoC.IContainer container, bool isStartup)
      : base(container, isStartup) { }

   /// <summary>
   /// 开机参数
   /// </summary>
   [Languages("开机参数")]
   public ObservableCollection<MesParameterItemModel> DeviceStartupParameters { get; set; } = new();

   /// <summary>
   /// 结果参数————已选参数列表
   /// </summary
   [Languages("结果参数")]
   public ObservableCollection<MesParameterItemModel> ResultParameters { get; set; } = new();

   [JsonIgnore]
   public static List<string> MesValueConverterNames { get; set; } = new();

   [JsonIgnore]
   private Dictionary<string, IMesValueConverter> _mesDic = new();

   public override void Load()
   {
      try
      {
         MesConverterInit();
         var dic = FileHelper.LoadToDictionary(this.GetType().Name);
         if (dic != null)
         {
            if (dic.TryGetValue(nameof(DeviceStartupParameters), out object value) && value != null)
            {
               var deviceStartupParameters = JsonSerializer.Deserialize<ObservableCollection<MesParameterItemModel>>(value.ToString()!)!;
               if (deviceStartupParameters != null)
               {
                  var group = deviceStartupParameters.GroupBy(x => x.MesCode); //去重
                  foreach (var item in group)
                  {
                     var resultParam = item.Last();
                     DeviceStartupParameters.Add(resultParam);
                  }
                  DeviceStartupParameters = DeviceStartupParameters.OrderBy(x => x.MesCode).ToObservableCollection();
               }
            }

            if (dic.TryGetValue(nameof(ResultParameters), out object value1) && value1 != null)
            {
               var resultParameters = JsonSerializer.Deserialize<ObservableCollection<MesParameterItemModel>>(value1.ToString()!)!;
               if (resultParameters != null)
               {
                  var group = resultParameters.GroupBy(x => x.MesCode); //去重
                  foreach (var item in group)
                  {
                     var resultParam = item.Last();
                     resultParam.ValueConverter = GetMesValueConverter(resultParam.ConverterName);
                     ResultParameters.Add(resultParam);
                  }
                  ResultParameters = ResultParameters.OrderBy(x => x.MesCode).ToObservableCollection();
               }
            }
         }
      }
      catch (Exception ex)
      {
         $"[初始化MesParameterConfig]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   /// <summary>
   /// 获取mes转换器
   /// </summary>
   /// <param name="name"></param>
   /// <returns></returns>
   public IMesValueConverter? GetMesValueConverter(string name)
   {
      if (!string.IsNullOrEmpty(name) && _mesDic.TryGetValue(name, out var converter))
      {
         return converter;
      }
      return null;
   }

   private void MesConverterInit()
   {
      MesValueConverterNames.Clear();
      _mesDic.Clear();
      var currentAssembly = _container.Get<Assembly>("MESDocking");
      // 获取当前程序集所有实现 IMesConverter 的类
      var converters = currentAssembly
         .GetTypes()
         .Where(t => t.IsClass && !t.IsAbstract && typeof(IMesValueConverter).IsAssignableFrom(t))
         .ToList();

      foreach (var converterType in converters)
      {
         var att = converterType.GetCustomAttribute<DescriptionAttribute>();
         if (att == null || string.IsNullOrEmpty(att.Description))
            continue;
         IMesValueConverter? converterInstance = null;
         try
         {
            converterInstance = Activator.CreateInstance(converterType) as IMesValueConverter;
         }
         catch (Exception ex)
         {
            $"创建转换器实例失败: {ex}".LogRun(Log4NetLevelEnum.警告, true);
            continue;
         }
         if (converterInstance == null)
            continue;
         if (_mesDic.ContainsKey(att.Description))
         {
            $"[初始化MesParameterConfig] MES值转换器 [{att.Description}] 重复，将覆盖旧转换器；".LogRun(Log4NetLevelEnum.错误, true);
         }
         else
         {
            MesValueConverterNames.Add(att.Description);
         }
         _mesDic[att.Description] = converterInstance;
      }
   }
}
