using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel
{
   [AddINotifyPropertyChangedInterface]
   [Languages(["参数配置", "Konfigurasi parameter", "Parameter config"], IsScanProperty = false)]
   [UIDisplayAttribute(true, 5, (ulong)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺), isRunEdit: true, "\xe795")]
   public class ConfigurationParameterViewModel : Screen, IMenu
   {
      IContainer _container;
      public StackPanel DeviceParameterPanel { get; set; }
      public StackPanel RunParameterPanel { get; set; }
      public StackPanel FunctionEnablePanel { get; set; }
      public StackPanel AdvancedConfigPanel { get; set; }
      public ParameterConfig ParameterCopy { get; set; }
      public ParameterConfig Parameter { get; set; }
      public RoleConfig Role { get; set; }
      public UsersStatusConfig UsersStatus { get; set; }
      public OtherParameterConfig OtherParameter { get; set; }
      private MesInterfaceParameterConfig _mesInterfaceParameterConfig;
      private MesService _mesService;
      ChangeTracker<ParameterConfig>? paramCfgTracker = null;
      private string _selectedRecipe;

      public string SelectedRecipe
      {
         get { return _selectedRecipe; }
         set
         {
            if (_selectedRecipe != value)
            {
               paramCfgTracker?.Dispose();
               paramCfgTracker = null;

               _selectedRecipe = value;
               var param = OtherParameter.OtherParameter.GetParameterConfig(value);
               ParameterCopy = new ParameterConfig(_container, false);
               ParameterConfig.Copy(param, ParameterCopy);
               paramCfgTracker = new ChangeTracker<ParameterConfig>(param, ParameterCopy);
            }
         }
      }

      protected override void OnClose()
      {
         base.OnClose();
         //  页面彻底关闭，释放资源
         paramCfgTracker?.Dispose();
         paramCfgTracker = null;
      }

      public ConfigurationParameterViewModel(IContainer container)
      {
         _container = container;
         OtherParameter = _container.Get<OtherParameterConfig>();
         Parameter = _container.Get<ParameterConfig>();
         Role = _container.Get<RoleConfig>();
         UsersStatus = _container.Get<UsersStatusConfig>();
         _mesInterfaceParameterConfig = _container.Get<MesInterfaceParameterConfig>();
         _mesService = _container.Get<MesService>();
         Parameter.GetUIParameter = () => (ParameterCopy, paramCfgTracker);
         Init();
      }

      #region Init
      private void Init()
      {
         DeviceParameterPanel = new StackPanel();
         RunParameterPanel = new StackPanel();
         FunctionEnablePanel = new StackPanel();
         AdvancedConfigPanel = new StackPanel();

         string parameterCopyName = nameof(ParameterCopy);
         string propertyName = nameof(ParameterCopy.DeviceParameter);
         string roleName = nameof(Role);
         string rolePropertyName = nameof(Role.DeviceParameters);
         CreateControl(
            Role.DeviceParameters,
            DeviceParameterPanel,
            $"{parameterCopyName}.{propertyName}",
            $"{roleName}.{rolePropertyName}",
            120
         );

         rolePropertyName = nameof(Role.RunParameters);
         propertyName = nameof(ParameterCopy.RunParameter);
         CreateControl(Role.RunParameters, RunParameterPanel, $"{parameterCopyName}.{propertyName}", $"{roleName}.{rolePropertyName}", 120);

         rolePropertyName = nameof(Role.FunctionEnables);
         propertyName = nameof(ParameterCopy.FunctionEnable);
         CreateControl(
            Role.FunctionEnables,
            FunctionEnablePanel,
            $"{parameterCopyName}.{propertyName}",
            $"{roleName}.{rolePropertyName}",
            120
         );

         rolePropertyName = nameof(Role.AdvancedConfigs);
         propertyName = nameof(ParameterCopy.AdvancedConfig);
         CreateControl(
            Role.AdvancedConfigs,
            AdvancedConfigPanel,
            $"{parameterCopyName}.{propertyName}",
            $"{roleName}.{rolePropertyName}",
            140
         );
      }

      private void CreateControl(
         ObservableCollection<ControlInfoModel> controlInfos,
         StackPanel stackPanel,
         string textBindingBase,
         string levelBindingBase,
         double titleWidth
      )
      {
         for (int i = 0; i < controlInfos.Count; i++)
         {
            string textBinding = $"{textBindingBase}.{controlInfos[i].BindingOrKey}";
            string editLevelBinding = $"{levelBindingBase}[{i}].{nameof(ControlInfoModel.EditLevel)}";
            FrameworkElement _control = null;
            if (controlInfos[i].Type == typeof(bool))
            {
               _control = CreateControlHelper.CreateCheckBox(textBinding, editLevelBinding, controlInfos[i], controlInfos[i].Margin);
            }
            else if (controlInfos[i].Type == typeof(TimeSpan))
            {
               _control = CreateControlHelper.CreateClockPicher(textBinding, editLevelBinding, controlInfos[i]);
            }
            else if (controlInfos[i].Type.BaseType == typeof(Enum))
            {
               _control = CreateControlHelper.CreateComboBox(
                  textBinding,
                  editLevelBinding,
                  controlInfos[i],
                  controlInfos[i].Type,
                  titleWidth,
                  controlInfos[i].Margin
               );
            }
            //else if (controlInfos[i].Type == typeof(List<PlcStatusColorModel>))
            //{
            //     _control = CreateControlHelper.CreatePlcStatusColorControl(textBinding, controlInfos[i], controlInfos[i].Margin);
            //}
            else
            {
               _control = CreateControlHelper.CreateTextBox(
                  textBinding,
                  editLevelBinding,
                  controlInfos[i],
                  titleWidth,
                  controlInfos[i].Margin
               );
            }
            if (_control != null)
               stackPanel.Children.Add(_control);
         }
      }
      #endregion

      public void ReplaceCmd()
      {
         if (Parameter.RecipeName == ParameterCopy.RecipeName)
         {
            Growl.Warning("当前使用配方和所选配方相同，无需应用！");
            return;
         }
         if (
            HandyControl.Controls.MessageBox.Show(
               "注意：一般在电池换型时才切换配方，请确保您已经清楚切换配方的风险及作用！确定要应用当前所选配方？",
               "警告",
               System.Windows.MessageBoxButton.YesNo,
               System.Windows.MessageBoxImage.Warning
            ) == System.Windows.MessageBoxResult.Yes
         )
         {
            if (paramCfgTracker.HasChanges)
            {
               if (!Save())
                  return;

               paramCfgTracker.ClearChanges();
            }
            string oldRecipeName = OtherParameter.OtherParameter.CurrentRecipe;
            OtherParameter.OtherParameter.CurrentRecipe = ParameterCopy.GetRecipeName();
            OtherParameter.Save(
               UsersStatus.LocalLoggedinUser.Account,
               $"配方从[{oldRecipeName}]切换为[{OtherParameter.OtherParameter.CurrentRecipe}]"
            );
            Parameter.Load();
         }
      }

      public string NewRecipeName { get; set; } = string.Empty;

      public void CreateRecipeCMD()
      {
         if (string.IsNullOrEmpty(NewRecipeName))
         {
            Growl.Warning("请输入配方名！");
            return;
         }
         if (OtherParameter.OtherParameter.Recipes.Any(x => x == NewRecipeName))
         {
            Growl.Warning("配方名和现有配方名重复，请重新输入！");
            return;
         }
         OtherParameter.OtherParameter.Recipes.Add(NewRecipeName);
         ParameterConfig parameterConfig = new ParameterConfig(_container, false);
         ParameterConfig.Copy(ParameterCopy, parameterConfig);
         parameterConfig.SetRecipeName(NewRecipeName);
         OtherParameter.OtherParameter.ParameterConfigDic.Add(parameterConfig.RecipeName, parameterConfig);
         SelectedRecipe = NewRecipeName;
         NewRecipeName = string.Empty;
         parameterConfig.Save(UsersStatus.LocalLoggedinUser.Account, $"新建参数配方 [{SelectedRecipe}]");
      }

      public void DeleteCMD()
      {
         if (SelectedRecipe == Parameter.RecipeName)
         {
            Growl.Warning("不能删除正在使用的配方！");
            return;
         }
         var deleteRecipe = SelectedRecipe;
         SelectedRecipe = Parameter.RecipeName;

         string parameterSavePath = $"{FileHelper.SaveBasePath}\\{FileHelper.ParameterSaveFolder}\\{deleteRecipe}.json";
         if (File.Exists(parameterSavePath))
            File.Delete(parameterSavePath);
         OtherParameter.OtherParameter.Recipes.Remove(deleteRecipe);
         OtherParameter.OtherParameter.ParameterConfigDic.Remove(deleteRecipe);
         OtherParameter.Save(UsersStatus.LocalLoggedinUser.Account, $"删除参数配方 [{deleteRecipe}]");
      }

      public async Task SaveCMD()
      {
         if (paramCfgTracker.HasChanges)
         {
            if (!Save())
               return;
            _ = MesSendParamHelper.LocalChangeParamNotifyMes(
               paramCfgTracker.GetChanges,
               _mesInterfaceParameterConfig,
               _container.Get<RemoteLocalParamSyncService>(),
               _mesService,
               ParameterCopy.AdvancedConfig.ProductionType,
               "本地参数修改上报MES"
            );
            paramCfgTracker.ClearChanges();
         }
         else
         {
            Growl.Success($"文件未修改！");
         }
      }

      public bool Save()
      {
         if (!ParmeterValidator(ParameterCopy))
            return false;

         var chages = paramCfgTracker.GetChanges;
         var msg = string.Join(',', chages.Values.Select(x => x.ToString()));
         var original = OtherParameter.OtherParameter.GetParameterConfig(SelectedRecipe);
         ParameterConfig.Copy(ParameterCopy, original);
         original.Save(UsersStatus.LocalLoggedinUser.Account, msg);
         if (Parameter.RecipeName == original.RecipeName)
            Parameter.Load();

         return true;
      }

      /// <summary>
      /// 保存前验证
      /// </summary>
      /// <param name="parameter"></param>
      /// <returns></returns>
      private bool ParmeterValidator(ParameterConfig parameter)
      {
         StringBuilder sb = new StringBuilder();
         if (parameter.FunctionEnable.IsRewrokMode)
         {
            if (parameter.FunctionEnable.IsEnableVariableInjection)
               sb.Append($"[启用复投] 但未启用 [变量注液]；");
            if (parameter.FunctionEnable.IsEnableReuseOldTest)
               sb.Append($"[启用复投] 但未启用 [复用旧短路数据]；");
         }
         if (sb.Length > 0)
         {
            var res = HandyControl.Controls.MessageBox.Show(
               sb.ToString(),
               "警告：请确认以下信息是有意为之，否则请取消重新设置！",
               MessageBoxButton.OKCancel,
               MessageBoxImage.Warning
            );

            if (res == MessageBoxResult.OK)
               return true;

            return false;
         }
         else
         {
            return true;
         }
      }

      public void Load()
      {
         _ = UIThreadHelper.InvokeOnUiThreadAsync(() => Task.Run(() => SelectedRecipe = OtherParameter.OtherParameter.CurrentRecipe));
      }

      public bool Unload()
      {
         if (paramCfgTracker.HasChanges)
         {
            var rs = System.Windows.MessageBox.Show("有修改未保存，是否保存？", "提示", MessageBoxButton.YesNoCancel);
            if (rs == MessageBoxResult.Yes)
            {
               if (!Save())
                  return false;

               paramCfgTracker.ClearChanges();
               return true;
            }
            else if (rs == MessageBoxResult.No)
            {
               var original = OtherParameter.OtherParameter.GetParameterConfig(SelectedRecipe);
               ParameterConfig.Copy(original, ParameterCopy);
               paramCfgTracker.ClearChanges();
               return true;
            }
            else
            {
               return false;
            }
         }
         return true;
      }
   }
}
