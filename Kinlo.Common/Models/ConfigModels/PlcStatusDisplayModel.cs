namespace Kinlo.Common.Models.ConfigModels;

[AddINotifyPropertyChangedInterface]
public class PlcStatusDisplayModel
{
   public string Description { get; set; } = string.Empty;

   [JsonIgnore]
   public Brush Color { get; set; } = Brushes.Black;
}

public class PlcStopReasonModel
{
   public long Id { get; set; }
   public PlcStopReasonTypeEnum StopReason { get; set; } = PlcStopReasonTypeEnum.其它停机;
   public DateTime StartTime { get; set; }
}
