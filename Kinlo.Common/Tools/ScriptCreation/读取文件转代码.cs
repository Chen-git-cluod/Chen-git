using Kinlo.Common.Tools.ScriptCreation.Model;

namespace Kinlo.Common.Tools.ScriptCreation;

/// <summary>
/// 读取文件转代码,暂时未用，主要用来做外挂脚本
/// </summary>
public static class FileToCodeHelper
{
   public static string BuildingCode<T>(List<string> parentClassPaths, List<T> subclasses, bool propertyNotice)
      where T : DynamicsBase
   {
      List<string> _usings = new List<string>(); //构建引用代码
      List<string> _attribules = new List<string>(); //
      List<string> _inherits = new List<string>(); // 继承
      string _namespace = $"namespace {Helper.GetNamespaceName()}"; //构建命名空间代码
      string _classHeader = $"    public class {Helper.ClassName}"; //构建合并类头代码
      string _baseCodeStr = string.Empty; //构建独立类或基类代码
      StringBuilder _mergeCode = new StringBuilder(); //构建合并的类代码

      foreach (var path in parentClassPaths)
      {
         List<string> _baseCodes = new List<string>();
         var _lines = System.IO.File.ReadAllLines(path);
         foreach (var line in _lines)
         {
            if (line.TrimStart().StartsWith("using"))
            {
               if (!_usings.Any(x => x == line))
                  _usings.Add(line);
            }
            else if (line.TrimStart().StartsWith("namespace"))
            {
               //if (string.IsNullOrEmpty(_namespace))
               //    _namespace = line + "\r\n{\r\n";
            }
            else
            {
               if (line.TrimStart().StartsWith(@"//"))
                  continue;
               _baseCodes.Add(line);
            }
         }
         string _baseCodeTemp = string.Join("\r\n", _baseCodes).Trim();
         //int _startIndex = _baseCodeTemp.IndexOf('{');
         //int _endIndex = _baseCodeTemp.LastIndexOf('}');
         //_baseCodeStr += _baseCodeTemp.Substring(_startIndex + 1, _endIndex - _startIndex - 1);
         _baseCodeStr += _baseCodeTemp.Substring(1, _baseCodeTemp.Length - 2);
      }

      bool _startLineNumber = false;
      bool _filePath = false;
      bool _endLineNumber = false;
      foreach (var item in subclasses)
      {
         string _filePaht = item.FilePath;
         int _startNumber = item.StartLineNumber;
         int _endNumber = item.EndLineNumber;

         var _lines = System.IO.File.ReadAllLines(_filePaht);
         for (int i = 0; i < _lines.Length; i++)
         {
            var line = _lines[i];
            if (line.TrimStart().StartsWith(@"//"))
               continue;
            if (i < _startNumber - 1)
            {
               if (line.TrimStart().StartsWith("using"))
               {
                  if (!_usings.Any(x => x == line))
                     _usings.Add(line);
               }
               else if (line.TrimStart().StartsWith("namespace"))
               {
                  //if (string.IsNullOrEmpty(_namespace))
                  //    _namespace = line + "\r\n{\r\n";
               }
               else if (line.Contains("class"))
               {
                  var _inheritsTemps = line.Split(':');
                  if (_inheritsTemps.Length > 1) //类的继承
                  {
                     var _temps = _inheritsTemps[1].Split(",");
                     foreach (var t in _temps)
                     {
                        if (!_inherits.Any(x => x == t))
                           _inherits.Add(t);
                     }
                  }
                  i = _startNumber - 2; //跳走
               }
               else //类特性
               {
                  if (line.Trim().Length > 1 && !_attribules.Any(x => x == line))
                     _attribules.Add(line);
               }
            }
            else
            {
               if (i >= _startNumber - 1 && i < _endNumber)
               {
                  if (line.Contains("StartLineNumber"))
                  {
                     if (_startLineNumber)
                        break;
                     else
                        _startLineNumber = true;
                  }
                  if (line.Contains("FilePath"))
                  {
                     if (_filePath)
                        break;
                     else
                        _filePath = true;
                  }
                  if (line.Contains("EndLineNumber"))
                  {
                     if (_endLineNumber)
                        break;
                     else
                        _endLineNumber = true;
                  }

                  if (propertyNotice)
                  {
                     var _isProperty = StructureProperty(line);
                     if (_isProperty.state)
                     {
                        _mergeCode.Append(_isProperty.msg);
                     }
                     else
                        _mergeCode.Append(line + "\r\n");
                  }
                  else
                     _mergeCode.Append(line + "\r\n");
               }
            }
         }
      }
      var _result =
         string.Join("\r\n", _usings)
         + "\r\n"
         + _namespace
         + "\r\n"
         + "{"
         + "\r\n"
         + _baseCodeStr
         + string.Join("\r\n", _attribules)
         + "\r\n"
         + _classHeader
         + (_inherits.Count > 0 ? ":" + string.Join(',', _inherits) : string.Empty)
         + "\r\n"
         + "    {\r\n"
         + _mergeCode.ToString()
         + "\r\n    }\r\n}";
      return _result;
   }

   /// <summary>
   /// 改属性更新通知
   /// </summary>
   /// <param name="line"></param>
   /// <returns></returns>
   public static (bool state, string msg) StructureProperty(string line)
   {
      if (line.Trim().StartsWith("public") && line.Contains("set") && line.Contains("get"))
      {
         var _group = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
         var _variable = _group[2];
         var v = _variable[0].ToString().ToLower();
         _variable = _variable.Remove(0, 1);
         _variable = '_' + v + _variable;
         string _value = string.Empty;
         if (_group.Length > 7)
         {
            for (int i = 7; i < _group.Length; i++)
            {
               _value += _group[i];
            }
         }

         var _property =
            $@"       {_group[0]} {_group[1]} {_group[2]} 
        {{
            get {{ return {_variable}; }}
            set {{
                   if( {_variable} != value)
                   {{ 
                        {_variable} = value;
                   }}                        
                   OnPropertyChanged(); 
                }}
        }}
        
       private {_group[1]} {_variable}{_value}{(!_value.Trim().EndsWith(';') ? ";" : string.Empty)}" + "\r\n";

         return (true, _property);
      }
      else
      {
         return (false, string.Empty);
      }
   }
}
