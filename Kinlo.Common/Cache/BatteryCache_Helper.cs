namespace Kinlo.Common.Cache;

public partial class BatteryCache : IBatteryCache
{
   /// <summary>
   /// 查最近生产电芯
   /// </summary>
   /// <returns></returns>
   private async Task<List<IBatMainModel>> GetBattereyListAsync()
   {
      DateTime lastTime = _appGlobalConfig.ShiftSwitchInfo.LastResetTime;
      DateTime startTime = lastTime.AddHours(-5);

      long count = await GetCountByTime(startTime);
      var staticTemp = _globalTemporaryLazy.Value;
      if (count > _maxCapacity)
      {
         var msg = $"数据缓存{count}大于上限{_maxCapacity}，无法开机";
         await UIThreadHelper.InvokeOnUiThreadAsync(() =>
            HandyControl.Controls.MessageBox.Show($"{msg}，请联系上位机工程师！", "无法开机错误", MessageBoxButton.OK)
         );

         staticTemp.RequestStop(
            new ShutdownReason
            {
               Category = ShutdownType.加载缓存超上限,
               Message = msg,
               Solution = "请联系上位机工程师",
            }
         );
         return new List<IBatMainModel>();
      }

      staticTemp.DeleteStopReason(ShutdownType.加载缓存超上限);
      return count >= _minCapacity ? await GetBatterysByTimeRange(startTime, count) : await _sugarDB.GetBattereyListAsync(_minCapacity);
   }

   private async Task<List<IBatMainModel>> GetBatterysByTimeRange(DateTime time, long count)
   {
      List<IBatMainModel> batterys = new((int)Math.Min(count, int.MaxValue));
      long startId = _snowflakeHelper.GetMinIdFromDateTime(time);
      Type type = _displayDatas.CompleteBatteryDatas.RuntimeBatteryType;
      DateTime now = DateTime.Now;
      var monthCount = time.GetMonthCount(now);

      await _dbFactory.UsingDbAsync(
         DatabaseRole.LocalDb1,
         async db =>
         {
            for (int i = 0; i < monthCount; i++)
            {
               var tableName = DbHelper.GetSplitTableNameByType(type, now.AddMonths(-i)); //根据时间获取表名
               if (!db.DbMaintenance.IsAnyTable(tableName, false))
               {
                  continue;
               }
               var datas = await db.QueryableByObject(type).AS(tableName).Where($"{nameof(BatMainModel.Id)}>={startId}").ToListAsync();
               var row = datas as IEnumerable;
               if (row != null)
               {
                  foreach (var item in row)
                  {
                     batterys.Add((IBatMainModel)item);
                  }
               }
            }
         }
      );

      return batterys;
   }

   private async Task<long> GetCountByTime(DateTime time)
   {
      long count = 0;
      long startId = _snowflakeHelper.GetMinIdFromDateTime(time);
      Type type = _displayDatas.CompleteBatteryDatas.RuntimeBatteryType;
      DateTime now = DateTime.Now;
      var monthCount = time.GetMonthCount(now);

      await _dbFactory.UsingDbAsync(
         DatabaseRole.LocalDb1,
         async db =>
         {
            for (int i = 0; i < monthCount; i++)
            {
               var tableName = DbHelper.GetSplitTableNameByType(type, now.AddMonths(-i)); //根据时间获取表名
               if (!db.DbMaintenance.IsAnyTable(tableName, false))
               {
                  continue;
               }
               count += await db.QueryableByObject(type).AS(tableName).Where($"{nameof(BatMainModel.Id)}>={startId}").CountAsync();

               if (count > _maxCapacity)
                  break;
            }
         }
      );

      return count;
   }
}
