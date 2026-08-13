namespace Kinlo.Common.Models.ConfigModels.MesConfigModels;

[AddINotifyPropertyChangedInterface]
public class ProcessPropertyModel
{
   public string Processes { get; set; }
   public ObservableCollection<ProcessPropertyItemModel> ProcessProperties { get; set; } =
      new ObservableCollection<ProcessPropertyItemModel>();

   public ProcessPropertyModel(string processesType)
   {
      Processes = processesType;
   }
}

[AddINotifyPropertyChangedInterface]
public class ProcessPropertyItemModel
{
   public string LanguagerKey { get; set; } = string.Empty;
   public string LocalPropertyName { get; set; } = string.Empty;

   [JsonIgnore]
   public Type PropertyType { get; set; }

   public ProcessPropertyItemModel(string languagerKey, string localPropertyName, Type type)
   {
      LanguagerKey = languagerKey;
      LocalPropertyName = localPropertyName;
      PropertyType = type;
   }
}
