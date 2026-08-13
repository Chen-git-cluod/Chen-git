namespace Kinlo.Equipment.ProtocolHelpers;

public class RJ6900SeriesProtocol : IProtocolHelper
{
   public int CheckLength { get; set; }

   public RJ6900SeriesProtocol(int messageLength)
   {
      CheckLength = messageLength;
   }

   public bool Verify(IList<byte> bytes)
   {
      if (bytes != null && bytes.Count == CheckLength)
         return GetVerifySum(bytes) == bytes[bytes.Count - 2];
      return false;
   }

   public static bool OnGetVerifySum(IList<byte> bytes)
   {
      if (bytes == null || bytes.Count < 5)
         return false;

      return GetVerifySum(bytes) == bytes[bytes.Count - 2];
   }

   /// <summary>
   /// 取校验和
   /// </summary>
   /// <param name="bytes"></param>
   /// <returns></returns>
   public static byte GetVerifySum(IList<byte> bytes)
   {
      byte[] _verifyBytes = bytes.Skip(1).Take(bytes.Count - 3).ToArray();
      var _sum = _verifyBytes.Sum(x => x);
      var _sumBytes = BitConverter.GetBytes(_sum);
      return _sumBytes[0];
   }

   public byte[] Serialize(IList<byte> data)
   {
      return data.ToArray();
   }

   public byte[] Deserialize(IList<byte> data)
   {
      return data.ToArray();
   }
}
