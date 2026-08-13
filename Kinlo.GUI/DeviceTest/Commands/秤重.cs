using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinlo.GUI.DeviceTest.tool;

namespace Kinlo.GUI.DeviceTest.Commands
{
   [DeviceCommand([ProcessTypeEnum.前称重, ProcessTypeEnum.后称重, ProcessTypeEnum.手动补液])]
   internal class ScaleDeviceCommand
   {
      [DeviceAction("读取重量", ActionType.Read, group: "操作", icon: "⚡", style: ActionStyle.Info, order: 0)]
      public async Task<DeviceActionResult> ReadAsync(
         DeviceClientModel device,
         ParameterConfig parameterConfig,
         ProcessTypeEnum processType
      )
      {
         try
         {
            float value = -9999;
            await device.WithCreatedDeviceAsync(d =>
               Task.Run(() =>
               {
                  value = d.ReadValue<float>(null, "称重").Value;
               })
            );

            return value == -9999 ? DeviceActionResult.Fail("读取失败，设备无响应") : DeviceActionResult.Ok($"{value}");
         }
         catch (Exception e)
         {
            return DeviceActionResult.Fail($"读取异常：{e.Message}");
         }
      }

      [DeviceAction("清零", ActionType.Action, group: "操作", icon: "🗑️", style: ActionStyle.Danger, order: 1)]
      public async Task<DeviceActionResult> WriteAsync(DeviceClientModel device, ParameterConfig parameterConfig, string value)
      {
         bool success = false;

         await device.WithCreatedDeviceAsync(d =>
            Task.Run(() =>
            {
               // 尝试转换要写入的值
               object? valToWrite = null;
               if (float.TryParse(value, out float f))
                  valToWrite = f;

               success = d.WriteValue(valToWrite, null, "称重清零"); // 写入设备，并把结果存到外部变量
            })
         );

         return success ? DeviceActionResult.Ok($"清零成功") : DeviceActionResult.Fail("清零失败");
      }
   }
}
