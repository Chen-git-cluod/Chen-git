using System.Diagnostics;

namespace Kinlo.Update;

public class Program
{
   static string _7zipPath = "C:\\Program Files\\7-Zip\\7z.exe";

   /// <summary>
   /// 主程序路径
   /// </summary>
   static string mainProgramFilePath = string.Empty;

   /// <summary>
   /// 更新文件
   /// </summary>
   static string fromFilePath = string.Empty;

   /// <summary>
   /// 目标文件夹（被备份路径）
   /// </summary>
   static string toDirectoryPath = Environment.CurrentDirectory;

   /// <summary>
   /// 备份文件保存路径
   /// </summary>
   static string backupsDirectoryPath = @$"{new DirectoryInfo(toDirectoryPath).Parent.FullName}\WeightAppBackups";

   /// <summary>
   /// 备份时排除的文件,以 -x! 开头，可多个文件或文件夹
   /// </summary>
   static string backupIgnore = @$"-x!MESlog -x!logs -x!Log -x!TableData";

   /// <summary>
   /// 更新时排除的文件,以 -x! 开头，可多个文件或文件夹
   /// </summary>
   static string updateIgnore = @$"-x!config -x!appsettings.json";

   /// <summary>
   /// 1:更新，2：备份；3：还原; 4:重启
   /// </summary>
   static int state = 0;

   /// <summary>
   /// 传入数组应包含6个参数，
   /// 第一个代表状态；"1":为更新，会强制备份被更新文件夹；"2"：备份；"3":还原，还原和更新的区别为，还原不再备份
   /// 后4个分别为 mainProgramFilePath,fromFilePath,toDirectoryPath,backupsDirectoryPath,backupIgnore,updateIgnore
   /// </summary>
   /// <param name="args"></param>
   static void Main(string[] args)
   {
      Init(args);

      if (state == 1 || state == 3 || state == 4)
      {
         //启动主程序
         if (File.Exists(mainProgramFilePath))
         {
            Console.WriteLine("接任意键启动称重程序...");
            Console.ReadKey();
            Process.Start(mainProgramFilePath);
         }
         else
         {
            Console.WriteLine("无法找到称重程序，接任意键关闭...");
            Console.ReadKey();
         }
      }
      else
      {
         Console.WriteLine("接任意键关闭...");
         Console.ReadKey();
      }
      return;
   }

   static void Init(string[] args)
   {
      try
      {
         Console.WriteLine("开始执行更新程序...");
         Console.WriteLine($"开始检测所有路径合法性...");
         if (args.Length < 2)
         {
            Console.WriteLine($"更新参数最少需要2个；{string.Join(",", args)}");
            return;
         }
         state = int.Parse(args[0]);
         mainProgramFilePath = args[1];
         //自我重启
         if (state == 4)
            return;

         if (args.Length < 7)
         {
            Console.WriteLine($"更新参数最少需要7个；{string.Join(",", args)}");
            return;
         }
         //检测路径是否合法, 后6个分别为 mainProgramFilePath,fromFilePath,toDirectoryPath,backupsDirectoryPath,backupIgnore,
         for (int i = 1; i < args.Length - 1; i++)
         {
            string arg = args[i];
            if (args[0] == "2" && i == 2)
               continue;

            switch (i)
            {
               case 1:
               case 2:
                  if (!File.Exists(arg))
                  {
                     Console.WriteLine($"未找到{arg}文件！");
                     return;
                  }
                  break;
               case 3:
                  if (!Directory.Exists(arg))
                  {
                     Console.WriteLine($"未找到{arg}文件夹！");
                     return;
                  }
                  break;
            }
         }

         Console.WriteLine($"检测7-Zip程序是否安装在目录：[{_7zipPath}]");
         if (!File.Exists(_7zipPath))
         {
            Console.WriteLine("未安装7-Zip解压程序，请安装");
            return;
         }

         fromFilePath = args[2];
         toDirectoryPath = args[3];
         backupsDirectoryPath = args[4];
         backupIgnore = args[5];
         updateIgnore = args[6];

         if (args[0] == "1")
         {
            if (Backup())
            {
               if (Update())
                  Console.WriteLine("更新成功...");
               else
                  Console.WriteLine("更新失败...");
            }
            else
               Console.WriteLine("备份失败...");
         }
         else if (args[0] == "2")
         {
            if (Backup())
               Console.WriteLine("备份成功...");
            else
               Console.WriteLine("备份失败...");
         }
         else if (args[0] == "3")
         {
            if (Update())
               Console.WriteLine("还原成功...");
            else
               Console.WriteLine("还原失败...");
         }
      }
      catch (Exception ex)
      {
         Console.WriteLine($"异常：{ex}");
      }
   }

