using System.IO;
using FluentFTP;

namespace Kinlo.MESDocking.Ftp;

/// <summary>
/// FTP
/// </summary>
public class FtpService : IDisposable, IAsyncDisposable
{
   private MesInterfaceParameterConfig _mesInterfaceParameterConfig;
   private FtpServiceInfoModel _ftpServiceInfo;
   private AsyncFtpClient? _asyncClient;
   private IContainer _container;

   public FtpService(IContainer container)
   {
      _container = container;
      _mesInterfaceParameterConfig = _container.Get<MesInterfaceParameterConfig>();
      if (_mesInterfaceParameterConfig != null)
         _ftpServiceInfo = _mesInterfaceParameterConfig.FtpServiceInfo;
   }

   public async Task<bool> OpenAsync()
   {
      try
      {
         if (_ftpServiceInfo == null)
            return false;

         await DisposeAsync();
         var openResult = await OnOpenAsync(
            _ftpServiceInfo.Host,
            _ftpServiceInfo.Name,
            _ftpServiceInfo.Password,
            _ftpServiceInfo.Port,
            _ftpServiceInfo.UploadTimeoutMs
         );
         if (openResult.status)
         {
            _asyncClient = openResult.asyncFtpClient;
            return true;
         }
         else
            return false;
      }
      catch (Exception ex)
      {
         $"FTP连接异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
         return false;
      }
   }

   public async Task<(bool status, AsyncFtpClient? asyncFtpClient)> OnOpenAsync(
      string host,
      string name,
      string password,
      int port = 21,
      int timeout = 30000
   )
   {
      try
      {
         FtpConfig ftpConfig = new FtpConfig
         {
            ConnectTimeout = 8000,
            ReadTimeout = timeout,
            SocketKeepAlive = true,
            RetryAttempts = 3,
            DataConnectionType = FtpDataConnectionType.AutoPassive,
         };
         var asyncClient = new AsyncFtpClient(host, name, password, port, ftpConfig);
         await asyncClient.AutoConnect();
         if (!asyncClient.IsConnected)
         {
            $"FTP 连接失败！".LogRun(Log4NetLevelEnum.错误);
         }
         return (asyncClient.IsConnected, asyncClient);
      }
      catch (Exception ex)
      {
         $"FTP连接异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
         return (false, null);
      }
   }

   /// <summary>
   /// 上传文件,每次调用都会新建连接
   /// </summary>
   /// <param name="localFilePath">本地文件路径</param>
   /// <param name="remoteDirectory">服务器目录</param>
   /// <param name="remoteFileName">在服务器上保存的文件名，如未定义取本地文件名</param>
   /// <param name="cancellationToken"></param>
   /// <returns></returns>
   public async Task<bool> UploadFileWithNewConnectionAsync(string localFilePath, string remoteDirectory, string remoteFileName)
   {
      $"[FTP]开始上传文件：{localFilePath}".LogRun(Log4NetLevelEnum.信息);

      if (!File.Exists(localFilePath))
      {
         $"本地文件不存在：{localFilePath}".LogRun(Log4NetLevelEnum.错误);
         return false;
      }

      AsyncFtpClient? asyncClient = null;
      try
      {
         //   确保已连接
         var openResult = await OnOpenAsync(
            _ftpServiceInfo.Host,
            _ftpServiceInfo.Name,
            _ftpServiceInfo.Password,
            _ftpServiceInfo.Port,
            _ftpServiceInfo.UploadTimeoutMs
         );
         if (!openResult.status)
         {
            $"FTP未连接，无法上传文件：{localFilePath}".LogRun(Log4NetLevelEnum.错误);
            return false;
         }

         asyncClient = openResult.asyncFtpClient!;
         //  确保远程目录存在（不存在则创建）
         if (!await asyncClient.DirectoryExists(remoteDirectory))
         {
            await asyncClient.CreateDirectory(remoteDirectory, true); // 递归
         }

         var remotePath = $"{remoteDirectory.TrimEnd('/')}/{remoteFileName}";
         $"上传文件：{localFilePath}，远程路径：{remotePath}".LogRun(Log4NetLevelEnum.信息);

         //   上传文件
         var status = await asyncClient.UploadFile(
            localFilePath,
            remotePath,
            FtpRemoteExists.Overwrite, //Overwrite为覆盖；FtpRemoteExists.Resume为断点续传
            true, // 如有需要，创建远程目录
            FtpVerify.None
         ); //不需要验证

         $"上传文件完成：{localFilePath}，远程路径：{remotePath}，状态：{(status == FtpStatus.Success ? "成功" : "失败")}".LogRun(
            Log4NetLevelEnum.信息
         );
         return status == FtpStatus.Success;
      }
      catch (OperationCanceledException) // 取消操作,传入的cancellationToken被取消
      {
         // 取消上传，不算异常
         return false;
      }
      catch (Exception ex)
      {
         $"FTP上传异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
         return false;
      }
      finally
      {
         await OnDisposeAsync(asyncClient);
      }
   }

   public void Dispose()
   {
      DisposeAsync().GetAwaiter().GetResult();
   }

   public async ValueTask DisposeAsync()
   {
      OnDisposeAsync(_asyncClient);
   }

   public async ValueTask OnDisposeAsync(AsyncFtpClient? asyncClient)
   {
      if (asyncClient != null)
      {
         if (asyncClient.IsConnected)
            await asyncClient.Disconnect();

         await asyncClient.DisposeAsync();
         asyncClient = null;
      }
   }
}
