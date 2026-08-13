namespace Kinlo.Common.Models.ConfigModels;

public class LanguageModel
{
   public int Index { get; set; }

   // public string Key { get; set; } = string.Empty;
   public string Icon { get; set; } = string.Empty;
   public string Title { get; set; } = string.Empty;
   public bool IsSelected { get; set; }
}
