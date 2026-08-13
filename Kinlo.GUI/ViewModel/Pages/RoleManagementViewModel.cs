using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[Languages(["角色管理", "Manajemen Peran", "Role management"], IsScanProperty = false)]
[UIDisplayAttribute(true, 22, ((ulong)1) << 62, isRunEdit: true, "\xe631")]
public class RoleManagementViewModel : Screen, IMenu
{
   public RoleConfig Role { get; set; }
   public RoleConfig RoleCopy { get; set; }

   public RoleModel SelectRole { get; set; }
   private int _selectIndex = -1;

   public int SelectIndex
   {
      get { return _selectIndex; }
      set
      {
         if (_selectIndex != value)
         {
            _selectIndex = value;
            SelectRole = RoleCopy.Roles[SelectIndex];
            SetRole(RoleCopy.Roles[value]);
         }
      }
   }

   public RoleModel EditRole { get; set; } = new();
   private UsersStatusConfig _usersStatus;
   private IContainer _container;

   public RoleManagementViewModel(IContainer container)
   {
      _container = container;
      _usersStatus = container.Get<UsersStatusConfig>();
      Role = container.Get<RoleConfig>();
      Copy();
   }

   private void SetRole(RoleModel role)
   {
      RoleCopy.Menus.ForEach(x => x.SetSelect(role));
      RoleCopy.RunParameters.ForEach(x => x.SetSelect(role));
      RoleCopy.DeviceParameters.ForEach(x => x.SetSelect(role));
      RoleCopy.FunctionEnables.ForEach(x => x.SetSelect(role));
      RoleCopy.AdvancedConfigs.ForEach(x => x.SetSelect(role));
   }

   public void SelectLevelCMD(ToggleButton sender)
   {
      ControlInfoModel controlInfo = sender.DataContext as ControlInfoModel;
      controlInfo.SetLevel(SelectRole, (bool)sender.IsChecked);
   }

   public void CreateRoleCMD() => EditRole = new();

   public void EditRoleCMD() => ExpressionAssignmentMapper<RoleModel, RoleModel>.Trans(SelectRole, EditRole);

   public void SaveRoleCMD(string val)
   {
      if (string.IsNullOrWhiteSpace(EditRole.Name))
      {
         Growl.Warning($"请填写部门名称！");
         return;
      }
      if (val == "1") //1为修改，0为创建
      {
         if (RoleCopy.Roles.Any(x => x.Name == EditRole.Name && x.Level != EditRole.Level))
         {
            Growl.Warning($"[{EditRole.Name}] 和现有部门重复！");
            return;
         }

         var roleCopy = RoleCopy.Roles.First(x => x.Level == EditRole.Level);
         var role = Role.Roles.First(x => x.Level == EditRole.Level);
         roleCopy.Name = role.Name = EditRole.Name;
         roleCopy.PlcLevel = role.PlcLevel = EditRole.PlcLevel;
         roleCopy.MESLevel = role.MESLevel = EditRole.MESLevel;
      }
      else
      {
         if (RoleCopy.Roles.Count > 60) //最后4个为预留
         {
            Growl.Warning($"部门超出上限60个！");
            return;
         }
         if (RoleCopy.Roles.Any(x => x.Name == EditRole.Name))
         {
            Growl.Warning($"[{EditRole.Name}] 和现有部门重复！");
            return;
         }
         var allRole = RoleCopy.Roles.Where(x => x.Level < ulong.MaxValue >> 3).Select(x => x.Level).Aggregate((acc, num) => acc | num); //除管理员外所有部门的权限
         for (int i = 0; i < 60; i++) //最多60个部门，61 62 为预留
         {
            ulong level = (ulong)1 << i;
            if ((allRole & level) == 0)
            {
               EditRole.Level = level;
               RoleCopy.Roles.Add(EditRole);
               Role.Roles.Add(EditRole);
               RoleCopy.Roles = RoleCopy.Roles.OrderBy(x => x.Level).ToObservableCollection();
               Role.Roles = Role.Roles.OrderBy(x => x.Level).ToObservableCollection();
               break;
            }
         }
      }
      Role.Save(_usersStatus.LocalLoggedinUser.Account, $"{(val == "0" ? "新增部门" : "修改部门")}：{EditRole.Name};");
      var view = (this.View as RoleManagementView);
      if (view != null)
         view.popup.IsOpen = false;
   }

