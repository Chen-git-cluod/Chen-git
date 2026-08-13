namespace Kinlo.Equipment.ProtocolHelpers.Fins;

public class FinsTcpProtocol : IProtocolHelper
{
   #region file
   private byte ICF = 0x80;
   private byte RSV = 0x00;
   private byte GCT = 0x02;

   /// <summary>
   /// PLC网络号 一般为0x00
   /// </summary>
   private byte DNA = 0x00;

   /// <summary>
   /// 服务器节点（握手包返回）
   /// </summary>
   private byte DA1;

   /// <summary>
   /// PLC单元号
   /// </summary>
   private byte DA2 = 0x00;

   /// <summary>
   /// 本机网络号
   /// </summary>
   private byte SNA = 0x00;

   /// <summary>
   /// 本机节点 IP最后一位
   /// </summary>
   private byte SA1;

   /// <summary>
   /// 源单元号 与目标一致
   /// </summary>
   private byte SA2 = 0x00;

   /// <summary>
   /// 读取时候为：0xFF 写入时为：0x00
   /// </summary>
   private byte SID = 0x00;

   /// <summary>
   /// 读写内存区域都为1
   /// </summary>
   private byte MR = 0x01;
   #endregion
   public FinsTcpProtocol(byte sa1, byte da1)
   {
      SA1 = sa1;
      DA1 = da1;
   }

   public byte[] Serialize(IList<byte> data)
   {
      List<byte> bytes = new List<byte>();
      #region Fins Tcp Head
      bytes.Add(0x46); //F
      bytes.Add(0x49); //I
      bytes.Add(0x4e); //N
      bytes.Add(0x53); //S
      bytes.Add(0x00);
      bytes.Add(0x00);
      bytes.Add(0x00);
      bytes.Add((byte)(data[0] == 0x01 ? 0x1a : 19 + data.Count)); //12字节：从命令开始算数据长度

      //命令码
      bytes.Add(0x00);
      bytes.Add(0x00);
      bytes.Add(0x00);
      bytes.Add(0x02);

      //错误代码
      bytes.Add(0x00);
      bytes.Add(0x00);
      bytes.Add(0x00);
      bytes.Add(0x00);
      #endregion
      bytes.Add(ICF);
      bytes.Add(RSV);
      bytes.Add(GCT);
      bytes.Add(DNA);
      bytes.Add(DA1);
      bytes.Add(DA2);
      bytes.Add(SNA);
      bytes.Add(SA1);
      bytes.Add(SA2);
      bytes.Add(SID);
      bytes.Add(MR);
      bytes.AddRange(data);
      // $"SendTcpFins：{BitConverter.ToString(bytes.ToArray())}".LogRun(Log4NetLevelEnum.信息);
      return bytes.ToArray();
   }

   public byte[] Deserialize(IList<byte> data)
   {
      // $"ReciceTcpFins：{BitConverter.ToString(data.ToArray())}".LogRun(Log4NetLevelEnum.信息);
      return data.Skip(30).Take(data.Count - 30).ToArray();
   }

   public bool Verify(IList<byte> bytes)
   {
      if (bytes.Count < 8)
         return false;
      if (bytes.Count < ByteToInt(bytes.Skip(4).Take(4).ToArray()))
         return false;
      int error = ByteToInt(bytes.Skip(12).Take(4).ToArray());
      string str = "";
      if (error == 0 && CheckEndCode(bytes[28], bytes[29], out str))
      {
         return true;
      }
      throw new Exception($"FINS TCP 错误代码：[{bytes[28]}], [{bytes[29]}] {str} 长度：[{bytes.Count}]");
   }

   /// <summary>
   /// 字节转整形
   /// </summary>
   /// <param name="bytes"></param>
   /// <returns></returns>
   private int ByteToInt(byte[] bytes) => (bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3];

