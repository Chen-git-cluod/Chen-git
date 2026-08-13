using System.Windows.Interop;
using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[Languages(["MES参数", "Parameter MES", "MES parameter"], IsScanProperty = false)]
[UIDisplayAttribute(true, 52, (ulong)(DefaultRoleEnum.设备 | DefaultRoleEnum.工艺), isRunEdit: true, "\xe608")]
public class ConfigurationMesParameterViewModel : Screen, IMenu
{
   public MesParameterConfig MesParameterCopy { get; set; }
   public DisplayDataCollection DisplayData { get; set; }
   public RoleConfig Role { get; set; }
   public int TabIndex { get; set; }
   private MesParameterConfig MesParameter;
   private UsersStatusConfig _usersStatus;
   private IContainer _container;

   public ConfigurationMesParameterViewModel(IContainer container)
   {
      _container = container;
      MesParameter = _container.Get<MesParameterConfig>();
      DisplayData = _container.Get<DisplayDataCollection>();
      _usersStatus = _container.Get<UsersStatusConfig>();
      Role = _container.Get<RoleConfig>();
      Init();
   }

   private void Init()
   {
      MesParameterCopy = new MesParameterConfig(_container, false);
      foreach (var item in MesParameter.DeviceStartupParameters)
      {
         MesParameterItemModel _mesParameterItem = new MesParameterItemModel();
         ExpressionAssignmentMapper<MesParameterItemModel, MesParameterItemModel>.Trans(item, _mesParameterItem);
         MesParameterCopy.DeviceStartupParameters.Add(_mesParameterItem);
      }
      foreach (var item in MesParameter.ResultParameters)
      {
         MesParameterItemModel _mesParameterItem = new MesParameterItemModel();
         ExpressionAssignmentMapper<MesParameterItemModel, MesParameterItemModel>.Trans(item, _mesParameterItem);
         MesParameterCopy.ResultParameters.Add(_mesParameterItem);
      }
   }

   public void RemoveResultParamCmd(MesParameterItemModel mesParameterItem) => MesParameterCopy.ResultParameters.Remove(mesParameterItem);

   public void RemoveStartDeviceCmd(MesParameterItemModel mesParameterItem) =>
      MesParameterCopy.DeviceStartupParameters.Remove(mesParameterItem);

