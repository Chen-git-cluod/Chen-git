namespace Kinlo.GUI.ViewModel;

/// <summary>
/// 点检
/// </summary>
[UIDisplay(IsSingleton = true)]
public class InspectionViewModel : Screen
{
   public InspectionConfig InspectionParameter { get; set; }
   private UsersStatusConfig _userStatusConfig;
   private ParameterConfig _parameterConfig;

   public InspectionViewModel(IContainer container)
   {
      InspectionParameter = container.Get<InspectionConfig>();
      _userStatusConfig = container.Get<UsersStatusConfig>();
      _parameterConfig = container.Get<ParameterConfig>();
   }

   protected override void OnViewLoaded()
   {
      InspectionParameter.AfterScanBarcodeParameter.Visible = Visibility.Collapsed;
      InspectionParameter.BeforeScanBarcodeParameter.Visible = Visibility.Visible;
      InspectionParameter.BeforeWeighParameter.Visible = _parameterConfig.AdvancedConfig.ProductionType
         is ProductionTypeEnum.回氦
            or ProductionTypeEnum.清洗机
         ? Visibility.Collapsed
         : Visibility.Visible;
      InspectionParameter.AfterWeighParameter.Visible = _parameterConfig.AdvancedConfig.ProductionType
         is ProductionTypeEnum.回氦
            or ProductionTypeEnum.清洗机
         ? Visibility.Collapsed
         : Visibility.Visible;
      InspectionParameter.ShortCircuitParameter.Visible =
         _parameterConfig.AdvancedConfig.ProductionType is ProductionTypeEnum.一次注液 ? Visibility.Visible : Visibility.Collapsed;
      InspectionParameter.TestVoltageParameter.Visible =
         _parameterConfig.AdvancedConfig.ProductionType is ProductionTypeEnum.二次注液 ? Visibility.Visible : Visibility.Collapsed;

      base.OnViewLoaded();
   }

   protected override void OnClose()
   {
      InspectionParameter.Save(_userStatusConfig.LocalLoggedinUser.Name, "点检自动保存！");
      base.OnClose();
   }

   public void SelectItem(ProcessTypeEnum process)
   {
      if (View is not InspectionView view)
         return;
      foreach (var item in view.tabControl.Items)
      {
         if (item is not TabItem tabItem || tabItem.DataContext is not InspectionItem inspection)
            break;

         if (inspection.Process == process)
         {
            tabItem.IsSelected = true;
            return;
         }
      }
   }

   public void SaveCmd(TabControl tabControl)
   {
      if (tabControl.SelectedItem is TabItem item && item.DataContext is InspectionItem inspection)
      {
         if (inspection.SetLaneCount < 1)
         {
            HandyControl.Controls.Growl.Warning("通道数量不能低于1！");
            return;
         }
         if (inspection.Lanes.Count > inspection.SetLaneCount)
         {
            inspection.Lanes.RemoveAtRange(inspection.SetLaneCount - 1, inspection.Lanes.Count - inspection.SetLaneCount);
         }
         else if (inspection.Lanes.Count < inspection.SetLaneCount)
         {
            if (!inspection.Lanes.Any())
            {
               for (int i = 0; i < inspection.SetLaneCount; i++)
               {
                  inspection.Lanes.Add(new InspectionLaneModel());
               }
            }
            else
            {
               int index = inspection.Lanes.Count - 1;
               int count = inspection.SetLaneCount - inspection.Lanes.Count;
               for (int i = 0; i < count; i++)
               {
                  var newItem = ExpressionCopyMapper<InspectionLaneModel, InspectionLaneModel>.Trans(inspection.Lanes[index]);
                  inspection.Lanes.Add(newItem);
               }
            }
         }
         for (int i = 0; i < inspection.Lanes.Count; i++)
         {
            inspection.Lanes[i].Index = i + 1;
         }
         InspectionParameter.Save(_userStatusConfig.LocalLoggedinUser.Name, "修改点检");
      }
   }
}
