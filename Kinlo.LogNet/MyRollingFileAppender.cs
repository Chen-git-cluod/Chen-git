namespace Kinlo.LogNet;

internal class MyRollingFileAppender : log4net.Appender.RollingFileAppender
{
   private bool isFirstTime = true;

   protected override void OpenFile(string fileName, bool append)
   {
      if (isFirstTime)
      {
         isFirstTime = false; //不自动生成日志文件
         return;
      }

      base.OpenFile(fileName, append);
   }
}
