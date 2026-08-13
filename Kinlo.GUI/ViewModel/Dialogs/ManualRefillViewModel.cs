using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

[UIDisplay(IsSingleton = true)]
public class ManualRefillViewModel : Screen
{
   public float ReplenishWeight { get; set; }

   /// <summary>
   /// 清除倒计时
   /// </summary>
   public string CurrentTime { get; set; } = string.Empty;
   public string lastBattryText { get; set; } = string.Empty;

   private int _tabIndex;

   public int TabIndex
   {
      get { return _tabIndex; }
      set
      {
         if (_tabIndex != value)
         {
            _tabIndex = value;
            RegistrationHook(false, BarcodeLength);
            RegistrationHook(true, BarcodeLength);
            _ = GetBattery();
         }
      }
   }

   private bool _isHasReInjectionScale;

   private int _clearTime = 5;

   /// <summary>
   /// 数据清除时间（清除到上一个补液信息处）
   /// </summary>
   public int ClearTime
   {
      get { return _clearTime; }
      set
      {
         if (_clearTime != value)
         {
            _otherParameter.ReInjectionElectrolyte.ClearTime = _clearTime = value;
            _otherParameter.Save("自动保存", $"", isPopup: false);
         }
      }
   }

   /// <summary>
   /// 是否有补液称
   /// </summary>
   public bool IsHasReInjectionScale
   {
      get { return _isHasReInjectionScale; }
      set
      {
         if (_isHasReInjectionScale != value)
         {
            _otherParameter.ReInjectionElectrolyte.IsHasReInjectionScale = _isHasReInjectionScale = value;
            _otherParameter.Save("自动保存", $"", isPopup: false);
         }
      }
   }

   private bool _isHasPlc;

   /// <summary>
   /// 是否要和PLC交互
   /// </summary>
   public bool IsHasPlc
   {
      get { return _isHasPlc; }
      set
      {
         if (_isHasPlc != value)
         {
            _otherParameter.ReInjectionElectrolyte.IsHasPlc = _isHasPlc = value;
            _otherParameter.Save("自动保存", $"", isPopup: false);
         }
      }
   }

   private bool _isHasPump;

   /// <summary>
   /// 是否有补液泵
   /// </summary>
   public bool IsHasPump
   {
      get { return _isHasPump; }
      set
      {
         if (_isHasPump != value)
         {
            _otherParameter.ReInjectionElectrolyte.IsHasPump = _isHasPump = value;
            _otherParameter.Save("自动保存", $"", isPopup: false);
         }
      }
   }
   private int _barcodeLength = 24; //条码长度;

   public int BarcodeLength
   {
      get { return _barcodeLength; }
      set
      {
         if (value != _barcodeLength)
         {
            RegistrationHook(false, _barcodeLength);
            RegistrationHook(true, value);
            _otherParameter.ReInjectionElectrolyte.BarcodeLength = _barcodeLength = value;
            _otherParameter.Save("自动保存", $"", isPopup: false);
         }
      }
   }

   private IBatMainModel? _mainBattery;

   public IBatMainModel? MainBattery
   {
      get { return _mainBattery; }
      set
      {
         _mainBattery = value;
         if (value == null)
            return;
         if (TabIndex == 0)
         {
            if (IsHasReInjectionScale)
               _ = AutoWeighting(value).ContinueWith(t => ClearBatteryAsync());
         }
         //else
         //{
         //    _ = Reinvest(value);
         //}
      }
   }

