namespace Kinlo.Equipment.Helpers;

/// <summary>
/// 锐捷波形解析
/// </summary>
public static class HipotCurveConverter_RJ
{
   // 单条波形点数
   private const int CurvePointCount = 480;

   // 每个点占 4 字节（高低各 2 字节）
   private const int BytesPerPoint = 4;

   // 单条波形 byte 长度
   private const int SingleCurveBytesLength = CurvePointCount * BytesPerPoint; // 1920

   // 三条波形 byte 长度
   private const int CurveBytesLength = SingleCurveBytesLength * 3; // 5760

   // 报文头 + 校验尾
   private const int HeaderLength = 6;
   private const int TailLength = 2;

   // 总报文最小长度
   private const int TotalBytesLength = HeaderLength + CurveBytesLength + TailLength;

   private static readonly string[] LaneNames = ["正负"];

   /// <summary>
   /// 解析波形
   /// </summary>
   /// <param name="bytes"></param>
   /// <returns></returns>
   public static string ParseLineCurvePoint(this byte[] bytes)
   {
      if (bytes == null || bytes.Length < TotalBytesLength)
         return "";

      var laneCurveBytes = bytes.AsSpan(HeaderLength, CurveBytesLength);
      var results = new List<string>(1); // 3条波形 × 2行 = 6 修改只取一行

      for (int i = 0; i < 1; i++)
      {
         var lane = laneCurveBytes.Slice(i * SingleCurveBytesLength, SingleCurveBytesLength);
         var (highCurve, bottomCurve) = ParseSingleCurve(lane);

         //var name = LaneNames[i];
         results.Add($"{string.Join(',', highCurve)}");
         //results.Add($"{name}最小值:{string.Join(',', bottomCurve)}"); 只取最大值
        }
      return string.Join('|', results);
   }

   /// <summary>
   /// 解析单条
   /// </summary>
   /// <param name="bytes"></param>
   /// <returns></returns>
   private static (ushort[] HighCurve, ushort[] BottomCurve) ParseSingleCurve(ReadOnlySpan<byte> bytes)
   {
      var high = new ushort[CurvePointCount];
      var bottom = new ushort[CurvePointCount];

      for (int i = 0; i < CurvePointCount; i++)
      {
         var offset = i * BytesPerPoint;

         high[i] = (ushort)(bytes[offset] << 8 | bytes[offset + 1]);
         bottom[i] = (ushort)(bytes[offset + 2] << 8 | bytes[offset + 3]);
      }

      return (high, bottom);
   }
}
