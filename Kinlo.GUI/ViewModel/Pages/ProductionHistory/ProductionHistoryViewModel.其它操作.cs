using System.Diagnostics.CodeAnalysis;
using HandyControl.Controls;
using Kinlo.Services.PeriodicTasks;

namespace Kinlo.GUI.ViewModel;

public partial class ProductionHistoryViewModel : Screen
{
   #region propertys

   /// <summary>
   /// 数据总量
   /// </summary>
   public int TotalCount { get; set; }

   /// <summary>
   /// 总页数
   /// </summary>
   public int TotalPage { get; set; }

   /// <summary>
   /// 选中的页面索引
   /// </summary>
   public int PageIndex { get; set; } = 1;

   private int _dataCountPerPage = 25;

   /// <summary>
   /// 每页数量
   /// </summary>
   public int DataCountPerPage
   {
      get { return _dataCountPerPage; }
      set
      {
         if (_dataCountPerPage != value)
         {
            _dataCountPerPage = value;
            if (DataList != null) //如果有数据 即重新查询
            {
               QueryCMD();
            }
         }
      }
   }
   public EntityPropertyVisibleViewModel EntityPropertyVisibleVM { get; set; }
   public List<ExpandoObject> DataList { get; set; } = new();

   /// <summary>
   /// 显示数据的View
   /// </summary>
   public System.ComponentModel.ICollectionView? DisplayView { get; private set; }
   public object ShowGridData { get; set; }

   [Inject]
   private IWindowManager _windowManager { get; set; } = null!;
   #endregion

