namespace Kinlo.Equipment.Exceptions;

public class CIPHeaderException : Exception
{
   public CIPHeaderException(byte[] bytes)
      : base($"CIP：{FormatMessage(bytes)}") { }

   private static string FormatMessage(byte[] bytes)
   {
      int status = bytes[0] + (bytes[1] << 8);
      switch (status)
      {
         case 0x0001:
            return "发出了无效或不受支持的封装命令";
         case 0x0002:
            return "接收器中的内存资源不足，无法处理命令";
         case 0x0003:
            return "封装消息的数据部分中的数据形成不良或不正确";
         case 0x0004:
            return "Reserved for legacy(RA)";
         case 0x0064:
            return "向目标发送封装消息时，始发者使用了无效的会话句柄";
         case 0x0065:
            return "目标收到一个无效长度的信息";
         case 0x0069:
            return "不支持的封装协议修订";
         default:
            return $"未找到对应的错误代码：{status}";
      }
   }
}
