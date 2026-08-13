using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dm.util;
using Kinlo.GUI.DeviceTest.tool;

namespace Kinlo.GUI.DeviceTest.Commands
{
   [DeviceCommand([CommunicationEnum.RFID_RD900M])]
   internal class RFID_RD900MCommand
   {
      [DeviceAction("读取套杯托盘码", ActionType.Read, group: "读取", icon: "⚡", style: ActionStyle.Info, order: 0)]
      public async Task<DeviceActionResult> ReadAsync(DeviceClientModel device, ParameterConfig parameterConfig)
      {
         string result = "";

         await device.WithCreatedDeviceAsync(d =>
            Task.Run(() =>
            {
               var rd900MAddress = new SignalAddressModel { Address = 1 };

               var val = d.ReadValue<string>(rd900MAddress, "读取套杯托盘码").Value;
               result = val ?? "";
            })
         );

         return (result == "") ? DeviceActionResult.Fail("读取失败s") : DeviceActionResult.Ok($"{result}");
      }

      public async Task<bool> WriteAsync(DeviceClientModel device, ParameterConfig parameterConfig, string value)
      {
         return true;
      }
   }
}
