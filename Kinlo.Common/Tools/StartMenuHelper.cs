using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Kinlo.Common.Tools;

/// <summary>
/// 从开始菜单扫描快捷方式并启动
/// </summary>
public static class StartMenuHelper
{
   /// <summary>
   /// 从开始菜单扫描快捷方式并启动
   /// </summary>
   /// <param name="keyword">
   /// 关键字
   /// 例如：
   /// 向日葵
   /// AweSun
   /// TeamViewer
   /// </param>
   /// <returns></returns>
   public static bool StartFromStartMenu(this string keyword, out string path, out string msg)
   {
      // 所有用户开始菜单
      string commonPrograms = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);

      // 当前用户开始菜单
      string userPrograms = Environment.GetFolderPath(Environment.SpecialFolder.Programs);

      // 扫描两个目录
      string[] searchDirs = [commonPrograms, userPrograms];

      foreach (string dir in searchDirs)
      {
         if (!Directory.Exists(dir))
         {
            continue;
         }

         // 递归扫描所有 .lnk
         string[] links = Directory.GetFiles(dir, "*.lnk", SearchOption.AllDirectories);

         foreach (string link in links)
         {
            // 文件名（不带后缀）
            string fileName = Path.GetFileNameWithoutExtension(link);

            // 模糊匹配
            // if (fileName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            if (fileName == keyword) //完整匹配
            {
               if (link.StartApp(out string m))
               {
                  msg = string.Empty;
                  path = link;
                  return true;
               }
               else
               {
                  msg = $"启动{keyword}失败:{m}";
                  path = "";
                  return false;
               }
            }
         }
      }
      msg = $"在快捷方式中未找到{keyword}";
      path = "";
      return false;
   }

   public static bool StartApp(this string path, out string msg)
   {
      try
      {
         Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
         msg = string.Empty;

         return true;
      }
      catch (Exception ex)
      {
         msg = $"启动{path}发生异常:{ex}";
         path = "";
         return false;
      }
   }
}
