namespace Kinlo.Common.Cache;

public interface IBatteryCache
{
   /// <summary>
   /// 缓存预热
   /// </summary>
   /// <returns></returns>
   Task LoadCache();
   void Put(IBatMainModel value, string logHeader);

   /// <summary>
   ///
   /// </summary>
   /// <param name="id"></param>
   /// <param name="logHeader"></param>
   /// <param name="useCacheOnly">仅使用缓存，不查数据库</param>
   /// <returns></returns>
   Task<IBatMainModel?> GetByIdAsync(long id, string logHeader, bool useCacheOnly = false);

   /// <summary>
   ///
   /// </summary>
   /// <param name="barcode"></param>
   /// <param name="logHeader"></param>
   /// <param name="useCacheOnly">仅使用缓存，不查数据库</param>
   /// <returns></returns>
   Task<IBatMainModel?> GetByBarcodeAsync(string barcode, string logHeader, bool useCacheOnly = false);

   /// <summary>
   /// 批量删除缓存
   /// </summary>
   /// <param name="ids"></param>
   void RemoveByIds(IEnumerable<long> ids);
   IBatMainModel[] GetAll();
}
