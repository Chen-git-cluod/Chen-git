using HandyControl.Controls;

namespace Kinlo.Common.Tools;

public static class FileHelper
{
   public static string SaveBasePath = "Configs";

   /// <summary>
   /// 配方参数保存文件夹
   /// </summary>
   public static string ParameterSaveFolder { get; set; } = "ParameterRecip";

   public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
   {
      WriteIndented = true,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
   };

   static FileHelper()
   {
      if (!Directory.Exists(SaveBasePath))
         Directory.CreateDirectory(SaveBasePath);
   }

   public static T? LoadToEntity<T>() => LoadToEntity<T>(typeof(T).Name);

   public static T? LoadToEntity<T>(string fileName)
   {
      string _path = $"{SaveBasePath}\\{fileName}.json";
      try
      {
         if (File.Exists(_path))
         {
            var _str = File.ReadAllText(_path, Encoding.UTF8);
            var _entity = JsonSerializer.Deserialize<T>(_str);
            $"[加载文件] {_path} 加载成功".LogRun(Log4NetLevelEnum.成功);
            return _entity;
         }
         else
         {
            $"[加载文件] {_path} 无此文件".LogRun(Log4NetLevelEnum.警告);
            return default;
         }
      }
      catch (Exception ex)
      {
         $"[加载文件] {_path} 异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         return default;
      }
   }

   public static ObservableCollection<T>? LoadToObservableCollection<T>(string fileName)
   {
      string _path = $"{SaveBasePath}\\{fileName}.json";
      try
      {
         if (File.Exists(_path))
         {
            var _str = File.ReadAllText(_path, Encoding.UTF8);
            var _entity = JsonSerializer.Deserialize<ObservableCollection<T>>(_str);
            $"[加载文件] {_path} 加载成功".LogRun(Log4NetLevelEnum.成功);
            return _entity;
         }
         else
         {
            $"[加载文件] {_path} 无此文件".LogRun(Log4NetLevelEnum.警告);
            return default;
         }
      }
      catch (Exception ex)
      {
         $"[加载文件] {_path} 异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         return default;
      }
   }

   public static Dictionary<string, object>? LoadToDictionary(string fileName)
   {
      string _path = $"{SaveBasePath}\\{fileName}.json";
      try
      {
         if (File.Exists(_path))
         {
            var _str = File.ReadAllText(_path, Encoding.UTF8);
            var _dic = JsonSerializer.Deserialize<Dictionary<string, object>>(_str);
            $"[加载文件] {_path} 加载成功".LogRun(Log4NetLevelEnum.成功);
            return _dic;
         }
         else
         {
            $"[加载文件] {_path} 无此文件".LogRun(Log4NetLevelEnum.警告);
            return null;
         }
      }
      catch (Exception ex)
      {
         $"[加载文件] {_path} 异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         return null;
      }
   }

   public static string LoadToString<T>() => LoadToString(typeof(T).Name);

   public static string LoadToString(string name)
   {
      string path = $"{SaveBasePath}\\{name}.json";
      try
      {
         if (File.Exists(path))
         {
            var str = File.ReadAllText(path, Encoding.UTF8);
            $"[加载文件至字符串] {path} 加载成功".LogRun(Log4NetLevelEnum.成功);
            return str;
         }
         else
         {
            $"[加载文件至字符串] {path} 无此文件".LogRun(Log4NetLevelEnum.警告);
            return string.Empty;
         }
      }
      catch (Exception ex)
      {
         $"[加载文件至字符串] {path} 异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         return string.Empty;
      }
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="obj"></param>
   /// <param name="userNmae"></param>
   /// <param name="revise">修改记录</param>
   /// <param name="isPopup"></param>
   /// <param name="isPrintLog">是否打印日志</param>
   /// <returns></returns>
   public static bool FileSave(
      this object obj,
      string userNmae,
      string revise,
      bool isPopup = true,
      bool isPrintLog = true,
      string saveName = "",
      JsonSerializerOptions? serializerOptions = null
   )
   {
      string fileName = saveName == "" ? obj.GetType().Name : saveName;
      string path = $"{SaveBasePath}\\{fileName}.json";
      try
      {
         if (!Directory.Exists(SaveBasePath))
         {
            Directory.CreateDirectory(SaveBasePath);
         }

         string json = JsonSerializer.Serialize(obj, options: serializerOptions == null ? JsonOptions : serializerOptions);
         File.WriteAllText(path, json, Encoding.UTF8);
         string msg = $"[保存文件] 用户：{userNmae}, 修改文件：{fileName}，修改内容：{revise} 保存成功；";
         if (isPopup)
            Growl.Success(msg);
         if (isPrintLog)
            msg.LogSetting(Log4NetLevelEnum.成功);
         return true;
      }
      catch (Exception ex)
      {
         string msg = $"[保存文件] 用户：{userNmae}, 修改文件：{fileName}，修改内容：{revise}; \r\n发生异常：{ex}";
         Growl.Error(msg);
         msg.LogSetting(Log4NetLevelEnum.错误);
         return false;
      }
   }

   /// <summary>
   /// 验证路径是否合法（不会访问磁盘，所以即使路径不存在也会返回 true，只要格式合法）
   /// </summary>
   /// <param name="path"></param>
   /// <returns></returns>
   public static bool IsValidPath(this string path)
   {
      if (string.IsNullOrWhiteSpace(path))
         return false;

      try
      {
         _ = Path.GetFullPath(path);
         return true;
      }
      catch (ArgumentException) { }
      catch (NotSupportedException) { }
      catch (PathTooLongException) { }

      return false;
   }
}
