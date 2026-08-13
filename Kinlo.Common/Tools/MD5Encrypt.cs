using System.Security.Cryptography;

namespace Kinlo.Common.Tools;

public class MD5Encrypt
{
   /// <summary>
   /// 32位MD5加密
   /// </summary>
   /// <param name="password"></param>
   /// <returns></returns>
   public static string MD5Encrypt32(string password)
   {
      if (string.IsNullOrEmpty(password))
         return string.Empty;

      StringBuilder stringBuilder = new StringBuilder();
      using (MD5 md5 = MD5.Create())
      {
         byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
         foreach (byte b in hashBytes)
         {
            //得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符
            //但是在和对方测试过程中，发现我这边的MD5加密编码，经常出现少一位或几位的问题；
            //后来分析发现是 字符串格式符的问题， X 表示大写， x 表示小写，
            //X2和x2表示不省略首位为0的十六进制数字；
            stringBuilder.Append(b.ToString("x2"));
         }
      }
      return stringBuilder.ToString();
   }
}