   /// <summary>
   /// 备份
   /// </summary>
   /// <returns></returns>
   /// <exception cref="Exception"></exception>
   static bool Backup()
   {
      try
      {
         if (!Directory.Exists(backupsDirectoryPath))
            Directory.CreateDirectory(backupsDirectoryPath);
         string _backupName = Path.Combine(backupsDirectoryPath, $"WeightAppBackup{DateTime.Now:yyMMddHHmmssfff}.7z");
         string _7zRunCmd = $"7z a -y -t7z -bsp1 {_backupName} {toDirectoryPath}\\* {backupIgnore}";

         var p = new Process();
         p.StartInfo.FileName = "cmd.exe";
         p.StartInfo.UseShellExecute = false;
         p.StartInfo.RedirectStandardInput = true;
         p.StartInfo.RedirectStandardOutput = true;
         p.StartInfo.RedirectStandardError = true;
         p.StartInfo.CreateNoWindow = true;
         //p.OutputDataReceived += SortOutputHandler;
         p.Start(); //启动程序
         p.StandardInput.AutoFlush = true;
         string root = Path.GetPathRoot(_7zipPath);
         root = root.Remove(root.Length - 1);
         p.StandardInput.WriteLine($"{root}");
         p.StandardInput.WriteLine($"cd {Path.GetDirectoryName(_7zipPath)}");
         p.StandardInput.WriteLine(_7zRunCmd);
         p.StandardInput.WriteLine("exit");
         StreamReader reader = p.StandardOutput;
         string line = reader.ReadLine();
         while (!reader.EndOfStream)
         {
            string msg = reader.ReadLine();

            Console.WriteLine(reader.ReadLine());
            //  if (System.Text.RegularExpressions.Regex.IsMatch(msg, "[\\d.]+%"))
            //  if (msg.Contains("%"))
            // Console.SetCursorPosition(0, Console.CursorTop - 1);
         }
         reader.Close();
         p.Close();
         return true;
      }
      catch (Exception ex)
      {
         throw new Exception("备份异常", ex);
      }
   }

   /// <summary>
   /// 更新
   /// </summary>
   /// <returns></returns>
   /// <exception cref="Exception"></exception>
   static bool Update()
   {
      try
      {
         // string _7zRunCmd = $"7z x -y -t{Path.GetExtension(fromFilePath).Substring(1)}  {fromFilePath} -aoa -o{toDirectoryPath}";
         string _7zRunCmd =
            $"7z x -y -bsp1 -t{Path.GetExtension(fromFilePath).Substring(1)}  {fromFilePath} {updateIgnore} -aoa -o{toDirectoryPath}";
         var p = new Process();
         p.StartInfo.FileName = "cmd.exe";
         p.StartInfo.UseShellExecute = false;
         p.StartInfo.RedirectStandardInput = true;
         p.StartInfo.RedirectStandardOutput = true;
         p.StartInfo.RedirectStandardError = true;
         p.StartInfo.CreateNoWindow = true;
         //p.OutputDataReceived += SortOutputHandler;
         p.Start(); //启动程序
         p.StandardInput.AutoFlush = true;
         string root = Path.GetPathRoot(_7zipPath);
         root = root.Remove(root.Length - 1);
         p.StandardInput.WriteLine($"{root}");
         p.StandardInput.WriteLine($"cd {Path.GetDirectoryName(_7zipPath)}");
         p.StandardInput.WriteLine(_7zRunCmd);
         p.StandardInput.WriteLine("exit");
         StreamReader reader = p.StandardOutput;
         string line = reader.ReadLine();
         while (!reader.EndOfStream)
         {
            Console.WriteLine(reader.ReadLine());
         }
         reader.Close();
         p.Close();
         return true;
      }
      catch (Exception ex)
      {
         throw new Exception("更新异常", ex);
      }
   }
}
