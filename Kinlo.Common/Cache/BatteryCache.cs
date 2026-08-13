namespace Kinlo.Common.Cache;

/// <summary>
/// 电池缓存
///  由于缓存用于统计 不再使用 Queue FIFO 直接使用 ConcurrentDictionary
///  删除由业务层主动控制
/// </summary>
public partial class BatteryCache : IBatteryCache
{
   #region field

   /// <summary>
   /// 主缓存
   /// </summary>
   private readonly ConcurrentDictionary<long, IBatMainModel> _cache = new();

   /// <summary>
   /// 条码索引
   /// 注意：
   /// 一个条码默认只保留最新一个电池
   /// 后写入会覆盖前写入
   /// </summary>
   private readonly ConcurrentDictionary<string, long> _barcodeIndex = new(StringComparer.OrdinalIgnoreCase);

   /// <summary>
   /// 防缓存击穿（按ID）
   /// 多线程同时查询同一个ID时：
   /// 只允许一个线程访问数据库
   /// 其它线程等待同一个Task
   /// </summary>
   private readonly ConcurrentDictionary<long, Lazy<Task<IBatMainModel?>>> _pendingById = new();

   /// <summary>
   /// 防缓存击穿（按条码）
   /// 多线程同时查询同一个条码时：
   /// 只允许一个线程访问数据库
   /// </summary>
   private readonly ConcurrentDictionary<string, Lazy<Task<IBatMainModel?>>> _pendingByBarcode = new(StringComparer.OrdinalIgnoreCase);

   /// <summary>
   /// 最开始加载缓存时最小数量
   /// </summary>
   private readonly int _minCapacity;

   /// <summary>
   /// 缓存数量警告
   /// </summary>
   private readonly int _maxCapacity;

   private readonly DisplayDataCollection _displayDataCollection;

   private readonly DbHelper _sugarDB;

   private readonly AppGlobalConfig _appGlobalConfig;

   private readonly ISqlSugarDbFactory _dbFactory;

   private readonly SnowflakeHelper _snowflakeHelper;

   private readonly DisplayDataCollection _displayDatas;

   private readonly Lazy<GlobalStaticTemporary> _globalTemporaryLazy;

   #endregion

   public BatteryCache(IContainer container)
   {
      _displayDataCollection = container.Get<DisplayDataCollection>();
      _sugarDB = container.Get<DbHelper>();
      _appGlobalConfig = container.Get<AppGlobalConfig>();
      _dbFactory = container.Get<ISqlSugarDbFactory>();
      _snowflakeHelper = container.Get<SnowflakeHelper>();
      _displayDatas = container.Get<DisplayDataCollection>();

      _globalTemporaryLazy = new Lazy<GlobalStaticTemporary>(() => container.Get<GlobalStaticTemporary>());

      _minCapacity = 20000;
      // 超过20万报警
      _maxCapacity = 300000;
   }

   #region 缓存预热

   /// <summary>
   /// 缓存预热
   /// 程序启动时：预加载最近数据进入内存
   /// </summary>
   public async Task LoadCache()
   {
      var sw = Stopwatch.StartNew();

      try
      {
         var batteryList = await GetBattereyListAsync();

         if (batteryList == null || batteryList.Count == 0)
         {
            "[缓存预热] 无数据可加载".LogRun(Log4NetLevelEnum.信息);
            return;
         }

         foreach (var item in batteryList.OrderBy(x => x.Id))
         {
            Put(item, "缓存预热");
         }

         $"[缓存预热] 成功加载 {_cache.Count} 条数据，用时:{sw.ElapsedMilliseconds}ms".LogRun(Log4NetLevelEnum.信息);
      }
      catch (Exception ex)
      {
         $"[缓存预热] 异常:{ex}".LogRun(Log4NetLevelEnum.错误);
      }
      finally
      {
         sw.Stop();
      }
   }

   #endregion

   #region Put

