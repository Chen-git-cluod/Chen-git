namespace Kinlo.Common.Configurations;

public class RoleConfig : ConfigurationBase
{
   public RoleConfig(StyletIoC.IContainer container, bool isStartup)
      : base(container, isStartup) { }

   /// <summary>
   /// 角色列表
   /// </summary>
   public ObservableCollection<RoleModel> Roles { get; set; } = new();
   public ObservableCollection<ControlInfoModel> Menus { get; set; } = new ObservableCollection<ControlInfoModel>();
   public ObservableCollection<ControlInfoModel> DeviceParameters { get; set; } = new ObservableCollection<ControlInfoModel>();
   public ObservableCollection<ControlInfoModel> RunParameters { get; set; } = new ObservableCollection<ControlInfoModel>();
   public ObservableCollection<ControlInfoModel> FunctionEnables { get; set; } = new ObservableCollection<ControlInfoModel>();
   public ObservableCollection<ControlInfoModel> AdvancedConfigs { get; set; } = new ObservableCollection<ControlInfoModel>();

   /// <summary>
   /// 除本地权限外的第三方（如mes）的权限管控
   /// </summary>
   [JsonIgnore]
   public RuntimeInterlockState InterlockState { get; set; } = new RuntimeInterlockState();

   public override void Load()
   {
      try
      {
         var dic = FileHelper.LoadToDictionary(this.GetType().Name);

         if (dic != null && dic.TryGetValue(nameof(Menus), out object menus) && menus != null)
         {
            Menus = JsonSerializer.Deserialize<ObservableCollection<ControlInfoModel>>(menus.ToString());
         }
         if (Menus == null)
            Menus = new ObservableCollection<ControlInfoModel>();

         if (dic != null && dic.TryGetValue(nameof(Roles), out object roles) && roles != null)
         {
            Roles = JsonSerializer.Deserialize<ObservableCollection<RoleModel>>(roles.ToString());
         }
         if (Roles == null)
            Roles = new ObservableCollection<RoleModel>();

         if (!Roles.Any(x => x.Level == (ulong)DefaultRoleEnum.管理员))
         {
            Roles.Add(new RoleModel((ulong)DefaultRoleEnum.管理员, DefaultRoleEnum.管理员.ToString(), 5));
         }
         if (!Roles.Any(x => x.Level == (ulong)DefaultRoleEnum.生产))
         {
            Roles.Add(new RoleModel((ulong)DefaultRoleEnum.生产, DefaultRoleEnum.生产.ToString(), 2));
         }
         if (!Roles.Any(x => x.Level == (ulong)DefaultRoleEnum.工艺))
         {
            Roles.Add(new RoleModel((ulong)DefaultRoleEnum.工艺, DefaultRoleEnum.工艺.ToString(), 3));
         }
         if (!Roles.Any(x => x.Level == (ulong)DefaultRoleEnum.设备))
         {
            Roles.Add(new RoleModel((ulong)DefaultRoleEnum.设备, DefaultRoleEnum.设备.ToString(), 4));
         }
         Roles = new ObservableCollection<RoleModel>(Roles.OrderBy(x => x.Level));
         //   Menus = GetMenus(_dic);

         DeviceParameters = GetControlInfos<DeviceParameterModel>(dic, nameof(DeviceParameters));
         RunParameters = GetControlInfos<RunParameterModel>(dic, nameof(RunParameters));
         FunctionEnables = GetControlInfos<FunctionEnableModel>(dic, nameof(FunctionEnables));
         ;
         AdvancedConfigs = GetControlInfos<AdvancedConfigModel>(dic, nameof(AdvancedConfigs));
      }
      catch (Exception ex)
      {
         $"[初始化权限]异常：{ex};".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   public override void Save(string userName, string revise, bool isPopup = true, bool isPrintLog = true, string saveName = "")
   {
      base.Save(userName, revise, isPopup, isPrintLog, saveName);
   }

   public void AddMenus(List<Type> types)
   {
      List<string> typeNames = new();
      foreach (var type in types)
      {
         var meunAttritube = type.GetCustomAttribute<UIDisplayAttribute>();
         var language = type.GetCustomAttribute<LanguagesAttribute>();
         if (meunAttritube != null && language != null) //加载菜单
         {
            typeNames.Add(type.Name);
            var displayName = language.Languages.Length > 0 ? language.Languages[0] : string.Empty;
            var existingMeun = Menus.FirstOrDefault(x => x.BindingOrKey == type.Name);
            if (existingMeun != null)
            {
               UpdateLevel(existingMeun, meunAttritube, displayName, type);
            }
            else
            {
               var menu = CreateLevel(type.Name, meunAttritube, displayName, type);
               menu.IsSelected = false;
               Menus.Add(menu);
            }
         }
      }

      for (int i = Menus.Count - 1; i >= 0; i--)
      {
         if (!typeNames.Any(x => x == Menus[i].BindingOrKey))
         {
            Menus.RemoveAt(i);
         }
      }
      Menus = Menus.OrderBy(x => x.Index).ToObservableCollection();
   }

   private ObservableCollection<ControlInfoModel> GetControlInfos<T>(Dictionary<string, object> dic, string key)
      where T : class
   {
      var newControlInfos = new ObservableCollection<ControlInfoModel>();

      var oldControlInfos = new ObservableCollection<ControlInfoModel>();
      if (dic != null && dic.TryGetValue(key, out object value) && value != null)
         oldControlInfos = JsonSerializer.Deserialize<ObservableCollection<ControlInfoModel>>(value.ToString());
      if (oldControlInfos == null)
         oldControlInfos = new ObservableCollection<ControlInfoModel>();

      var propertyInfos = typeof(T).GetProperties();
      foreach (var properInfo in propertyInfos)
      {
         var attribute = properInfo.GetCustomAttribute<UIDisplayAttribute>();
         var language = properInfo.GetCustomAttribute<LanguagesAttribute>();

         if (attribute != null && language != null)
         {
            var displayName = language.Languages.Length > 0 ? language.Languages[0] : string.Empty;

            var config = oldControlInfos.FirstOrDefault(x => x.BindingOrKey == properInfo.Name);
            if (config != null)
            {
               UpdateLevel(config, attribute, displayName, properInfo.PropertyType);
            }
            else
            {
               config = CreateLevel(properInfo.Name, attribute, displayName, properInfo.PropertyType);
            }
            newControlInfos.Add(config);
         }
      }
      // newControlInfos = new ObservableCollection<ControlInfoModel>(newControlInfos.OrderBy(x => x.Index));//如需排序
      return newControlInfos;
   }

   private ControlInfoModel CreateLevel(string bindingOrKey, UIDisplayAttribute attribute, string displayName, Type type)
   {
      var visibilitys = Enum.GetValues<ProductionTypeEnum>().Where(x => !attribute.Hiddens.Any(k => k == x));
      var control = new ControlInfoModel();
      control.BindingOrKey = bindingOrKey;
      control.Index = attribute.Index;
      control.DisplayName = displayName;
      control.Icon = attribute.Icon;
      control.Type = type;
      control.EditLevel = attribute.EditLevel;
      control.IsRunEdit = attribute.IsRunEdit;
      control.Margin = attribute.Margin;
      control.ProductVisibility = visibilitys.Count() == 0 ? "无" : string.Join(',', visibilitys);
      return control;
   }

   private void UpdateLevel(ControlInfoModel controlInfo, UIDisplayAttribute attribute, string displayName, Type type)
   {
      var visibilitys = Enum.GetValues<ProductionTypeEnum>().Where(x => !attribute.Hiddens.Any(k => k == x));
      controlInfo.Index = attribute.Index;
      controlInfo.DisplayName = displayName;
      controlInfo.Icon = attribute.Icon;
      controlInfo.Type = type;
      controlInfo.IsRunEdit = attribute.IsRunEdit;
      controlInfo.Margin = attribute.Margin;
      controlInfo.ProductVisibility = visibilitys.Count() == 0 ? "无" : string.Join(',', visibilitys);
   }
}