   /// <summary>
   /// 修正胶钉
   /// </summary>
   /// <param name="listView"></param>
   /// <returns></returns>
   public async Task CorrectingSealingNail(ListView? listView)
   {
      if (listView == null || listView.SelectedItems.Count == 0)
      {
         Growl.Warning("请先选择列！");
         return;
      }
      foreach (var item in listView.SelectedItems)
      {
         try
         {
            //var _batMain = item as BatMainModel;
            //if (_batMain != null)
            //{
            //    BatNailModel? _batNail = _batMain.GetProcess<BatNailModel>();
            //    if (_batNail != null)
            //    {
            //        _batNail.AfterNailResult = ResultTypeEnum.合格;
            //        //更新指定列
            //        var _upDic = new Dictionary<string, object>
            //        {
            //            { nameof(BatMainModel.Id),_batMain.Id },
            //            { nameof(BatMainModel.FinalStatus),_batMain.FinalStatus },
            //        };

            //        StringBuilder stringBuilder = new StringBuilder();
            //        stringBuilder.Append($"ID:{_batMain.Id}，条码：{_batMain.Barcode},密封钉结果修正为[{ResultTypeEnum.合格}];");
            //        if (await _sugarDB.UpdateBatteryAsync(_batNail))
            //        {
            //            if (await _sugarDB.UpdateColumnsAsync<BatMainModel>(_upDic, _batMain.Id, _batMain.Barcode))
            //            {
            //                stringBuilder.ToString().LogRun(Log4NetLevelEnum.成功);
            //                Growl.Success(stringBuilder.ToString());
            //            }
            //            else
            //            {
            //                Growl.Warning($"密封钉条码：{_batMain.Barcode},修正失败!");
            //            }
            //        }
            //        else
            //        {
            //            Growl.Warning($"密封钉条码：{_batMain.Barcode},修正失败!");
            //        }

            //    }
            //}
         }
         catch (Exception ex)
         {
            $"[修正胶钉]异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         }
      }
   }

   /// <summary>
   /// MES进站
   /// </summary>
   /// <param name="listView"></param>
   /// <returns></returns>
   public async Task MesInbound(ListView? listView)
   {
      //try
      //{
      //    if (listView == null || listView.SelectedItems.Count == 0)
      //    {
      //        Growl.Warning("请先选择列！");
      //        return;
      //    }
      //    var dictionarys = listView.SelectedItems.OfType<IDictionary<string, object>>();
      //    var ids = dictionarys.Select(x => (long)x[nameof(BatMainModel.Id)]).ToList();
      //    await Task.Run(async () =>
      //    {
      //        foreach (var id in ids)
      //        {
      //            string logHeader = $"[手动MES进站]ID:{id}！";
      //            var mainBattery = await _batteryCache.GetByIdAsync(id, logHeader);

      //            if (mainBattery != null)
      //            {
      //                var call = _mesInterfaceParameterConfig.GetApiCall(new MesRequestBuildNJGX.ArgsProductEntry(mainBattery.Barcode));
      //                if (call == null || !call.IsEnable)
      //                {
      //                    Growl.Warning($"[手动MES进站]接口未启用或未找到接口信息！");
      //                    return;
      //                }
      //                var mesReslut = await _mesService.SendAsync(call, mainBattery.Barcode,
      //                    receive => receive.MesGeneralParse(logHeader));

      //                mainBattery.MesInputStatus = mesReslut.ResultStatus switch
      //                {
      //                    MesResultStatusEnum.成功 => ResultTypeEnum.OK,
      //                    MesResultStatusEnum.MES判定NG => ResultTypeEnum.MES判定NG,
      //                };

      //                logHeader = $"[手动MES进站]条码：{mainBattery.Barcode},ID:{mainBattery.Id},MES结果：{mesReslut.ResultStatus}！";
      //                logHeader.LogRun(mainBattery.MesInputStatus == ResultTypeEnum.OK ? Log4NetLevelEnum.成功 : Log4NetLevelEnum.错误, true);
      //                //更新指定列
      //                var _upDic = new Dictionary<string, object>
      //                {
      //                       { nameof(BatMainModel.Id),mainBattery.Id },
      //                       { nameof(BatMainModel.MesInputStatus), mainBattery.MesInputStatus },
      //                       { nameof(BatMainModel.FinalStatus), mainBattery.FinalStatus },
      //                };

      //                if (!await _sugarDB.UpdateColumnsAsync(_upDic, mainBattery.Id, mainBattery.Barcode, logHeader))
      //                {
      //                    logHeader = $"[手动MES进站]条码：{mainBattery.Barcode},ID:{mainBattery.Id} ,保存数据失败!";
      //                    mainBattery.MesInputStatus = ResultTypeEnum.保存数据库失败;
      //                    logHeader.LogRun(mainBattery.MesInputStatus == ResultTypeEnum.OK ? Log4NetLevelEnum.成功 : Log4NetLevelEnum.错误, true);
      //                }

      //                UIThreadHelper.InvokeOnUiThreadAsync(() =>
      //                {
      //                    var dictionary = dictionarys.FirstOrDefault(x => (long)x[nameof(BatMainModel.Id)] == mainBattery.Id);
      //                    dictionary[nameof(BatMainModel.MesInputStatus)] = mainBattery.MesInputStatus;
      //                    dictionary[nameof(BatMainModel.FinalStatus)] = mainBattery.FinalStatus;
      //                });
      //            }
      //        }
      //    });
      //}
      //catch (Exception ex)
      //{
      //    $"[手动MES进站]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      //}
   }

   /// <summary>
   /// MES出站
   /// </summary>
   /// <param name="listView"></param>
   /// <returns></returns>
   public async Task MesOutbound(ListView? listView)
   {
      try
      {
         if (listView == null || listView.SelectedItems.Count == 0)
         {
            Growl.Warning("请先选择列！");
            return;
         }
         var dictionarys = listView.SelectedItems.OfType<IDictionary<string, object>>();
         var ids = dictionarys.Select(x => (long)x[nameof(BatMainModel.Id)]).ToList();
         await Task.Run(
            (Func<Task?>)(
               async () =>
               {
                  StringBuilder stringBuilder = new StringBuilder();
                  List<MesResendModel> upResends = new List<MesResendModel>();
                  List<IBatMainModel> upBatterys = new();
                  foreach (var id in ids)
                  {
                     string logHeader = $"[手动出站-id{id}]";
                     var batMain = await _batteryCache.GetByIdAsync(id, logHeader);
                     if (batMain != null)
                     {
                        var sendResult = await MesOutboundHelper.MesOutput(_container, _mesService, batMain, logHeader);
                        if (sendResult == OutputStatus.未上传)
                        {
                           Growl.Warning("接口未开启，请开启后再上传；");
                           return;
                        }

                        if (sendResult is OutputStatus.成功 or OutputStatus.MES判定NG) //只有成功或失败才需更新补传表
                        {
                           upBatterys.Add(batMain);
                           stringBuilder.AppendLine(
                              $"[手动MES出站]条码：{batMain.Barcode},ID:{batMain.Id},MES结果：{batMain.MesOutputStatus}！"
                           );
                           var factory = _container.Get<ISqlSugarDbFactory>();
                           var resend = await factory.UsingDbAsync(async db =>
                              await db.Queryable<MesResendModel>().FirstAsync(x => x.Id == batMain.Id)
                           ); //取MES补传表

                           if (resend != null)
                           {
                              resend.ResendCount++;
                              resend.LastResult = batMain.MesOutputStatus;
                              resend.LastUpdateTime = batMain.OutputTime;
                              resend.ResendStatus = sendResult switch
                              {
                                 OutputStatus.成功 => ResendStatusEnum.上传成功,
                                 _ => ResendStatusEnum.上传失败,
                              };
                              upResends.Add(resend);
                           }
                        }
                        else
                        {
                           stringBuilder.AppendLine(
                              $"[手动MES出站]条码：{batMain.Barcode},ID:{batMain.Id},MES结果：{batMain.MesOutputStatus}，不更新数据！"
                           );
                        }
                     }
                  }
                  stringBuilder.ToString().LogProcess("[手动MES出站]", isPrompt: true);
                  if (
                     await PeriodicTasksHelper.UpdateTran(_sugarDB, upBatterys, upResends, _batteryCache, "[手动MES出站]")
                     == PeriodicTasksHelper.ResendResultEnum.成功
                  )
                     "出站后更新成功".LogProcess("[手动MES出站]", Log4NetLevelEnum.成功);
                  else
                     "出站后更新失败".LogProcess("[手动MES出站]", Log4NetLevelEnum.错误, isPrompt: true);
               }
            )
         );
      }
      catch (Exception ex)
      {
         $"[手动MES出站]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   #region UI
   /// <summary>
   /// 调整列
   /// </summary>
   /// <param name="processesType"></param>
   public void AdjustCmd()
   {
      _displayData.CompleteBatteryDatas.PropertyBindings.ForEach(propertyBinding =>
      {
         propertyBinding.IsSelected = false;
      });
      EntityPropertyVisibleVM.DisplayData = _displayData.CompleteBatteryDatas;
   }

   [MemberNotNull(nameof(ShowGridData))]
   public void CreateBatteryDataGrid()
   {
      bool isGridData = false; //如果想切换可以把此选项加入设置，DataGrid相比ListView 性能太差
      if (isGridData)
         ShowGridData = CreateControlHelper.CreateDataGrid(
            $"{nameof(DisplayView)}",
            _displayData.CompleteBatteryDatas.PropertyBindings,
            true
         );
      else
      {
         ShowGridData = CreateControlHelper.CreateListView(
            $"{nameof(DisplayView)}",
            _displayData.CompleteBatteryDatas.PropertyBindings,
            true,
            true
         );

         #region 出站菜单
         //修正胶钉
         //MenuItem _menuItemCorrectingSealingNail = new MenuItem();
         //_menuItemCorrectingSealingNail.SetResourceReference(MenuItem.HeaderProperty, "修正胶钉");
         //_menuItemCorrectingSealingNail.Command = AsyncCommand.Create(async () => await CorrectingSealingNail(ShowGridData as ListView));

         //MES进站
         //MenuItem menuItemOutbound = new MenuItem();
         //menuItemOutbound.SetResourceReference(MenuItem.HeaderProperty, "MES进站");
         //menuItemOutbound.Command = AsyncCommand.Create(async () => await MesInbound(ShowGridData as ListView));
         //MES出站
         MenuItem menuItemInbound = new MenuItem();
         menuItemInbound.SetResourceReference(MenuItem.HeaderProperty, "MES出站");
         menuItemInbound.Command = AsyncCommand.Create(async () => await MesOutbound(ShowGridData as ListView));
         //MES进站及出站
         //MenuItem menuItemInAndOut = new MenuItem();
         //menuItemInAndOut.SetResourceReference(MenuItem.HeaderProperty, "MES进出站");
         //menuItemInAndOut.Command = AsyncCommand.Create(async () =>
         //{
         //    await MesInbound(ShowGridData as ListView);
         //    await MesOutbound(ShowGridData as ListView);
         //});
         MenuItem menuHipotCurveExport = new MenuItem();
         menuHipotCurveExport.SetResourceReference(MenuItem.HeaderProperty, "显示Hipot波形图表");
         menuHipotCurveExport.Command = AsyncCommand.Create(async () => await HipotCurveShowAsync(ShowGridData as ListView));

         ContextMenu contextMenu = new ContextMenu();
         // contextMenu.Items.Add(_menuItemCorrectingSealingNail);
         contextMenu.Items.Add(menuItemInbound);
         contextMenu.Items.Add(menuHipotCurveExport);
         //contextMenu.Items.Add(menuItemOutbound);
         //contextMenu.Items.Add(menuItemInAndOut);
         ((ListView)ShowGridData).ContextMenu = contextMenu;
         #endregion
      }
   }
   #endregion
}