   public void AddReslutParamCmd(DisplayPropertyBindingDto processProperty)
   {
      try
      {
         var item = new MesParameterItemModel();
         item.LocalPropertyName = processProperty.BindingPaht;
         item.LanguagerKey = processProperty.Description;
         item.LocalType = processProperty?.PropertyType;
         item.IsSelected = true;
         UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            MesParameterCopy.ResultParameters.Add(item);
         });
      }
      catch (Exception) { }
   }

   public void AddStartDeviceParamCmd(ControlInfoModel processProperty)
   {
      try
      {
         var item = new MesParameterItemModel();
         item.LocalPropertyName = processProperty.BindingOrKey;
         item.LanguagerKey = processProperty.DisplayName;
         item.LocalType = processProperty?.Type;
         item.IsSelected = true;
         UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            MesParameterCopy.DeviceStartupParameters.Add(item);
         });
      }
      catch (Exception ex) { }
   }

   /// <summary>
   /// 替换
   /// </summary>
   /// <param name="mesParameterItem"></param>
   public void ReplaceReslutParamCmd(DisplayPropertyBindingDto mesParameterItem)
   {
      for (int i = 0; i < MesParameterCopy.ResultParameters.Count; i++)
      {
         var item = MesParameterCopy.ResultParameters[i];
         if (item.IsSelected)
         {
            item.LocalPropertyName = mesParameterItem.BindingPaht;
            item.LanguagerKey = mesParameterItem.Description;
            item.LocalType = mesParameterItem?.PropertyType;
            if (MesParameterCopy.ResultParameters.Count > i + 1)
               MesParameterCopy.ResultParameters[i + 1].IsSelected = true;
            return;
         }
      }
   }

   /// <summary>
   /// 替换
   /// </summary>
   /// <param name="mesParameterItem"></param>
   public void ReplaceStartupParamCmd(ControlInfoModel mesParameterItem)
   {
      for (int i = 0; i < MesParameterCopy.DeviceStartupParameters.Count; i++)
      {
         var item = MesParameterCopy.DeviceStartupParameters[i];
         if (item.IsSelected)
         {
            item.LocalPropertyName = mesParameterItem.BindingOrKey;
            item.LanguagerKey = mesParameterItem.DisplayName;
            item.LocalType = mesParameterItem?.Type;
            if (MesParameterCopy.DeviceStartupParameters.Count > i + 1)
               MesParameterCopy.DeviceStartupParameters[i + 1].IsSelected = true;
            return;
         }
      }
   }

   public async Task SaveCMD()
   {
      var res = Compare();
      if (res.state != CompareResult.Modified)
      {
         System.Windows.MessageBox.Show(res.msg, "警告", MessageBoxButton.OK);
         return;
      }
      await Save(res.msg);
   }

   private bool CheckParam()
   {
      if (
         MesParameterCopy.DeviceStartupParameters.Any(x =>
            x.IsEnable && (string.IsNullOrEmpty(x.LocalPropertyName) || string.IsNullOrEmpty(x.MesCode))
         )
      )
      {
         Growl.Warning("[开机参数] 在启用上传送MES时，本地属性、MES编码不能为空！");
         return false;
      }
      if (
         MesParameterCopy.ResultParameters.Any(x =>
            x.IsEnable && (string.IsNullOrEmpty(x.LocalPropertyName) || string.IsNullOrEmpty(x.MesCode))
         )
      )
      {
         Growl.Warning("[结果参数] 在启用上传送MES时，本地属性、MES编码不能为空！");
         return false;
      }
      return true;
   }

   private async Task Save(string contrastMsg)
   {
      await Task.Run(async () =>
      {
         await UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            MesParameter.DeviceStartupParameters.Clear();
            MesParameter.ResultParameters.Clear();
            MesParameterCopy.DeviceStartupParameters = new ObservableCollection<MesParameterItemModel>(
               MesParameterCopy.DeviceStartupParameters.OrderByDescending(x => x.IsEnable)
            );
            MesParameterCopy.ResultParameters = new ObservableCollection<MesParameterItemModel>(
               MesParameterCopy.ResultParameters.OrderByDescending(x => x.IsEnable)
            );

            foreach (var item in MesParameterCopy.DeviceStartupParameters)
            {
               MesParameterItemModel mesParameterItem = new MesParameterItemModel();
               ExpressionAssignmentMapper<MesParameterItemModel, MesParameterItemModel>.Trans(item, mesParameterItem);
               MesParameter.DeviceStartupParameters.Add(mesParameterItem);
            }
            foreach (var item in MesParameterCopy.ResultParameters)
            {
               MesParameterItemModel mesParameterItem = new MesParameterItemModel();
               ExpressionAssignmentMapper<MesParameterItemModel, MesParameterItemModel>.Trans(item, mesParameterItem);
               mesParameterItem.ValueConverter = MesParameter.GetMesValueConverter(mesParameterItem.ConverterName);
               MesParameter.ResultParameters.Add(mesParameterItem);
            }
         });
         MesParameter.Save(_usersStatus.LocalLoggedinUser.Account, contrastMsg);

         return true;
      });
   }

   public void ImportExcelCmd()
   {
      var dialog = new OpenFileDialog
      {
         Title = "请选择文件",
         Filter = "Excel 文件|*.xlsx;*.xls", // 比如 "Excel 文件|*.xlsx;*.xls|所有文件|*.*"
         Multiselect = false, // 是否允许多选
      };

      bool? result = dialog.ShowDialog();
      if (result == true)
      {
         var lists = ExcelHelper.ImproExcel<MesParameterItemModel>(dialog.FileName);
         foreach (var item in lists)
         {
            if (!string.IsNullOrEmpty(item.MesCode))
            {
               if (TabIndex == 0)
               {
                  if (!MesParameterCopy.ResultParameters.Any(x => x.MesCode == item.MesCode))
                  {
                     MesParameterCopy.ResultParameters.Add(item);
                  }
               }
               else
               {
                  if (!MesParameterCopy.DeviceStartupParameters.Any(x => x.MesCode == item.MesCode))
                  {
                     MesParameterCopy.DeviceStartupParameters.Add(item);
                  }
               }
            }
         }
      }
   }

   public void ExportExcelCmd()
   {
      SaveFileDialog dlg = new SaveFileDialog();
      dlg.Filter = "Excel 文件|*.xlsx;*.xls";
      dlg.FileName = DateTime.Now.ToString("文件名_yyyy-MM-dd HH点mm分ss秒");
      if (dlg.ShowDialog() == true)
      {
         var data = TabIndex == 0 ? MesParameterCopy.ResultParameters.ToList() : MesParameterCopy.DeviceStartupParameters.ToList();
         data.ExportExcel(dlg.FileName, true);
      }
   }

   public void ExpandCmd()
   {
      if (TabIndex == 0)
      {
         foreach (var item in DisplayData.AvailableResultParameters)
         {
            item.IsSelected = true;
         }
      }
   }

   public void FoldCmd()
   {
      if (TabIndex == 0)
      {
         foreach (var item in DisplayData.AvailableResultParameters)
         {
            item.IsSelected = false;
         }
      }
   }

   /// <summary>
   /// 相似度阈值
   /// </summary>
   public double Threshold { get; set; } = 0.25;
   public int DisplayCount { get; set; } = 6;

   public void OpenRecommendedCmd()
   {
      if (this.View is ConfigurationMesParameterView view)
      {
         var col = view.ResultParamDataGrid.Columns.FirstOrDefault(x => x.Header is string s && s == "本地推荐属性");
         if (col != null)
            col.Visibility = Visibility.Visible;
      }
      if (TabIndex == 0)
      {
         foreach (var item in MesParameterCopy.ResultParameters)
         {
            var lists = new List<FieldMatchResult>();
            foreach (var locals in DisplayData.AvailableResultParameters)
            {
               var list = FuzzyMatcherUniversal.Match(
                  item.MesName,
                  locals.OriginalClassProperties.Select(x => (x.Description, (object)x.BindingPaht)).ToList(),
                  topN: DisplayCount,
                  threshold: Threshold
               );
               if (list.Count > 0)
                  lists.AddRange(list);
            }
            item.Candidates.Clear();
            item.Candidates.AddRange(
               lists.OrderByDescending(x => x.Score).Select(x => new CandidateItem(x.LocalField, (string)x.Tag)).Take(DisplayCount)
            );
         }
      }
   }

   public void ClearRecommendedCmd()
   {
      if (this.View is ConfigurationMesParameterView view)
      {
         var col = view.ResultParamDataGrid.Columns.FirstOrDefault(x => x.Header is string s && s == "本地推荐属性");
         if (col != null)
            col.Visibility = Visibility.Collapsed;
      }
      if (TabIndex == 0)
      {
         foreach (var item in MesParameterCopy.ResultParameters)
         {
            item.Candidates.Clear();
         }
      }
   }

   public void SelectedCmd(CandidateItem candidate)
   {
      var p = MesParameterCopy.ResultParameters.FirstOrDefault(x => x.IsSelected);
      if (p != null)
      {
         p.LanguagerKey = candidate.Description;
         p.LocalPropertyName = candidate.PropertyName;
      }
   }

   /// <summary>
   /// 一键应用最相似属性
   /// </summary>
   public void UseHighestSimilarityCmd()
   {
      foreach (var item in MesParameterCopy.ResultParameters)
      {
         if (
            string.IsNullOrEmpty(item.LocalPropertyName)
            && item.Candidates.Count > 0
            && !string.IsNullOrEmpty(item.Candidates[0].PropertyName)
         )
         {
            item.LocalPropertyName = item.Candidates[0].PropertyName;
            item.LanguagerKey = item.Candidates[0].Description;
         }
      }
   }

   public void ClearLocalCmd()
   {
      var p = MesParameterCopy.ResultParameters.FirstOrDefault(x => x.IsSelected);
      if (p != null)
      {
         p.LocalPropertyName = p.LanguagerKey = string.Empty;
      }
   }

   #region 对比
   /// <summary>
   /// 对比内容
   /// </summary>
   /// <returns></returns>
   private (CompareResult state, string msg) Compare()
   {
      try
      {
         StringBuilder contrastMsg = new();

         contrastMsg.Append(GetDuplicateMesCode(MesParameterCopy.DeviceStartupParameters));
         contrastMsg.Append(GetDuplicateMesCode(MesParameterCopy.ResultParameters));

         if (contrastMsg.Length > 0)
            return (CompareResult.Error, contrastMsg.ToString());

         contrastMsg.Append(CompareCore(MesParameter.DeviceStartupParameters, MesParameterCopy.DeviceStartupParameters));
         contrastMsg.Append(CompareCore(MesParameter.ResultParameters, MesParameterCopy.ResultParameters));

         return contrastMsg.Length > 0 ? (CompareResult.Modified, contrastMsg.ToString()) : (CompareResult.Unchanged, "未修改内容！");
      }
      catch (Exception ex)
      {
         return (CompareResult.Error, ex.Message.ToString());
      }
   }

   /// <summary>
   /// 检查MesCode重复
   /// </summary>
   /// <param name="items"></param>
   /// <returns></returns>
   public static string GetDuplicateMesCode(IEnumerable<MesParameterItemModel> items)
   {
      var duplicates = items.GroupBy(x => x.MesCode).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

      return duplicates.Any() ? $"MES编号重复: {string.Join(", ", duplicates)}" : "";
   }

   /// <summary>
   /// 对比
   /// </summary>
   /// <param name="originals"></param>
   /// <param name="copys"></param>
   /// <returns></returns>
   private string CompareCore(ObservableCollection<MesParameterItemModel> originals, ObservableCollection<MesParameterItemModel> copys)
   {
      var originalDic = originals.ToDictionary(x => x.MesCode);
      var copyDic = copys.ToDictionary(x => x.MesCode);
      StringBuilder contrastMsg = new();

      var originalKeys = originalDic.Keys.ToHashSet();
      var copyKeys = copyDic.Keys.ToHashSet();

      // 处理删除
      foreach (var code in originalKeys.Except(copyKeys))
         contrastMsg.Append($"删除：MES编号[{code}] (属性:{originalDic[code].LocalPropertyName}); ");

      // 处理新增
      foreach (var code in copyKeys.Except(originalKeys))
         contrastMsg.Append($"新增：MES编号[{code}] (属性:{copyDic[code].LocalPropertyName}); ");

      // 处理修改 (只处理相交的部分)
      foreach (var code in originalKeys.Intersect(copyKeys))
      {
         contrastMsg.Append(originalDic[code].CompareObject(copyDic[code], new Dictionary<string, DifferenceResultDto>()));
      }
      return contrastMsg.ToString();
   }

   #endregion
   public void Load() { }

   public bool Unload()
   {
      try
      {
         var res = Compare();
         if (res.state == CompareResult.Unchanged)
            return true;
         if (res.state == CompareResult.Error)
         {
            System.Windows.MessageBox.Show(res.msg, "警告", MessageBoxButton.OK);
            return false;
         }

         var rs = System.Windows.MessageBox.Show("有修改未保存，是否保存？", "提示", MessageBoxButton.YesNoCancel);
         if (rs == MessageBoxResult.Yes)
         {
            _ = Save(res.msg);
            return true;
         }
         else if (rs == MessageBoxResult.No)
         {
            UIThreadHelper.InvokeOnUiThreadAsync(() => Init());
            return true;
         }
         else
         {
            return false;
         }
      }
      catch (Exception ex)
      {
         $"{ex}".LogRun();
         return false;
      }
   }
}

public enum CompareResult
{
   Unchanged, // 没变化
   Modified, // 有变化
   Error, // 对比过程发生异常或状态不可用
}
