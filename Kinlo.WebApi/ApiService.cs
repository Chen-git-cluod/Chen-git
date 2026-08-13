using System.Net;
using System.Net.NetworkInformation;

namespace Kinlo.WebApi;

public class ApiService : IApiService
{
   //过滤控制器，不在此列的不加载，可以由外部传入
   // string[] allowedLanguages = ["国轩接口", "报表统计接口"];
   private readonly IContainer _container;
   private WebApplication? _app;
   public Func<bool, Task>? RunStatusFunc { get; set; }

   public ApiService(IContainer container)
   {
      _container = container;
   }

   public async Task<bool> StartApiAsync()
   {
      if (_app != null)
      {
         $"WebApi状态为运行，无须重复运行!".LogRun(LogNet.Enums.Log4NetLevelEnum.信息);
         return true;
      }

      try
      {
         var webApiConfig = _container.Get<WebApiConfig>();

         var builder = WebApplication.CreateBuilder();

#if DEBUG   //强制控制环境（自托管情况下让 IsDevelopment() 生效）
         builder.Environment.EnvironmentName = "Development";
#else
         builder.Environment.EnvironmentName = "Production";
#endif
         string finalHost = "localhost";
         builder.WebHost.ConfigureKestrel(options =>
         {
            if (webApiConfig.ListeningIP.Contains('*'))
            {
               options.ListenAnyIP(webApiConfig.Port); //监听全网口 相当于 http://*:{port}
            }
            else
            {
               var ipResult = TryParseIP(webApiConfig.ListeningIP);
               if (ipResult.status)
               {
                  finalHost = webApiConfig.ListeningIP;
                  options.Listen(ipResult.ip!, webApiConfig.Port);
               }
               else
               {
                  $"注意：设置的IP[{webApiConfig.ListeningIP}]无效，自动改为监听全网口".LogRun(LogNet.Enums.Log4NetLevelEnum.警告);
                  options.ListenAnyIP(webApiConfig.Port); //监听全网口 相当于 http://*:{port}
               }
            }
         });

         builder.Services.AddSingleton(_container);

         builder
            .Services.AddControllers(options =>
            {
               options.Conventions.Add(new DynamicRouteConvention(webApiConfig)); //添加过滤及动态路由映射
            })
            //.ConfigureApplicationPartManager(manager =>
            //manager.FeatureProviders.Add(new SpecificControllerFeatureProvider(allowedLanguages)))//过滤控制器（暂未启用）
            .AddApplicationPart(this.GetType().Assembly);

         builder.Services.AddEndpointsApiExplorer(); //Swagger接口浏览器
         builder.Services.AddSwaggerGen(); //Swagger文档生成器
         builder.Services.AddAuthorization(); //授权服务

         // 添加 CORS 服务
         //builder.Services.AddCors(options =>
         //{
         //    options.AddDefaultPolicy(policy =>
         //    {
         //        policy //浏览器可跨域
         //             // .WithOrigins("http://localhost:5173") // 前端运行地址
         //             .AllowAnyOrigin()   // 允许所有前端请求(相对危险)
         //            .AllowAnyMethod()
         //            .AllowAnyHeader();
         //    });
         //});

         _app = builder.Build();

         // 注册框架状态
         var lifetime = _app.Services.GetRequiredService<IHostApplicationLifetime>();
         lifetime.ApplicationStarted.Register(() => _ = RunStatusFunc?.Invoke(true));
         lifetime.ApplicationStopped.Register(() => _ = RunStatusFunc?.Invoke(false));

         //if (_app.Environment.IsDevelopment())   // Swagger中间件配置
         if (webApiConfig.IsEnableSwagger) // Swagger中间件配置
         {
            _app.UseSwagger();
            _app.UseSwaggerUI(c =>
            {
               c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kinlo API v1");
            });

            OpenBrowser($"http://{finalHost}:{webApiConfig.Port}/swagger");
         }

         //   _app.UseCors();//启用 CORS 中间件（一定要在 UseAuthorization 之前）

         _app.UseAuthorization(); //权限管理

         _app.MapControllers(); // 映射路由

         await _app.StartAsync();

         $"WebApi启动成功，端口 [{webApiConfig.Port}]".LogRun(LogNet.Enums.Log4NetLevelEnum.成功);
         return true;
      }
      catch (Exception ex)
      {
         await StopApiAsync();
         _ = RunStatusFunc?.Invoke(false); // 变灰
         $"WebApi启动失败：{ex.Message}".LogRun(LogNet.Enums.Log4NetLevelEnum.错误, true);
         return false;
      }
   }

   private (bool status, IPAddress? ip) TryParseIP(string strIp)
   {
      if (!IPAddress.TryParse(strIp, out var ip))
      {
         return (false, null);
      }
      // 如果是回环地址，永远可用（127.0.0.1、::1）
      if (IPAddress.IsLoopback(ip))
         return (true, ip);

      // 获取所有活动的网络接口
      var interfaces = NetworkInterface.GetAllNetworkInterfaces().Where(ni => ni.OperationalStatus == OperationalStatus.Up);

      foreach (var ni in interfaces)
      {
         var props = ni.GetIPProperties();
         // 检查 IPv4 和 IPv6 地址
         if (props.UnicastAddresses.Any(ua => ua.Address.Equals(ip)))
         {
            return (true, ip);
         }
      }
      return (false, null);
   }

   private void OpenBrowser(string url)
   {
      try
      {
         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
      }
      catch (Exception ex)
      {
         $"打开浏览器异常：{ex}".LogRun(LogNet.Enums.Log4NetLevelEnum.警告);
      }
   }

   public async Task StopApiAsync()
   {
      if (_app != null)
      {
         await _app.StopAsync();
         await _app.DisposeAsync();
         _app = null;
         $"WebApi停止成功!".LogRun(LogNet.Enums.Log4NetLevelEnum.成功);
         return;
      }
      else
      {
         $"WebApi状态为停止，无须重复停止!".LogRun(LogNet.Enums.Log4NetLevelEnum.信息);
      }
   }
}
