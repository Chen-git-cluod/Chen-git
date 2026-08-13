using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace Kinlo.GUI.ViewModel;

public class HipotCurveViewModel : Screen
{
   public ISeries[] ChatSeries { get; set; }
   public Axis[] XAxes { get; set; }
   public Axis[] YAxes { get; set; }
   public LabelVisual Title { get; set; }
}
