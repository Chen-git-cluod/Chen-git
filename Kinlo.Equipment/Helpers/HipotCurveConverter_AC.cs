namespace Kinlo.Equipment.Helpers;

/// <summary>
/// 艾测波形转换
/// </summary>
public static class HipotCurveConverter_AC
{
   /// <summary>
   /// 艾测 hipot 波形转换
   /// </summary>
   /// <param name="bytes"></param>
   /// <returns></returns>
   public static double[] ToAcHipotCurve(this byte[] bytes)
   {
      if (bytes == null || bytes.Length < 2)
         return [];
      int pointCount = bytes.Length / 2;
      double[] curve = new double[pointCount];

      for (int i = 0; i < pointCount; i++)
      {
         //  获取字节对，并明确为小端序转换
         //  将其强制转换为 short，这样可以正确处理负数（补码）
         short rawValue = (short)(bytes[i * 2 + 1] << 8 | bytes[i * 2]);

         // 3. 赋值给 double
         curve[i] = (double)rawValue;
      }
      return curve.ToArray();
   }
}
