using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[Languages(["信号配置", "Konfigurasi sinyal", "Signal Config"], IsScanProperty = false)]
[UIDisplayAttribute(true, 31, ((ulong)1) << 62, isRunEdit: true, "\xe649")]
public class ConfigurationInteractionSignalViewModel : Screen, IMenu
{
   IContainer _container;
   IWindowManager _windowManager;
   UsersStatusConfig _usersStatus;

   // DisplayDataCollection _displayDataCollection;
   public PLCSignalConfig PLCSignal { get; set; }
   public PLCScanSignalModel SelectedPLCScanSignal { get; set; } = new();

   public ConfigurationInteractionSignalViewModel(IContainer container, IWindowManager windowManager)
   {
      _container = container;
      _windowManager = windowManager;
      PLCSignal = container.Get<PLCSignalConfig>();
      _usersStatus = container.Get<UsersStatusConfig>();
      //_displayDataCollection = container.Get<DisplayDataCollection>();
   }

   #region 扫描信号
   /// <summary>
   /// 新建扫描信号
   /// </summary>
   public void AddScanSignalCMD()
   {
      PLCScanSignalViewModel _plcScanSignalVM = _container.Get<PLCScanSignalViewModel>();
      _plcScanSignalVM.SetPLCScanSignal(new PLCScanSignalModel(), 1);
      _windowManager.ShowDialog(_plcScanSignalVM);
   }

   public void ScanSignalCMD_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
   {
      EditScanSignalCMD(sender as ListView);
   }

