using System.Data;
using System.Dynamic;
using HandyControl.Tools.Extension;

namespace Kinlo.Common.DAL;

public partial class DbHelper
{
   #region 电芯插入数据
   /// <summary>
   /// 电芯插入数据
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   public async Task<bool> InsertableByObjectAsync<T>(T data, string logHeader)
      where T : IBatMainModel
   {
      return await _dbFactory.UsingDbAsync(
         DatabaseRole.LocalDb1,
         async db =>
         {
            try
            {
               for (int i = 0; i < 3; i++)
               {
                  if ((await db.InsertableByObject(data).SplitTable().ExecuteCommandAsync()) > 0)
                  {
                     $"[插入数据]第{i + 1}次成功;".LogProcess(logHeader, Log4NetLevelEnum.成功);
                     return true;
                  }
                  else
                  {
                     $"[插入数据]第{i + 1}次失败;".LogProcess(logHeader, Log4NetLevelEnum.错误);
                  }
               }
            }
            catch (Exception ex)
            {
               $"[插入数据]异常： {ex};".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
            }
            return false;
         }
      );
   }

   /// <summary>
   /// 批量插入数据
   /// </summary>
   /// <param name="datas"></param>
   /// <returns></returns>
   public async Task<bool> InsertableByObjectsAsync<T>(string logHeader, params T[] datas)
      where T : IBatMainModel =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnInsertableByObjectsAsync(db, logHeader, datas));

   private async Task<bool> OnInsertableByObjectsAsync<T>(ISqlSugarClient db, string logHeader, params T[] datas)
      where T : IBatMainModel
   {
      try
      {
         var stopwatch = new Stopwatch();
         stopwatch.Start();
         Dictionary<string, List<T>> upDic = new();
         foreach (var ds in datas)
         {
            string tableName = GetSplitTableNameByType(typeof(BatMainModel), ds.CreateTime);
            if (upDic.TryGetValue(tableName, out var ls))
            {
               ls.Add(ds);
            }
            else
            {
               upDic.Add(tableName, new List<T> { ds });
            }
         }
         StringBuilder stringBuilder = new StringBuilder();
         bool[] isSuccess = Enumerable.Repeat(false, upDic.Count).ToArray();
         for (int k = 0; k < upDic.Count; k++)
         {
            var item = upDic.ElementAt(k);
            for (int i = 0; i < 3; i++)
            {
               var retInt = await db.InsertableByObject(item.Value).SplitTable().ExecuteCommandAsync();
               if (retInt >= item.Value.Count)
               {
                  stringBuilder.AppendLine(
                     $"[批量插入数据]第{i + 1}次成功,表名：[{item.Key}],ID：{string.Join(',', item.Value.Select(x => x.Id))},条码：{string.Join(',', item.Value.Select(x => x.Barcode))}"
                  );
                  isSuccess[k] = true;
                  break;
               }
               else
               {
                  isSuccess[k] = false;
                  stringBuilder.AppendLine(
                     $"[批量插入数据]第{i + 1}次失败,表名：[{item.Key}],ID：{string.Join(',', item.Value.Select(x => x.Id))},条码：{string.Join(',', item.Value.Select(x => x.Barcode))}"
                  );
               }
               Thread.Sleep(2);
            }
         }
         stopwatch.Stop();
         bool isSuccessAll = isSuccess.All(x => x);
         $"[批量插入数据]用时{stopwatch.ElapsedMilliseconds}ms,{stringBuilder}".LogProcess(
            logHeader,
            isSuccessAll ? Log4NetLevelEnum.成功 : Log4NetLevelEnum.错误
         );
         return isSuccessAll;
      }
      catch (Exception ex)
      {
         $"[批量插入数据]异常,ID：{string.Join(',', datas.Select(x => x.Id))},条码：{string.Join(',', datas.Select(x => x.Barcode))}，详情： {ex}".LogProcess(
            logHeader,
            Log4NetLevelEnum.错误,
            true
         );
      }
      return false;
   }

   /// <summary>
   /// 泛型插入数据
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   public async Task<bool> InsertableAsync<T>(T data, string logHeader)
      where T : class, new() =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnInsertableAsync(db, data, logHeader));

   private async Task<bool> OnInsertableAsync<T>(ISqlSugarClient db, T data, string logHeader)
      where T : class, new()
   {
      try
      {
         for (int i = 0; i < 3; i++)
         {
            if ((await db.Insertable(data).SplitTable().ExecuteCommandAsync()) > 0)
            {
               $"[插入数据] 类名:[{data.GetType().Name}] 第{i + 1}次成功;".LogProcess(logHeader, Log4NetLevelEnum.成功);
               return true;
            }
            else
            {
               $"[插入数据] 类名:[{data.GetType().Name}] 第{i + 1}次失败;".LogProcess(logHeader, Log4NetLevelEnum.错误);
            }
         }
      }
      catch (Exception ex)
      {
         $"[插入数据]异常：{ex};".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }
      return false;
   }
   #endregion

   #region 插入或更新
   /// <summary>
   /// 工序插入数据需调用此方法（插入或更新）
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   public async Task<bool> InsertOrUpdateByBatteryBase<T>(T data, string logHeader)
      where T : IBatMainModel =>
      await _dbFactory.UsingDbAsync(
         DatabaseRole.LocalDb1,
         async db => await Task.Run(() => OnInsertOrUpdateByBatteryBase(db, data, logHeader))
      );

   private bool OnInsertOrUpdateByBatteryBase<T>(ISqlSugarClient db, T data, string logHeader)
      where T : IBatMainModel
   {
      try
      {
         for (int i = 0; i < 3; i++)
         {
            if (db.StorageableByObject(data).SplitTable().ExecuteCommand() > 0) //插入或更新
            {
               $"[动态插入或更新数据]第{i + 1}成功,类：{typeof(T).Name};".LogProcess(logHeader, Log4NetLevelEnum.成功);
               return true;
            }
            else
            {
               $"[动态插入或更新数据]第{i + 1}次失败,类：{typeof(T).Name};".LogProcess(logHeader, Log4NetLevelEnum.错误);
            }
         }
      }
      catch (Exception ex)
      {
         $"[动态插入或更新数据]类：{typeof(T).Name},异常,：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }
      return false;
   }
   #endregion

   #region 更新数据
   /// <summary>
   /// 更新数据
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   public async Task<bool> UpdateBatteryAsync<T>(T data, string logHeader)
      where T : class, IBatMainModel, new() =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnUpdateBatteryAsync(db, data, logHeader));

   private async Task<bool> OnUpdateBatteryAsync<T>(ISqlSugarClient db, T data, string logHeader)
      where T : class, IBatMainModel, new()
   {
      try
      {
         var _tableName = db.SplitHelper<T>().GetTableName(data.CreateTime); //根据时间获取表名,精准更新表
         for (int i = 0; i < 3; i++)
         {
            if ((await db.Updateable(data).AS(_tableName).ExecuteCommandAsync()) > 0)
            {
               $"[更新数据]第{i + 1}次成功,表名：[{_tableName}]".LogProcess(logHeader, Log4NetLevelEnum.成功);
               return true;
            }
            else
            {
               $"[更新数据]第{i + 1}次失败,表名：[{_tableName}]".LogProcess(logHeader, Log4NetLevelEnum.错误);
            }
            Thread.Sleep(2);
         }
      }
      catch (Exception ex)
      {
         $"[更新数据] 工序类：[{typeof(T).Name}]异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }
      return false;
   }

   /// <summary>
   /// 更新数据
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   public async Task<bool> UpdateByObjectAsync<T>(T data, string logHeader)
      where T : IBatMainModel =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnUpdateByObjectAsync(db, data, logHeader));

   private async Task<bool> OnUpdateByObjectAsync<T>(ISqlSugarClient db, T data, string logHeader)
      where T : IBatMainModel
   {
      try
      {
         var _tableName = GetSplitTableNameByType(typeof(BatMainModel), data.CreateTime); //根据时间获取表名,精准更新表
         for (int i = 0; i < 3; i++)
         {
            if ((await db.UpdateableByObject(data).AS(_tableName).ExecuteCommandAsync()) > 0)
            {
               $"[更新数据]第{i + 1}次成功,表名：[{_tableName}]".LogProcess(logHeader, Log4NetLevelEnum.成功);
               return true;
            }
            else
            {
               $"[更新数据]第{i + 1}次失败,表名：[{_tableName}]".LogProcess(logHeader, Log4NetLevelEnum.错误);
            }
            Thread.Sleep(2);
         }
      }
      catch (Exception ex)
      {
         $"[更新数据]工序类：[{typeof(T).Name}]异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }
      return false;
   }

   /// <summary>
   /// 批量更新数据
   /// </summary>
   /// <param name="datas"></param>
   /// <returns></returns>
   public async Task<bool> UpdateByObjectsAsync<T>(string logHeader, params T[] datas)
      where T : IBatMainModel =>
      await _dbFactory.UsingDbAsync(DatabaseRole.LocalDb1, async db => await OnUpdateByObjectsAsync(db, logHeader, datas));

   private async Task<bool> OnUpdateByObjectsAsync<T>(ISqlSugarClient db, string logHeader, params T[] datas)
      where T : IBatMainModel
   {
      try
      {
         Stopwatch stopwatch = new Stopwatch();
         stopwatch.Start();
         Dictionary<string, List<T>> upDic = new();
         foreach (var ds in datas)
         {
            string tableName = GetSplitTableNameByType(typeof(BatMainModel), ds.CreateTime); //根据时间获取表名,精准更新表
            if (upDic.TryGetValue(tableName, out var ls))
            {
               ls.Add(ds);
            }
            else
            {
               upDic.Add(tableName, new List<T> { ds });
            }
         }
         bool[] isSuccess = Enumerable.Repeat(false, upDic.Count).ToArray();
         StringBuilder stringBuilder = new StringBuilder();
         for (int k = 0; k < upDic.Count; k++)
         {
            var item = upDic.ElementAt(k);
            for (int i = 0; i < 3; i++)
            {
               var retInt = await db.UpdateableByObject(item.Value).AS(item.Key).ExecuteCommandAsync();
               if (retInt >= item.Value.Count)
               {
                  stringBuilder.AppendLine(
                     $"[批量更新数据]第{i + 1}次成功,表名：[{item.Key}],ID：{string.Join(',', item.Value.Select(x => x.Id))},条码：{string.Join(',', item.Value.Select(x => x.Barcode))}"
                  );
                  isSuccess[k] = true;
                  break;
               }
               else
               {
                  isSuccess[k] = false;
                  stringBuilder.AppendLine(
                     $"[批量更新数据]第{i + 1}次失败,表名：[{item.Key}],ID：{string.Join(',', item.Value.Select(x => x.Id))},条码：{string.Join(',', item.Value.Select(x => x.Barcode))}"
                  );
               }
               Thread.Sleep(2);
            }
         }
         stopwatch.Stop();
         bool isSuccessAll = isSuccess.All(x => x);
         $"[批量更新数据]用时{stopwatch.ElapsedMilliseconds}ms,{stringBuilder}".LogProcess(
            logHeader,
            isSuccessAll ? Log4NetLevelEnum.成功 : Log4NetLevelEnum.错误
         );
         return isSuccessAll;
      }
      catch (Exception ex)
      {
         $"[批量更新数据]异常,ID：{string.Join(',', datas.Select(x => x.Id))},条码：{string.Join(',', datas.Select(x => x.Barcode))}，详情：{ex}".LogProcess(
            logHeader,
            Log4NetLevelEnum.错误,
            true
         );
      }
      return false;
   }

   /// <summary>
   /// 指定列更新，注意（需更新的字典索引0务必为表id,主表为Id）
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="upDictionary">需要更新的键值对（索引0务必为表id,主表为Id）</param>
   /// <param name="id">电池ID，注意务必为电池ID，非表ID</param>
   /// <param name="barcode">电池条码</param>
   /// <returns></returns>
   public async Task<bool> UpdateColumnsAsync(Dictionary<string, object> upDictionary, long id, string barcode, string logHeader) =>
      await _dbFactory.UsingDbAsync(
         DatabaseRole.LocalDb1,
         async db => await OnUpdateColumnsAsync(db, upDictionary, id, barcode, logHeader)
      );

   private async Task<bool> OnUpdateColumnsAsync(
      ISqlSugarClient db,
      Dictionary<string, object> upDictionary,
      long id,
      string barcode,
      string logHeader
   )
   {
      try
      {
         var tableName = GetSplitTableNameByType(typeof(BatMainModel), SnowflakeHelper.GetDateTimeFromId(id)); //根据时间获取表名,精准更新表
         for (int i = 0; i < 3; i++)
         {
            var _ret = await db.Updateable(upDictionary).AS(tableName).WhereColumns(upDictionary.ElementAt(0).Key).ExecuteCommandAsync();
            if (_ret > 0)
            {
               $"[指定列更新]{i + 1}成功".LogProcess(logHeader, Log4NetLevelEnum.成功);
               return true;
            }
            else
            {
               $"[指定列更新]第{i + 1}次失败".LogProcess(logHeader, Log4NetLevelEnum.错误);
            }
            Thread.Sleep(2);
         }
      }
      catch (Exception ex)
      {
         $"[指定列更新]异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
      }
      return false;
   }
   #endregion
}
