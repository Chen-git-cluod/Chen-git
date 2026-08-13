namespace Kinlo.Common.Model;

public class ShortcutKeyModel
{
   public bool IsCrtl { get; set; }
   public bool IsAlt { get; set; }
   public bool IsShift { get; set; }
   public char Key { get; set; }
   public Action? Action { get; set; } = null;
}
