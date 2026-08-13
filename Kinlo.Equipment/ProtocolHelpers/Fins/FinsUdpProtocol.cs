namespace Kinlo.Equipment.ProtocolHelpers.Fins;

internal class FinsUdpProtocol : IProtocolHelper
{
   byte DA1 = 0x01; // 目的节点地址
   byte SA1 = 0x01; //  源节点地址 (通常为1)

   public FinsUdpProtocol(byte destinationNode, byte localNode)
   {
      DA1 = destinationNode;
      SA1 = localNode;
   }

   public byte[] Deserialize(IList<byte> data) => data.Skip(14).Take(data.Count - 14).ToArray();

   public byte[] Serialize(IList<byte> data)
   {
      // FINS头部 (10字节)
      List<byte> finsHeader =
      [
         0x80, // ICF: 响应请求, 客户端到服务器
         0x00, // RSV: 保留
         0x02, // GCT: 网关计数
         0x00, // DNA: 目的网络地址
         DA1, // DA1: 目的节点地址
         0x00, // DA2: 目的单元地址
         0x00, // SNA: 源网络地址
         SA1, // SA1: 源节点地址 (通常为1)
         0x00, // SA2: 源单元地址
         0x00, // SID: 服务ID
         0x01, // 命令: 读存储器区域
      ];
      finsHeader.AddRange(data);
      // $"fins udp 发送 message: {BitConverter.ToString(finsHeader.ToArray())}".LogRun();
      return finsHeader.ToArray();
   }

   public bool Verify(IList<byte> bytes)
   {
      // $"fins udp 返回 message: {BitConverter.ToString(bytes.ToArray())}".LogRun();
      if (bytes.Count < 14)
         return false;
      string msg = "";
      if (FinsTcpProtocol.CheckEndCode(bytes[12], bytes[13], out msg))
         return true;
      throw new Exception($"FINS udp 错误代码：[{bytes[12]}], [{bytes[13]}] {msg} 长度：[{bytes.Count}]");
   }
}
