namespace Kinlo.WebApi;

public interface IApiService
{
   /// <summary>
   /// 运行状态
   /// </summary>
   Func<bool, Task>? RunStatusFunc { get; set; }

   /// <summary>
   /// 启动WebApi
   /// </summary>
   /// <returns></returns>
   Task<bool> StartApiAsync();

   /// <summary>
   /// 停止WebApi
   /// </summary>
   /// <returns></returns>
   Task StopApiAsync();
}
