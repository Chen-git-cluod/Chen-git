namespace Kinlo.Equipment.Devices.Fingerprints.Live20R;

public class zkfp2
{
   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_Init();

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_Terminate();

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_GetDeviceCount();

   [DllImport("libzkfp.dll")]
   private static extern IntPtr ZKFPM_OpenDevice(int index);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_CloseDevice(IntPtr handle);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_GetCaptureParamsEx(IntPtr handle, ref int width, ref int height, ref int dpi);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_SetParameters(IntPtr handle, int nParamCode, byte[] paramValue, int cbParamValue);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_GetParameters(IntPtr handle, int nParamCode, byte[] paramValue, ref int cbParamValue);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_AcquireFingerprint(IntPtr handle, byte[] fpImage, uint cbFPImage, byte[] fpTemplate, ref int cbTemplate);

   [DllImport("libzkfp.dll")]
   private static extern IntPtr ZKFPM_CreateDBCache();

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_CloseDBCache(IntPtr hDBCache);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_GenRegTemplate(
      IntPtr hDBCache,
      byte[] temp1,
      byte[] temp2,
      byte[] temp3,
      byte[] regTemp,
      ref int cbRegTemp
   );

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_AddRegTemplateToDBCache(IntPtr hDBCache, uint fid, byte[] fpTemplate, uint cbTemplate);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_DelRegTemplateFromDBCache(IntPtr hDBCache, uint fid);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_ClearDBCache(IntPtr hDBCache);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_GetDBCacheCount(IntPtr hDBCache, ref int count);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_Identify(IntPtr hDBCache, byte[] fpTemplate, uint cbTemplate, ref int FID, ref int score);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_VerifyByID(IntPtr hDBCache, uint fid, byte[] fpTemplate, uint cbTemplate);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_MatchFinger(IntPtr hDBCache, byte[] fpTemplate1, uint cbTemplate1, byte[] fpTemplate2, uint cbTemplate2);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_ExtractFromImage(IntPtr hDBCache, string FilePathName, int DPI, byte[] fpTemplate, ref int cbTemplate);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_AcquireFingerprintImage(IntPtr hDBCache, byte[] fpImage, uint cbFPImage);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_Base64ToBlob(string src, IntPtr blob, uint cbBlob);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_BlobToBase64(IntPtr src, uint cbSrc, StringBuilder dst, uint cbDst);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_DBSetParameter(IntPtr handle, int nParamCode, int paramValue);

   [DllImport("libzkfp.dll")]
   private static extern int ZKFPM_DBGetParameter(IntPtr handle, int nParamCode, ref int paramValue);

   [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
   public static extern void OutputDebugString(string message);

   public static int Init()
   {
      return ZKFPM_Init();
   }

   public static int Terminate()
   {
      return ZKFPM_Terminate();
   }

   public static int GetDeviceCount()
   {
      return ZKFPM_GetDeviceCount();
   }

   public static IntPtr OpenDevice(int index)
   {
      return ZKFPM_OpenDevice(index);
   }

   public static int CloseDevice(IntPtr devHandle)
   {
      return ZKFPM_CloseDevice(devHandle);
   }

   public static int SetParameters(IntPtr devHandle, int code, byte[] pramValue, int size)
   {
      if (IntPtr.Zero == devHandle)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_HANDLE;
      }

      if (pramValue == null || pramValue.Length < size || size <= 0)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_PARAM;
      }

