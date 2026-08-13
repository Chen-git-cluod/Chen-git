namespace Kinlo.Equipment.Model.PLCInteractive;

public class PlcScanSignalDTU
{
   public PlcScanSignalDTU()
   {
      Cmd = new short[96];
      Resection = new bool[16];
   }

   public PlcScanSignalDTU(int cmdLength, int resectionLength)
   {
      Cmd = new short[cmdLength];
      Resection = new bool[resectionLength];
   }

   public short[] Cmd { get; set; }
   public bool[] Resection { get; set; }
}
