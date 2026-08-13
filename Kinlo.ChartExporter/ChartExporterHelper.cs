using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

namespace Kinlo.ChartExporter;

public static class ChartExporterHelper
{
   // 复用画笔（减少 GC 压力）
   // 注意：SolidColorPaint 是托管对象，但内部有 Skia native 资源
   private static readonly SolidColorPaint LineStroke = new(SKColor.Parse("#006CBE"), 1);

   private static readonly SolidColorPaint GeometryFill = new(SKColor.Parse("#006CBE"));

   private static readonly SolidColorPaint GeometryStroke = new(SKColor.Parse("#006CBE"), 3);

   // 复用坐标轴（避免每张图重新分配 Axis）
   // 注意：Axis 是“状态对象”，不要运行时修改它
   public static readonly Axis[] DefaultXAxes = { new Axis { SeparatorsPaint = new SolidColorPaint(SKColors.LightGray, 1) } };

   public static readonly Axis[] DefaultYAxes = { new Axis { SeparatorsPaint = new SolidColorPaint(SKColors.LightGray, 0) } };

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
   /// 导出PNG
   /// </summary>
   /// <param name="datas"></param>
   /// <param name="filePath"></param>
   /// <param name="title"></param>
   public static void ExportToPng(double[] datas, string filePath, string title)
   {
      try
      {
         var chart = new SKCartesianChart
         {
            Width = 800,
            Height = 500,
            Series = new[] { CreateSeries(datas) },
            XAxes = DefaultXAxes,
            YAxes = DefaultYAxes,

            Title = new LabelVisual
            {
               Text = title,
               TextSize = 15,
               Padding = new LiveChartsCore.Drawing.Padding(15),
               Paint = new SolidColorPaint(SKColors.Black),
            },
         };

         // 确保目录存在
         Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

         // 写入图片（内部会进行 PNG Encode）
         chart.SaveImage(filePath);
      }
      catch (Exception ex)
      {
         Console.WriteLine(ex.ToString());
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

   /// <summary>
   /// Worker导出主流程
   /// </summary>
   /// <param name="obj"></param>
   /// <param name="writer"></param>
   public static void ExportCurveChart(HipotCurveModel[] obj, StreamWriter writer)
   {
      string basePath = Path.Combine("D:\\Hipot波形", DateTime.Now.ToString("yyyy年MM月dd日_HH时mm分ss秒"));

      Directory.CreateDirectory(basePath);

      int total = obj.Length;

      for (int i = 0; i < total; i++)
      {
         var item = obj[i];

         var curve = item.CurvePoint.ToChartPoint();
         if (curve == null)
            continue;

         var title = GetCurveTiele(item.Barcode, item.HipotIndex, item.HipotTime);

         var file = Path.Combine(basePath, $"Hipot_{item.Barcode}_{item.Id}.png");

         ExportToPng(curve, file, title);

         // 进度上报（每5条或最后一条）
         if (i % 5 == 0 || i == total - 1)
         {
            Console.WriteLine($"{i + 1}/{total}_{title}_{file}");
            writer.WriteLine($"PROGRESS|{i + 1}|{total}");
         }
      }
   }
}

public class HipotCurveModel
{
   public long Id { get; set; }
   public string Barcode { get; set; } = "";
   public DateTime HipotTime { get; set; }
   public byte HipotIndex { get; set; }
   public string CurvePoint { get; set; } = "";
}
