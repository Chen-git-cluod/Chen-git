using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.GUI.DeviceTest.Commands
{
   [DeviceCommand([ProcessTypeEnum.注液量发送, ProcessTypeEnum.补液量发送])]
   internal class LiquidSendDeviceCommand
   {
      //int address = 3502;
      Dictionary<int, int> datas = new Dictionary<int, int>
      {
         { 1, 3502 },
         { 2, 3504 },
         { 3, 3506 },
         { 4, 3508 },
         { 5, 3510 },
         { 6, 3512 },
         { 7, 3514 },
         { 8, 3516 },
         { 9, 3518 },
         { 10, 3520 },
      };

      [DeviceAction("读取注液量", ActionType.Read, group: "读取", icon: "⚡", style: ActionStyle.Info, order: 0)]
      public async Task<string> ReadAsync(DeviceClientModel device, ParameterConfig parameterConfig)
      {
         int address = datas[device.Index];

         float value = 0;

         await device.WithCreatedDeviceAsync(d =>
            Task.Run(() => value = d.ReadValue<float>(new SignalAddressModel { Address = address }, "注液量发送").Value)
         );

         return value.ToString();
      }

      [DeviceAction(
         "写入注液量",
         ActionType.Write,
         group: "写入",
         icon: "✏️",
         style: ActionStyle.Warning,
         placeholder: "输入注液量",
         order: 0
      )]
      public async Task<bool> WriteAsync(DeviceClientModel device, ParameterConfig parameterConfig, string value)
      {
         int address = datas[device.Index];
         bool success = false;

         await device.WithCreatedDeviceAsync(d =>
            Task.Run(() =>
            {
               if (float.TryParse(value, out float sendVal))
                  success = d.WriteValue(sendVal, new SignalAddressModel(address), "注液量发送");
            })
         );

         return success;
      }
   }
}
