using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinlo.GUI.DeviceTest.tool;

namespace Kinlo.GUI.DeviceTest.Commands
{
   [DeviceCommand([CommunicationEnum.ScanCode_SR1000])]
   internal class ScanCodeDeviceCommand
   {
      [DeviceAction("读取条码", ActionType.Read, group: "读取", icon: "⚡", style: ActionStyle.Info, order: 0)]
      public async Task<DeviceActionResult> ReadAsync(DeviceClientModel device, ParameterConfig parameterConfig)
      {
         string result = "";

         await device.WithCreatedDeviceAsync(d =>
            Task.Run(() =>
            {
               var val = d.ReadValue<string>(null, "扫码").Value;
               result = val ?? "";
            })
         );

         return (result == "") ? DeviceActionResult.Fail("扫码失败") : DeviceActionResult.Ok($"{result}");
      }

      public Task<bool> WriteAsync(DeviceClientModel device, ParameterConfig parameterConfig, string value)
      {
         throw new NotSupportedException("扫码设备不支持写入");
      }
   }
}
