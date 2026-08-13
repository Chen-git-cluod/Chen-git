using HandyControl.Tools.Extension;
using Kinlo.Common.Tools.ExpressionHelpers;
using Kinlo.Common.Tools.ScriptCreation;

namespace Kinlo.Common.DisplayDatas;

[AddINotifyPropertyChangedInterface]
public class DisplayDataCollection : ConfigurationBase
{
   /// <summary>
   /// 各工序UI显示数据
   /// </summary>
   public ObservableCollection<DisplayDataDto> ProcessesDatas { get; set; } = new();

   /// <summary>
   /// 完整电芯UI显示数据
   /// </summary>
   public DisplayDataDto CompleteBatteryDatas { get; set; } = new();

   /// <summary>
   /// 结果参数————待选参数列表
   /// </summary>
   public ObservableCollection<MesResultParamDto> AvailableResultParameters { get; set; } = new();

   public DisplayDataCollection(IContainer container, bool isStartup)
      : base(container, isStartup) { }

   public override void Load()
   {
      try
      {
         var _dic = FileHelper.LoadToDictionary(this.GetType().Name);
         if (_dic != null)
         {
            if (_dic.TryGetValue(nameof(ProcessesDatas), out object value) && value != null)
               ProcessesDatas = JsonSerializer.Deserialize<ObservableCollection<DisplayDataDto>>(value.ToString())!;

            if (_dic.TryGetValue(nameof(CompleteBatteryDatas), out object value1) && value1 != null)
               CompleteBatteryDatas = JsonSerializer.Deserialize<DisplayDataDto>(value1.ToString())!;
         }
      }
      catch (Exception ex)
      {
         $"[加载DisplayDataCollection]异常：{ex}".LogRun(Log4NetLevelEnum.错误);
      }
   }

   public async Task InitAsync()
   {
      InitProcessesType(); //初始化各工序主页显示
      await BuildingDynamic(); //生成动态类
      InitProcessProperty(); //初始化电芯属性
      SyncMesResultParamList(); //同步MES参数列表
      // InitiaStatistics(); //初始化界面显示工序合格率
      CreateControl();

      this.Save("自动保存", "DynamicPropertyConfig自动保存", false);
   }

   #region 初始化主页工序显示
   record BatteryProcessDto(ProcessTypeEnum[] prcessTypes, CommunicationEnum[] commTypes, Type type);