   /// <summary>
   /// 编辑扫描信号
   /// </summary>
   /// <param name="listView"></param>
   public void EditScanSignalCMD(ListView listView)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      PLCScanSignalViewModel _plcScanSignalVM = _container.Get<PLCScanSignalViewModel>();
      _plcScanSignalVM.SetPLCScanSignal(listView.SelectedItems[0] as PLCScanSignalModel, 2);
      _windowManager.ShowDialog(_plcScanSignalVM);
   }

   /// <summary>
   /// 删除扫描信号
   /// </summary>
   /// <param name="listView"></param>
   public void DeleteSelectScanSignalCMD(ListView listView)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      List<string> _names = new List<string>();
      foreach (var item in listView.SelectedItems)
      {
         _names.Add($"[{(item as PLCScanSignalModel).ServiceName}]");
      }
      if (
         HandyControl.Controls.MessageBox.Show($"确定要删除 {string.Join(',', _names)} 吗?", "删除警告！", MessageBoxButton.OKCancel)
         != MessageBoxResult.OK
      )
         return;

      for (int i = listView.SelectedItems.Count - 1; i > -1; i--)
      {
         var _scan = PLCSignal.PLCScanSignals.FirstOrDefault(x =>
            x.ServiceName == (listView.SelectedItems[i] as PLCScanSignalModel).ServiceName
         );
         PLCSignal.PLCScanSignals.Remove(_scan);
      }
      PLCSignal.Save(_usersStatus.LocalLoggedinUser.Account, $"删除服务 {string.Join(',', _names, false)} ;");
      Growl.Success("删除成功！");
   }

   public void InputStartListCMD(ListView listView)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("需选择一个服务,请重新选择！");
         return;
      }
      PLCScanSignalViewModel _plcScanSignalVM = _container.Get<PLCScanSignalViewModel>();
      _plcScanSignalVM.SetPLCScanSignal(listView.SelectedItems[0] as PLCScanSignalModel, 2);
      if (_plcScanSignalVM.InputStartTagsFun(_plcScanSignalVM.PLCScanSignalCopy.StartSignas, true).GetAwaiter().GetResult())
      {
         _windowManager.ShowDialog(_plcScanSignalVM);
      }
   }

   public void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      ListView listView = sender as ListView;
      if (listView.SelectedItems.Count < 1)
      {
         return;
      }
      PLCScanSignalModel _plcScanSignal = listView.SelectedItems[0] as PLCScanSignalModel;
      SelectedPLCScanSignal = _plcScanSignal;
   }
   #endregion

   #region 数据交互信号

   /// <summary>
   /// 启用
   /// </summary>
   /// <param name="listView"></param>
   public void EnableCmd(ListView listView) => ChangeState(listView, true);

   /// <summary>
   /// 禁用
   /// </summary>
   /// <param name="listView"></param>
   public void DisableCmd(ListView listView) => ChangeState(listView, false);

   private void ChangeState(ListView listView, bool state)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      StringBuilder stringBuilder = new StringBuilder();
      foreach (var item in listView.SelectedItems)
      {
         if (item is PLCInteractAddressModel plcInter)
         {
            if (plcInter.IsEnable != state)
            {
               plcInter.IsEnable = state;
               stringBuilder.Append($"工序：[{plcInter.ProcessesType}],启动索引：[{plcInter.StartCommand.Index}]  ");
            }
         }
      }
      PLCSignal.Save(_usersStatus.LocalLoggedinUser.Account, $"{(state ? "启用" : "禁用")} {stringBuilder} ;");
      _ = _container.Get<MainViewModel>().SyncResourcesAfterConfigUpdate();
   }

   /// <summary>
   /// 新建数据交互信号
   /// </summary>
   public void AddInteractCMD()
   {
      var _interact = PLCSignal.PLCInteractAddresses.OrderBy(x => x.ProductionIndex).LastOrDefault();
      var _interactAddress = new PLCInteractAddressModel() { ProductionIndex = _interact == null ? 1 : _interact.ProductionIndex + 1 };
      PLCInteractAddressViewModel _plcInteractAddressVM = _container.Get<PLCInteractAddressViewModel>();
      _plcInteractAddressVM.SetPLCInteractAddress(_interactAddress, 1);
      _windowManager.ShowDialog(_plcInteractAddressVM);
   }

   public void InteractCMD_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
   {
      EditInteractCMD(sender as ListView);
   }

   public void Custom_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
   {
      EditCustomInteractCmd(sender as ListView);
   }

   /// <summary>
   /// 复制数据交互信号
   /// </summary>
   public void CopyInteractCMD(ListView listView)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }

      PLCInteractAddressModel _plcInteractAddressCopy = new PLCInteractAddressModel();
      PLCInteractAddressModel _plcInteractAddress = listView.SelectedItems[0] as PLCInteractAddressModel;

      ExpressionAssignmentMapper<PLCInteractAddressModel, PLCInteractAddressModel>.Trans(_plcInteractAddress, _plcInteractAddressCopy);
      ExpressionAssignmentMapper<GenericCommandModel, GenericCommandModel>.Trans(
         _plcInteractAddress.StartCommand,
         _plcInteractAddressCopy.StartCommand
      );
      ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
         _plcInteractAddress.StartCommand.Tag,
         _plcInteractAddressCopy.StartCommand.Tag
      );
      ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
         _plcInteractAddress.DataAddress,
         _plcInteractAddressCopy.DataAddress
      );
      ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
         _plcInteractAddress.ExtraDataAddress,
         _plcInteractAddressCopy.ExtraDataAddress
      );

      PLCInteractAddressViewModel _plcInteractAddressVM = _container.Get<PLCInteractAddressViewModel>();
      _plcInteractAddressVM.SetPLCInteractAddress(_plcInteractAddressCopy, 1);
      _windowManager.ShowDialog(_plcInteractAddressVM);
   }

   public int AggregateValue { get; set; }

   /// <summary>
   /// 复制并累加数据交互信号
   /// </summary>
   public void AggregateCMD(ListView listView)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("需选择一行,请重新选择！");
         return;
      }
      if (AggregateValue < 1)
      {
         return;
      }

      PLCInteractAddressModel _plcInteractAddress = listView.SelectedItems[0] as PLCInteractAddressModel;
      List<string> strings = new List<string>();
      for (int i = 1; i < AggregateValue + 1; i++)
      {
         PLCInteractAddressModel _plcInteractAddressCopy = new PLCInteractAddressModel();
         ExpressionAssignmentMapper<PLCInteractAddressModel, PLCInteractAddressModel>.Trans(_plcInteractAddress, _plcInteractAddressCopy);
         ExpressionAssignmentMapper<GenericCommandModel, GenericCommandModel>.Trans(
            _plcInteractAddress.StartCommand,
            _plcInteractAddressCopy.StartCommand
         );
         ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
            _plcInteractAddress.StartCommand.Tag,
            _plcInteractAddressCopy.StartCommand.Tag
         );
         ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
            _plcInteractAddress.DataAddress,
            _plcInteractAddressCopy.DataAddress
         );
         ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
            _plcInteractAddress.ExtraDataAddress,
            _plcInteractAddressCopy.ExtraDataAddress
         );
         _plcInteractAddressCopy.StartCommand.Tag = AggregateTag(_plcInteractAddressCopy.StartCommand.Tag, i);
         _plcInteractAddressCopy.DataAddress = AggregateTag(_plcInteractAddressCopy.DataAddress, i);
         _plcInteractAddressCopy.ExtraDataAddress = AggregateTag(_plcInteractAddressCopy.ExtraDataAddress, i);
         _plcInteractAddressCopy.StartCommand.Index += i;
         _plcInteractAddressCopy.DeviceStartIndex += (byte)(_plcInteractAddressCopy.DataLength * i);
         PLCSignal.PLCInteractAddresses.Add(_plcInteractAddressCopy);
         strings.Add(_plcInteractAddressCopy.StartCommand.Tag.Lable);
      }

      PLCSignal.Save(_usersStatus.LocalLoggedinUser.Account, $"复制并累加服务 {string.Join(',', strings)};");
      _ = _container.Get<MainViewModel>().SyncResourcesAfterConfigUpdate();
   }

   /// <summary>
   /// 累加
   /// </summary>
   /// <param name="address"></param>
   /// <param name="accrualValue"></param>
   /// <returns></returns>
   SignalAddressModel AggregateTag(SignalAddressModel address, int accrualValue)
   {
      if (address == null || string.IsNullOrWhiteSpace(address.Lable))
         return new SignalAddressModel("");

      if (int.TryParse(address.Lable, out int add))
      {
         add += accrualValue;
         return new SignalAddressModel(add.ToString(), add);
      }
      else
      {
         var _index = address.Lable.LastIndexOf('[');
         var _index2 = address.Lable.LastIndexOf(']');
         if (_index > 0 && _index2 > 0)
         {
            var _oldNumber = address.Lable.Substring(_index + 1, _index2 - _index - 1);
            int.TryParse(_oldNumber, out int newNumber);
            string _newTag = address.Lable.Substring(0, _index);
            return new SignalAddressModel($"{_newTag}[{newNumber + accrualValue}]", 0);
         }
      }

      return new SignalAddressModel("");
      ;
   }

   /// <summary>
   /// 编辑数据交互信号
   /// </summary>
   /// <param name="listView"></param>
   public void EditInteractCMD(ListView listView)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      PLCInteractAddressViewModel _plcInteractAddressVM = _container.Get<PLCInteractAddressViewModel>();
      _plcInteractAddressVM.SetPLCInteractAddress(listView.SelectedItems[0] as PLCInteractAddressModel, 2);
      _windowManager.ShowDialog(_plcInteractAddressVM);
   }

   /// <summary>
   /// 删除数据交互信号
   /// </summary>
   /// <param name="listView"></param>
   public void DeleteSelectInteractCMD(ListView listView)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      StringBuilder stringBuilder = new StringBuilder();
      foreach (var item in listView.SelectedItems)
      {
         var plcInter = (PLCInteractAddressModel)item;
         stringBuilder.Append($"工序：[{plcInter.ProcessesType}],启动索引：[{plcInter.StartCommand.Index}]");
      }
      if (
         HandyControl.Controls.MessageBox.Show($"确定要删除 {stringBuilder} 吗?", "删除警告！", MessageBoxButton.OKCancel)
         != MessageBoxResult.OK
      )
         return;

      for (int i = listView.SelectedItems.Count - 1; i > -1; i--)
      {
         var plcInter = (PLCInteractAddressModel)listView.SelectedItems[i];
         var _interact = PLCSignal.PLCInteractAddresses.FirstOrDefault(x => x.StartCommand.Index == plcInter.StartCommand.Index);
         PLCSignal.PLCInteractAddresses.Remove(_interact);
      }
      PLCSignal.Save(_usersStatus.LocalLoggedinUser.Account, $"删除服务 {stringBuilder} ;");
      _container.Get<MainViewModel>().SyncResourcesAfterConfigUpdate();
      Growl.Success("删除成功！");
   }

   public void PlcCommandTestCmd()
   {
      PlcTestView _plcCommandTestView = new PlcTestView(_container, PLCSignal.PLCInteractAddresses);
      _plcCommandTestView.Show();
   }

   #endregion

   #region 切除信号
   ///// <summary>
   ///// 新建切除信号
   ///// </summary>
   //public void AddResectionSignalCMD()
   //{
   //    PLCResectionDialog _pLCResectionDialog = new PLCResectionDialog();
   //    _pLCResectionDialog.DataContext = new PLCResectionDialogModel(_container, new PLCResectionModel(), 1, _pLCResectionDialog);
   //    var _result = _pLCResectionDialog.ShowDialog();
   //}

   //public void EditResectionSignalCMD_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
   //{
   //    EditResectionSignalCMD(sender as ListView);
   //}

   ///// <summary>
   ///// 编辑切除信号
   ///// </summary>
   ///// <param name="listView"></param>
   //public void EditResectionSignalCMD(ListView listView)
   //{
   //    if (listView.SelectedItems.Count < 1)
   //    {
   //        Growl.Warning("最少需选择一行,请重新选择！");
   //        return;
   //    }
   //    PLCResectionDialog _pLCResectionDialog = new PLCResectionDialog();
   //    _pLCResectionDialog.DataContext = new PLCResectionDialogModel(_container, listView.SelectedItems[0] as PLCResectionModel, 2, _pLCResectionDialog);
   //    var _result = _pLCResectionDialog.ShowDialog();
   //}
   ///// <summary>
   ///// 删除切除信号
   ///// </summary>
   ///// <param name="listView"></param>
   //public void DeleteSelectResectionSignalCMD(ListView listView)
   //{
   //    if (listView.SelectedItems.Count < 1)
   //    {
   //        Growl.Warning("最少需选择一行,请重新选择！");
   //        return;
   //    }
   //    List<string> _names = new List<string>();
   //    foreach (var item in listView.SelectedItems)
   //    {
   //        _names.Add($"[{(item as PLCResectionModel).Name}]");
   //    }
   //    if (HandyControl.Controls.MessageBox.Show($"确定要删除 {string.Join(',', _names)} 吗?", "删除警告！", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
   //        return;

   //    for (int i = listView.SelectedItems.Count - 1; i > -1; i--)
   //    {
   //        var _model = PLCSignal.PLCResections.FirstOrDefault(x => x.Name == (listView.SelectedItems[i] as PLCResectionModel).Name);
   //        PLCSignal.PLCResections.Remove(_model);
   //    }
   //    PLCSignal.Save(_usersStatus.LocalLoggedinUser.Name, $"删除服务 {string.Join(',', _names, false)} ;");
   //    Growl.Success("删除成功！");
   //}
   #endregion

   #region 自定义信号
   public void AddCustomInteractCmd()
   {
      CustomInteractViewModel customInteractVM = _container.Get<CustomInteractViewModel>();
      customInteractVM.SetCustomSignal(new CustomPlcInteractAddressModel(), 1);
      _windowManager.ShowDialog(customInteractVM);
   }

   public void EditCustomInteractCmd(ListView listView)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      CustomInteractViewModel customInteractVM = _container.Get<CustomInteractViewModel>();
      if (listView.SelectedItems[0] is CustomPlcInteractAddressModel custom)
      {
         customInteractVM.SetCustomSignal(custom, 2);
         _windowManager.ShowDialog(customInteractVM);
      }
   }

   public void EnableCustomInteractCmd(ListView listView)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      List<string> names = new List<string>();
      foreach (var item in listView.SelectedItems)
      {
         if (item is CustomPlcInteractAddressModel custom)
         {
            custom.IsEnable = true;
            names.Add($"[{custom.CustomInteractName}]");
         }
      }
      PLCSignal.Save(_usersStatus.LocalLoggedinUser.Account, $"启用自定义信号 {string.Join(',', names, false)} ;");
      Growl.Success("启用自定义信号成功！");
   }

   public void DisabledCustomInteractCmd(ListView listView)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      List<string> names = new List<string>();
      foreach (var item in listView.SelectedItems)
      {
         if (item is CustomPlcInteractAddressModel custom)
         {
            custom.IsEnable = false;
            names.Add($"[{custom.CustomInteractName}]");
         }
      }
      PLCSignal.Save(_usersStatus.LocalLoggedinUser.Account, $"禁用自定义信号 {string.Join(',', names, false)} ;");
      Growl.Success("禁用自定义信号成功！");
   }

   public void DeleteCustomCmd(ListView listView)
   {
      if (listView.SelectedItems.Count < 1)
      {
         Growl.Warning("最少需选择一行,请重新选择！");
         return;
      }
      List<string> names = new List<string>();
      foreach (var item in listView.SelectedItems)
      {
         if (item is CustomPlcInteractAddressModel custom)
         {
            names.Add($"[{custom.CustomInteractName}]");
         }
      }
      if (
         HandyControl.Controls.MessageBox.Show($"确定要删除 {string.Join(',', names)} 吗?", "删除警告！", MessageBoxButton.OKCancel)
         != MessageBoxResult.OK
      )
         return;

      for (int i = listView.SelectedItems.Count - 1; i > -1; i--)
      {
         if (listView.SelectedItems[i] is CustomPlcInteractAddressModel custom)
         {
            var remove = PLCSignal.CustomPlcInteractAddresses.FirstOrDefault(x => x.Id == custom.Id);
            if (remove != null)
               PLCSignal.CustomPlcInteractAddresses.Remove(remove);
         }
      }
      PLCSignal.Save(_usersStatus.LocalLoggedinUser.Account, $"删除自定义 {string.Join(',', names, false)} ;");
      Growl.Success("删除成功！");
   }
   #endregion
   public void Load() { }

   public bool Unload()
   {
      Task.Run(() =>
         _container.Get<ISqlSugarDbFactory>().UsingDb(DatabaseRole.LocalDb1, db => _container.Get<DbHelper>().SyncSplitTableFiled(db))
      ); //离开当前页面时同步分表字段
      return true;
   }
}
