namespace Kinlo.Common.Models.ConfigModels;

[AddINotifyPropertyChangedInterface]
public class GenericCommandModel
{
   public int Index { get; set; }
   public SignalAddressModel Tag { get; set; } = new();

   private bool _isExcision;

   /// <summary>
   /// 是否被切除
   /// </summary>
   [JsonIgnore]
   public bool IsExcision
   {
      get { return _isExcision; }
      set
      {
         if (_isExcision != value)
         {
            _isExcision = value;
         }
      }
   }
   private bool _isEnabled = true;

   /// <summary>
   ///  是否启用
   /// </summary>
   public bool IsEnabled
   {
      get { return _isEnabled; }
      set
      {
         if (_isEnabled != value)
         {
            _isEnabled = value;
         }
      }
   }
}
