namespace Kinlo.Common.Models.ConfigModels;

[AddINotifyPropertyChangedInterface]
public class OtherParameterModel
{
   [JsonIgnore]
   public static string DefaultName = "默认";
   public string CurrentRecipe { get; set; } = string.Empty;

   [JsonIgnore]
   public ObservableCollection<string> Recipes { get; set; } = new();

   [JsonIgnore]
   public readonly Dictionary<string, ParameterConfig> ParameterConfigDic = new Dictionary<string, ParameterConfig>();

   public void LoadRecipes(IContainer container)
   {
      string parameterSavePath = $"{FileHelper.SaveBasePath}\\{FileHelper.ParameterSaveFolder}";
      if (!Directory.Exists(parameterSavePath))
         Directory.CreateDirectory(parameterSavePath);
      var files = Directory.GetFiles(parameterSavePath, "*.json", SearchOption.AllDirectories);

      ParameterConfigDic.Clear();
      if (files.Length > 0)
      {
         Recipes = files.Select(x => Path.GetFileNameWithoutExtension(x)).ToObservableCollection();

         for (int i = Recipes.Count - 1; i >= 0; i--)
         {
            var name = Recipes[i];
            string path = Path.Combine(FileHelper.ParameterSaveFolder, name);
            var json = FileHelper.LoadToString(path);
            ParameterConfig? parameterConfig = null;
            if (!string.IsNullOrEmpty(json))
            {
               JsonElement element;
               parameterConfig = json.ParseJson(root =>
               {
                  ParameterConfig p = new ParameterConfig(container, false);
                  p.SetRecipeName(name);

                  if (root.TryGetProperty(nameof(ParameterConfig.DeviceParameter), out element))
                     p.DeviceParameter = element.Deserialize<DeviceParameterModel>();

                  if (root.TryGetProperty(nameof(ParameterConfig.FunctionEnable), out element))
                     p.FunctionEnable = element.Deserialize<FunctionEnableModel>();

                  if (root.TryGetProperty(nameof(ParameterConfig.RunParameter), out element))
                     p.RunParameter = element.Deserialize<RunParameterModel>();

                  if (root.TryGetProperty(nameof(ParameterConfig.AdvancedConfig), out element))
                     p.AdvancedConfig = element.Deserialize<AdvancedConfigModel>();
                  return p;
               });
            }

            if (parameterConfig != null)
               ParameterConfigDic.Add(name, parameterConfig);
            else
               Recipes.RemoveAt(i);
         }
      }

      if (ParameterConfigDic.Count == 0)
      {
         ParameterConfigDic["默认"] = new ParameterConfig(container, false);
         CurrentRecipe = DefaultName;
      }

      if (!ParameterConfigDic.ContainsKey(CurrentRecipe))
      {
         $"未找到[{CurrentRecipe}配方],使用第一个！".LogRun(Log4NetLevelEnum.错误, true);
         CurrentRecipe = ParameterConfigDic.ElementAt(0).Value.GetRecipeName();
      }
   }

   public ParameterConfig GetParameterConfig() => GetParameterConfig(CurrentRecipe)!;

   public ParameterConfig GetParameterConfig(string name) => ParameterConfigDic[name];
}
