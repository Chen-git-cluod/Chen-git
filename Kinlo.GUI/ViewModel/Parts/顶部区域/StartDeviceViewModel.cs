using HandyControl.Controls;
using Kinlo.Common.Tools.ExpressionHelpers;

namespace Kinlo.GUI.ViewModel;

[UIDisplayAttribute(true)]
public class StartDeviceViewModel : Screen
{
   public GlobalStaticTemporary Temporary { get; set; }

    public UsersStatusConfig UsersStatusConfig { get; set; }
   private IContainer _container;

   public async Task RunCmd()
   {
      try
      {
            if (UsersStatusConfig.LoggedInUserType != LoggedInTypeEnum.未登陆)
            {
                if (!Temporary.IsRunning)
                {
                    await Temporary.RequestStartAsync();
                }
                else
                {
                    Temporary.RequestStop();
                }
            }
            else 
            {
                $"账户未登录，不可以运行".LogRun(Log4NetLevelEnum.警告,true);
            }
      }
      catch (Exception ex)
      {
         Growl.Error(ex.ToString());
      }
   }

   public StartDeviceViewModel(IContainer container)
   {
      _container = container;
      Temporary = container.Get<GlobalStaticTemporary>();
      UsersStatusConfig = container.Get<UsersStatusConfig>();
      //Task.Run(async () =>
      //{
      //    IDevice _plc = null;
      //    Thread.Sleep(6000);

        //    //while (_plc == null)
        //    //{
        //    //    _plc = container.Get<DevicesConfig>().RunDeviceList.FirstOrDefault(t
        //    //                => t.DeviceInfo.Communication == CommunicationEnum.CIP_Omron_PLC) as IPLC;
        //    //    Thread.Sleep(1000);
        //    //}

        //    TestClass testClass = new TestClass(container, _plc, new PLCInteractAddressModel { ProcessesType = ProcessTypeEnum.前扫码 }, new CancellationTokenSource());
        //    testClass.TestAddData();
        //});
   }

   public class TestClass : ServiceHandlerBase
   {
      public GlobalStaticTemporary Temporary { get; set; }
      UsersStatusConfig _usersStatus;
      DisplayDataCollection _displayData;

      public TestClass(IContainer container, IDevice plc, PLCInteractAddressModel plcInteractAddress, CancellationTokenSource taskToken)
         : base(container, plc, plcInteractAddress, taskToken)
      {
         _container = container;
         Temporary = container.Get<GlobalStaticTemporary>();
         _usersStatus = container.Get<UsersStatusConfig>();
         _displayData = container.Get<DisplayDataCollection>();
         _sugarDB = container.Get<DbHelper>();
         //  _sugarDB.GetDataById(typeof(ShortCircuitTestRJModel), 16865621197942784);
      }

