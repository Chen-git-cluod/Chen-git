using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.GUI.ViewModel;

public class EnergyViewModel
{
   public ElectricMeterView ElectricView { get; set; }
   public DisplayDashboardPanelView DashboardPanelView { get; set; }

   public EnergyViewModel(IContainer container)
   {
      DashboardPanelView = new DisplayDashboardPanelView(container);
      ElectricView = new ElectricMeterView(container);
   }
}
