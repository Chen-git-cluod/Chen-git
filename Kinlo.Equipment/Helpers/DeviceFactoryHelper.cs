namespace Kinlo.Equipment.Helpers;

public static class DeviceFactory
{
   static Type[]? _types = null;
   static ConcurrentDictionary<CommunicationEnum, Type> Devices = new ConcurrentDictionary<CommunicationEnum, Type>();

   public static IDevice? CreateDevice(this DeviceInfoModel deviceInfo)
   {
      if (!Devices.ContainsKey(deviceInfo.Communication))
      {
         _types = _types ?? Assembly.GetExecutingAssembly().GetTypes();
         var _type = _types.FirstOrDefault(type =>
         {
            if (type.IsClass && typeof(IDevice).IsAssignableFrom(type))
            {
               var _attribute = type.GetCustomAttribute<DeviceConnecAttribute>();
               if (_attribute != null && _attribute.DeviceCommunicationTypes.Any(x => x == deviceInfo.Communication))
                  return true;
            }
            return false;
         });
         if (_type != null)
            Devices.TryAdd(deviceInfo.Communication, _type);
      }

      if (Devices.TryGetValue(deviceInfo.Communication, out Type? _typeValue))
      {
         var _attribute = _typeValue.GetCustomAttribute<DeviceConnecAttribute>()!;
         var _parameterPropertyInfos = _typeValue.GetConstructors()[^1].GetParameters();
         var _parameters = new object[_parameterPropertyInfos.Length];
         for (int i = 0; i < _parameters.Length; i++)
         {
            _parameters[i] = _parameterPropertyInfos[i].ParameterType switch
            {
               var _p when _p == typeof(DeviceInfoModel) => deviceInfo,
               _ => null!,
            };
         }
         var _device = Activator.CreateInstance(_typeValue, _parameters) as IDevice;
         return _device;
      }
      return null;
   }
}
