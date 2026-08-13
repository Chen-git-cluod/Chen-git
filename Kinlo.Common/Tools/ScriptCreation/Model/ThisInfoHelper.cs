namespace Kinlo.Common.Tools.ScriptCreation.Model;

public static class ThisInfoHelper
{
   /// <summary>
   ///  允许获取包含调用方的源文件的完整路径。 这是编译时的文件路径。
   /// </summary>
   /// <param name="path"></param>
   /// <returns></returns>
   public static string GetThisFilePath([CallerFilePath] string path = "") => path;

   /// <summary>
   /// 允许获取源文件中调用方法的行号。
   /// </summary>
   /// <param name="path"></param>
   /// <returns></returns>
   public static int GetThisLineNumber([CallerLineNumber] int path = 0) => path;

   /// <summary>
   /// 允许获取方法调用方的方法或属性名称
   /// </summary>
   /// <param name="name"></param>
   /// <returns></returns>
   public static string GetThisMemberName([CallerMemberName] string name = "") => name;

   public static string ThisFilePath { get; } = GetThisFilePath();
}
