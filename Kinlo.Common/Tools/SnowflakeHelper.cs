namespace Kinlo.Common.Tools;

public class SnowflakeHelper
{
   /*
   |     1bit     |41bit 时间戳 | 10bit 机器ID |  12bit 序列号 |
   |--------------|------------|--------------|--------------|
   | 符号位(未使用) | 毫秒级时间戳 |   机器编号   | 每毫秒的序列号 |
    */

   private const long Twepoch = 1735689600000L; // 自定义起始时间 2025-01-01 00:00:00；（在此时间上约69年左右不会重复）
   private const int MachineIdBits = 10; // 机器 ID 位数
   private const int SequenceBits = 12; // 序列号位数

   private const long MaxMachineId = (1L << MachineIdBits) - 1;
   private const long MaxSequence = (1L << SequenceBits) - 1;

   private const int MachineIdShift = SequenceBits; // 机器码偏移
   private const int TimestampLeftShift = SequenceBits + MachineIdBits; //时间戳偏移

   private readonly long _machineId;
   private long _sequence = 0L; //序号
   private long _lastTimestamp = -1L; //最后时间戳
   private readonly object _lock = new object();

   /// <summary>
   ///
   /// </summary>
   /// <param name="machineId">设备ID，1-1023</param>
   /// <exception cref="ArgumentException"></exception>
   public SnowflakeHelper(long machineId)
   {
      if (machineId < 0 || machineId > MaxMachineId)
         throw new ArgumentException($"设备ID必须介于0和{MaxMachineId}之间!");

      _machineId = machineId;
   }

   public long NextId()
   {
      lock (_lock)
      {
         var timestamp = DateTime.Now.ToUnixTimeMilliseconds();
         if (timestamp < _lastTimestamp)
            throw new Exception("时钟不能向后移动（时间调小）,拒绝生成ID");

         if (timestamp == _lastTimestamp) //同一毫秒中生成了ID
         {
            _sequence = (_sequence + 1) & MaxSequence;
            if (_sequence == 0)
            {
               timestamp = WaitForNextMillis(_lastTimestamp); //一毫秒内产生的ID计数已达上限，等待下一毫秒
            }
         }
         else
         {
            _sequence = 0;
         }

         _lastTimestamp = timestamp;
         return CreateId(timestamp, _sequence);
      }
   }

   /// <summary>
   /// 获取当前时间最小雪花ID(当前秒最小ID)，注意：不能用于生成唯一ID，只能用于查询 ；
   /// </summary>
   /// <param name="dateTime"></param>
   /// <returns></returns>
   public long GetMinIdFromDateTime(DateTime dateTime) =>
      CreateId(UnixTimeHelper.ToUnixTimeMilliseconds(dateTime.AddMilliseconds(-dateTime.Millisecond)), 0);

   /// <summary>
   /// 获取当前时间最大雪花ID(当前秒最大ID)，注意：不能用于生成唯一ID，只能用于查询 ；
   /// </summary>
   /// <param name="dateTime"></param>
   /// <returns></returns>
   public long GetMaxIdFromDateTime(DateTime dateTime) =>
      CreateId(UnixTimeHelper.ToUnixTimeMilliseconds(dateTime.AddMilliseconds(-dateTime.Millisecond).AddMilliseconds(999)), MaxSequence);

   /// <summary>
   /// 雪花ID转时间
   /// </summary>
   /// <param name="id">雪花ID</param>
   /// <returns>时间</returns>
   public static DateTime GetDateTimeFromId(long id)
   {
      var timeStamp = (id >> TimestampLeftShift) + Twepoch;
      return DateTimeOffset.FromUnixTimeMilliseconds(timeStamp).LocalDateTime;
   }

   /// <summary>
   /// 等待下一个毫秒，生成时间戳
   /// </summary>
   /// <param name="lastTimestamp"></param>
   /// <returns></returns>
   private long WaitForNextMillis(long lastTimestamp)
   {
      long timestamp = DateTime.Now.ToUnixTimeMilliseconds();
      while (timestamp <= lastTimestamp)
      {
         timestamp = DateTime.Now.ToUnixTimeMilliseconds();
      }
      return timestamp;
   }

   /// <summary>
   /// 组装ID
   /// </summary>
   /// <param name="timestamp"></param>
   /// <param name="sequence"></param>
   /// <returns></returns>
   private long CreateId(long timestamp, long sequence) =>
      ((timestamp - Twepoch) << TimestampLeftShift) | (_machineId << MachineIdShift) | sequence;
}
