using System.Diagnostics.CodeAnalysis;
using System.Windows.Data;
using HandyControl.Controls;
using Kinlo.GUI.Helpers;
using Kinlo.SharedBase.Model;

namespace Kinlo.GUI.ViewModel;

[UIDisplayAttribute(true)]
public partial class ProductionHistoryViewModel : Screen
{
   #region propertys
   public ObservableCollection<TimePropertyDisplayMapModel> SelectTimes { get; set; } =
      new ObservableCollection<TimePropertyDisplayMapModel>();

   /// <summary>
   /// 筛选条件Option
   /// </summary>
   public ResultQueryFilterModel FilterOption { get; set; }
   public ParameterConfig ParameterCfg { get; set; }
   #endregion

   #region field
   private OtherParameterConfig _otherParameter;
   private DisplayDataCollection _displayData;
   private IContainer _container;
   private DbHelper _sugarDB;
   private MesInterfaceParameterConfig _mesInterfaceParameterConfig;
   private IBatteryCache _batteryCache;
   private MesService _mesService;
   #endregion
   public ProductionHistoryViewModel(IContainer container)
   {
      _container = container;
      EntityPropertyVisibleVM = container.Get<EntityPropertyVisibleViewModel>();
      _otherParameter = container.Get<OtherParameterConfig>();
      _mesInterfaceParameterConfig = container.Get<MesInterfaceParameterConfig>();
      _sugarDB = container.Get<DbHelper>();
      _displayData = container.Get<DisplayDataCollection>();
      ParameterCfg = container.Get<ParameterConfig>();
      _batteryCache = container.Get<IBatteryCache>();
      _mesService = container.Get<MesService>();

      InitFilterOptions();
      CreateBatteryDataGrid();
   }

   #region 初始化筛选选项

   private readonly ResultRange _ignoreRange = ResultArea.Ignore.GetResultAreaBounds();
   private readonly ResultRange _okRange = ResultArea.OK.GetResultAreaBounds();
   private readonly ResultRange _ngRange = ResultArea.NG.GetResultAreaBounds();

   /// <summary>
   /// 初始化筛选选项
   /// </summary>
   [MemberNotNull(nameof(FilterOption))]
   private void InitFilterOptions()
   {
      var resultFilter = new ResultFilterModel();

      #region 添加MES组
      ProcessFilterGroup mesGroup = new ProcessFilterGroup("MES");
      string mesInputName = nameof(BatMainModel.MesInputStatus);
      string mesOutputName = nameof(BatMainModel.MesOutputStatus);
      mesGroup.Items =
      [
         new ProcessFilterCondition(resultFilter, mesInputName, "未进站", _ignoreRange),
         new ProcessFilterCondition(resultFilter, mesInputName, "进站失败", _ngRange),
         new ProcessFilterCondition(resultFilter, mesInputName, "进站成功", _okRange),
         new ProcessFilterCondition(resultFilter, mesOutputName, "未出站", _ignoreRange),
         new ProcessFilterCondition(resultFilter, mesOutputName, "出站失败", _ngRange),
         new ProcessFilterCondition(resultFilter, mesOutputName, "出进成功", _okRange),
      ];
      resultFilter.FilterGroups.Add(mesGroup);
      #endregion

      #region 添加注液组（如果有注液工序的话）
      //注液
      string injName = nameof(BatWeightAfterModel.FinalInjectResult);
      if (_displayData.CompleteBatteryDatas.PropertyBindings.Any(x => x.BindingPaht == injName))
      {
         ProcessFilterGroup injGroup = new ProcessFilterGroup("注液");

         injGroup.Items =
         [
            new ProcessFilterCondition(
               resultFilter,
               injName,
               "注液过少",
               new((int)ResultTypeEnum.注液量偏少, (int)ResultTypeEnum.注液量偏少)
            ),
            new ProcessFilterCondition(
               resultFilter,
               injName,
               "注液过多",
               new((int)ResultTypeEnum.注液量偏多, (int)ResultTypeEnum.注液量偏多)
            ),
         ];
         resultFilter.FilterGroups.Add(injGroup);
      }
      #endregion

      #region 动态生成时间及其它工序组
      ProcessFilterGroup otherGroup = new ProcessFilterGroup("其它");
      foreach (var item in _displayData.CompleteBatteryDatas.PropertyBindings)
      {
         //添加时间
         if (
            item.PropertyType == typeof(DateTime)
            && item.BindingPaht != nameof(BatMainModel.Id)
            && item.BindingPaht != nameof(BatMainModel.CreateTime)
            && item.BindingPaht != nameof(BatScanBeforeModel.BeforeScanTime)
            && item.BindingPaht != nameof(BatMainModel.OutputTime)
         )
         {
            SelectTimes.Add(new TimePropertyDisplayMapModel { PropertyName = item.BindingPaht, Display = item.Description });
         }

         //添加其它组
         if (
            item.PropertyType == typeof(ResultTypeEnum)
            && item.BindingPaht != mesInputName
            && item.BindingPaht != mesOutputName
            && item.BindingPaht != nameof(BatMainModel.FinalStatus)
         )
         {
            otherGroup.Items.Add(new ProcessFilterCondition(resultFilter, item.BindingPaht, item.Description, _ngRange, "NG"));
         }
      }
      resultFilter.FilterGroups.Add(otherGroup);
      #endregion

      #region 添加进站及出站时间
      var now = DateTime.Now;
      var inputTimeMap = GenericHelper.BuildInputTimeMap();
      var outputTimeMap = GenericHelper.GetOutputTimeMap();
      SelectTimes.Insert(0, outputTimeMap);
      SelectTimes.Insert(0, inputTimeMap);

      var timeFilter = new TimeFilterModel
      {
         PropertyDisplayMap = inputTimeMap,
         EndTime = now,
         StartTime = now.AddDays(-1),
      };
      #endregion

      FilterOption = new ResultQueryFilterModel(timeFilter, resultFilter);
   }
   #endregion


