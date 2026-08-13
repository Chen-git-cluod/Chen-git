using System.Data;
using System.Dynamic;
using HandyControl.Tools.Extension;
using NPOI.HSSF.Record;

namespace Kinlo.Common.DAL;

public partial class DbHelper
{
   #region 电芯查询相关

   /// <summary>
   ///  按进站时间范围取数据
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="batteryId"></param>
   /// <returns></returns>
   public async Task<List<T>?> GetDatasByInputTimeRangeAsync<T>(DateTime startTime, DateTime endTime, string exp)
      where T : class, IRownNumber, new() =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnGetTimeRangeDataAsync<T>(db, startTime, endTime, exp));

   private async Task<List<T>> OnGetTimeRangeDataAsync<T>(ISqlSugarClient db, DateTime startTime, DateTime endTime, string exp)
      where T : class, IRownNumber, new()
   {
      try
      {
         long startId = _snowflakeHelper.GetMinIdFromDateTime(startTime);
         long endId = _snowflakeHelper.GetMaxIdFromDateTime(endTime);
         //var monthCount = endTime.Month - startTime.Month + 1;
         var monthCount = startTime.GetMonthCount(endTime);

         List<ISugarQueryable<T>> methods = new();
         for (int i = 0; i < monthCount; i++)
         {
            var tableName = db.SplitHelper<T>().GetTableName(startTime.AddMonths(i)); //根据时间获取表名
            if (!db.DbMaintenance.IsAnyTable(tableName, false))
            {
               $"[获取时间范围内数据]无此表：[{tableName}]".LogRun(Log4NetLevelEnum.警告);
               continue;
            }
            var processData = db.Queryable<T>().AS(tableName).Where(x => x.Id >= startId && x.Id <= endId);

            if (!string.IsNullOrEmpty(exp))
               processData.Where(exp);

            methods.Add(processData);
         }
         if (methods.Count > 0)
         {
            var results = await db.UnionAll(methods).Select((x) => new T { RowNumber = SqlFunc.RowNumber(x.Id) }, true).ToListAsync();
            return results;
         }
      }
      catch (Exception ex)
      {
         $"[获取时间范围内数据]异常,开始时间：{startTime:yyyy-MM-dd HH:mm:ss},结束时间：{endTime:yyyy-MM-dd HH:mm:ss}，详情：{ex}".LogRun(
            Log4NetLevelEnum.错误
         );
      }
      return new List<T>();
   }

   /// <summary>
   /// 按出站时间范围取电芯数据
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="startTime"></param>
   /// <param name="endTime"></param>
   /// <param name="byInputTime"></param>
   /// <param name="isFuzzyQuery"></param>
   /// <returns></returns>
   public async Task<List<T>?> GetBatterysByOutputTimeRangeAsync<T>(QueryFilterDto option)
      where T : class, new() =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await GetBatterysByOutputTimeRangeAsync<T>(db, option));

   private async Task<List<T>> GetBatterysByOutputTimeRangeAsync<T>(ISqlSugarClient db, QueryFilterDto option)
      where T : class, new()
   {
      try
      {
         string sql = await GetQueryableByBarcdoe(option);
         if (sql == null || string.IsNullOrEmpty(sql))
         {
            return new List<T>();
         }
         var sugarQueryable = db.SqlQueryable<T>(sql);
         return await Task.Run(() => sugarQueryable.ToList());
      }
      catch (Exception ex)
      {
         $"[按出站时间范围取电芯数据]异常,开始时间：{option.TimeFilter.StartTime:yyyy-MM-dd HH:mm:ss},结束时间：{option.TimeFilter.EndTime:yyyy-MM-dd HH:mm:ss}，详情：{ex}".LogRun(
            Log4NetLevelEnum.错误
         );
      }
      return new List<T>();
   }

   public record OeeStateDto(DateTime CreateTime, DateTime MesOutputTime, ResultTypeEnum FinalStatus, ProcessTypeEnum NgProcesses);

   /// <summary>
   /// 按时间范围查询部分字段（进出站）并按创建时间排序
   /// </summary>
   /// <param name="startTime">为进站时间</param>
   /// <param name="endTime">为出站时间</param>
   /// <returns></returns>
   public async Task<List<OeeStateDto>> GetBattereyListByTimeRangeAsync(DateTime startTime, DateTime endTime) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnGetBattereyListByTimeRangeAsync(db, startTime, endTime));

   private async Task<List<OeeStateDto>> OnGetBattereyListByTimeRangeAsync(ISqlSugarClient db, DateTime startTime, DateTime endTime)
   {
      var monthCount = startTime.GetMonthCount(endTime);
      List<OeeStateDto> batterys = new();
      Type type = _displayDatas.CompleteBatteryDatas.RuntimeBatteryType!;
      try
      {
         for (int i = 0; i < monthCount; i++)
         {
            var tableName = GetSplitTableNameByType(type, startTime.AddMonths(i)); //根据时间获取表名
            if (!db.DbMaintenance.IsAnyTable(tableName, false))
            {
               $"[按时间范围查询电池（进出站）]无此表[{tableName}]!".LogRun(Log4NetLevelEnum.信息);
               continue;
            }

            var datas = await db.QueryableByObject(type)
               .AS(tableName)
               .Where(
                  $"{nameof(BatMainModel.CreateTime)}>=@createTime AND {nameof(IBatMainModel.OutputTime)}<@outputTime",
                  new { createTime = startTime, outputTime = endTime }
               )
               .Select(
                  $"{nameof(OeeStateDto.CreateTime)},{nameof(OeeStateDto.MesOutputTime)},{nameof(OeeStateDto.FinalStatus)},{nameof(OeeStateDto.NgProcesses)}"
               )
               .OrderBy(
                  new List<OrderByModel>
                  {
                     new OrderByModel { FieldName = nameof(BatMainModel.Id), OrderByType = OrderByType.Asc },
                  }
               )
               .ToDataTableAsync();

            if (datas is DataTable dt)
            {
               foreach (DataRow row in dt.Rows)
               {
                  batterys.Add(
                     new OeeStateDto(
                        // 索引取值，性能好
                        Convert.ToDateTime(row[0]),
                        Convert.ToDateTime(row[1]),
                        (ResultTypeEnum)Convert.ToInt32(row[2]),
                        (ProcessTypeEnum)Convert.ToInt32(row[3])
                     )
                  );
               }
            }
         }
      }
      catch (Exception ex)
      {
         $"[按时间范围查询电池（进出站）]异常,详情：{ex}".LogRun(Log4NetLevelEnum.错误);
      }

      return batterys;
   }

   /// <summary>
   /// 查最近生产电芯
   /// </summary>
   /// <param name="barcode"></param>
   /// <param name="months">要查几个月</param>
   /// <returns></returns>
   public async Task<List<IBatMainModel>> GetBattereyListAsync(int count = 3000) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnGetBattereyListAsync(db, count)) ?? [];

   private async Task<List<IBatMainModel>> OnGetBattereyListAsync(ISqlSugarClient db, int count = 3000)
   {
      List<IBatMainModel> batterys = new();
      int months = 2;
      int queryCount = count;
      DateTime dateTime = DateTime.Now;
      Type type = _displayDatas.CompleteBatteryDatas.RuntimeBatteryType!;
      try
      {
         for (int i = 0; i < months; i++)
         {
            var tableName = GetSplitTableNameByType(type, dateTime.AddMonths(-i)); //根据时间获取表名
            if (!db.DbMaintenance.IsAnyTable(tableName, false))
            {
               $"[查询最近生产电芯]无此表[{tableName}]!".LogRun(Log4NetLevelEnum.信息);
               continue;
            }

            var datas = await db.QueryableByObject(type)
               .AS(tableName)
               .OrderBy(
                  new List<OrderByModel>
                  {
                     new OrderByModel { FieldName = nameof(BatMainModel.Id), OrderByType = OrderByType.Desc },
                  }
               )
               .ToPageListAsync(1, queryCount);

            var row = datas as IEnumerable;
            if (row != null)
            {
               foreach (var item in row)
               {
                  batterys.Add((IBatMainModel)item);
               }
            }
            if (batterys.Count >= count)
               return batterys;
            queryCount -= batterys.Count;
         }
      }
      catch (Exception ex)
      {
         $"[根据条码查最近生产电芯]异常,详情：{ex}".LogRun(Log4NetLevelEnum.错误);
      }

      return batterys;
   }

   /// <summary>
   /// ID查询数据(泛型)
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="Id"></param>
   /// <returns></returns>
   public async Task<T?> QueryableByIdAsync<T>(long Id, string logHeader)
      where T : class, new() =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnQueryableByIdAsync<T>(db, Id, logHeader));

   private async Task<T?> OnQueryableByIdAsync<T>(ISqlSugarClient db, long Id, string logHeader)
      where T : class, new()
   {
      try
      {
         if (Id == 0)
            return null;
         var tableName = db.SplitHelper<T>().GetTableName(SnowflakeHelper.GetDateTimeFromId(Id)); //根据时间获取表名
         if (!db.DbMaintenance.IsAnyTable(tableName, false))
         {
            $"[根据ID查询数据(泛型)]无此表[{tableName}],ID：{Id}".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return null;
         }
         var result = await db.Queryable<T>().AS(tableName).InSingleAsync(Id);
         return result;
      }
      catch (Exception ex)
      {
         $"[根据ID查询数据(泛型)]异常,ID：{Id},详情：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }
      return null;
   }

   /// <summary>
   /// ID查询电芯
   /// </summary>
   /// <param name="type"></param>
   /// <param name="Id"></param>
   /// <returns></returns>
   public async Task<IBatMainModel?> GetBatteryByIdAsync(long Id, string logHeader) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnGetBatteryByIdAsync(db, Id, logHeader));

   private async Task<IBatMainModel?> OnGetBatteryByIdAsync(ISqlSugarClient db, long Id, string logHeader)
   {
      try
      {
         Type type = _displayDatas.CompleteBatteryDatas.RuntimeBatteryType!;
         if (Id == 0)
            return null;
         string tableName = GetSplitTableNameByType(type, SnowflakeHelper.GetDateTimeFromId(Id)); //根据时间获取表名
         if (!db.DbMaintenance.IsAnyTable(tableName, false))
         {
            $"[根据ID查询数据]无此表[{tableName}],ID：{Id}".LogProcess(logHeader, Log4NetLevelEnum.错误);
            return null;
         }
         var battery = await db.QueryableByObject(type).AS(tableName).InSingleAsync(Id);
         if (battery != null)
            return (IBatMainModel)battery;
      }
      catch (Exception ex)
      {
         $"[根据ID查询数据]异常,ID：{Id},详情：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }
      return null;
   }

   /// <summary>
   /// 根据ID批量查询
   /// </summary>
   /// <param name="ids"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public async Task<List<IBatMainModel>> GetBatteryListByIdsAsync(string logHeader, params long[] ids) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnGetBatteryListByIdsAsync(db, logHeader, ids)) ?? [];

   private async Task<List<IBatMainModel>> OnGetBatteryListByIdsAsync(ISqlSugarClient db, string logHeader, params long[] ids)
   {
      List<IBatMainModel> batterys = new List<IBatMainModel>();
      try
      {
         Type type = _displayDatas.CompleteBatteryDatas.RuntimeBatteryType!;
         Dictionary<string, List<long>> tableNames = new();
         foreach (var id in ids)
         {
            string tableName = GetSplitTableNameByType(type, SnowflakeHelper.GetDateTimeFromId(id)); //根据时间获取表名
            if (!db.DbMaintenance.IsAnyTable(tableName, false))
            {
               $"[根据多ID查询数据]无此表[{tableName}],ID：{id},忽略ID;".LogProcess(logHeader, Log4NetLevelEnum.错误);
            }
            else
            {
               if (tableNames.TryGetValue(tableName, out var nameList))
               {
                  nameList.Add(id);
               }
               else
               {
                  tableNames[tableName] = [id];
               }
            }
         }

         foreach (var item in tableNames)
         {
            var obj = await db.QueryableByObject(type).AS(item.Key).Where("Id IN (@ids)", new { ids = item.Value }).ToListAsync();
            var list = ((IEnumerable)obj).Cast<IBatMainModel>().ToList();
            if (list.Count > 0)
            {
               batterys.AddRange(list);
            }
         }
      }
      catch (Exception ex)
      {
         $"[根据多ID查询数据]异常,详情：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }

      return batterys;
   }

   /// <summary>
   /// 根据条码查最近生产电芯
   /// </summary>
   /// <param name="barcode"></param>
   /// <param name="months">要查几个月</param>
   /// <returns></returns>
   public async Task<IBatMainModel?> GetLastBattereyByBarcodeAsync(string barcode, string logHeader, int months = 2) =>
      await _dbFactory.UsingDbAsync(
         DatabaseRole.LocalDb1,
         async db => await OnGetLastBattereyByBarcodeAsync(db, barcode, logHeader, months)
      );

   private async Task<IBatMainModel?> OnGetLastBattereyByBarcodeAsync(ISqlSugarClient db, string barcode, string logHeader, int months = 2)
   {
      DateTime dateTime = DateTime.Now;
      Type type = _displayDatas.CompleteBatteryDatas.RuntimeBatteryType!;
      try
      {
         for (int i = 0; i < months; i++)
         {
            var tableName = GetSplitTableNameByType(type, dateTime.AddMonths(-i)); //根据时间获取表名
            if (!db.DbMaintenance.IsAnyTable(tableName, false))
            {
               $"[根据条码查最近生产电芯]无此表[{tableName}],条码：{barcode}".LogProcess(logHeader, Log4NetLevelEnum.信息);
               continue;
            }

            var battery = await db.QueryableByObject(type)
               .AS(tableName)
               .Where($"{nameof(BatMainModel.Barcode)}=@barcode", new { barcode = barcode })
               .OrderBy(
                  new List<OrderByModel>
                  {
                     new OrderByModel { FieldName = nameof(BatMainModel.Id), OrderByType = OrderByType.Desc },
                  }
               )
               .FirstAsync();
            if (battery != null)
               return battery as IBatMainModel;
         }
      }
      catch (Exception ex)
      {
         $"[根据条码查最近生产电芯]异常,条码：{barcode},详情：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }
      return null;
   }

   /// <summary>
   /// 根据条码模糊查询数据
   /// </summary>
   /// <param name="barcode"></param>
   /// <param name="days">查最近几天</param>
   /// <returns></returns>
   public async Task<ObservableCollection<IBatMainModel>> GetProcessByBarcodeFuzzyAsync(string barcode, string logHeader, int days = 10) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnGetProcessByBarcodeFuzzyAsync(db, barcode, logHeader, days))
      ?? [];

   private async Task<ObservableCollection<IBatMainModel>> OnGetProcessByBarcodeFuzzyAsync(
      ISqlSugarClient db,
      string barcode,
      string logHeader,
      int days = 10
   )
   {
      ObservableCollection<IBatMainModel> result = new();
      try
      {
         Type type = _displayDatas.CompleteBatteryDatas.RuntimeBatteryType;
         var endTime = DateTime.Now;
         var startTime = endTime.AddDays(-days);
         long startId = _snowflakeHelper.GetMinIdFromDateTime(startTime);
         long endId = _snowflakeHelper.GetMaxIdFromDateTime(endTime);
         //var months = endTime.Month - startTime.Month + 1;
         var monthCount = startTime.GetMonthCount(endTime);
         for (int i = 0; i < monthCount; i++)
         {
            var tableName = GetSplitTableNameByType(type, endTime.AddMonths(-i)); //根据时间获取表名 取表名
            if (!db.DbMaintenance.IsAnyTable(tableName, false))
            {
               $"[根据条码模糊查询数据]无此表[{tableName}],条码：{barcode}".LogProcess(logHeader, Log4NetLevelEnum.信息);
               continue;
            }
            var battery = await db.QueryableByObject(type)
               .AS(tableName)
               .Where($"{nameof(BatMainModel.Barcode)} LIKE '%{barcode}%'")
               .OrderBy(
                  new List<OrderByModel>
                  {
                     new OrderByModel { FieldName = nameof(BatMainModel.Id), OrderByType = OrderByType.Desc },
                  }
               )
               .ToListAsync();
            if (battery != null)
               result.AddRange((IEnumerable<IBatMainModel>)battery);
         }
      }
      catch (Exception ex)
      {
         $"[根据条码模糊查询数据]异常,条码：{barcode},详情：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
      }
      return result;
   }

   /// <summary>
   /// 查询hipot波形数据
   /// </summary>
   /// <param name="option"></param>
   /// <param name="alias"></param>
   /// <param name="selectFields"></param>
   /// <returns></returns>
   public async Task<List<T>> QueryHipotCurveAsync<T>(QueryFilterDto option, string alias, string selectFields)
      where T : class, new()
   {
      return await _dbFactory.UsingDbAsync(
            DatabaseRole.LocalDb1,
            async db =>
               await Task.Run(() =>
               {
                  var sql = OnGetQueryableByBarcdoe(db, option, AliasName, selectFields);
                  return db.SqlQueryable<T>(sql).ToList();
               })
         ) ?? [];
   }

   /// <summary>
   /// 多条件筛选查询，返回sql
   /// </summary>
   /// <param name="option"></param>
   /// <returns></returns>
   public async Task<string> GetQueryableByBarcdoe(QueryFilterDto option) =>
      await _dbFactory.UsingDbAsync(
         DatabaseRole.LocalDb1,
         async db => await Task.Run(() => OnGetQueryableByBarcdoe(db, option, AliasName, _aliasFields))
      ) ?? "";

   /// <summary>
   ///
   /// </summary>
   /// <param name="db"></param>
   /// <param name="option"></param>
   /// <param name="alias">查询数据时的别名</param>
   /// <param name="selectFields">带别名的select处字段</param>
   /// <returns></returns>
   private string OnGetQueryableByBarcdoe(ISqlSugarClient db, QueryFilterDto option, string alias, string selectFields)
   {
      try
      {
         var whereSql = BuildWhereClause(option, $"{alias}.");

         var monthCount = option.TimeFilter.StartTime.GetMonthCount(option.TimeFilter.EndTime);

         //如果不是按ID查询，那数据有可能在上一个月，所以需加一个月
         if (!option.TimeFilter.IsQueryById)
            ++monthCount;

         var queryParts = new List<string>();
         for (int i = 0; i < monthCount; i++)
         {
            var tableName = GetSplitTableNameByType(typeof(BatMainModel), option.TimeFilter.EndTime.AddMonths(-i)); //根据时间获取表名 取表名
            if (!db.DbMaintenance.IsAnyTable(tableName, false))
            {
               $"[多条件查询生成sql]无此表[{tableName}]".LogRun(Log4NetLevelEnum.信息);
               continue;
            }

            string partSql = string.Empty;
            if (option.IsNotRepeat) //去重
            {
               partSql =
                  $@"
                    SELECT {selectFields}
                    FROM {tableName} {alias}
                    INNER JOIN (
                        SELECT {nameof(BatMainModel.Barcode)}, MAX({nameof(BatMainModel.Id)}) AS {nameof(BatMainModel.Id)}
                        FROM {tableName}
                        {whereSql.RawWhere}
                        GROUP BY Barcode
                    ) sub ON {alias}.{nameof(BatMainModel.Id)} = sub.{nameof(BatMainModel.Id)}
                    {whereSql.AliasedWhere} ";
            }
            else //不去重
            {
               partSql =
                  $@"
                    SELECT {selectFields}
                    FROM {tableName} {alias}
                    {whereSql.AliasedWhere} ";
            }
            queryParts.Add(partSql);
         }

         if (queryParts.Count == 0)
            return string.Empty;

         return string.Join(" UNION ALL ", queryParts);
      }
      catch (Exception ex)
      {
         $"[多条件查询生成sql]异常，条件：{JsonSerializer.Serialize(option)},详情：{ex}".LogRun(Log4NetLevelEnum.错误);
      }
      return string.Empty;
   }

   public record SqlWhere(string RawWhere, string AliasedWhere);

   /// <summary>
   /// 构建查询where语句
   /// </summary>
   /// <param name="option"></param>
   /// <param name="alias"></param>
   /// <returns>RawWhere 不带别名的where,AliasedWhere 带别名的 where</returns>
   private SqlWhere BuildWhereClause(QueryFilterDto option, string alias)
   {
      var timeWhere = BuildTimeWhereClause(option.TimeFilter, alias);
      var barcodeWhere = BuildBarcodeWhereClause(option.BarcodeFilter, alias);
      var resultWhere = BuildResultWhereClause(option.ResultFilter, alias);

      SqlWhere[] whereArray = [timeWhere, barcodeWhere, resultWhere];
      List<string> rawStrings = new List<string>();
      List<string> aliasStrings = new List<string>();
      foreach (var item in whereArray)
      {
         if (string.IsNullOrWhiteSpace(item.RawWhere))
            continue;
         rawStrings.Add($"({item.RawWhere})");
         aliasStrings.Add($"({item.AliasedWhere})");
      }

      if (rawStrings.Count == 0)
         return new SqlWhere("", "");

      var where = new SqlWhere($"WHERE {string.Join(" AND ", rawStrings)}", $"WHERE {string.Join(" AND ", aliasStrings)}");
      return where;
   }

   /// <summary>
   /// 构建条码 where
   /// </summary>
   /// <param name="filter"></param>
   /// <param name="alias"></param>
   /// <returns></returns>
   /// <exception cref="Exception"></exception>
   private SqlWhere BuildBarcodeWhereClause(BarcodeFilterDto? filter, string alias)
   {
      if (filter == null || filter.Barcodes.Count == 0)
         return new SqlWhere("", "");

      // 模糊查询限制数量
      if (filter.IsFuzzyQuery && filter.Barcodes.Count > 3)
         throw new Exception("模糊查询最多允许3个条码");

      if (!filter.IsFuzzyQuery)
      {
         string values = string.Join(",", filter.Barcodes.Select(x => $"'{x.Trim()}'"));

         return new SqlWhere($"{nameof(BatMainModel.Barcode)} IN ({values})", $"{alias}{nameof(BatMainModel.Barcode)} IN ({values})");
      }

      string rawWhere = string.Join(" OR ", filter.Barcodes.Select(x => $"{nameof(BatMainModel.Barcode)} LIKE '%{x.Trim()}%'"));

      string aliasWhere = string.Join(" OR ", filter.Barcodes.Select(x => $"{alias}{nameof(BatMainModel.Barcode)} LIKE '%{x.Trim()}%'"));

      return new SqlWhere(rawWhere, aliasWhere);
   }

   /// <summary>
   /// 构建时间区间 where
   /// </summary>
   /// <param name="filter"></param>
   /// <param name="alias"></param>
   /// <returns></returns>
   private SqlWhere BuildTimeWhereClause(TimeFilterDto filter, string alias)
   {
      if (filter.IsQueryById)
      {
         long startId = _snowflakeHelper.GetMinIdFromDateTime(filter.StartTime);
         long endId = _snowflakeHelper.GetMaxIdFromDateTime(filter.EndTime);

         return new SqlWhere(
            $"{filter.PropertyName} BETWEEN {startId} AND {endId}",
            $"{alias}{filter.PropertyName} BETWEEN {startId} AND {endId}"
         );
      }

      var start = filter.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
      var end = filter.EndTime.ToString("yyyy-MM-dd HH:mm:ss");
      return new SqlWhere(
         $"{filter.PropertyName} BETWEEN '{start}' AND '{end}'",
         $"{alias}{filter.PropertyName} BETWEEN '{start}' AND '{end}'"
      );
   }

   //结果类型总数
   private static readonly int _resultTypeTotalCount = Enum.GetValues<ResultArea>().Length;

   /// <summary>
   /// 构建结果 的 where
   /// </summary>
   /// <param name="filter"></param>
   /// <param name="alias"></param>
   /// <returns></returns>
   private SqlWhere BuildResultWhereClause(ResultFilterDto filter, string alias)
   {
      if (filter.FilterMode == ResultFilterMode.All)
         return new SqlWhere("", "");

      //只看合格
      if (filter.FilterMode == ResultFilterMode.Passed)
      {
         var okAres = ResultArea.OK.GetResultAreaBounds();
         return new SqlWhere(
            $"{nameof(BatMainModel.FinalStatus)} BETWEEN {okAres.Min} AND {okAres.Max}",
            $"{alias}{nameof(BatMainModel.FinalStatus)} BETWEEN {okAres.Min} AND {okAres.Max}"
         );
      }

      //只看不合格
      if (filter.FilterMode == ResultFilterMode.Failed)
      {
         var ngAres = ResultArea.NG.GetResultAreaBounds();
         return new SqlWhere(
            $"{nameof(BatMainModel.FinalStatus)} BETWEEN {ngAres.Min} AND {ngAres.Max}",
            $"{alias}{nameof(BatMainModel.FinalStatus)} BETWEEN {ngAres.Min} AND {ngAres.Max}"
         );
      }

      List<string> rawWhereClause = new();
      List<string> aliasWhereClause = new();
      foreach (var item in filter.ResultFilters)
      {
         var count = item.ResultRanges.Count;
         if (count == 0 || count == _resultTypeTotalCount) //如果全部选择了或一个也没有选择，就不用筛选了
            continue;

         var rawProcessWheres = item.ResultRanges.Select(x => $"{item.PropertyName} BETWEEN {x.Min} AND {x.Max}");
         var aliasProcessWheres = item.ResultRanges.Select(x => $"{alias}{item.PropertyName} BETWEEN {x.Min} AND {x.Max}");

         rawWhereClause.Add($"({string.Join(" OR ", rawProcessWheres)})");
         aliasWhereClause.Add($"({string.Join(" OR ", aliasProcessWheres)})");
      }
      return new SqlWhere(string.Join(" OR ", rawWhereClause), string.Join(" OR ", aliasWhereClause));
   }

   #endregion

   #region 辅助方法
   /// <summary>
   /// 取分表表名
   /// </summary>
   /// <param name="type"></param>
   /// <param name="dateTime"></param>
   /// <returns></returns>
   public static string GetSplitTableNameByType(Type type, DateTime dateTime)
   {
      var _attribe = type.GetCustomAttribute<SugarTable>();
      if (_attribe != null)
      {
         var _tableNames = _attribe.TableName.Split('_');
         if (_tableNames.Length > 0)
         {
            return $"{_tableNames[0]}_{GetSplitTableSuffix(dateTime)}".ToLower();
         }
      }
      return $"{type.Name}_{GetSplitTableSuffix(dateTime)}".ToLower();
   }

   /// <summary>
   /// 生成分表后缀
   /// </summary>
   /// <param name="type"></param>
   /// <returns></returns>
   private static string GetSplitTableSuffix(DateTime dateTime) => $"{dateTime.Year}{dateTime.Month:D2}01";

   /// <summary>
   /// 检查日期是否为当月最后一天
   /// </summary>
   /// <param name="dateTime"></param>
   /// <returns></returns>
   private static bool CheckIsMonthLastDay(DateTime dateTime)
   {
      var _newDtae = dateTime.AddMonths(1);
      int _lastDay = (new DateTime(_newDtae.Year, _newDtae.Month, 1)).AddDays(-1).Day;
      return _lastDay == dateTime.Day;
   }

   #endregion
}