      return ZKFPM_SetParameters(devHandle, code, pramValue, size);
   }

   public static int GetParameters(IntPtr devHandle, int code, byte[] paramValue, ref int size)
   {
      if (IntPtr.Zero == devHandle)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_HANDLE;
      }

      size = paramValue.Length;
      return ZKFPM_GetParameters(devHandle, code, paramValue, ref size);
   }

   public static void WriteLog(string text)
   {
      _ = AppDomain.CurrentDomain.BaseDirectory;
      try
      {
         using StreamWriter streamWriter = File.AppendText("d:\\fplog\\zkfplogcsharp.log");
         streamWriter.WriteLine(text);
         streamWriter.Close();
      }
      catch (Exception) { }
   }

   public static int AcquireFingerprint(IntPtr devHandle, byte[] imgBuffer, byte[] template, ref int size)
   {
      if (IntPtr.Zero == devHandle)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_HANDLE;
      }

      size = template.Length;
      return ZKFPM_AcquireFingerprint(devHandle, imgBuffer, (uint)imgBuffer.Length, template, ref size);
   }

   public static int AcquireFingerprintImage(IntPtr devHandle, byte[] imgbuf)
   {
      if (IntPtr.Zero == devHandle)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_HANDLE;
      }

      return ZKFPM_AcquireFingerprintImage(devHandle, imgbuf, (uint)imgbuf.Length);
   }

   public static IntPtr DBInit()
   {
      return ZKFPM_CreateDBCache();
   }

   public static int DBFree(IntPtr dbHandle)
   {
      return ZKFPM_ClearDBCache(dbHandle);
   }

   public static int DBSetParameter(IntPtr dbHandle, int code, int value)
   {
      return ZKFPM_DBSetParameter(dbHandle, code, value);
   }

   public static int DBGetParameter(IntPtr dbHandle, int code, ref int value)
   {
      return ZKFPM_DBGetParameter(dbHandle, code, ref value);
   }

   public static int DBMerge(IntPtr dbHandle, byte[] temp1, byte[] temp2, byte[] temp3, byte[] regTemp, ref int regTempLen)
   {
      if (IntPtr.Zero == dbHandle)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_HANDLE;
      }

      regTempLen = regTemp.Length;
      return ZKFPM_GenRegTemplate(dbHandle, temp1, temp2, temp3, regTemp, ref regTempLen);
   }

   public static int DBAdd(IntPtr dbHandle, int fid, byte[] regTemp)
   {
      if (IntPtr.Zero == dbHandle)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_HANDLE;
      }

      return ZKFPM_AddRegTemplateToDBCache(dbHandle, (uint)fid, regTemp, (uint)regTemp.Length);
   }

   public static int DBDel(IntPtr dbHandle, int fid)
   {
      if (IntPtr.Zero == dbHandle)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_HANDLE;
      }

      return ZKFPM_DelRegTemplateFromDBCache(dbHandle, (uint)fid);
   }

   public static int DBClear(IntPtr dbHandle)
   {
      if (IntPtr.Zero == dbHandle)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_HANDLE;
      }

      return ZKFPM_ClearDBCache(dbHandle);
   }

   public static int DBCount(IntPtr dbHandle)
   {
      if (IntPtr.Zero == dbHandle)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_HANDLE;
      }

      int count = 0;
      ZKFPM_GetDBCacheCount(dbHandle, ref count);
      return count;
   }

   public static int DBIdentify(IntPtr dbHandle, byte[] temp, ref int fid, ref int score)
   {
      if (IntPtr.Zero == dbHandle)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_HANDLE;
      }

      return ZKFPM_Identify(dbHandle, temp, (uint)temp.Length, ref fid, ref score);
   }

   public static int DBMatch(IntPtr dbHandle, byte[] temp1, byte[] temp2)
   {
      if (IntPtr.Zero == dbHandle)
      {
         return zkfperrdef.ZKFP_ERR_INVALID_HANDLE;
      }

      return ZKFPM_MatchFinger(dbHandle, temp1, (uint)temp1.Length, temp2, (uint)temp2.Length);
   }

   public static int ExtractFromImage(IntPtr dbHandle, string FileName, int DPI, byte[] template, ref int size)
   {
      if (IntPtr.Zero == dbHandle)
      {
         return zkfperrdef.ZKFP_ERR_NOT_INIT;
      }

      return ZKFPM_ExtractFromImage(dbHandle, FileName, DPI, template, ref size);
   }

   public static byte[] Base64ToBlob(string base64Str)
   {
      if (base64Str == null || base64Str.Length <= 0 || base64Str.Length % 4 != 0)
      {
         return null;
      }

      byte[] array = Convert.FromBase64String(base64Str);
      int num = (array[8] << 8) & 0xFF00;
      num += array[9];
      if (num > 2048 || array.Length < num)
      {
         return null;
      }

      return array;
   }

   public static string BlobToBase64(byte[] blob, int nDataLen)
   {
      if (blob == null || blob.Length <= 0 || nDataLen <= 0 || blob.Length < nDataLen)
      {
         return "";
      }

      return Convert.ToBase64String(blob, 0, nDataLen, Base64FormattingOptions.None);
   }

   public static bool ByteArray2Int(byte[] buf, ref int value)
   {
      if (buf.Length < 4)
      {
         return false;
      }

      value = BitConverter.ToInt32(buf, 0);
      return true;
   }

   public static bool Int2ByteArray(int value, byte[] buf)
   {
      if (buf == null)
      {
         return false;
      }

      if (buf.Length < 4)
      {
         return false;
      }

      buf[0] = (byte)((uint)value & 0xFFu);
      buf[1] = (byte)((value & 0xFF00) >> 8);
      buf[2] = (byte)((value & 0xFF0000) >> 16);
      buf[3] = (byte)((uint)(value >> 24) & 0xFFu);
      return true;
   }
}
