using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinlo.GUI.DeviceTest.Commands;

namespace Kinlo.GUI.DeviceTest.Helpers
{
   public static class DeviceCommandFactory
   {
      private static readonly Dictionary<ProcessTypeEnum, object> _processCommands = new();
      private static readonly Dictionary<CommunicationEnum, object> _commCommands = new();

      static DeviceCommandFactory()
      {
         var commandTypes = typeof(DeviceCommandFactory)
            .Assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.GetCustomAttribute<DeviceCommandAttribute>() != null);

         foreach (var type in commandTypes)
         {
            var attr = type.GetCustomAttribute<DeviceCommandAttribute>()!;
            var instance = Activator.CreateInstance(type)!;

            foreach (var processType in attr.ProcessTypes)
               _processCommands[processType] = instance;

            foreach (var comm in attr.Communications)
               _commCommands[comm] = instance;
         }
      }

      // ✅ 返回 object，不再依赖 IDeviceCommand
      public static object? GetCommand(DeviceClientModel device)
      {
         if (_processCommands.TryGetValue(device.ProcessesType, out var cmd))
            return cmd;
         if (_commCommands.TryGetValue(device.Communication, out cmd))
            return cmd;
         return null;
      }
   }

   //internal static class DeviceCommandFactory
   //{
   //    private static readonly Dictionary<ProcessTypeEnum, IDeviceCommand> _processCommands = new();
   //    private static readonly Dictionary<CommunicationEnum, IDeviceCommand> _commCommands = new();

   //    static DeviceCommandFactory()
   //    {
   //        // 反射扫描程序集，找到所有实现 IDeviceCommand 的类
   //        var commandTypes = typeof(DeviceCommandFactory).Assembly.GetTypes()
   //            .Where(t => typeof(IDeviceCommand).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

   //        foreach (var type in commandTypes)
   //        {
   //            var attr = type.GetCustomAttribute<DeviceCommandAttribute>();
   //            if (attr == null)
   //                continue;

   //            // 创建实例
   //            var instance = (IDeviceCommand)Activator.CreateInstance(type)!;

   //            // 注册所有 ProcessType
   //            foreach (var processType in attr.ProcessTypes)
   //            {
   //                _processCommands[processType] = instance;
   //            }

   //            // 注册所有 Communication
   //            foreach (var comm in attr.Communications)
   //            {
   //                _commCommands[comm] = instance;
   //            }
   //        }
   //    }

   //    public static IDeviceCommand? GetCommand(DeviceClientModel device)
   //    {
   //        // 优先 ProcessType 匹配
   //        if (_processCommands.TryGetValue(device.ProcessesType, out var cmd))
   //            return cmd;

   //        // 再匹配 Communication
   //        if (_commCommands.TryGetValue(device.Communication, out cmd))
   //            return cmd;

   //        return null;
   //    }
   //}

   //internal static class DeviceCommandFactory
   //{
   //    private static readonly Dictionary<ProcessTypeEnum, IDeviceCommand> _commands = new()
   //    {
   //        { ProcessTypeEnum.前秤重, new ScaleDeviceCommand() },
   //        { ProcessTypeEnum.后秤重, new ScaleDeviceCommand() },
   //        { ProcessTypeEnum.补液秤, new ScaleDeviceCommand() },
   //        { ProcessTypeEnum.测短路, new ShortCircuitDeviceCommand() },
   //        { ProcessTypeEnum.补液量发送, new LiquidSendDeviceCommand() },
   //        { ProcessTypeEnum.注液量发送, new LiquidSendDeviceCommand() },
   //        { ProcessTypeEnum.测电压电阻, new VoltageResistanceDeviceCommand() },
   //        { ProcessTypeEnum.测电压电阻多通道, new VoltageResistanceMultiChannelCommand() },
   //        { ProcessTypeEnum.打胶塞前扫码, new ScanCodeDeviceCommand() },
   //        { ProcessTypeEnum.前扫码, new ScanCodeDeviceCommand() },
   //        { ProcessTypeEnum.托盘扫码, new ScanCodeDeviceCommand() },
   //        { ProcessTypeEnum.拆盘扫码, new ScanCodeDeviceCommand() },
   //        { ProcessTypeEnum.组盘扫码, new ScanCodeDeviceCommand() },
   //        { ProcessTypeEnum.后扫码, new ScanCodeDeviceCommand() },
   //        { ProcessTypeEnum.下料扫码, new ScanCodeDeviceCommand() },
   //        { ProcessTypeEnum.清洗托盘扫码, new ScanCodeDeviceCommand() },
   //        { ProcessTypeEnum.套膜前扫码, new ScanCodeDeviceCommand() },
   //        { ProcessTypeEnum.绝缘测试仪, new InsulationTestDeviceCommand() },
   //    };

   //    public static IDeviceCommand GetCommand(DeviceClientModel device)
   //    {
   //        if (_commands.TryGetValue(device.ProcessesType, out var command))
   //            return command;

   //        if (device.Communication == CommunicationEnum.DTK温控表 || device.Communication == CommunicationEnum.PH160S)
   //            return new TemperatureDeviceCommand();

   //        return null;
   //    }
   //}
}