   /// <summary>
   /// 检查命令帧中的EndCode
   /// </summary>
   /// <param name="Main">主码</param>
   /// <param name="Sub">副码</param>
   /// <param name="info">错误信息</param>
   /// <returns>指示程序是否可以继续进行</returns>
   public static bool CheckEndCode(byte Main, byte Sub, out string info)
   {
      info = default;
      if ((Main & 0x80) == 0x80)
      {
         info = "Network relay error";
         return false;
      }
      switch (Main)
      {
         case 0x00:
            switch (Sub)
            {
               case 0x00:
                  return true; //成功的唯一情况
               case 0x01:
                  info = "service canceled";
                  return false;
               case 0x40:
                  info = "Non-fatal CPU Unit error";
                  return true;
               case 0x80:
                  info = "Fatal CPU Unit error";
                  return true;
            }
            break;
         case 0x01:
            switch (Sub)
            {
               case 0x01:
                  info = "local node not in network";
                  return false;
               case 0x02:
                  info = "token timeout";
                  return false;
               case 0x03:
                  info = "retries failed";
                  return false;
               case 0x04:
                  info = "too many send frames";
                  return false;
               case 0x05:
                  info = "node address range error";
                  return false;
               case 0x06:
                  info = "node address duplication";
                  return false;
            }
            break;
         case 0x02:
            switch (Sub)
            {
               case 0x01:
                  info = "destination node not in network";
                  return false;
               case 0x02:
                  info = "unit missing";
                  return false;
               case 0x03:
                  info = "third node missing";
                  return false;
               case 0x04:
                  info = "destination node busy";
                  return false;
               case 0x05:
                  info = "response timeout";
                  return false;
            }
            break;
         case 0x03:
            switch (Sub)
            {
               case 0x01:
                  info = "communications controller error";
                  return false;
               case 0x02:
                  info = "CPU unit error";
                  return false;
               case 0x03:
                  info = "controller error";
                  return false;
               case 0x04:
                  info = "unit number error";
                  return false;
            }
            break;
         case 0x04:
            switch (Sub)
            {
               case 0x01:
                  info = "undefined command";
                  return false;
               case 0x02:
                  info = "not supported by model/version";
                  return false;
            }
            break;
         case 0x05:
            switch (Sub)
            {
               case 0x01:
                  info = "destination address setting error";
                  return false;
               case 0x02:
                  info = "no routing tables";
                  return false;
               case 0x03:
                  info = "routing table error";
                  return false;
               case 0x04:
                  info = "too many relays";
                  return false;
            }
            break;
         case 0x10:
            switch (Sub)
            {
               case 0x01:
                  info = "command too long";
                  return false;
               case 0x02:
                  info = "command too short";
                  return false;
               case 0x03:
                  info = "elements/data don't match";
                  return false;
               case 0x04:
                  info = "command format error";
                  return false;
               case 0x05:
                  info = "header error";
                  return false;
            }
            break;
         case 0x11:
            switch (Sub)
            {
               case 0x01:
                  info = "area classification missing";
                  return false;
               case 0x02:
                  info = "access size error";
                  return false;
               case 0x03:
                  info = "address range error";
                  return false;
               case 0x04:
                  info = "address range exceeded";
                  return false;
               case 0x06:
                  info = "program missing";
                  return false;
               case 0x09:
                  info = "relational error";
                  return false;
               case 0x0a:
                  info = "duplicate data access";
                  return false;
               case 0x0b:
                  info = "response too long";
                  return false;
               case 0x0c:
                  info = "parameter error";
                  return false;
            }
            break;
         case 0x20:
            switch (Sub)
            {
               case 0x02:
                  info = "protected";
                  return false;
               case 0x03:
                  info = "table missing";
                  return false;
               case 0x04:
                  info = "data missing";
                  return false;
               case 0x05:
                  info = "program missing";
                  return false;
               case 0x06:
                  info = "file missing";
                  return false;
               case 0x07:
                  info = "data mismatch";
                  return false;
            }
            break;
         case 0x21:
            switch (Sub)
            {
               case 0x01:
                  info = "read-only";
                  return false;
               case 0x02:
                  info = "protected , cannot write data link table";
                  return false;
               case 0x03:
                  info = "cannot register";
                  return false;
               case 0x05:
                  info = "program missing";
                  return false;
               case 0x06:
                  info = "file missing";
                  return false;
               case 0x07:
                  info = "file name already exists";
                  return false;
               case 0x08:
                  info = "cannot change";
                  return false;
            }
            break;
         case 0x22:
            switch (Sub)
            {
               case 0x01:
                  info = "not possible during execution";
                  return false;
               case 0x02:
                  info = "not possible while running";
                  return false;
               case 0x03:
                  info = "wrong PLC mode";
                  return false;
               case 0x04:
                  info = "wrong PLC mode";
                  return false;
               case 0x05:
                  info = "wrong PLC mode";
                  return false;
               case 0x06:
                  info = "wrong PLC mode";
                  return false;
               case 0x07:
                  info = "specified node not polling node";
                  return false;
               case 0x08:
                  info = "step cannot be executed";
                  return false;
            }
            break;
         case 0x23:
            switch (Sub)
            {
               case 0x01:
                  info = "file device missing";
                  return false;
               case 0x02:
                  info = "memory missing";
                  return false;
               case 0x03:
                  info = "clock missing";
                  return false;
            }
            break;
         case 0x24:
            switch (Sub)
            {
               case 0x01:
                  info = "table missing";
                  return false;
            }
            break;
         case 0x25:
            switch (Sub)
            {
               case 0x02:
                  info = "memory error";
                  return false;
               case 0x03:
                  info = "I/O setting error";
                  return false;
               case 0x04:
                  info = "too many I/O points";
                  return false;
               case 0x05:
                  info = "CPU bus error";
                  return false;
               case 0x06:
                  info = "I/O duplication";
                  return false;
               case 0x07:
                  info = "CPU bus error";
                  return false;
               case 0x09:
                  info = "SYSMAC BUS/2 error";
                  return false;
               case 0x0a:
                  info = "CPU bus unit error";
                  return false;
               case 0x0d:
                  info = "SYSMAC BUS No. duplication";
                  return false;
               case 0x0f:
                  info = "memory error";
                  return false;
               case 0x10:
                  info = "SYSMAC BUS terminator missing";
                  return false;
            }
            break;
         case 0x26:
            switch (Sub)
            {
               case 0x01:
                  info = "no protection";
                  return false;
               case 0x02:
                  info = "incorrect password";
                  return false;
               case 0x04:
                  info = "protected";
                  return false;
               case 0x05:
                  info = "service already executing";
                  return false;
               case 0x06:
                  info = "service stopped";
                  return false;
               case 0x07:
                  info = "no execution right";
                  return false;
               case 0x08:
                  info = "settings required before execution";
                  return false;
               case 0x09:
                  info = "necessary items not set";
                  return false;
               case 0x0a:
                  info = "number already defined";
                  return false;
               case 0x0b:
                  info = "error will not clear";
                  return false;
            }
            break;
         case 0x30:
            switch (Sub)
            {
               case 0x01:
                  info = "no access right";
                  return false;
            }
            break;
         case 0x40:
            switch (Sub)
            {
               case 0x01:
                  info = "service aborted";
                  return false;
            }
            break;
      }
      info = "unknown exception";
      return false;
   }
}
