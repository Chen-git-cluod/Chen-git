namespace Kinlo.GUI.ViewModel;

public class PLCResectionViewModel
{
   public PLCSignalConfig PLCSignal { get; set; }

   public PLCResectionViewModel(IContainer container)
   {
      PLCSignal = container.Get<PLCSignalConfig>();
   }
}
