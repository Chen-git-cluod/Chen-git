namespace Kinlo.GUI.Helpers;

public static class ChartExporter
{
   // 复用画笔（减少 GC 压力）
   // 注意：SolidColorPaint 是托管对象，但内部有 Skia native 资源
   private static readonly SolidColorPaint LineStroke = new(SKColor.Parse("#006CBE"), 1);

   private static readonly SolidColorPaint GeometryFill = new(SKColor.Parse("#006CBE"));

   private static readonly SolidColorPaint GeometryStroke = new(SKColor.Parse("#006CBE"), 3);

   /// <summary>
   /// 创建折线 Series（每张图都会调用）
   /// </summary>
   /// <param name="datas"></param>
   /// <returns></returns>
   public static ISeries CreateSeries(double[] datas)
   {
      return new LineSeries<double>
      {
         Values = datas,
         Stroke = LineStroke,
         GeometryFill = GeometryFill,
         GeometryStroke = GeometryStroke,
         LineSmoothness = 0, // 0 = 折线
         GeometrySize = 0, // 不画点，降低渲染成本
         Fill = null,
         Name = "Hipot测试波形",
      };
   }

   // 曲线字符串解析
   // 支持：
   // 1. 新版 传入的本来就是 double
   // 2. 旧版 传入的是 byte[]
   public static double[]? ToChartPoint(this string curveStr)
   {
      var curveStrArray = curveStr.Split(',');

      if (curveStrArray.Length < 3)
         return null;

      // 旧协议（byte编码）
      if (curveStrArray.Length > 1100)
      {
         // 去掉校验尾
         curveStrArray = curveStrArray[..^2];

         var curve = new List<double>();

         for (int i = 0; i < curveStrArray.Length; i += 2)
         {
            if (curveStrArray.Length <= i + 1)
               break;

            byte.TryParse(curveStrArray[i], out var b1);
            byte.TryParse(curveStrArray[i + 1], out var b2);

            var point = (b2 << 8) | b1;
            curve.Add(point);
         }

         return curve.ToArray();
      }
      else
      {
         // 新协议（double）
         return curveStrArray.Select(x => double.TryParse(x, out var d) ? d : 0).ToArray();
      }
   }

   /// <summary>
   /// 标题生成
   /// </summary>
   /// <param name="barcoe"></param>
   /// <param name="index"></param>
   /// <param name="time"></param>
   /// <returns></returns>
   public static string GetCurveTiele(string barcoe, byte index, DateTime time) =>
      $"条码：{barcoe}，通道：{index}，时间：{time:yyyy/MM/dd HH:mm:ss}";
}

public class HipotCurveModel
{
   public long Id { get; set; }
   public string Barcode { get; set; } = "";
   public DateTime HipotTime { get; set; }
   public byte HipotIndex { get; set; }
   public string CurvePoint { get; set; } = "";
}
