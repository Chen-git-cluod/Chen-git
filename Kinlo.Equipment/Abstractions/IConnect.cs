namespace Kinlo.Equipment.Interfaces;

public interface IConnect
{
   DeviceInfoModel DeviceInfo { get; set; }

   bool Open();
   void Close();

   /// <summary>
   ///
   /// </summary>
   /// <param name="buffer"></param>
   /// <param name="description">备注，传标记</param>
   /// <returns></returns>
   CommResult Write(byte[] buffer, string logHeader);

   /// <summary>
   ///
   /// </summary>
   /// <param name="length"></param>
   /// <param name="description">备注，传标记</param>
   /// <returns></returns>
   CommResult<byte[]> Read(int length, string logHeader);

   /// <summary>
   /// 尝试读取，不抛异常
   /// </summary>
   /// <param name="length"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   CommResult<byte[]> TryRead(int length, string logHeader);

   /// <summary>
   ///
   /// </summary>
   /// <param name="buffer"></param>
   /// <param name="protocol"></param>
   /// <param name="readLength"></param>
   /// <param name="description">备注，传标记</param>
   /// <returns></returns>
   CommResult<byte[]> WriteAndRead(byte[] buffer, IProtocolHelper? protocol, string logHeader, int readLength = 1024);

   ///// <summary>
   ///// 检查是否连接
   ///// </summary>
   ///// <returns></returns>
   //bool CheckConnected();
   /// <summary>
   /// 清理缓存
   /// </summary>
   void ClearCache(string logHeader);
   void SetTimeOut(int timeout);
}
