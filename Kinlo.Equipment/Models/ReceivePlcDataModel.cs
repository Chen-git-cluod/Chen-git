using Kinlo.Equipment.Model.PLCInteractive;

namespace Kinlo.Equipment.Models;

public class ReceivePlcDataModel : PlcGeneric2DTU
{
   public byte Index { get; set; }

   /// <summary>
   ///
   /// </summary>
   public SignalAddressModel DataAddress { get; set; } = new SignalAddressModel();
}