   /// <summary>
   /// 添加或更新缓存
   /// </summary>
   public void Put(IBatMainModel value, string logHeader)
   {
      if (value == null)
      {
         $"无法添加 null 缓存".LogProcess(logHeader);
         return;
      }

      try
      {
         // 写入主缓存
         _cache[value.Id] = value;

         // 写入条码索引
         if (!string.IsNullOrWhiteSpace(value.Barcode))
         {
            _barcodeIndex[value.Barcode] = value.Id;
         }

         // 缓存数量报警
         CheckCacheCapacity(logHeader);
      }
      catch (Exception ex)
      {
         $"添加缓存异常:{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }
   }

   #endregion

   #region Remove

   /// <summary>
   /// 批量删除缓存
   /// 外部统计线程计算好需要删除的ID后 调用此方法
   /// </summary>
   public void RemoveByIds(IEnumerable<long> ids)
   {
      foreach (var id in ids)
      {
         // 删除主缓存
         if (_cache.TryRemove(id, out var bat))
         {
            // 删除条码索引
            if (!string.IsNullOrWhiteSpace(bat.Barcode))
            {
               _barcodeIndex.TryRemove(bat.Barcode, out _);
            }
         }
      }
   }

   #endregion

   #region GetById

   /// <summary>
   /// 根据ID获取
   ///
   /// 顺序：
   /// 1. 先查缓存
   /// 2. 缓存没有再查数据库
   /// 3. 自动回填缓存
   /// </summary>
   public async Task<IBatMainModel?> GetByIdAsync(long id, string logHeader, bool useCacheOnly = false)
   {
      try
      {
         // 先查缓存
         if (_cache.TryGetValue(id, out var cached))
         {
            return cached;
         }

         // 仅使用缓存
         if (useCacheOnly)
         {
            return null;
         }

         // 防缓存击穿
         var factory = _pendingById.GetOrAdd(id, _ => new Lazy<Task<IBatMainModel?>>(() => LoadBatteryFromDbByIdAsync(id, logHeader)));

         try
         {
            var result = await factory.Value;

            // 自动回填缓存
            if (result != null)
            {
               Put(result, logHeader);
            }

            return result;
         }
         finally
         {
            // 清除等待任务
            _pendingById.TryRemove(id, out _);
         }
      }
      catch (Exception ex)
      {
         $"根据ID取缓存异常:{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误, true);

         return null;
      }
   }

   #endregion

   #region GetByBarcode

   /// <summary>
   /// 根据条码获取
   ///
   /// 查询流程：
   /// 1. 条码索引
   /// 2. 主缓存
   /// 3. 数据库
   /// </summary>
   public async Task<IBatMainModel?> GetByBarcodeAsync(string barcode, string logHeader, bool useCacheOnly = false)
   {
      if (string.IsNullOrWhiteSpace(barcode))
      {
         return null;
      }

      try
      {
         // 先查条码索引
         if (_barcodeIndex.TryGetValue(barcode, out var id))
         {
            // 再查主缓存
            if (_cache.TryGetValue(id, out var cached))
            {
               return cached;
            }

            // 索引脏数据自动清理
            _barcodeIndex.TryRemove(barcode, out _);
         }

         // 仅使用缓存
         if (useCacheOnly)
         {
            return null;
         }

         // 防缓存击穿
         var factory = _pendingByBarcode.GetOrAdd(
            barcode,
            _ => new Lazy<Task<IBatMainModel?>>(() => LoadBatteryFromDbByBarcodeAsync(barcode, logHeader))
         );

         try
         {
            var result = await factory.Value;

            // 自动回填缓存
            if (result != null)
            {
               Put(result, logHeader);
            }

            return result;
         }
         finally
         {
            // 清除等待任务
            _pendingByBarcode.TryRemove(barcode, out _);
         }
      }
      catch (Exception ex)
      {
         $"根据条码取缓存异常:{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误, true);

         return null;
      }
   }

   #endregion

   #region GetAll

   /// <summary>
   /// 获取全部缓存快照
   /// </summary>
   public IBatMainModel[] GetAll()
   {
      return _cache.Values.ToArray();
   }

   #endregion

   #region 缓存统计

   /// <summary>
   /// 当前缓存数量
   /// </summary>
   public int Count => _cache.Count;

   /// <summary>
   /// 检查缓存数量
   /// </summary>
   private void CheckCacheCapacity(string logHeader)
   {
      var count = _cache.Count;
      // 超过警告值
      if (count > _maxCapacity)
      {
         $"注意：缓存数量:{count} 已超过警告值:{_maxCapacity}".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }
   }

   #endregion

   #region 内部数据库加载

   /// <summary>
   /// 根据ID从数据库加载
   /// </summary>
   private async Task<IBatMainModel?> LoadBatteryFromDbByIdAsync(long id, string logHeader)
   {
      $"缓存无数据，从数据库加载".LogProcess(logHeader);

      var battery = await _sugarDB.GetBatteryByIdAsync(id, logHeader);

      if (battery == null)
      {
         $"数据库未找到数据".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }

      return battery;
   }

   /// <summary>
   /// 根据条码从数据库加载
   /// </summary>
   private async Task<IBatMainModel?> LoadBatteryFromDbByBarcodeAsync(string barcode, string logHeader)
   {
      $"缓存无数据，从数据库加载(条码:{barcode})".LogProcess(logHeader);

      var battery = await _sugarDB.GetLastBattereyByBarcodeAsync(barcode, logHeader);

      if (battery == null)
      {
         $"条码:{barcode} 未找到数据".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }

      return battery;
   }

   #endregion
}
