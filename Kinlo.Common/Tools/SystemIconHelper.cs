using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Kinlo.Common.Tools;

/// <summary>
/// 取系统图标
/// </summary>
public static class SystemIconHelper
{
   [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
   public struct SHFILEINFO
   {
      public IntPtr hIcon;
      public int iIcon;
      public uint dwAttributes;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string szDisplayName;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
      public string szTypeName;
   }

   [DllImport("shell32.dll", CharSet = CharSet.Auto)]
   public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

   [DllImport("user32.dll", SetLastError = true)]
   private static extern bool DestroyIcon(IntPtr hIcon);

   private const uint SHGFI_ICON = 0x100;
   private const uint SHGFI_LARGEICON = 0x0;
   private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
   private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
   private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

   public static ImageSource GetFileIcon(string path)
   {
      if (string.IsNullOrEmpty(path))
         return null;

      SHFILEINFO shfi = new SHFILEINFO();
      uint flags = SHGFI_ICON | SHGFI_LARGEICON;
      uint dwAttr = FILE_ATTRIBUTE_NORMAL;

      // 正确识别文件夹
      // 如果路径是真实存在的文件夹，或者以斜杠结尾，或者是扩展名
      if (Directory.Exists(path) || path.EndsWith("/") || path.EndsWith("\\"))
      {
         dwAttr = FILE_ATTRIBUTE_DIRECTORY;
         // 如果路径不存在（只是个路径字符串），必须加这个标志位才能拿到图标
         if (!Directory.Exists(path))
            flags |= SHGFI_USEFILEATTRIBUTES;
      }
      else if (path.StartsWith("."))
      {
         // 处理类似 ".xlsx" 这种纯扩展名
         flags |= SHGFI_USEFILEATTRIBUTES;
      }

      IntPtr res = SHGetFileInfo(path, dwAttr, ref shfi, (uint)Marshal.SizeOf(shfi), flags);

      if (res == IntPtr.Zero || shfi.hIcon == IntPtr.Zero)
         return null;

      try
      {
         var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(shfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
         bitmapSource.Freeze();
         return bitmapSource;
      }
      finally
      {
         DestroyIcon(shfi.hIcon);
      }
   }
}
