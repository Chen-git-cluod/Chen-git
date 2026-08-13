using HandyControl.Controls;
using NPOI.SS.UserModel;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(IsSingleton = false)]
internal class PLCScanSignalViewModel : Screen
{
   int _code;
   private IContainer _container;

   public string Title { get; set; } = string.Empty;
   public PLCScanSignalModel PLCScanSignalCopy { get; set; } = new PLCScanSignalModel();
   private PLCScanSignalModel _plcScanSignal;
   private UsersStatusConfig _usersStatus;
   private PLCSignalConfig _pLCSignalConfig;

   /// <summary>
   ///
   /// </summary>
   /// <param name="container">1 新建，2 编辑</param>
   /// <param name="pLCScanSignal"></param>
   /// <param name="code"></param>
   public PLCScanSignalViewModel(IContainer container)
   {
      _container = container;
      _usersStatus = container.Get<UsersStatusConfig>();
      _pLCSignalConfig = _container.Get<PLCSignalConfig>();
   }

   public void SetPLCScanSignal(PLCScanSignalModel pLCScanSignal, int code)
   {
      Title = code == 1 ? "新建扫描信号" : "编辑扫描信号";
      _code = code;
      if (code == 2)
      {
         _plcScanSignal = pLCScanSignal;
         ExpressionAssignmentMapper<PLCScanSignalModel, PLCScanSignalModel>.Trans(pLCScanSignal, PLCScanSignalCopy);
         ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
            pLCScanSignal.AddressStart,
            PLCScanSignalCopy.AddressStart
         );
         ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(pLCScanSignal.Heartbeat, PLCScanSignalCopy.Heartbeat);
         ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(pLCScanSignal.Status, PLCScanSignalCopy.Status);
         foreach (var item in pLCScanSignal.StartSignas)
         {
            PLCScanSignalCopy.StartSignas.Add(
               new GenericCommandModel { Index = item.Index, Tag = new SignalAddressModel(item.Tag.Lable, item.Tag.Address) }
            );
         }
         foreach (var item in pLCScanSignal.PLCResections)
         {
            PLCScanSignalCopy.PLCResections.Add(
               new GenericCommandModel
               {
                  Index = item.Index,
                  Tag = new SignalAddressModel(item.Tag.Lable, item.Tag.Address),
                  IsEnabled = item.IsEnabled,
               }
            );
         }
      }
      else
      {
         PLCScanSignalCopy = pLCScanSignal;
      }
   }

   public void ConfirmCMD()
   {
      if (
         string.IsNullOrEmpty(PLCScanSignalCopy.ServiceName)
         || string.IsNullOrEmpty(PLCScanSignalCopy.AddressStart.Lable)
         || string.IsNullOrEmpty(PLCScanSignalCopy.AddressingMethod)
         || string.IsNullOrEmpty(PLCScanSignalCopy.Heartbeat.Lable)
         || string.IsNullOrEmpty(PLCScanSignalCopy.Status.Lable)
      )
      {
         Growl.Warning("属性不能为空！");
         return;
      }

      switch (_code)
      {
         case 1:
            if (_pLCSignalConfig.PLCScanSignals.Any(x => x.ServiceName == PLCScanSignalCopy.ServiceName))
            {
               Growl.Warning("服务名重复！");
            }
            else
            {
               _pLCSignalConfig.PLCScanSignals.Add(PLCScanSignalCopy);
               _pLCSignalConfig.Save(_usersStatus.LocalLoggedinUser.Account, "");
               this.RequestClose(true);
            }
            break;
         case 2:
            StringBuilder _contrastMsg = _plcScanSignal.CompareObject(PLCScanSignalCopy, new Dictionary<string, DifferenceResultDto>());

            if (!string.IsNullOrEmpty(_contrastMsg.ToString()))
            {
               ExpressionAssignmentMapper<PLCScanSignalModel, PLCScanSignalModel>.Trans(PLCScanSignalCopy, _plcScanSignal);
               ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
                  PLCScanSignalCopy.AddressStart,
                  _plcScanSignal.AddressStart
               );
               ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
                  PLCScanSignalCopy.Heartbeat,
                  _plcScanSignal.Heartbeat
               );
               ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(PLCScanSignalCopy.Status, _plcScanSignal.Status);
               _plcScanSignal.StartSignas.Clear();
               for (int i = 0; i < PLCScanSignalCopy.StartSignas.Count; i++)
               {
                  _plcScanSignal.StartSignas.Add(
                     new GenericCommandModel
                     {
                        Index = PLCScanSignalCopy.StartSignas[i].Index,
                        Tag = new SignalAddressModel(
                           PLCScanSignalCopy.StartSignas[i].Tag.Lable,
                           PLCScanSignalCopy.StartSignas[i].Tag.Address
                        ),
                     }
                  );
               }
               _plcScanSignal.PLCResections.Clear();
               for (int i = 0; i < PLCScanSignalCopy.PLCResections.Count; i++)
               {
                  _plcScanSignal.PLCResections.Add(
                     new GenericCommandModel
                     {
                        Index = PLCScanSignalCopy.PLCResections[i].Index,
                        Tag = new SignalAddressModel(
                           PLCScanSignalCopy.PLCResections[i].Tag.Lable,
                           PLCScanSignalCopy.PLCResections[i].Tag.Address
                        ),
                        IsEnabled = PLCScanSignalCopy.PLCResections[i].IsEnabled,
                     }
                  );
               }
               _pLCSignalConfig.SyncPlcCmdIndex(); //同步PLC交互信号
               _pLCSignalConfig.Save(_usersStatus.LocalLoggedinUser.Account, _contrastMsg.ToString());
               this.RequestClose(true);
            }
            else
            {
               Growl.Success($"文件未修改！");
            }
            break;
      }
   }

   /// <summary>
   /// 导入触发信号
   /// </summary>
   public async Task InputStartTagsCMD() => await InputStartTagsFun(PLCScanSignalCopy.StartSignas, true);

   /// <summary>
   /// 导入切除信号
   /// </summary>
   public async Task InputResectionsCMD() => await InputStartTagsFun(PLCScanSignalCopy.PLCResections, false);

   public async Task<bool> InputStartTagsFun(ObservableCollection<GenericCommandModel> genericCommands, bool isStart)
   {
      string _msg = isStart ? "触发标签" : "切除标签";
      if (!string.IsNullOrEmpty(PLCScanSignalCopy.ServiceName))
      {
         var _dialog = HandyControl.Controls.MessageBox.Show(
            $"导入{_msg}将覆盖 [{PLCScanSignalCopy.ServiceName}] 现有标签,是否确定?",
            "提示:",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning
         );
         if (_dialog != MessageBoxResult.OK)
            return await Task.FromResult(false);
      }
      Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
      openFileDialog.Title = $"选择导入PLC{_msg}文件";
      openFileDialog.Filter = "文件(*.xlsx)|*.xlsx";
      openFileDialog.FileName = "选择文件";
      openFileDialog.FilterIndex = 1;
      openFileDialog.ValidateNames = false;
      openFileDialog.CheckFileExists = false;
      openFileDialog.CheckPathExists = true;
      openFileDialog.Multiselect = false; //允许同时选择多个文件
      if (openFileDialog.ShowDialog() == true)
      {
         try
         {
            using (var fs = new FileStream(openFileDialog.FileName, FileMode.Open))
            {
               NPOI.XSSF.UserModel.XSSFWorkbook book = new NPOI.XSSF.UserModel.XSSFWorkbook(fs);
               ISheet sheet = book.GetSheetAt(0); //获取第一个工作薄

               genericCommands.Clear();
               int _index = 0;
               for (int i = 2; i <= sheet.LastRowNum; i++)
               {
                  string _tag = Kinlo.Common.Tools.ExcelHelper.GetCellValue(sheet.GetRow(i).GetCell(1))!;
                  string _nextTag = string.Empty;
                  if (i < sheet.LastRowNum)
                  {
                     _nextTag = Kinlo.Common.Tools.ExcelHelper.GetCellValue(sheet.GetRow(i + 1).GetCell(1))!;
                     if (_nextTag == $"{_tag}[0]")
                        continue;
                  }

                  int.TryParse(_tag, out int add);
                  genericCommands.Add(new GenericCommandModel { Index = _index, Tag = new SignalAddressModel(_tag, add) });
                  _index++;
               }
               if (isStart)
               {
                  PLCScanSignalCopy.LengthSignal = genericCommands.Count;
                  if (PLCScanSignalCopy.LengthSignal > 0)
                  {
                     if (int.TryParse(genericCommands[0].Tag.Lable, out int add))
                        PLCScanSignalCopy.AddressStart = new SignalAddressModel(genericCommands[0].Tag.Lable, add);
                     else
                        PLCScanSignalCopy.AddressStart = new SignalAddressModel(genericCommands[0].Tag.Lable.Split('.')[0], 0);
                  }
               }
               else
                  PLCScanSignalCopy.LengthResection = genericCommands.Count;
            }
         }
         catch (Exception ex)
         {
            $"[导入Excel]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
         }
      }
      return await Task.FromResult(true);
   }
}
