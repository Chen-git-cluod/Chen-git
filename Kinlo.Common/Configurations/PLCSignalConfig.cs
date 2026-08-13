namespace Kinlo.Common.Configurations
{
   public class PLCSignalConfig : ConfigurationBase
   {
      public PLCSignalConfig(StyletIoC.IContainer container, bool isStartup)
         : base(container, isStartup) { }

      /// <summary>
      /// plc扫描服务
      /// </summary>
      public ObservableCollection<PLCScanSignalModel> PLCScanSignals { get; set; } = new();

      /// <summary>
      /// plc数据交互信号
      /// </summary>
      public ObservableCollection<PLCInteractAddressModel> PLCInteractAddresses { get; set; } = new();

      /// <summary>
      /// PLC报警地址
      /// </summary>
      public PLCAlarmAddressModel PLCAlarmAddresses { get; set; } = new();

      /// <summary>
      /// 自定义PLC交互地址
      /// </summary>
      public ObservableCollection<CustomPlcInteractAddressModel> CustomPlcInteractAddresses { get; set; } = new();

      public override void Load()
      {
         try
         {
            var _dic = FileHelper.LoadToDictionary(this.GetType().Name);
            if (_dic != null)
            {
               if (_dic.TryGetValue(nameof(PLCScanSignals), out object value) && value != null)
                  PLCScanSignals = JsonSerializer.Deserialize<ObservableCollection<PLCScanSignalModel>>(value.ToString())!;
               if (_dic.TryGetValue(nameof(PLCInteractAddresses), out object value1) && value1 != null)
                  PLCInteractAddresses = JsonSerializer.Deserialize<ObservableCollection<PLCInteractAddressModel>>(value1.ToString())!;
               if (_dic.TryGetValue(nameof(PLCAlarmAddresses), out object value2) && value2 != null)
                  PLCAlarmAddresses = JsonSerializer.Deserialize<PLCAlarmAddressModel>(value2.ToString())!;
               if (_dic.TryGetValue(nameof(CustomPlcInteractAddresses), out object value3) && value3 != null)
                  CustomPlcInteractAddresses = JsonSerializer.Deserialize<ObservableCollection<CustomPlcInteractAddressModel>>(
                     value3.ToString()
                  )!;
            }
         }
         catch (Exception ex)
         {
            $"[初始化PLCSignalConfig]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
         }
      }

      public override void Save(string userName, string revise, bool isPopup = true, bool isPrintLog = true, string saveName = "")
      {
         PLCInteractAddresses = PLCInteractAddresses
            .OrderBy(x => x.ProductionIndex)
            .ThenBy(x => x.StartCommand.Index)
            .ThenBy(x => x.DeviceStartIndex)
            .ToObservableCollection();
         base.Save(userName, revise, isPopup, isPrintLog, saveName);
      }

      /// <summary>
      /// 同步cmd索引
      /// </summary>
      public void SyncPlcCmdIndex()
      {
         foreach (var interact in PLCInteractAddresses)
         {
            var service = PLCScanSignals.FirstOrDefault(m => m.ServiceName == interact.ServiceName);
            if (service != null)
            {
               var add = service.StartSignas.FirstOrDefault(x => x.Tag.Lable == interact.StartCommand.Tag.Lable);
               if (add != null)
               {
                  interact.StartCommand.Index = add.Index;
               }
            }
         }
      }
   }
}