   public void DeleteCMD()
   {
      try
      {
         if (
            SelectRole.Name == DefaultRoleEnum.管理员.ToString()
            || SelectRole.Name == DefaultRoleEnum.工艺.ToString()
            || SelectRole.Name == DefaultRoleEnum.生产.ToString()
            || SelectRole.Name == DefaultRoleEnum.设备.ToString()
         )
         {
            Growl.Warning($"不能删除默认部门！");
            return;
         }
         List<string> userNames = new List<string>();
         foreach (var item in _usersStatus.LocalUsers)
         {
            if (item.Level == SelectRole.Level)
               userNames.Add(item.Name);
         }
         if (userNames.Count > 0)
         {
            Growl.Warning($"不能删除包含员工的部门！\r\n[{SelectRole.Name}] 部门含员工 [{string.Join(',', userNames)}]");
            return;
         }

         if (
            HandyControl.Controls.MessageBox.Show(
               $"确认要删除[{SelectRole.Name}]吗？",
               "提示：",
               MessageBoxButton.OKCancel,
               MessageBoxImage.Warning
            ) == MessageBoxResult.OK
         )
         {
            string name = SelectRole.Name;
            RoleCopy.Roles.Remove(SelectRole);
            Role.Roles.Remove(Role.Roles.First(x => x.Name == name));

            Role.Save(_usersStatus.LocalLoggedinUser.Account, $"删除部门：{name};");
            var view = (this.View as RoleManagementView);
            if (view != null)
               view.popup.IsOpen = false;
         }
      }
      catch (Exception ex)
      {
         $"删除部门出现异常：{ex}".LogSetting(Log4NetLevelEnum.错误);
      }
   }

   public async Task SaveCMD() => await Save(Compare());

