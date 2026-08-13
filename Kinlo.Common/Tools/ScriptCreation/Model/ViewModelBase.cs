using System.ComponentModel;

namespace Kinlo.Common.Tools.ScriptCreation.Model;

/// <summary>
/// 实现INotifyPropertyChanged
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
   {
      if (this.PropertyChanged != null)
         this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
   }
}