      public async Task TestAddData()
      {
         //await _sugarDB.InsertOrUpdateMesResendAsync(new Common.Models.OhtenModels.MesResendModel
         // {
         //     Barcode = "test123",
         //     Id = 12345689
         // },"");
         // _sugarDB.GetBatteryListByIdsAsync([104406113063014400, 104406360489201665, 110170774240759809, 110170956642652161, 110167149112201216]).GetAwaiter().GetResult();
         //var mainBattery = (IBatMainModel)Activator.CreateInstance(_displayDataCollection.CompleteBatteryDatas.RuntimeBatteryModel);
         //var call = _mesInterfaceParameterConfig.GetApiCall(new MesRequestBuildNJGX.ArgsMesExit(mainBattery));
         //while (true)
         //{
         //    var _tag = new SignalAddressModel($"PC_TankMES[0]");
         //    var _tag2 = new SignalAddressModel($"PC_AfterWeight.ToPCData[0]");

         //    var _plcGenericData = _plc.ReadObject<PlcToTank>(_tag, 1);
         //    //var _plcGenericData = _plc.ReadObjects<PlcGeneric2DTU>(_tag, 1);

         //  $"ReadObject :{JsonSerializer.Serialize(_plcGenericData)}".LogRun(Log4NetLevelEnum.成功);

         //    //var _plcGenericData2 = _plc.ReadClass<PlcToTank>(_tag, null);
         //    ////var _plcGenericData = _plc.ReadObjects<PlcGeneric2DTU>(_tag, 1);

         //    //$"ReadClass :{JsonSerializer.Serialize(_plcGenericData)}".LogRun(Log4NetLevelEnum.成功);

         //    Thread.Sleep(1000);

         //}

         _ = Task.Run(async () =>
         {
            //  var data = await _sugarDB.GetProcessByBarcodeFuzzyAsync("TestCode");
            Random ran = new Random();
            while (true)
            {
               if (true)
               //if (Temporary.IsRunning)
               {
                  try
                  {
                     List<IBatMainModel> values = new();
                     //for (int i = 0;i < 10;i++)
                     // Parallel.For(0, 8, async (_, i) =>
                     //{
                     #region 前扫码
                     var n = ran.NextInt64(1000000000000, 9000000000000);
                     var n2 = ran.NextInt64(1000000000000, 9000000000000);
                     var mainBattery = (IBatMainModel)
                        Activator.CreateInstance(_displayDataCollection.CompleteBatteryDatas.RuntimeBatteryType);
                     var Id = _snowflakeHelper.NextId();
                     mainBattery.Id = Id;
                     IBatScanBeforeModel batBefScan = (IBatScanBeforeModel)mainBattery;
                     batBefScan.BeforeScanIndex = (byte)ran.Next(1, 5);
                     batBefScan.BeforeScanTime = DateTime.Now;
                     mainBattery.DeviceCode = _parameterConfig.DeviceParameter.DeviceCode;
                     mainBattery.ElectrolyteBatch = _parameterConfig.DeviceParameter.ElectrolyteLotCode;
                     batBefScan.BeforeScanResult = n % 3 == 0 ? ResultTypeEnum.NG : ResultTypeEnum.OK;
                     mainBattery.Barcode = $"TestCode{n}";
                     string logheader = Context.ToProcessLogHeader(id: Id, barcode: mainBattery.Barcode);

                     $"测试了\r\n扫码！".LogProcess(logheader);
                     _batteryCache.Put(mainBattery, "Test"); //电芯缓存
                     values.Add(mainBattery);
                     _displayDataCollection.AddDisplayData(ProcessTypeEnum.前扫码, ProcessRoleEnum.进站, mainBattery); //添加界面显示
                     #endregion

                     #region 前称重
                     var beforeWeight = (IBatWeightBeforeModel)await _batteryCache.GetByIdAsync(Id, "Test"); //取缓存
                     beforeWeight.BeforeWeightTime = DateTime.Now;
                     beforeWeight.BeforWeightRange =
                        $"{_parameterConfig.RunParameter.IncomingWeightUpper}~{_parameterConfig.RunParameter.IncomingWeightLower}";
                     beforeWeight.BeforeWeightIndex = (byte)ran.Next(1, 9);
                     beforeWeight.BeforeWeight =
                        ran.NextInt64(
                           (long)(_parameterConfig.RunParameter.IncomingWeightLower - 20) * 100,
                           (long)(_parameterConfig.RunParameter.IncomingWeightUpper + 20) * 100
                        ) / 100.0f;
                     beforeWeight.BeforeWeightResult = BatteryWeightValidator.IncomingWeightRangeCheck(
                        beforeWeight.BeforeWeight,
                        beforeWeight.BeforWeightRange,
                        _parameterConfig,
                        "Test"
                     );

                     _displayDataCollection.AddDisplayData(ProcessTypeEnum.前称重, ProcessRoleEnum.None, (IBatMainModel)beforeWeight); //添加界面显示
                     #endregion

                     #region 后称重
                     Random random1 = new Random();
                     var batAftWeight = (IBatWeightAfterModel)await _batteryCache.GetByIdAsync(Id, "Test"); //取缓存
                     var autoRefill = (IBatWeightAutoRefillModel)await _batteryCache.GetByIdAsync(Id, "Test"); //取缓存
                     batAftWeight.FirstInjectResult = n2 switch
                     {
                        var r when r % 3 == 0 => ResultTypeEnum.注液量偏少,
                        var r when r % 4 == 0 => ResultTypeEnum.注液量偏多,
                        _ => ResultTypeEnum.OK,
                     };
                     autoRefill.AutoRefillResult = n2 switch
                     {
                        var r when r % 5 == 0 => ResultTypeEnum.注液量偏少,
                        var r when r % 7 == 0 => ResultTypeEnum.注液量偏多,
                        _ => ResultTypeEnum.OK,
                     };
                     batAftWeight.FinalWeighingResult = n2 % 4 == 0 ? ResultTypeEnum.NG : ResultTypeEnum.OK;
                     batAftWeight.AfterWeightTime = DateTime.Now;
                     batAftWeight.AfterWeightIndex = 1;
                     batAftWeight.ActualInjectionVolume = Math.Round(
                        batAftWeight.FinalAfterWeight
                           - ((IBatWeightBeforeModel)batAftWeight).BeforeWeight
                           - _parameterConfig.RunParameter.NailWeight,
                        3
                     );
                     batAftWeight.TargetInjectionVolumeDeviation = Math.Round(
                        batAftWeight.ActualInjectionVolume - ((IBatWeightBeforeModel)batAftWeight).TargetInjectionVolume,
                        3
                     );
                     batAftWeight.TotalInjectionVolume = Math.Round(
                        batAftWeight.FinalAfterWeight - batBefScan.NetWeight - _parameterConfig.RunParameter.NailWeight,
                        3
                     );
                     batAftWeight.TotalInjectionVolumeDeviation = Math.Round(
                        batAftWeight.TotalInjectionVolume - _parameterConfig.RunParameter.InjectionStandard,
                        3
                     );

                     _displayDataCollection.AddDisplayData(ProcessTypeEnum.后称重, ProcessRoleEnum.None, (IBatMainModel)batAftWeight); //添加界面显示
                     #endregion

                     #region 注液
                     var injectStationModel = (IBatInjectStationModel)await _batteryCache.GetByIdAsync(Id, "Test"); //取缓存
                     injectStationModel.InjectPumpNo = (byte)ran.Next(1, 5);
                     injectStationModel.InjectStationNo = (byte)ran.Next(1, 3);
                     injectStationModel.InjectNozzleNo = (byte)ran.Next(1, 25);
                     injectStationModel.LineIndex = (byte)ran.Next(1, 5);
                     injectStationModel.ColumnIndex = (byte)ran.Next(1, 7);
                     injectStationModel[nameof(BatTestLeakModel.LeakResult)] = n2 % 6 == 0 ? ResultTypeEnum.NG : ResultTypeEnum.OK;
                     injectStationModel[nameof(BatNailModel.NailResult)] = n2 % 13 == 0 ? ResultTypeEnum.NG : ResultTypeEnum.OK;
                     injectStationModel[nameof(BatMainModel.MesInputStatus)] = n2 % 16 == 0 ? ResultTypeEnum.NG : ResultTypeEnum.OK;
                     injectStationModel[nameof(BatMainModel.MesOutputStatus)] = n2 % 33 == 0 ? ResultTypeEnum.NG : ResultTypeEnum.OK;
                     injectStationModel[nameof(BatInjectStationModel.TrayCode)] = "F00000" + ran.Next(1, 17).ToString();
                     injectStationModel[nameof(BatInjectStationModel.CupCode)] = "E00000" + ran.Next(1, 17).ToString();

                     _displayDataCollection.AddDisplayData(ProcessTypeEnum.注液, ProcessRoleEnum.出站, (IBatMainModel)injectStationModel); //添加界面显示
                     #endregion

                     var config = new ParameterConfig(_container, false);
                     config.AdvancedConfig.ProductionType = ProductionTypeEnum.二次注液;
                     var mainBattery2 = new ParameterConfig(_container, true);

                     config.DeepSyncTo(mainBattery2);
                     ((ParameterConfig)mainBattery2).AdvancedConfig.ProductionType = ProductionTypeEnum.回氦;
                     //});
                     //}
                     //  _displayPercentageCollection.Save("", "",isPopup:false);

                     var f = await _sugarDB.InsertableByObjectsAsync("Test", values.ToArray());

                     foreach (var item in values)
                     {
                        item.WorkOrderNumber = "QRREB588";
                     }
                     var rr = await _sugarDB.UpdateByObjectsAsync("Test", values.ToArray());
                     values.Clear();
                     Random random = new Random();
                     short plcStatus = (short)random.Next(1, 6);
                     // PLcStatusAndAlarmHandler.PlcStatusHandle(plcStatus, _plcStatusConfig, _snowflakeHelper, _sugarDB, _taskLogHeader);
                  }
                  catch (Exception ex) { }
                  finally
                  {
                     await Task.Delay(500);
                  }
               }
            }
         });
      }

      protected override Task HandleCore(short plcValue)
      {
         throw new NotImplementedException();
      }
   }
}
