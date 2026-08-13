using System.Dynamic;
using NPOI.SS.Formula.Functions;

namespace Kinlo.Common.DAL;

public partial class DbHelper
{
   #region MES补传
   /// <summary>
   /// MES补传插入数据
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="data"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public async Task InsertOrUpdateMesResendAsync(MesResendModel data, string logHeader) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnInsertOrUpdateMesResendAsync(db, data, logHeader));

   private async Task OnInsertOrUpdateMesResendAsync(ISqlSugarClient db, MesResendModel data, string logHeader)
   {
      try
      {
         var old = await db.Queryable<MesResendModel>().Where(x => x.Barcode == data.Barcode).ToListAsync();

         bool neddInsert = true; //是否需要插入新数据
         if (old != null && old.Count > 0)
         {
            var delList = new List<MesResendModel>();
            foreach (var item in old)
            {
               if (item.Id < data.Id)
               {
                  delList.Add(item);
               }
               else
               {
                  neddInsert = false; //无需
               }
            }
            if (delList.Count > 0)
            {
               var delCount = await db.Deleteable(delList).ExecuteCommandAsync();
               $"[插入MES补传列表] 条码 [{data.Barcode}] 有[{delList.Count}]条旧数据，删除[{delCount}]条数据;".LogProcess(logHeader);
            }
         }
         if (neddInsert)
         {
            for (int i = 0; i < 3; i++)
            {
               if (await db.Storageable(data).ExecuteCommandAsync() > 0) //插入或更新
               {
                  $"[插入MES补传列表] 类名:[{data.GetType().Name}] 第{i + 1}次成功;".LogProcess(logHeader, Log4NetLevelEnum.成功);
                  return;
               }
               else
               {
                  $"[插入MES补传列表] 类名:[{data.GetType().Name}] 第{i + 1}次失败;".LogProcess(logHeader, Log4NetLevelEnum.错误);
               }
            }
         }
         else
         {
            $"[插入MES补传列表] 条码 [{data.Barcode}] 表内有更新的数据，无需插入数据;".LogProcess(logHeader);
         }
      }
      catch (Exception ex)
      {
         $"[插入MES补传列表] 类名:[{data.GetType().Name}] ,异常,：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }
   }

   /// <summary>
   /// 取MES补传数据
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="count"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public async Task<List<MesResendModel>> QueryMesResendListAsync(int count, string logHeader) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnQueryMesResendListAsync(db, count, logHeader));

   private async Task<List<MesResendModel>> OnQueryMesResendListAsync(ISqlSugarClient db, int count, string logHeader)
   {
      try
      {
         var data = await db.Queryable<MesResendModel>()
            .Where(x => x.ResendStatus != ResendStatusEnum.上传成功 && x.ResendCount <= 10)
            .OrderBy(x => x.Id)
            .Take(count)
            .ToListAsync();
         if (data != null)
         {
            $"[取MES补传数据] 返回数据 [{data.Count}] 条 ;".LogProcess(logHeader, Log4NetLevelEnum.成功);
            return data;
         }
         else
         {
            $"[取MES补传数据] 返回数据 为空;".LogProcess(logHeader, Log4NetLevelEnum.成功);
            return new List<MesResendModel>();
         }
      }
      catch (Exception ex)
      {
         $"[取MES补传数据] 异常,：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }
      return new List<MesResendModel>();
   }

   /// <summary>
   /// 范围删除补传成功数据
   /// </summary>
   /// <param name="dayCount">超期天数</param>
   /// <returns></returns>
   public async Task RangeDeleteResendAsync(int dayCount) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnRangeDeleteResendAsync(db, dayCount));

   private async Task OnRangeDeleteResendAsync(ISqlSugarClient db, int dayCount)
   {
      try
      {
         var endTime = DateTime.Now.AddDays(-dayCount);
         long endId = _snowflakeHelper.GetMaxIdFromDateTime(endTime);
         var count = await db.Deleteable<MesResendModel>()
            .Where(x => x.ResendStatus == ResendStatusEnum.上传成功 && x.Id < endId)
            .ExecuteCommandAsync();

         $"[范围删除补传成功数据] [{count}] 条 ;".LogRun(Log4NetLevelEnum.成功);
      }
      catch (Exception ex)
      {
         $"[范围删除补传成功数据] 异常,：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   public async Task<bool> UpdateMesResendAndMainBattery(List<MesResendModel> updateResends, List<IBatMainModel> bats, string logHeader)
   {
      var tranResult = await _dbFactory.UsingTransactionAsync<bool>(
         DatabaseRole.LocalDb1,
         async db =>
         {
            if (updateResends.Count > 0)
            {
               var upCount = await db.Updateable(updateResends).ExecuteCommandAsync();
               $"[更新MES补传列表] 应该更新[{updateResends.Count}]条，实际更新[{upCount}]条!".LogProcess(logHeader);
               if (upCount != updateResends.Count)
                  throw new Exception("更新数量不一致，事务回滚");
            }
            if (bats.Count > 0)
               await UpdatedMainTableMesStatusAsync(db, bats, logHeader);
            return true;
         }
      );
      return tranResult.IsSuccess;
   }

   /// <summary>
   /// 补传后批量更新主表
   /// </summary>
   /// <param name="bats"></param>
   /// <param name="logHeader"></param>
   /// <param name="tran">支持传入实例，确保事务回滚</param>
   /// <returns></returns>
   public async Task UpdatedMainTableMesStatusAsync(
      ISqlSugarClient db,
      List<IBatMainModel> bats,
      string logHeader,
      SqlSugarScope? tran = null
   )
   {
      Type type = _displayDatas.CompleteBatteryDatas.RuntimeBatteryType;
      Dictionary<string, List<IBatMainModel>> tableNames = new();
      foreach (var bat in bats)
      {
         string tableName = GetSplitTableNameByType(type, SnowflakeHelper.GetDateTimeFromId(bat.Id)); //根据时间获取表名
         if (!db.DbMaintenance.IsAnyTable(tableName, false))
         {
            $"[MES数据补传] 无此表[{tableName}],ID：{bat.Id},忽略ID;".LogProcess(logHeader, Log4NetLevelEnum.错误);
         }
         else
         {
            if (tableNames.TryGetValue(tableName, out var nameList))
            {
               nameList.Add(bat);
            }
            else
            {
               tableNames[tableName] = [bat];
            }
         }
      }

      foreach (var dic in tableNames)
      {
         var sb = new StringBuilder();
         sb.AppendLine($"UPDATE {dic.Key} SET");

         sb.AppendLine($"{nameof(IBatMainModel.MesOutputStatus)} = CASE {nameof(IBatMainModel.Id)}");
         foreach (var d in dic.Value)
         {
            sb.AppendLine($"WHEN {d.Id} THEN {(int)d.MesOutputStatus}");
         }
         sb.AppendLine("END,");

         sb.AppendLine($"{nameof(IBatMainModel.OutputTime)} = CASE {nameof(IBatMainModel.Id)}");
         foreach (var d in dic.Value)
         {
            sb.AppendLine($"WHEN {d.Id} THEN '{d.OutputTime}'");
         }
         sb.AppendLine("END");

         sb.AppendLine($"WHERE Id IN ({string.Join(',', dic.Value.Select(d => d.Id))})");
         var sql = sb.ToString();
         var upCount = await db.Ado.ExecuteCommandAsync(sql);
         $"[MES数据补传] 更新表 [{dic.Key}] 应更新[{dic.Value.Count}],实更新 [{upCount}] 条;".LogProcess(logHeader);
         if (upCount != dic.Value.Count)
            throw new Exception("更新数量不一致，事务回滚");
      }
   }

   #endregion

   #region  注液量图表数据
   /// <summary>
   /// 插入注液量统计表数据
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   public async Task<bool> InsertInjectionAsync(InjectionDataModel data) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnInsertInjectionAsync(db, data));

   private async Task<bool> OnInsertInjectionAsync(ISqlSugarClient db, InjectionDataModel data)
   {
      try
      {
         if (await db.Insertable(data).SplitTable().ExecuteCommandAsync() > 0)
         {
            $"[注液量图表数据]插入成功,ID：{data.Id},条码：{data.Barcode};".LogRun(Log4NetLevelEnum.成功);
            return true;
         }
         else
         {
            $"[注液量图表数据]插入失败,ID：{data.Id},条码：{data.Barcode};".LogRun(Log4NetLevelEnum.错误);
         }
      }
      catch (Exception ex)
      {
         $"[动态插入或更新数据]异常,类：{typeof(T).Name}，详情：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
      return false;
   }

   /// <summary>
   /// 插入或更新 注液量表
   /// </summary>
   /// <param name="data"></param>
   /// <param name="isInjectValue">是否只更新注液量</param>
   /// <returns></returns>
   public async Task<bool> InsertOrUpdateInjectionAsync(InjectionDataModel data) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnInsertOrUpdateInjectionAsync(db, data));

   private async Task<bool> OnInsertOrUpdateInjectionAsync(ISqlSugarClient db, InjectionDataModel data)
   {
      try
      {
         var name = db.SplitHelper<InjectionDataModel>().GetTableName(data.CreateTime);
         var count = await db.Queryable<InjectionDataModel>()
            .Where(x => x.Id == data.Id)
            .SplitTable(tabs => tabs.InTableNames(name))
            .CountAsync();

         if (count > 0)
         {
            if (await db.Updateable(data).AS(name).ExecuteCommandAsync() > 0)
            {
               $"[注液量图表数据]更新成功,ID：{data.Id},条码：{data.Barcode};".LogRun(Log4NetLevelEnum.成功);
               return true;
            }
            else
            {
               $"[注液量图表数据]更新失败,ID：{data.Id},条码：{data.Barcode};".LogRun(Log4NetLevelEnum.错误);
            }
         }
         else
         {
            if (await db.Insertable(data).SplitTable().ExecuteCommandAsync() > 0)
            {
               $"[注液量图表数据]插入成功,ID：{data.Id},条码：{data.Barcode};".LogRun(Log4NetLevelEnum.成功);
               return true;
            }
            else
            {
               $"[注液量图表数据]插入失败,ID：{data.Id},条码：{data.Barcode};".LogRun(Log4NetLevelEnum.错误);
            }
         }
      }
      catch (Exception ex)
      {
         $"[动态插入或更新数据]异常,类：{typeof(T).Name}，详情：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
      return false;
   }

   /// <summary>
   /// 按时间查询注液数据
   /// </summary>
   /// <param name="startTime"></param>
   /// <param name="endTime"></param>
   /// <returns></returns>
   public async Task<List<InjectionDataModel>> GetInjectionDataAsync(DateTime startTime, DateTime endTime) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnGetInjectionCpksAsync(db, startTime, endTime));

   private async Task<List<InjectionDataModel>> OnGetInjectionCpksAsync(ISqlSugarClient db, DateTime startTime, DateTime endTime)
   {
      try
      {
         var monthCount = startTime.GetMonthCount(endTime);
         //var monthCount = endTime.Month - startTime.Month + 1;
         StringBuilder queryTableExp = new StringBuilder();
         queryTableExp.Append($"{nameof(InjectionDataModel.InjectionTime)} BETWEEN '{startTime}' AND '{endTime}' ");
         List<ISugarQueryable<InjectionDataModel>> methods = new List<ISugarQueryable<InjectionDataModel>>();
         for (int i = 0; i < monthCount; i++)
         {
            var tableName = db.SplitHelper<InjectionDataModel>().GetTableName(endTime.AddMonths(-i)); //根据时间获取表名
            if (!db.DbMaintenance.IsAnyTable(tableName, false))
            {
               $"[查询注液泵数据]无此表[{tableName}]".LogRun(Log4NetLevelEnum.信息);
               continue;
            }

            var method = db.Queryable<InjectionDataModel>().AS(tableName).Where(queryTableExp.ToString());
            methods.Add(method);
         }
         if (methods.Count < 1)
            return [];
         // 合并所有 Query
         var unionQuery = db.UnionAll(methods);
         var result = await unionQuery.ToListAsync();
         return result;
      }
      catch (Exception ex)
      {
         $"[查询注液泵数据]异常：{ex}".LogRun(Log4NetLevelEnum.警告, true);
         return [];
      }
   }

   #endregion

   #region 统计
   /// <summary>
   /// 查询工序统计数据
   /// </summary>
   /// <param name="startTime"></param>
   /// <param name="endTime"></param>
   /// <param name="selectFields"></param>
   /// <returns></returns>
   public async Task<List<T>?> GetStatisticsData<T>(DateTime startTime, DateTime endTime, List<string> selectFields)
      where T : class, new() =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnGetStatisticsData<T>(db, startTime, endTime, selectFields));

   private async Task<List<T>?> OnGetStatisticsData<T>(ISqlSugarClient db, DateTime startTime, DateTime endTime, List<string> selectFields)
      where T : class, new()
   {
      try
      {
         long startId = _snowflakeHelper.GetMinIdFromDateTime(startTime);
         long endId = _snowflakeHelper.GetMaxIdFromDateTime(endTime);
         string whereExp = $"{nameof(BatMainModel.Id)} >= {startId} AND {nameof(BatMainModel.Id)} <= {endId} ";
         string aliasWhereExp = $"{AliasName}.{nameof(BatMainModel.Id)} >= {startId} AND {AliasName}.{nameof(BatMainModel.Id)} <= {endId} ";

         if (!selectFields.Any(x => x == nameof(BatMainModel.Barcode)))
            selectFields.Add(nameof(BatMainModel.Barcode));
         if (!selectFields.Any(x => x == nameof(BatMainModel.Id)))
            selectFields.Add(nameof(BatMainModel.Id));
         var aliasSelectExp = string.Join(", ", selectFields.Select(f => $"{AliasName}.{f.Trim()}"));

         var monthCount = startTime.GetMonthCount(endTime);
         //var months = endTime.Month - startTime.Month + 1;
         var sqls = new List<string>();
         for (int i = 0; i < monthCount; i++)
         {
            var tableName = GetSplitTableNameByType(typeof(BatMainModel), endTime.AddMonths(-i)); //根据时间获取表名 取表名
            if (!db.DbMaintenance.IsAnyTable(tableName, false))
               continue;

            string partSql = string.Empty;
            //去重

            partSql =
               $@"
                    SELECT {aliasSelectExp}
                    FROM {tableName} {AliasName}
                    INNER JOIN (
                        SELECT {nameof(BatMainModel.Barcode)}, MAX({nameof(BatMainModel.Id)}) AS {nameof(BatMainModel.Id)}
                        FROM {tableName}
                        WHERE {whereExp}
                        GROUP BY Barcode
                    ) sub ON {AliasName}.{nameof(BatMainModel.Id)} = sub.{nameof(BatMainModel.Id)}
                    WHERE {aliasWhereExp} ";

            sqls.Add(partSql);
         }
         if (sqls.Count == 0)
            return null;
         string unionSql = string.Join(" UNION ALL ", sqls);
         var sugarQueryable = db.SqlQueryable<T>(unionSql);

         var list = await sugarQueryable.ToListAsync();
         return list;
      }
      catch (Exception ex)
      {
         $"[查询统计数据]异常：{ex}".LogRun(Log4NetLevelEnum.错误);
      }

      return null;
   }
   #endregion

   #region PLC状态
   /// <summary>
   /// 查询时间范围内PLC状态，并按创建时间排序
   /// </summary>
   /// <param name="startTime"></param>
   /// <param name="endTime"></param>
   /// <returns></returns>
   public async Task<List<PlcStatusModel>> GetTimeRangePlcStatusAsync(
      DateTime startTime,
      DateTime endTime,
      ShiftType? shift,
      DeviceStateEnum[]? deviceState
   ) =>
      await _dbFactory.UsingDbAsync(
         DatabaseRole.LocalDb1,
         async db => await OnGetTimeRangePlcStatusAsync(db, startTime, endTime, shift, deviceState)
      );

   public async Task<List<PlcStatusModel>> OnGetTimeRangePlcStatusAsync(
      ISqlSugarClient db,
      DateTime startTime,
      DateTime endTime,
      ShiftType? shift,
      DeviceStateEnum[]? deviceState
   )
   {
      try
      {
         if (startTime >= endTime)
            return new List<PlcStatusModel>();
         var yearCount = endTime.Year - startTime.Year + 1;
         List<ISugarQueryable<PlcStatusModel>> queryList = new();
         for (int i = 0; i < yearCount; i++)
         {
            var tableName = db.SplitHelper<PlcStatusModel>().GetTableName(startTime.AddYears(i)); //根据时间获取表名
            if (!db.DbMaintenance.IsAnyTable(tableName, false))
            {
               $"[获取时间范围内PLC状态数据]无此表：[{tableName}]".LogRun(Log4NetLevelEnum.警告);
               continue;
            }

            var yearQueryable = db.Queryable<PlcStatusModel>()
               .AS(tableName)
               .Where(x => x.StartTime >= startTime && x.StartTime <= endTime)
               .WhereIF(shift.HasValue, x => x.Shift == shift!.Value)
               .WhereIF(deviceState != null && deviceState.Length > 0, x => deviceState.Contains(x.Status));

            queryList.Add(yearQueryable);
         }
         if (queryList.Any())
         {
            var unionQuery = db.UnionAll(queryList).OrderBy(x => x.Id, OrderByType.Asc);
            //var sql = db.UnionAll(queryList).ToSql();
            var results = await db.UnionAll(queryList).ToListAsync();
            return results;
         }
      }
      catch (Exception ex)
      {
         $"[获取时间范围内PLC状态数据]异常,开始时间：{startTime:yyyy-MM-dd HH:mm:ss},结束时间：{endTime:yyyy-MM-dd HH:mm:ss}，详情：{ex}".LogRun(
            Log4NetLevelEnum.错误
         );
      }
      return new List<PlcStatusModel>();
   }
   #endregion

   #region PLC报警
   /// <summary>
   /// 查询时间范围内PLC报警
   /// </summary>
   /// <param name="startTime"></param>
   /// <param name="endTime"></param>
   /// <returns></returns>
   public async Task<List<PlcAlarmModel>> GetTimeRangePlcAlarmsAsync(DateTime startTime, DateTime endTime, PlcAalrmLevelEnum[] types) =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnGetTimeRangePlcAlarmsAsync(db, startTime, endTime, types));

   public async Task<List<PlcAlarmModel>> OnGetTimeRangePlcAlarmsAsync(
      ISqlSugarClient db,
      DateTime startTime,
      DateTime endTime,
      PlcAalrmLevelEnum[] types
   )
   {
      try
      {
         long startId = _snowflakeHelper.GetMinIdFromDateTime(startTime);
         long endId = _snowflakeHelper.GetMaxIdFromDateTime(endTime);
         // var monthCount = endTime.Month - startTime.Month + 1;
         var monthCount = startTime.GetMonthCount(endTime);
         List<ISugarQueryable<PlcAlarmModel>> months = new();
         for (int i = 0; i < monthCount; i++)
         {
            var tableName = db.SplitHelper<PlcAlarmModel>().GetTableName(startTime.AddMonths(i)); //根据时间获取表名
            if (!db.DbMaintenance.IsAnyTable(tableName, false))
            {
               $"[获取时间范围内PLC报警数据]无此表：[{tableName}]".LogRun(Log4NetLevelEnum.警告);
               continue;
            }
            var monthQueryable = db.Queryable<PlcAlarmModel>()
               .AS(tableName)
               .Where(x => x.Id >= startId && x.Id <= endId && types.Contains(x.PlcAalrmLevel));
            months.Add(monthQueryable);
         }
         if (months.Count > 0)
         {
            var results = await db.UnionAll(months).ToListAsync();
            return results;
         }
      }
      catch (Exception ex)
      {
         $"[获取时间范围内PLC报警数据]异常,开始时间：{startTime:yyyy-MM-dd HH:mm:ss},结束时间：{endTime:yyyy-MM-dd HH:mm:ss}，详情：{ex}".LogRun(
            Log4NetLevelEnum.错误
         );
      }
      return new List<PlcAlarmModel>();
   }
   #endregion

   #region 显示面板数据保存

   #endregion
}
