using System.Text.RegularExpressions;

namespace Kinlo.Common.Tools;

public static class TrayCodeConverter
{
   /// <summary>
   /// 托盘码string转int，为提高数据库查询效率
   /// </summary>
   /// <param name="strCode"></param>
   /// <param name="tattery">正则规则 @"(?<=^.{5}).{1,2}"</param>
   /// <returns></returns>
   public static int TrayCodeToInt(this string strCode, string tattery)
   {
      var _result = Regex.Match(strCode, tattery, RegexOptions.IgnoreCase);
      if (_result.Success && int.TryParse(_result.Value, out int _code))
         return _code;
      else
         return -1;
   }
}
