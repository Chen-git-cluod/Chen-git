using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Kinlo.GUI.View
{
   /// <summary>
   /// CpkShowView.xaml 的交互逻辑
   /// </summary>
   public partial class InjectCPKChartSingleShowView : Window
   {
      public InjectionCpkDto InjectionCpk { get; set; }
      public SolidColorPaint TextPaint { get; set; } =
         new SolidColorPaint() { Color = SKColors.DarkSlateGray, SKTypeface = SKFontManager.Default.MatchCharacter('汉') };

      public InjectCPKChartSingleShowView(InjectionCpkDto injectionCpk)
      {
         InitializeComponent();
         this.DataContext = this;
         InjectionCpk = injectionCpk;
      }
   }
}