   /// <summary>
   /// 初始化主页工序显示
   /// </summary>
   public void InitProcessesType()
   {
      try
      {
         CompleteBatteryDatas.Processes = ProcessTypeEnum._;
         CompleteBatteryDatas.Description = "完整电芯数据";
         var signalConfig = _container.Get<PLCSignalConfig>();

         var processGroup = signalConfig.PLCInteractAddresses.GroupBy(x => x.ProcessesType).ToList();
         List<DisplayDataDto> displayDatas = new List<DisplayDataDto>();

         #region 聚合所有包含 BatteryDisplayAttribute 特性的类
         var commonTypes = _container.Get<Assembly>("Common").GetTypes();
         List<BatteryProcessDto> displayBatteryInfos = new List<BatteryProcessDto>();
         foreach (var item in commonTypes)
         {
            var attributes = item.GetCustomAttributes<BatteryDisplayAttribute>();
            if (attributes == null || attributes.Count() == 0)
               continue;

            foreach (var att in attributes)
               displayBatteryInfos.Add(new BatteryProcessDto(att.DisplayProcesses, att.DeviceCommunicationType, item));
         }
         #endregion
         for (int g = 0; g < processGroup.Count; g++)
         {
            PLCInteractAddressModel plcInteract = processGroup[g].ToList()[0];
            if (
               !plcInteract.IsEnable
               || plcInteract.ProductionDataType is ProductionDataTypeEnum.主页不显示数据 or ProductionDataTypeEnum.无操作无显示
            )
               continue;

            #region 添加或更新各工序类
            var batteryProcessInfos = displayBatteryInfos.Where(x => x.prcessTypes.Any(y => y == plcInteract.ProcessesType)).ToList();

            var info = batteryProcessInfos.FirstOrDefault(x => x.commTypes.Any(y => y == plcInteract.DeviceCommunicationType)); //精确匹配该工序

            if (info == null)
               info = batteryProcessInfos.FirstOrDefault(x => x.commTypes.Any(y => y == CommunicationEnum.None)); //如果精确无设备，则匹配该工序通用的类

            if (info != null)
            {
               var displayData = ProcessesDatas.FirstOrDefault(x => x.Processes == plcInteract.ProcessesType);
               if (displayData == null)
               {
                  displayData = new DisplayDataDto { Processes = plcInteract.ProcessesType };
                  ProcessesDatas.Add(displayData);
               }
               displayData.Index = plcInteract.ProductionIndex;
               displayData.Description = plcInteract.ProcessesType.ToString();
               displayData.OriginalClass = info.type;
               displayDatas.Add(displayData);
            }
            else
            {
               $"[生成界面工序类型]工序：{plcInteract.ProcessesType}，设备：{plcInteract.DeviceCommunicationType} 未找到指定类；".LogRun(
                  Log4NetLevelEnum.错误,
                  true
               );
            }
            #endregion
         }
         //排序
         var _list = displayDatas.OrderBy(x => x.Index).ToList();
         ProcessesDatas.Clear();
         for (int i = 0; i < _list.Count; i++)
         {
            _list[i].Index = i;
            ProcessesDatas.Add(_list[i]);
         }
      }
      catch (Exception ex)
      {
         $"初始化主页工序显示异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }
   #endregion

   #region 生成动态类
   /// <summary>
   /// 生成动态类
   /// </summary>
   private Task BuildingDynamic()
   {
      if (ProcessesDatas.Count == 0)
         return Task.CompletedTask;
      return Task.Run(() =>
      {
         $"[生成动态类] 开始".LogRun();
         var temp = _container.Get<GlobalStaticTemporary>();
         try
         {
            string headerCode = TypesToCode.BuildHeaderCode();

            List<Type?> runtimeBatteryTypes = [typeof(BatMainModel)];
            var processClassTypes = ProcessesDatas //去重复类
               .Where(x => x.OriginalClass != null)
               .GroupBy(x => x.OriginalClass!.Name)
               .Select(x => x.First().OriginalClass!);
            runtimeBatteryTypes.AddRange(processClassTypes);
            string runtimeBatteryStr = TypesToCode.BuildClassCode(runtimeBatteryTypes, "RuntimeBattery");

            string classCode = headerCode + runtimeBatteryStr;
            if (_container.Get<ParameterConfig>().AdvancedConfig.PrintDynamicClass)
            {
               "\r\n\r\n动态类代码：==================================================================>".LogSetting();
               classCode.LogSetting();
               "动态类代码完成：===================================================================>\r\n".LogSetting();
            }
            var dynamicAssembly = classCode.GenerateMergedClass();
            if (dynamicAssembly == null)
            {
               $"[生成动态类] 生成动态类失败".LogRun(Log4NetLevelEnum.错误, true);
               return;
            }
            ExpressionAssignmentMapper2.ClearAll(); //清除deep copy 委托
            ExpressionDeepSync.ClearAll(); //清除deep copy 委托
            CompleteBatteryDatas.OriginalClass = CompleteBatteryDatas.RuntimeBatteryType = dynamicAssembly.GetCalssType("RuntimeBattery");
            ProcessesDatas.ForEach(pro => pro.RuntimeBatteryType = CompleteBatteryDatas.RuntimeBatteryType);
            $"[生成动态类] 完成".LogRun(Log4NetLevelEnum.成功);
         }
         catch (Exception ex)
         {
            $"[生成动态类]异常： {ex}".LogRun(Log4NetLevelEnum.错误, true);
         }
      });
   }
   #endregion

   #region 初始化工序电芯属性
   /// <summary>
   /// 初始化工序电芯属性
   /// </summary>
   private void InitProcessProperty()
   {
      UpdateProcessProperty(CompleteBatteryDatas);
      ProcessesDatas.ForEach(process => UpdateProcessProperty(process));
   }

   /// <summary>
   /// 更新运行时完整电芯属性
   /// </summary>
   /// <param name="displayData"></param>
   public void UpdateProcessProperty(DisplayDataDto displayData)
   {
      try
      {
         var originalProInfos = displayData.OriginalClass.GetProperties().ToList();
         var propertyInfos = displayData.RuntimeBatteryType.GetProperties().ToList();
         for (int i = 0; i < propertyInfos.Count; i++)
         {
            var propertyInfo = propertyInfos[i];
            bool isContinue = false;
            if (propertyInfo.Name == "Item")
               isContinue = true;

            var attribute = propertyInfo.GetCustomAttribute<BatteryDisplayAttribute>();
            if (attribute != null && attribute.IsIgnore)
               isContinue = true;

            if (isContinue)
            {
               var banding = displayData.PropertyBindings.FirstOrDefault(x => x.BindingPaht == propertyInfo.Name);
               if (banding != null)
                  displayData.PropertyBindings.Remove(banding);
               continue;
            }

            var languageAtt = propertyInfo.GetCustomAttribute<LanguagesAttribute>();
            string language =
               languageAtt != null && languageAtt.Languages.Length > 0 ? languageAtt.Languages[0] : $"bat_{propertyInfo.Name}";

            var displayPropertyBinding = displayData.PropertyBindings.FirstOrDefault(x => x.BindingPaht == propertyInfo.Name);
            if (displayPropertyBinding == null)
            {
               var isVisible =
                  propertyInfo.Name == nameof(BatMainModel.Id)
                  || propertyInfo.Name == nameof(BatMainModel.Barcode)
                  || propertyInfo.Name == nameof(BatMainModel.NgProcesses)
                  || propertyInfo.Name == nameof(BatMainModel.FinalStatus)
                  || originalProInfos.Any(x => x.Name == propertyInfo.Name); //是否显示
               displayData.PropertyBindings.Add(
                  new DisplayPropertyBindingDto
                  {
                     BindingPaht = propertyInfo.Name,
                     PropertyType = propertyInfo.PropertyType,
                     Description = language,
                     IsExport = isVisible,
                     IsVisible = isVisible,
                     Index = i,
                  }
               );
            }
            else
            {
               displayPropertyBinding.PropertyType = propertyInfo.PropertyType;
               displayPropertyBinding.Description = language;
            }
         }

         for (int i = displayData.PropertyBindings.Count - 1; i >= 0; i--)
         {
            var binding = displayData.PropertyBindings[i];

            if (!propertyInfos.Any(x => x.Name == binding.BindingPaht))
            {
               displayData.PropertyBindings.RemoveAt(i);
            }
         }
         displayData.PropertyBindings.SortBy(x => x.Index);
         //  displayData.PropertyBindings = displayData.PropertyBindings.OrderBy(x => x.Index).ToObservableCollection();

         string sortDescription = nameof(BatMainModel.Id);
         foreach (var item in originalProInfos)
         {
            var attribute = item.GetCustomAttribute<OrderMarkerAttribute>();
            if (attribute != null)
            {
               sortDescription = item.Name;
               break;
            }
         }
         displayData.InitView(sortDescription);
      }
      catch (Exception ex)
      {
         $"初始化工序电芯属性异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   #endregion

   #region 同步MES参数列表
   /// <summary>
   /// 同步资源
   /// </summary>
   public void SyncMesResultParamList()
   {
      if (AvailableResultParameters.Count == 0)
         AvailableResultParameters.Add(GetAvailableParameter(0, "公共参数", typeof(BatMainModel)));

      // 移除已删除的工序
      for (int i = AvailableResultParameters.Count - 1; i > 0; i--)
      {
         if (!ProcessesDatas.Any(x => x.Processes.ToString() == AvailableResultParameters[i].Processes))
            AvailableResultParameters.RemoveAt(i);
      }
      List<MesResultParamDto> list = new();
      foreach (var item in ProcessesDatas)
      {
         if (item.OriginalClass == null)
            continue;
         if (!AvailableResultParameters.Any(x => x.Processes == item.Processes.ToString()))
         {
            list.Add(GetAvailableParameter(item.Index + 1, item.Processes.ToString(), item.OriginalClass));
         }
      }
      if (list.Count > 0)
      {
         AvailableResultParameters.AddRange(list);
      }
      AvailableResultParameters = AvailableResultParameters.OrderBy(x => x.Index).ToObservableCollection();

      foreach (var item in _container.Get<MesParameterConfig>().ResultParameters)
      {
         if (CompleteBatteryDatas.PropertyBindings.Any(x => x.BindingPaht == item.LocalPropertyName))
            item.IsExpired = false;
         else
         {
            item.IsExpired = true;
            item.IsEnable = false;
         }
      }
   }

   private MesResultParamDto GetAvailableParameter(int index, string process, Type type) =>
      new MesResultParamDto
      {
         Index = index,
         Processes = process,
         OriginalClass = type,
         IsSelected = index < 3,
         OriginalClassProperties = GetParam(type),
      };

   private ObservableCollection<DisplayPropertyBindingDto> GetParam(Type type)
   {
      ObservableCollection<DisplayPropertyBindingDto> list = new ObservableCollection<DisplayPropertyBindingDto>();
      var originalProInfos = type.GetProperties().ToList();
      for (int i = 0; i < originalProInfos.Count; i++)
      {
         var propertyInfo = originalProInfos[i];
         if (
            propertyInfo.Name
            is "Item"
               or nameof(BatMainModel.Id)
               or nameof(BatMainModel.MesInputStatus)
               or nameof(BatMainModel.MesOutputStatus)
               or nameof(BatMainModel.OutputTime)
         )
            continue;
         var attribute = propertyInfo.GetCustomAttribute<BatteryDisplayAttribute>();
         if (attribute != null && attribute.IsIgnore)
            continue;
         var languageAtt = propertyInfo.GetCustomAttribute<LanguagesAttribute>();
         string language = languageAtt != null && languageAtt.Languages.Length > 0 ? languageAtt.Languages[0] : $"bat_{propertyInfo.Name}";
         var binding = new DisplayPropertyBindingDto
         {
            BindingPaht = propertyInfo.Name,
            PropertyType = propertyInfo.PropertyType,
            Description = language,
            IsExport = true,
            IsVisible = true,
            Index = i,
         };
         list.Add(binding);
      }
      return list;
   }
   #endregion

   #region 创建Control
   /// <summary>
   /// 创建Control
   /// </summary>
   public void CreateControl(bool isDataGrid = false)
   {
      foreach (var item in ProcessesDatas)
      {
         if (isDataGrid)
         {
            var _dataGrid = CreateControlHelper.CreateDataGrid($"{nameof(DisplayDataDto.Datas)}", item.PropertyBindings, false);
            _dataGrid.ColumnDisplayIndexChanged += _dataGrid_ColumnDisplayIndexChanged;
            item.DataDisplayControl = _dataGrid;
         }
         else
         {
            item.DataDisplayControl = CreateControlHelper.CreateListView(
               $"{nameof(DisplayDataDto.DisplayView)}",
               item.PropertyBindings,
               false,
               false
            );
            //  item.DataDisplayControl = CreateControlHelper.CreateListView($"{nameof(DisplayDataDto.Datas)}", item.PropertyBindings, false);
         }
      }
      if (isDataGrid)
      {
         var _dataGrid = CreateControlHelper.CreateDataGrid(
            $"{nameof(DisplayDataDto.Datas)}",
            CompleteBatteryDatas.PropertyBindings,
            false
         );
         _dataGrid.ColumnDisplayIndexChanged += _dataGrid_ColumnDisplayIndexChanged;
         CompleteBatteryDatas.DataDisplayControl = _dataGrid;
      }
      else
      {
         CompleteBatteryDatas.DataDisplayControl = CreateControlHelper.CreateListView(
            $"{nameof(DisplayDataDto.DisplayView)}",
            CompleteBatteryDatas.PropertyBindings,
            false,
            false
         );
      }
   }

   /// <summary>
   /// 保存拖动列位置
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   private void _dataGrid_ColumnDisplayIndexChanged(object? sender, DataGridColumnEventArgs e)
   {
      DataGridTextColumn? dataGridTextColumn = e.Column as DataGridTextColumn;
      if (dataGridTextColumn == null)
         return;

      DataGrid? dataGrid = sender as DataGrid;
      DisplayDataDto? _dsplayData = (DisplayDataDto)dataGrid.DataContext;
      string bindingPaht = ((System.Windows.Data.Binding)dataGridTextColumn.Binding).Path.Path;

      ObservableCollection<DisplayPropertyBindingDto> _propertyBindings = new();
      if (_dsplayData.Processes == ProcessTypeEnum._)
      {
         _propertyBindings = CompleteBatteryDatas.PropertyBindings;
      }
      else
      {
         _propertyBindings = ProcessesDatas.FirstOrDefault(x => x.Processes == _dsplayData.Processes)?.PropertyBindings;
      }
      var _propertyBinding = _propertyBindings.FirstOrDefault(x => x.BindingPaht.Equals(bindingPaht));
      if (_propertyBinding == null)
         return;

      int newIndex = e.Column.DisplayIndex;
      _propertyBinding.Index = newIndex;
      this.Save("自动保存", "DynamicPropertyConfig自动保存", false);
   }
   #endregion

   #region 添加显示数据
   public void AddDisplayData(ProcessTypeEnum processType, ProcessRoleEnum processRole, params IBatMainModel[] batMain)
   {
      var processesDatas = ProcessesDatas.FirstOrDefault(x => x.Processes == processType);
      if (processesDatas != null)
         _ = processesDatas.AddDisplayData(batMain);

      if (processRole == ProcessRoleEnum.出站) //如果当前工序是最后工序，就显示完整电芯
         _ = CompleteBatteryDatas.AddDisplayData(batMain);
   }
   #endregion
}
