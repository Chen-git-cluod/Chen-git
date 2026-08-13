using System.ComponentModel;

namespace Kinlo.Equipment.Devices.Fingerprints.Live20R;

/// <summary>
/// Live20R指纹器
/// </summary>
public class Live20R
{
   private nint _mDevHandle;
   private nint _mDBHandle;

   /// <summary>
   /// 读取指纹委托
   /// </summary>
   public Action<byte[], string>? FingerpringAction { get; set; } = null;
   CancellationToken _cancellationTokenSource;

   public Live20R(CancellationToken token)
   {
      _cancellationTokenSource = token;
   }

   public bool Open()
   {
      try
      {
         var _initRS = zkfp2.Init();
         if (_initRS != 0)
         {
            "[LIVE20R指纹仪器] 初始化失败".LogRun(Log4NetLevelEnum.错误);
            return false;
         }
         var flag = zkfp2.GetDeviceCount() <= 0;
         if (flag)
         {
            zkfp2.Terminate();
            "找不到设备".LogRun(Log4NetLevelEnum.错误);
            return false;
         }
         _mDevHandle = zkfp2.OpenDevice(0);
         if (_mDevHandle != nint.Zero)
         {
            _mDBHandle = zkfp2.DBInit();
            if (_mDBHandle != nint.Zero)
            {
               Read();
               "[LIVE20R指纹仪器]打开成功！".LogRun(Log4NetLevelEnum.成功);
               return true;
            }
         }
         $"[LIVE20R指纹仪器]打开失败，请关闭指纹其他软件，或者拔掉USB重插，并重启上位机尝试".LogRun(Log4NetLevelEnum.错误);
      }
      catch (Exception ex)
      {
         $"[LIVE20R指纹仪器]初始化异常：{ex}\r\n请关闭指纹其他软件，或者拔掉USB重插，并重启上位机尝试".LogRun(Log4NetLevelEnum.错误);
      }
      return false;
   }

   public void Close()
   {
      try
      {
         if (_mDevHandle != nint.Zero)
         {
            zkfp2.CloseDevice(_mDevHandle);
            zkfp2.Terminate();
         }
      }
      catch (Exception ex)
      {
         $"[LIVE20R指纹仪器] 退出异常：{ex}".LogRun(Log4NetLevelEnum.错误);
      }
   }

   private Task Read()
   {
      ThreadPool.QueueUserWorkItem(
         new WaitCallback(sender =>
         {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
               try
               {
                  var _result = GetTemplate();
                  if (_result.status)
                  {
                     FingerpringAction?.Invoke(_result.template, _result.strBase64);
                  }
               }
               catch (Exception ex)
               {
                  if (_cancellationTokenSource.IsCancellationRequested)
                     break;

                  $"[LIVE20R指纹仪器]循环异常：{ex}".LogRun(Log4NetLevelEnum.错误);
               }
               finally
               {
                  Thread.Sleep(20);
               }
            }
            Close();
         })
      );
      return Task.CompletedTask;
   }

   /// <summary>
   /// 获取指纹
   /// </summary>
   /// <returns>template 指纹模板,strBase64 指纹转string</returns>
   public (bool status, byte[] template, string strBase64) GetTemplate()
   {
      if (nint.Zero == _mDevHandle)
      {
         return default;
      }
      byte[] img_buffer = new byte[120000];
      byte[] template = new byte[2048];
      int size = template.Length;
      int ret = zkfp2.AcquireFingerprint(_mDevHandle, img_buffer, template, ref size);
      if (ret == 0)
      {
         string strBase64 = zkfp2.BlobToBase64(template, size);
         return (true, template, strBase64);
      }
      return (false, template, string.Empty);
      ;
   }

   public byte[] GetImage(ref MemoryStream ms)
   {
      if (nint.Zero == _mDevHandle)
      {
         return default;
      }
      byte[] img_buffer = new byte[120000];
      byte[] template = new byte[2048];
      int size = 2048;
      int ret = zkfp2.AcquireFingerprint(_mDevHandle, img_buffer, template, ref size);
      if (ret == 0)
      {
         BitmapFormat.GetBitmap(template, 100, 100, ref ms);
         return template;
      }
      return default;
   }

   public bool DBMatch(byte[] item1, byte[] item2)
   {
      if (nint.Zero == _mDBHandle)
      {
         $"[指纹对比]算法操作句柄未初始化！".LogRun(Log4NetLevelEnum.信息);
         return false;
      }
      int _RS = zkfp2.DBMatch(_mDBHandle, item1, item2);
      $"[指纹对比]对比值:[{_RS}]".LogRun(Log4NetLevelEnum.信息);
      return _RS > 100;
   }

   /// <summary>
   /// 添指纹至内存
   /// </summary>
   /// <param name="id"></param>
   /// <param name="finger"></param>
   /// <returns></returns>
   public bool DBAdd(int id, byte[] finger)
   {
      if (nint.Zero == _mDBHandle)
      {
         $"[添加指纹至内存]算法操作句柄未初始化！".LogRun(Log4NetLevelEnum.信息);
         return false;
      }
      var _RS = zkfp2.DBAdd(_mDBHandle, id, finger);
      if (_RS != 0)
      {
         $"[添加指纹至内存]失败！错误代码：[{_RS}]".LogRun(Log4NetLevelEnum.错误);
         return false;
      }
      else
         return true;
   }

   /// <summary>
   /// 删除内存指纹
   /// </summary>
   /// <param name="id"></param>
   /// <param name="finger"></param>
   /// <returns></returns>
   public bool DBDel(int id)
   {
      if (nint.Zero == _mDBHandle)
      {
         $"[删除内存指纹]算法操作句柄未初始化！".LogRun(Log4NetLevelEnum.信息);
         return false;
      }
      var _RS = zkfp2.DBDel(_mDBHandle, id);
      if (_RS != 0)
      {
         $"[删除内存指纹]失败！错误代码：[{_RS}]".LogRun(Log4NetLevelEnum.错误);
         return false;
      }
      else
         return true;
   }

   /// <summary>
   /// 识别
   /// </summary>
   /// <param name="finger"></param>
   /// <returns></returns>
   public int DBIdentify(byte[] finger)
   {
      if (nint.Zero == _mDBHandle)
      {
         $"[识别指纹]算法操作句柄未初始化！".LogRun(Log4NetLevelEnum.信息);
         return 0;
      }
      int _id = 0;
      int _score = 0;
      int _RS = zkfp2.DBIdentify(_mDBHandle, finger, ref _id, ref _score);
      if (_RS != 0)
      {
         $"[识别指纹]失败！错误代码：[{_RS}]".LogRun(Log4NetLevelEnum.错误);
         return -1;
      }
      else
      {
         return _id;
      }
   }

   /// <summary>
   /// 合并3枚指纹
   /// </summary>
   /// <param name="bytes"></param>
   /// <returns></returns>
   public (bool state, byte[] template) DBMerge(List<byte[]> bytes)
   {
      var regTemp = new byte[2048];
      int regTempLen = 0;
      if (nint.Zero == _mDBHandle)
      {
         $"[指纹合并]算法操作句柄未初始化！".LogRun(Log4NetLevelEnum.信息);
         return (false, regTemp);
      }
      if (zkfp2.DBMerge(_mDBHandle, bytes[0], bytes[1], bytes[2], regTemp, ref regTempLen) == 0)
      {
         return (true, regTemp);
      }
      return (false, regTemp);
   }
}