   async Task ClearBatteryAsync()
   {
      lastBattryText = string.Empty;
      await Task.Run(async () =>
      {
         for (int i = ClearTime; i > 0; i--)
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(() => CurrentTime = i.ToString());
            await Task.Delay(1000);
         }
         if (MainBattery != null)
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(() =>
            {
               CurrentTime = string.Empty;
               lastBattryText = BattryToString(MainBattery);
               MainBattery = null;
            });
         }
      });
   }

   /// <summary>
   /// 补录数据电池显示
   /// </summary>
   public string BatteryText { get; set; } = string.Empty;

   /// <summary>
   /// 补录数据时取基础数据的开始时间
   /// </summary>
   //   public DateTime StartTime { get; set; }
   /// <summary>
   /// 补录数据时取基础数据的个数
   /// </summary>
   public int Count { get; set; } = 5000;

   /// <summary>
   ///
   /// </summary>
   List<IBatMainModel> _cacheBatterys = new List<IBatMainModel>();

   private IBatteryCache _cache;
   private ParameterConfig _parameterConfig;
   private IContainer _container;
   private DbHelper _sugarDB;
   private DevicesConfig _devicesConfig;
   private PLCSignalConfig _plcSignalConfig;
   private OtherParameterConfig _otherParameter;
   private SnowflakeHelper _snowflakeHelper;

   //IPLC? _plc;
   public ManualRefillViewModel(IContainer container)
   {
      _container = container;
      //  StartTime = DateTime.Now.AddDays(-5);
      _cache = container.Get<IBatteryCache>();
      _parameterConfig = container.Get<ParameterConfig>();
      _devicesConfig = container.Get<DevicesConfig>();
      _plcSignalConfig = container.Get<PLCSignalConfig>();
      _otherParameter = container.Get<OtherParameterConfig>();
      IsHasReInjectionScale = _otherParameter.ReInjectionElectrolyte.IsHasReInjectionScale;
      IsHasPlc = _otherParameter.ReInjectionElectrolyte.IsHasPlc;
      IsHasPump = _otherParameter.ReInjectionElectrolyte.IsHasPump;
      BarcodeLength = _otherParameter.ReInjectionElectrolyte.BarcodeLength;
      ClearTime = _otherParameter.ReInjectionElectrolyte.ClearTime;
      _sugarDB = container.Get<DbHelper>();
      _snowflakeHelper = container.Get<SnowflakeHelper>();
   }

   [Inject]
   public QueryBatteryViewModel? QueryBatteryVM { get; set; }

   [Inject]
   public IWindowManager? Manager { get; set; }

   /// <summary>
   /// 清除电池条码命令
   /// </summary>
   public void ClearBatteryCmd() => MainBattery = null;

   /// <summary>
   /// 手动输入电池条码命令
   /// </summary>
   public void ManualInputCmd()
   {
      if (QueryBatteryVM != null)
      {
         QueryBatteryVM.TypeIndex = TabIndex;
         QueryBatteryVM.GetBarcode = async barcode => await BuildBattery(barcode);
         QueryBatteryVM.GetBatteryAction = bat =>
         {
            MainBattery = null;
            MainBattery = bat;
         };
         Manager?.ShowDialog(QueryBatteryVM);
      }
      else
      {
         Growl.Warning("未找到查询窗口！");
      }
   }

   /// <summary>
   /// 查找电池
   /// </summary>
   /// <param name="barcode"></param>
   /// <returns></returns>
   private async Task QueryBattery(string barcode)
   {
      MainBattery = null;
      MainBattery = await _cache.GetByBarcodeAsync(barcode, "");
      if (MainBattery == null)
      {
         Growl.Warning($"条码：{barcode}未找到电芯数据！");
         return;
      }
   }

   /// <summary>
   /// 自动称重
   /// </summary>
   /// <param name="barcode"></param>
   /// <returns></returns>
   private async Task AutoWeighting(IBatMainModel bat)
   {
      try
      {
         string logHeader = "[补液称]";

         var deviceResult = DeviceResult<double>.Failure("默认");
         await DoOperationWithDeviceAsnyc(
            ProcessTypeEnum.手动补液,
            _devicesConfig,
            _plcSignalConfig,
            async (device) =>
            {
               await Task.Run(() =>
               {
                  deviceResult = HandlerHelper.GetWeiging(device, logHeader);
               });
            }
         );

         if (deviceResult.Status != DeviceStatus.Success)
         {
            Growl.Warning($"补液称重量失败！");
            return;
         }
         if (deviceResult.Value < staWeight)
         {
            Growl.Warning($"补液称重量小于{staWeight}g，重量异常，请检查电池在称上是否放好！");
            return;
         }
         _ = Task.Run(async () => await Compute(deviceResult.Value, bat, logHeader));
      }
      catch (Exception ex)
      {
         Growl.Warning($"补液称重异常：{ex}！");
      }
   }

   float staWeight = 50;

   /// <summary>
   /// 手动输入重量
   /// </summary>
   public async Task ComputeCMD()
   {
      if (MainBattery == null)
      {
         Growl.Warning("请先查询出电池再操作！");
         return;
      }
      await Compute(ReplenishWeight, MainBattery, "[手动输入重量]");
   }

   /// <summary>
   /// 计算
   /// </summary>
   private async Task Compute(double weight, IBatMainModel bat, string logHeader)
   {
      try
      {
         if (bat is not IBatWeightManualRefillModel batteryReplenishWeight)
         {
            Growl.Warning("未找到补液工序，请先配置补液工序！");
            return;
         }
         if (bat is not IBatWeightAfterModel batteryAfterWeight)
         {
            Growl.Warning("未找到后称工序，请先配置后称工序！");
            return;
         }
         if (bat is not IBatScanBeforeModel batteryBeforeScan)
         {
            Growl.Warning("未找到前扫码工序，请先配置前扫码工序！");
            return;
         }

         await UIThreadHelper.InvokeOnUiThreadAsync(() =>
         {
            var replenishRs = bat.UpdateManualRefill(weight, _parameterConfig, logHeader);
            if (!replenishRs.state)
            {
               Growl.Warning(replenishRs.errMsg);
               return;
            }
         });

         if (batteryAfterWeight.FinalInjectResult == ResultTypeEnum.OK) //20260107 加入 OK上传MES
         {
            if (bat is IBatTestLeakModel test && test.LeakResult != ResultTypeEnum.OK)
            {
               test.LeakResult = ResultTypeEnum.OK; //如果注液OK。则测漏一定OK，20260409
            }

            var mesService = _container.Get<MesService>();
            await MesOutboundHelper.ProductionMesOutput(_container, mesService, bat, logHeader);
         }

         ResultTypeEnum sendPlcResult = batteryAfterWeight.FinalInjectResult;
         float injectVal =
            batteryAfterWeight.FinalInjectResult == ResultTypeEnum.注液量偏少
               ? (float)Math.Abs(batteryAfterWeight.TotalInjectionVolumeDeviation)
               : 0;
         if (IsHasPump)
         {
            var injectInteractAdd = _plcSignalConfig.PLCInteractAddresses.FirstOrDefault(x =>
               x.ProcessesType == ProcessTypeEnum.补液量发送
            );
            if (injectInteractAdd == null)
            {
               Growl.Warning($"未找到补液量发送信号交互配置，请先配置！");
               sendPlcResult = ResultTypeEnum.NG;
            }
            else
            {
               bool sendInjectResult = false;

               await DoOperationWithDeviceAsnyc(
                  ProcessTypeEnum.补液量发送,
                  _devicesConfig,
                  _plcSignalConfig,
                  async (device) =>
                  {
                     await Task.Run(() =>
                     {
                        sendInjectResult = HandlerHelper.SendInj(injectVal, injectInteractAdd.ExtraDataAddress, device, logHeader);
                     });
                  }
               );

               if (!sendInjectResult)
               {
                  sendPlcResult = ResultTypeEnum.注液量发送失败;
                  $"注液量[{injectVal}]发送失败！".LogProcess(logHeader, Log4NetLevelEnum.警告, true);
               }
               else
               {
                  $"注液量[{injectVal}]发送成功！".LogProcess(logHeader, Log4NetLevelEnum.成功, true);
               }
            }
         }

         if (!await _sugarDB.UpdateByObjectAsync(bat, logHeader))
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(() =>
               sendPlcResult = batteryAfterWeight.FinalInjectResult = ResultTypeEnum.保存数据库失败
            );
            $"[补液站]条码：{bat.Barcode},重量：{batteryReplenishWeight.ManualRefillWeight},保存失败".LogProcess(logHeader);
            Growl.Warning("保存失败！");
         }
         else
         {
            $"[补液站]条码：{bat.Barcode},重量：{batteryReplenishWeight.ManualRefillWeight},保存成功".LogProcess(logHeader);
            Growl.Success("保存成功！");
         }

         //发送PLC
         if (IsHasPlc)
         {
            var weightInteractAdd = _plcSignalConfig.PLCInteractAddresses.FirstOrDefault(x => x.ProcessesType == ProcessTypeEnum.手动补液);
            if (weightInteractAdd == null)
            {
               Growl.Warning($"未找到补液称信号交互配置，请先配置！");
               return;
            }
            await DoOperationWithDeviceAsnyc(
               ProcessTypeEnum.PLC,
               _devicesConfig,
               _plcSignalConfig,
               async (plc) =>
               {
                  await Task.Run(() =>
                  {
                     new SignalAddressModel($"{weightInteractAdd.DataAddress.Lable}.ToPLCData[0]").WritePlcResult(
                        sendPlcResult,
                        ResultTypeEnum.OK,
                        (IPLC)plc,
                        _parameterConfig,
                        logHeader
                     );

                     var dataAdd = new SignalAddressModel($"{weightInteractAdd.DataAddress.Lable}.ToPLCData[0].PCData1");
                     var sendDataRs = plc.WriteValue(injectVal, dataAdd, logHeader); //发送注液量给PLC
                     var sendDataFinish = plc.WriteValue((short)-2, weightInteractAdd.StartCommand.Tag, logHeader); //完成
                     $"发送注液量[{injectVal}]至PLC地址：{JsonSerializer.Serialize(dataAdd)}{(sendDataRs ? "成功" : "失败")}；发送完成信号至PLC地址：{JsonSerializer.Serialize(weightInteractAdd.StartCommand.Tag)}{(sendDataFinish ? "成功" : "失败")}".LogProcess(
                        logHeader
                     );
                  });
               }
            );
         }
      }
      catch (Exception ex)
      {
         Growl.Error($"计算补液异常：{ex}");
      }
   }

   /// <summary>
   /// 复投
   /// </summary>
   private async Task Reinvest(IBatMainModel bat)
   {
      //发送PLC
      await DoOperationWithDeviceAsnyc(
         ProcessTypeEnum.PLC,
         _devicesConfig,
         _plcSignalConfig,
         async (plc) =>
         {
            await Task.Run(() =>
            {
               var address = _plcSignalConfig.PLCInteractAddresses.FirstOrDefault(x => x.ProcessesType == ProcessTypeEnum.复投);
               if (address == null)
               {
                  address = new PLCInteractAddressModel
                  {
                     ProcessesType = ProcessTypeEnum.复投,
                     DataAddress = new SignalAddressModel("PC_ReturnPutScan"),
                     StartCommand = new GenericCommandModel { Tag = new SignalAddressModel($"PC_CMD.Reserve1[0]") },
                  };
               }
               var logHeader = address.ProcessesType.ToProcessLogHeader(
                  address.ServiceName,
                  address.DeviceCommunicationType,
                  1,
                  bat.Id,
                  bat.Barcode
               );
               new SignalAddressModel($"{address.DataAddress.Lable}.ToPLCData[0].PCResult").WritePlcResult(
                  ResultTypeEnum.OK,
                  ResultTypeEnum._,
                  (IPLC)plc,
                  _parameterConfig,
                  logHeader
               );

               var idTag = new SignalAddressModel($"{address.DataAddress.Lable}.ToPCData[0].ID", 0);
               for (int n = 1; n < 4; n++)
               {
                  if (plc.WriteValue(bat.Id, idTag, logHeader))
                  {
                     $"第[{n}]次地址[{JsonSerializer.Serialize(idTag)}]写入ID[{bat.Id}] 成功,条码：{bat.Barcode};".LogProcess(
                        logHeader,
                        Log4NetLevelEnum.成功
                     );
                     break;
                  }
                  else
                  {
                     $"第[{n}]次地址[{JsonSerializer.Serialize(idTag)}]写入ID[{bat.Id}] 失败,条码：{bat.Barcode};".LogProcess(
                        logHeader,
                        Log4NetLevelEnum.警告,
                        true
                     );
                  }
               }

               for (int n = 1; n < 4; n++)
               {
                  short finishValue = -2;
                  if (plc.WriteValue(finishValue, address.StartCommand.Tag, logHeader))
                  {
                     $"第[{n}]次地址[{JsonSerializer.Serialize(address.StartCommand.Tag)}]写入完成[{finishValue}] 成功,条码：{bat.Barcode};".LogProcess(
                        logHeader,
                        Log4NetLevelEnum.成功
                     );
                     break;
                  }
                  else
                  {
                     $"第[{n}]次地址[{JsonSerializer.Serialize(address.StartCommand.Tag)}]写入完成[{finishValue}] 失败,条码：{bat.Barcode};".LogProcess(
                        logHeader,
                        Log4NetLevelEnum.警告,
                        true
                     );
                  }
               }
            });
         }
      );
   }

   public void CancelCMD()
   {
      RegistrationHook(false);
      this.RequestClose();
   }

   /// <summary>
   /// 注册或注销条码监听钩子
   /// </summary>
   /// <param name="isRegistration"></param>
   /// <param name="length"></param>
   public void RegistrationHook(bool isRegistration, int length = 0)
   {
      if (length == 0)
         length = BarcodeLength;

      if (TabIndex == 0)
      {
         if (isRegistration)
            KeyboardHook.Hook.RegisterExclusive(length, async barcode => await QueryBattery(barcode));
         else
            KeyboardHook.Hook.UnregisterExclusive();
      }
      else
      {
         if (isRegistration)
            KeyboardHook.Hook.RegisterExclusive(length, async barcode => await BuildBattery(barcode));
         else
            KeyboardHook.Hook.UnregisterExclusive();
      }

      $"{(isRegistration ? "注册" : "注销")}监控条码长度：[{length}]！".LogRun();
   }

   /// <summary>
   /// 获取设备执行操作（如果多PLC或多补液设备可在界面做选择（大多为单台设备，多设备暂未实现））
   /// </summary>
   /// <param name="processes"></param>
   /// <param name="devicesConfig"></param>
   /// <param name="plcSignal"></param>
   /// <returns></returns>
   private async Task<bool> DoOperationWithDeviceAsnyc(
      ProcessTypeEnum processes,
      DevicesConfig devicesConfig,
      PLCSignalConfig plcSignal,
      Func<IDevice, Task> action
   )
   {
      var device = devicesConfig.GetRunDevice(x => x.DeviceInfo.ProcessesType == processes);
      if (device != null)
      {
         await action(device);
         return true;
      }
      var client = devicesConfig.DeviceList.FirstOrDefault(x => x.ProcessesType == processes);

      if (client == null)
      {
         Growl.Warning($"未找到{processes}设备配置，请先配置！");
         return false;
      }
      await client.WithCreatedDeviceAsync(async d => await action(d));
      return true;
   }

   private async Task GetBattery()
   {
      if (TabIndex == 1 && !_cacheBatterys.Any())
      {
         _cacheBatterys = await _sugarDB.GetBattereyListAsync(5000);
         _cacheBatterys = _cacheBatterys
            .Where(x =>
            {
               if (((int)x.FinalStatus) >= 21)
                  return false;

               if (x is IBatWeightAfterModel a)
               {
                  return a.FinalInjectResult == ResultTypeEnum.OK;
               }

               return false;
            })
            .ToList();

         if (_cacheBatterys == null || _cacheBatterys.Count == 0)
         {
            "[补录数据] 无数据可加载".LogRun(Log4NetLevelEnum.信息);
            return;
         }
         await UIThreadHelper.InvokeOnUiThreadAsync(() => Count = _cacheBatterys.Count);
         $"[补录数据] 成功加载 {_cacheBatterys.Count} 条数据；".LogRun(Log4NetLevelEnum.信息);
      }
   }

   Random random = new Random();

   /// <summary>
   /// 创建复制电池
   /// </summary>
   /// <param name="barcode"></param>
   /// <returns></returns>
   private async Task BuildBattery(string barcode)
   {
      var index = random.Next(0, _cacheBatterys.Count);
      var tempBattery = _cacheBatterys[index];
      tempBattery.Id = _snowflakeHelper.NextId();
      tempBattery.Barcode = barcode;
      tempBattery.MesOutputStatus = ResultTypeEnum._;
      tempBattery.MesInputStatus = ResultTypeEnum._;
      tempBattery.OutputTime = default;
      var time = DateTime.Now;
      IBatScanBeforeModel? beforeScan = null;
      IBatWeightBeforeModel? beforeWeight = null;
      IBatWeightAfterModel? afterWeight = null;
      if (tempBattery is IBatScanBeforeModel bs)
      {
         bs.BeforeScanTime = time;
         beforeScan = bs;
      }
      if (tempBattery is IBatWeightBeforeModel wb)
      {
         wb.BeforeWeightTime = time.AddMinutes(6);
         beforeWeight = wb;
      }
      if (tempBattery is IBatWeightAfterModel af)
      {
         af.AfterWeightTime = time.AddHours(1);
         afterWeight = af;
      }

      if (!await _sugarDB.InsertableByObjectAsync(tempBattery, "补录数据"))
      {
         if (tempBattery is IBatWeightAfterModel aft)
            aft.FinalWeighingResult = ResultTypeEnum.保存数据库失败;
      }
      var msg = BattryToString(tempBattery);
      await UIThreadHelper.InvokeOnUiThreadAsync(() => BatteryText = msg);
      $"手工补数据：{msg}".LogRun(); //260323-10:16新加
   }

   private string BattryToString(IBatMainModel tempBattery)
   {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine($"条码：{tempBattery.Barcode}");
      sb.AppendLine($"进站时间：{tempBattery.CreateTime.ToMesDateTime()}");
      if (tempBattery is IBatWeightBeforeModel beforeWeight)
      {
         sb.AppendLine($"前称重量：{beforeWeight.BeforeWeight}");
         sb.AppendLine($"前称结果：{beforeWeight.BeforeWeightResult}");
      }
      if (tempBattery is IBatWeightAfterModel afterWeight)
      {
         sb.AppendLine($"后称时间：{afterWeight.AfterWeightTime.ToMesDateTime()}");
         sb.AppendLine($"后称重量：{afterWeight.FinalAfterWeight}");
         sb.AppendLine($"后称结果：{afterWeight.FinalWeighingResult}");
         sb.AppendLine($"首注结果：{afterWeight.FirstInjectResult}");
      }
      if (tempBattery is IBatWeightAutoRefillModel autoRefill)
      {
         sb.AppendLine($"回流称时间：{autoRefill.AutoRefillTime.ToMesDateTime()}");
         sb.AppendLine($"回流称重量：{autoRefill.AutoRefillWeight}");
         sb.AppendLine($"回流补液结果：{autoRefill.AutoRefillResult}");
      }
      if (tempBattery is IBatWeightManualRefillModel maualRefill)
      {
         sb.AppendLine($"手动补液称时间：{maualRefill.ManualRefillTime.ToMesDateTime()}");
         sb.AppendLine($"手动补液称重量：{maualRefill.ManualRefillWeight}");
         sb.AppendLine($"手动补液结果：{maualRefill.ManualRefillResult}");
      }
      if (tempBattery is IBatWeightAfterModel after)
      {
         sb.AppendLine($"注液结果：{after.FinalInjectResult}");
      }
      sb.AppendLine($"生产状态：{tempBattery.FinalStatus}");
      sb.AppendLine($"MES出站时间：{tempBattery.OutputTime.ToMesDateTime()}");
      sb.AppendLine($"MES出站结果：{tempBattery.MesOutputStatus}");

      return sb.ToString();
   }
}
