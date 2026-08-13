using System.Data.Common;
using System.Threading.Tasks;
using HandyControl.Controls;
using Kinlo.Common.Models.OhtenModels;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.Painting;
using static NPOI.HSSF.Util.HSSFColor;

namespace Kinlo.GUI.View;

/// <summary>
/// InjectionPumpTemperature.xaml 的交互逻辑
/// </summary>
public partial class InjectionPumpView : UserControl
{
   public InjectionPumpView(IContainer container)
   {
      InitializeComponent();
   }
}
