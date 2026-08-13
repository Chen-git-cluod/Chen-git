namespace Kinlo.Common.Tools;

/// <summary>
/// httpclient 务必使用单例
/// </summary>
public class HttpClientSingleHelper : IDisposable
{
   private SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler()
   {
      UseCookies = false, // 是否自动处理cookie
      SslOptions = new System.Net.Security.SslClientAuthenticationOptions()
      {
         RemoteCertificateValidationCallback = (sender, cer, chain, err) => true,
      },

      //ConnectTimeout = Timeout.InfiniteTimeSpan, //建立TCP连接时的超时时间,默认不限制
      //Expect100ContinueTimeout = TimeSpan.FromSeconds(1),  //等待服务返回statusCode=100的超时时间,默认1秒
      //AllowAutoRedirect = true,//是否自动重定向
      //MaxAutomaticRedirections = 50//自动重定向的最大次数
      //MaxConnectionsPerServer = 100, //每个请求连接的最大数量，默认是int.MaxValue,可以认为是不限制
      //PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),//连接池中TCP连接最多可以闲置多久,默认2分钟
      //PooledConnectionLifetime = Timeout.InfiniteTimeSpan, //连接最长的存活时间,默认是不限制的,一般不用设置、
      //AutomaticDecompression = DecompressionMethods.GZip, //是否压缩，默认是None，即不压缩
      //MaxResponseHeadersLength = 64, //响应头数据大小限制,单位: KB默认：64，即：http响应头最大64KB，一般不用设置
   };
   public HttpClient HttpClientSingle;
   MesInterfaceParameterConfig mesParameter;

   public HttpClientSingleHelper(StyletIoC.IContainer container)
   {
      mesParameter = container.Get<MesInterfaceParameterConfig>();
      HttpClientSingle = new HttpClient(socketsHttpHandler);
      // HttpClientSingle.BaseAddress = new Uri(_mesParameter.MesInterfaceInfo.BaseAddress);
      //总超时（包含了从建立连接到读取完响应的所有时间,也就是发起请求到响应完全读取结束的总时间）
      HttpClientSingle.Timeout = TimeSpan.FromMilliseconds(mesParameter.MesInterfaceInfo.Timeout);
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="parameters">上传的参数字典</param>
   /// <param name="barcode">条码，如果有的话</param>
   /// <returns></returns>
   public async Task<(bool isSuccess, string content)> PostAsync(string url, HttpContent sendMessage)
   {
      for (int i = 0; i < mesParameter.MesInterfaceInfo.RetryCount + 1; i++)
      {
         try
         {
            // StringContent _stringContent = new StringContent(sendMessage, Encoding.UTF8, "application/json");
            var response = await HttpClientSingle.PostAsync(url, sendMessage);
            // _response.EnsureSuccessStatusCode();//如果不返回200就抛出异常
            var statusCode = response.StatusCode; //返回状态码
            var header = response.Headers; //返回响应头
            string responseBody = await response.Content.ReadAsStringAsync();
            return (true, responseBody ??= string.Empty);
         }
         catch (Exception ex) when (i < mesParameter.MesInterfaceInfo.RetryCount)
         {
            $"MES第{i + 1}次重试，URL[{url}]异常：{ex}".LogRun();
            await Task.Delay(1000);
            continue;
         }
         catch (Exception ex)
         {
            return (false, ex.Message);
         }
      }
      return (false, "重试次数已用尽");
   }

   public void Dispose()
   {
      HttpClientSingle?.Dispose();
      socketsHttpHandler?.Dispose();
   }
}
