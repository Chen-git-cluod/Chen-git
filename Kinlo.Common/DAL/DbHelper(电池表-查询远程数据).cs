using System.Data;
using System.Dynamic;
using HandyControl.Tools.Extension;

namespace Kinlo.Common.DAL;

public partial class DbHelper
{
   #region  查询其它数据库 注液前后数据
   /// <summary>
   /// 取前工序数据
   /// </summary>
   /// <param name="status"></param>
   /// <param name="frontWeight"></param>
   /// <param name="rearWeight"></param>
   /// <param name="preVoltage"></param>
   /// <param name="nailHeight"></param>
   public record PreProcessData(
      PrePrcessDataEnum status,
      double frontWeight,
      double rearWeight,
      double preVoltage,
      double nailHeight,
      string barcode
   );

   public enum PrePrcessDataEnum
   {
      成功,
      失败,
      前工序数据不在范围,
   }

   /// <summary>
   /// 查询其它数据库 注液前后数据
   /// </summary>
   /// <param name="barcode"></param>
   /// <param name="logHeader"></param>
   /// <param name="count"></param>
   /// <returns></returns>
   public async Task<PreProcessData> GetOtherDatabaseDataAsync(string barcode, string logHeader, int count = 2)
   {
      var result = await _dbFactory.UsingDbAsync(
         DatabaseRole.RemoteDb1,
         async db => await OnGetOtherDatabaseDataAsync(db, barcode, logHeader, count)
      );
      if (result.status == PrePrcessDataEnum.失败)
      {
         result = await _dbFactory.UsingDbAsync(
            DatabaseRole.RemoteDb2,
            async db => await OnGetOtherDatabaseDataAsync(db, barcode, logHeader, count)
         );
      }
      return result!;
   }

   private async Task<PreProcessData> OnGetOtherDatabaseDataAsync(ISqlSugarClient db, string barcode, string logHeader, int count = 2)
   {
      if (db == null)
         return new PreProcessData(PrePrcessDataEnum.失败, 0, 0, 0, 0, barcode);

      float frontWeight = 0,
         rearWeight = 0,
         preVoltage = 0,
         nailHeight = 0;
      DateTime dateTime = DateTime.Now;
      string fields = _parameterConfig.AdvancedConfig.ProductionType switch
      {
         ProductionTypeEnum.回氦 =>
            $"{nameof(BatScanBeforeModel.NetWeight)},{nameof(BatWeightAfterModel.FinalAfterWeight)},{nameof(BatWeightManualRefillModel.ManualRefillWeight)},{nameof(BatVoltageTestModel.TestVoltageValue)},{nameof(BatNailModel.NailHeight)}",
         _ =>
            $"{nameof(BatScanBeforeModel.NetWeight)},{nameof(BatWeightAfterModel.FinalAfterWeight)},{nameof(BatWeightManualRefillModel.ManualRefillWeight)}",
      };
      for (int i = 0; i < count; i++)
      {
         try
         {
            var tableName = GetSplitTableNameByType(typeof(BatMainModel), dateTime.AddMonths(-i)); //根据时间获取表名
            var sql = @$"SELECT {fields} FROM {tableName} WHERE Barcode='{barcode}' ORDER BY {nameof(BatMainModel.Id)} desc LIMIT 1";
            $"[查询其它数据库_注液前后数据]开始查询,表名[{tableName}]；".LogProcess(logHeader);
            var battery = await db.SqlQueryable<ExpandoObject>(sql).FirstAsync();
            if (battery != null)
            {
               var dic = (IDictionary<string, object>)battery;
               float.TryParse(dic[nameof(BatScanBeforeModel.NetWeight)].ToString(), out frontWeight);
               float afterWeight = 0,
                  replenishWeight = 0;
               float.TryParse(dic[nameof(BatWeightAfterModel.FinalAfterWeight)].ToString(), out afterWeight);
               float.TryParse(dic[nameof(BatWeightManualRefillModel.ManualRefillWeight)].ToString(), out replenishWeight);
               rearWeight = replenishWeight > 0 ? replenishWeight : afterWeight;
               if (_parameterConfig.AdvancedConfig.ProductionType == ProductionTypeEnum.回氦)
               {
                  float.TryParse(dic[nameof(BatVoltageTestModel.TestVoltageValue)].ToString(), out preVoltage);
                  float.TryParse(dic[nameof(BatNailModel.NailHeight)].ToString(), out nailHeight);
                  $"[查询其它数据库_注液前后数据]取到数据,表名[{tableName}]，干重：{frontWeight}，后称重：{afterWeight}，补液称重：{replenishWeight}，前工序电压：{preVoltage}，前工序胶钉高度：{nailHeight}；".LogProcess(
                     logHeader,
                     Log4NetLevelEnum.成功
                  );
                  if (frontWeight > 0 && preVoltage > 0)
                  {
                     $"[查询其它数据库_注液前后数据]取到数据,表名[{tableName}]；".LogProcess(logHeader, Log4NetLevelEnum.成功);
                     return new PreProcessData(PrePrcessDataEnum.成功, frontWeight, rearWeight, preVoltage, nailHeight, barcode);
                  }
                  else
                  {
                     return new PreProcessData(
                        PrePrcessDataEnum.前工序数据不在范围,
                        frontWeight,
                        rearWeight,
                        preVoltage,
                        nailHeight,
                        barcode
                     );
                  }
               }
               else
               {
                  $"[查询其它数据库_注液前后数据]取到数据,表名[{tableName}]，干重：{frontWeight}，后称重：{afterWeight}，补液称重：{replenishWeight}；".LogProcess(
                     logHeader,
                     Log4NetLevelEnum.成功
                  );
                  if (frontWeight > 0)
                  {
                     $"[查询其它数据库_注液前后数据]取到数据,表名[{tableName}]；".LogProcess(logHeader, Log4NetLevelEnum.成功);
                     return new PreProcessData(PrePrcessDataEnum.成功, frontWeight, rearWeight, preVoltage, nailHeight, barcode);
                  }
                  else
                  {
                     return new PreProcessData(
                        PrePrcessDataEnum.前工序数据不在范围,
                        frontWeight,
                        rearWeight,
                        preVoltage,
                        nailHeight,
                        barcode
                     );
                  }
               }
            }
         }
         catch (Exception ex)
         {
            $"[查询其它数据库]异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
            // return (false, frontWeight, rearWeight, preVoltage, nailHeight);
         }
      }

      $"[查询其它数据库]未取到数据！".LogProcess(logHeader, Log4NetLevelEnum.错误);
      return new PreProcessData(PrePrcessDataEnum.失败, frontWeight, rearWeight, preVoltage, nailHeight, barcode);
   }
   #endregion
}