   #region 查询
   /// <summary>
   ///
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   public async void PaginationPageUpdatedCMD(object sender, HandyControl.Data.FunctionEventArgs<int> e)
   {
      PageIndex = e.Info;
      if (DataCountPerPage > 2000)
      {
         Growl.Warning("单页数据量请勿大于2000！");
         return;
      }

      sw.Restart();
      DataList = await QueryData(FilterOption, true, false);
      DisplayView = CollectionViewSource.GetDefaultView(DataList);
      sw.Stop();
      $"查询用时:{sw.ElapsedMilliseconds}ms".LogRun();
   }

   Stopwatch sw = Stopwatch.StartNew();

   /// <summary>
   /// 查询
   /// </summary>
   public async void QueryCMD()
   {
      if (DataCountPerPage > 2000)
      {
         Growl.Warning("单页数据量请勿大于2000！");
         return;
      }
      sw.Restart();
      DataList = await QueryData(FilterOption, true, true);
      DisplayView = CollectionViewSource.GetDefaultView(DataList);
      sw.Stop();
      $"查询用时:{sw.ElapsedMilliseconds}ms".LogRun();
   }

   /// <summary>
   /// 导出当天数据
   /// </summary>
   public async void ExportExcelDailyDataCMD()
   {
      sw.Restart();
      DateTime now = DateTime.Now;

      var timeFilter = new TimeFilterModel
      {
         StartTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0),
         EndTime = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59),
         PropertyDisplayMap = GenericHelper.BuildInputTimeMap(),
      };
      var resultFilterModel = new ResultFilterModel();
      var filter = new ResultQueryFilterModel(timeFilter, resultFilterModel);

      var data = await QueryData(filter, false, true);
      if (data != null)
      {
         Dialog? Dialog = null;
         try
         {
            Dialog = Dialog.Show(GenericHelper.CreateLoadingCircle(), ProductionHistoryLayoutViewModel.DialogToke);
            await Task.Run(() => ExcelHelper.ExportBattery(data, _otherParameter, _displayData.CompleteBatteryDatas.PropertyBindings));
         }
         finally
         {
            Dialog?.Close();
         }
      }
      sw.Stop();
      $"导出当天数据用时:{sw.ElapsedMilliseconds}ms".LogRun();
   }

   /// <summary>
   /// 导出数据
   /// </summary>
   public async void ExportExcelCMD()
   {
      sw.Restart();
      var data = await QueryData(FilterOption, false, true);
      if (data != null)
      {
         Dialog? Dialog = null;
         try
         {
            Dialog = Dialog.Show(GenericHelper.CreateLoadingCircle(), ProductionHistoryLayoutViewModel.DialogToke);
            await Task.Run(() => ExcelHelper.ExportBattery(data, _otherParameter, _displayData.CompleteBatteryDatas.PropertyBindings));
         }
         finally
         {
            Dialog?.Close();
         }
      }
      sw.Stop();
      $"导出数据用时:{sw.ElapsedMilliseconds}ms".LogRun();
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="isDisplay">导出数据或展示数据</param>
   /// <param name="isFirst">展示数据第一次查询（非分页）</param>
   /// <returns></returns>
   private async Task<List<ExpandoObject>> QueryData(ResultQueryFilterModel filter, bool isDisplay, bool isFirst)
   {
      if (!filter.Validate(out var msg))
      {
         Growl.Warning(msg);
         return new List<ExpandoObject>();
      }

      var dialog = Dialog.Show(GenericHelper.CreateLoadingCircle(), ProductionHistoryLayoutViewModel.DialogToke);

      try
      {
         var queryFilter = filter.ToQueryFilter();
         var sql = await _sugarDB.GetQueryableByBarcdoe(queryFilter);

         if (sql == null || string.IsNullOrEmpty(sql))
         {
            TotalCount = 0;
            TotalPage = 0;
            return new List<ExpandoObject>();
         }
         var factory = _container.Get<ISqlSugarDbFactory>();
         using var db = factory.CreateClient(DatabaseRole.LocalDb1);
         if (db == null)
            return new List<ExpandoObject>();

         var sugarQueryable = db.SqlQueryable<ExpandoObject>(sql);
         if (isDisplay)
         {
            List<ExpandoObject> queryData = new();
            if (isFirst) //展示数据第一次查询（非分页）
            {
               RefAsync<int> totalCount = 0; //异步 REF和OUT不支持异步
               RefAsync<int> totalPage = 0; //异步 REF和OUT不支持异步
               queryData = await sugarQueryable.ToOffsetPageAsync(PageIndex, DataCountPerPage, totalCount, totalPage);
               TotalCount = totalCount.Value;
               TotalPage = totalPage.Value;
            }
            else
            {
               queryData = await sugarQueryable.ToOffsetPageAsync(PageIndex, DataCountPerPage);
            }
            return queryData;
         }
         else
         {
            return await Task.Run(() => sugarQueryable.ToList());
         }
      }
      catch (Exception ex)
      {
         $"[查询数据]出现异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
      finally
      {
         dialog.Close();
      }
      return new List<ExpandoObject>();
   }

   #endregion
}
