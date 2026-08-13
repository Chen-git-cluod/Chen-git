namespace Kinlo.GUI.ViewModel;

/// <summary>
/// 自定义信号
/// </summary>
public class CustomInteractViewModel : Screen
{
   private IContainer _container;
   private int _code = 0;
   PLCSignalConfig _plcSignalConfig;
   UsersStatusConfig _userStatus;
   public string Title { get; set; }
   public CustomPlcInteractAddressModel CustomPlcInteractAddress { get; set; } = new();
   public IEnumerable<CustomInteractNameEnum> CustomInteractNames => Enum.GetValues<CustomInteractNameEnum>();
   public List<int> Numbers { get; set; }

   public CustomInteractViewModel(IContainer container)
   {
      Numbers = Enumerable.Range(1, 32).ToList();
      _container = container;
      _plcSignalConfig = container.Get<PLCSignalConfig>();
      _userStatus = container.Get<UsersStatusConfig>();
   }

   public void SetCustomSignal(CustomPlcInteractAddressModel custom, int code)
   {
      Title = code == 1 ? "新建自定义信号" : "编辑自定义信号";
      _code = code;
      ExpressionAssignmentMapper<CustomPlcInteractAddressModel, CustomPlcInteractAddressModel>.Trans(custom, CustomPlcInteractAddress);
      ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(custom.DataAddress, CustomPlcInteractAddress.DataAddress);
      ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
         custom.ExtraDataAddress,
         CustomPlcInteractAddress.ExtraDataAddress
      );
   }

   public void ConfirmCMD()
   {
      if (CustomPlcInteractAddress.Id == 0)
      {
         int oldId = 0;
         if (_plcSignalConfig.CustomPlcInteractAddresses.Count > 0)
            oldId = _plcSignalConfig.CustomPlcInteractAddresses.Select(x => x.Id).Max();
         CustomPlcInteractAddress.Id = oldId + 1;
         _plcSignalConfig.CustomPlcInteractAddresses.Add(CustomPlcInteractAddress);
         _plcSignalConfig.Save(_userStatus.LocalLoggedinUser.Name, $"新加自定义信号[{JsonSerializer.Serialize(CustomPlcInteractAddress)}]");
      }
      else
      {
         var oldCustom = _plcSignalConfig.CustomPlcInteractAddresses.FirstOrDefault(x => x.Id == CustomPlcInteractAddress.Id);
         if (oldCustom != null)
         {
            ExpressionAssignmentMapper<CustomPlcInteractAddressModel, CustomPlcInteractAddressModel>.Trans(
               CustomPlcInteractAddress,
               oldCustom
            );
            ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
               CustomPlcInteractAddress.DataAddress,
               oldCustom.DataAddress
            );
            ExpressionAssignmentMapper<SignalAddressModel, SignalAddressModel>.Trans(
               CustomPlcInteractAddress.ExtraDataAddress,
               oldCustom.ExtraDataAddress
            );
         }
         else
         {
            _plcSignalConfig.CustomPlcInteractAddresses.Add(CustomPlcInteractAddress);
         }
         _plcSignalConfig.Save(_userStatus.LocalLoggedinUser.Name, $"修改自定义信号[{JsonSerializer.Serialize(CustomPlcInteractAddress)}]");
      }
      this.RequestClose();
   }
}
