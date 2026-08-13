namespace Kinlo.Equipment.Interfaces;

public interface IProtocolHelper
{
   /// <summary>
   /// 校验
   /// </summary>
   /// <param name="bytes"></param>
   /// <returns></returns>
   bool Verify(IList<byte> bytes);

   /// <summary>
   /// 序列化
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   byte[] Serialize(IList<byte> data);

   /// <summary>
   /// 反序列化
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   byte[] Deserialize(IList<byte> data);
}
