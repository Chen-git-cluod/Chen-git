namespace Kinlo.Common.Tools.ScriptCreation.Model;

/// <summary>
///
/// </summary>
public abstract class DynamicsBase : ViewModelBase
{
   public abstract int StartLineNumber { get; }
   public abstract string FilePath { get; }
   public abstract int EndLineNumber { get; }
   public static string ThisFilePath { get; } = ThisInfoHelper.GetThisFilePath();
}