   private async Task Save(string stringBuilder)
   {
      if (string.IsNullOrEmpty(stringBuilder))
      {
         Growl.Success($"角色管理未修改！");
         return;
      }
      await Task.Run(async () =>
      {
         await UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            foreach (var item in RoleCopy.Menus)
            {
               var _menu = Role.Menus.FirstOrDefault(x => x.BindingOrKey == item.BindingOrKey);
               if (_menu != null)
                  _menu.EditLevel = item.EditLevel;
            }
            foreach (var item in RoleCopy.DeviceParameters)
            {
               var _info = Role.DeviceParameters.FirstOrDefault(x => x.BindingOrKey == item.BindingOrKey);
               if (_info != null)
                  _info.EditLevel = item.EditLevel;
            }
            foreach (var item in RoleCopy.RunParameters)
            {
               var _info = Role.RunParameters.FirstOrDefault(x => x.BindingOrKey == item.BindingOrKey);
               if (_info != null)
                  _info.EditLevel = item.EditLevel;
            }
            foreach (var item in RoleCopy.FunctionEnables)
            {
               var _info = Role.FunctionEnables.FirstOrDefault(x => x.BindingOrKey == item.BindingOrKey);
               if (_info != null)
                  _info.EditLevel = item.EditLevel;
            }
            foreach (var item in RoleCopy.AdvancedConfigs)
            {
               var _info = Role.AdvancedConfigs.FirstOrDefault(x => x.BindingOrKey == item.BindingOrKey);
               if (_info != null)
                  _info.EditLevel = item.EditLevel;
            }
            foreach (var item in RoleCopy.Roles)
            {
               var _info = Role.Roles.FirstOrDefault(x => x.Level == item.Level);
               if (_info != null)
               {
                  _info.PlcLevel = item.PlcLevel;
                  _info.Name = item.Name;
               }
            }
         });
         Role.Save(_usersStatus.LocalLoggedinUser.Account, stringBuilder.ToString());
         HCC.Growl.Success("保存成功！");
      });
   }

   private string Compare()
   {
      StringBuilder stringBuilder = new StringBuilder();

      for (var i = 0; i < Role.Menus.Count; i++)
      {
         if (RoleCopy.Menus[i].EditLevel != Role.Menus[i].EditLevel)
         {
            stringBuilder.Append($"{RoleCopy.Menus[i].DisplayName}编辑权限：{Role.Menus[i].EditLevel} 改为 {RoleCopy.Menus[i].EditLevel};");
         }
      }
      for (var i = 0; i < Role.DeviceParameters.Count; i++)
      {
         if (RoleCopy.DeviceParameters[i].EditLevel != Role.DeviceParameters[i].EditLevel)
         {
            stringBuilder.Append(
               $"{RoleCopy.DeviceParameters[i].DisplayName}编辑权限：{Role.DeviceParameters[i].EditLevel} 改为 {RoleCopy.DeviceParameters[i].EditLevel};"
            );
         }
      }
      for (var i = 0; i < Role.RunParameters.Count; i++)
      {
         if (RoleCopy.RunParameters[i].EditLevel != Role.RunParameters[i].EditLevel)
         {
            stringBuilder.Append(
               $"{RoleCopy.RunParameters[i].DisplayName}编辑权限：{Role.RunParameters[i].EditLevel} 改为 {RoleCopy.RunParameters[i].EditLevel};"
            );
         }
      }
      for (var i = 0; i < Role.FunctionEnables.Count; i++)
      {
         if (RoleCopy.FunctionEnables[i].EditLevel != Role.FunctionEnables[i].EditLevel)
         {
            stringBuilder.Append(
               $"{RoleCopy.FunctionEnables[i].DisplayName}编辑权限：{Role.FunctionEnables[i].EditLevel} 改为 {RoleCopy.FunctionEnables[i].EditLevel};"
            );
         }
      }
      for (var i = 0; i < Role.AdvancedConfigs.Count; i++)
      {
         if (RoleCopy.AdvancedConfigs[i].EditLevel != Role.AdvancedConfigs[i].EditLevel)
         {
            stringBuilder.Append(
               $"{RoleCopy.AdvancedConfigs[i].DisplayName}编辑权限：{Role.AdvancedConfigs[i].EditLevel} 改为 {RoleCopy.AdvancedConfigs[i].EditLevel};"
            );
         }
      }
      for (var i = 0; i < Role.Roles.Count; i++)
      {
         if (RoleCopy.Roles[i].PlcLevel != Role.Roles[i].PlcLevel)
         {
            stringBuilder.Append($"{RoleCopy.Roles[i].Name}编辑对应PLC权限：{Role.Roles[i].PlcLevel} 改为 {RoleCopy.Roles[i].PlcLevel};");
         }
      }
      return stringBuilder.ToString();
   }

   public void SelectedAllCMD(object datas)
   {
      var _array = (object[])datas;
      var _controlInfos = (ObservableCollection<ControlInfoModel>)_array[0];
      bool _b = (bool)_array[1];
      foreach (var item in _controlInfos)
      {
         if (item.EditLevel >= ((ulong)1 << 62)) //不可选中最高权限
            continue;
         item.SetLevel(SelectRole, _b);
         item.SetSelect(SelectRole);
      }
   }

   #region 查找控件弃用
   public void CheckBox_Checked(object sender, RoutedEventArgs e)
   {
      RoleManagementView _view = (RoleManagementView)this.View;
      if (_view != null)
      {
         CheckBox checkBox = sender as CheckBox;
         bool _isSelect = (bool)checkBox.IsChecked;
         if (checkBox.Name == "MenusOperator")
         {
            FindChild(_view.MenusListBox, _isSelect);
         }
         else if (checkBox.Name == "DeviceParametersOperator")
         {
            FindChild(_view.DeviceParametersListBox, _isSelect);
         }
         else if (checkBox.Name == "RunParametersOperator")
         {
            FindChild(_view.RunParametersListBox, _isSelect);
         }
         else if (checkBox.Name == "FunctionEnablesOperator")
         {
            FindChild(_view.FunctionEnablesListBox, _isSelect);
         }
         else if (checkBox.Name == "AdvancedConfigsOperator")
         {
            FindChild(_view.AdvancedConfigsListBox, _isSelect);
         }
      }
   }

   private void FindChild(DependencyObject obj, bool ischecked)
   {
      var _count = VisualTreeHelper.GetChildrenCount(obj);
      for (int i = 0; i < _count; i++)
      {
         var _child = VisualTreeHelper.GetChild(obj, i);
         if (_child is CheckBox)
         {
            CheckBox _checkBox = (CheckBox)_child;
            var _info = _checkBox.DataContext as ControlInfoModel;
            if (_info == null || _info.EditLevel != ((uint)1 << 31)) //不可选中最高权限
            {
               _checkBox.IsChecked = ischecked;
               _info.SetLevel(SelectRole, (bool)_checkBox.IsChecked);
            }
         }
         else
         {
            FindChild(_child, ischecked);
         }
      }
   }

   #endregion
   public void Copy()
   {
      RoleCopy = new RoleConfig(_container, false);
      foreach (var item in Role.Roles)
      {
         RoleModel _roleModel = new RoleModel();
         ExpressionAssignmentMapper<RoleModel, RoleModel>.Trans(item, _roleModel);
         RoleCopy.Roles.Add(_roleModel);
      }
      foreach (var item in Role.Menus)
      {
         var _controlInfo = new ControlInfoModel();
         ExpressionAssignmentMapper<ControlInfoModel, ControlInfoModel>.Trans(item, _controlInfo);
         RoleCopy.Menus.Add(_controlInfo);
      }
      foreach (var item in Role.DeviceParameters)
      {
         var _controlInfo = new ControlInfoModel();
         ExpressionAssignmentMapper<ControlInfoModel, ControlInfoModel>.Trans(item, _controlInfo);
         RoleCopy.DeviceParameters.Add(_controlInfo);
      }
      foreach (var item in Role.RunParameters)
      {
         var _controlInfo = new ControlInfoModel();
         ExpressionAssignmentMapper<ControlInfoModel, ControlInfoModel>.Trans(item, _controlInfo);
         RoleCopy.RunParameters.Add(_controlInfo);
      }
      foreach (var item in Role.FunctionEnables)
      {
         var _controlInfo = new ControlInfoModel();
         ExpressionAssignmentMapper<ControlInfoModel, ControlInfoModel>.Trans(item, _controlInfo);
         RoleCopy.FunctionEnables.Add(_controlInfo);
      }
      foreach (var item in Role.AdvancedConfigs)
      {
         var _controlInfo = new ControlInfoModel();
         ExpressionAssignmentMapper<ControlInfoModel, ControlInfoModel>.Trans(item, _controlInfo);
         RoleCopy.AdvancedConfigs.Add(_controlInfo);
      }
   }

   public void Load()
   {
      //SetRole(SelectRole);
   }

   public bool Unload()
   {
      var msg = Compare();
      if (!string.IsNullOrEmpty(msg))
      {
         var rs = System.Windows.MessageBox.Show("有修改未保存，是否保存？", "提示", MessageBoxButton.YesNoCancel);
         if (rs == MessageBoxResult.Yes)
         {
            _ = Save(msg);
            return true;
         }
         else if (rs == MessageBoxResult.No)
         {
            UIThreadHelper.InvokeOnUiThreadAsync(() => Copy());
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
