using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Kinlo.Equipment.Interfaces;
using Kinlo.GUI.DeviceTest.tool;
using Kinlo.SharedBase.Enums;

namespace Kinlo.GUI.DeviceTest.Commands
{
   [DeviceCommand([ProcessTypeEnum.测短路])]
   internal class VoltageCommand
   {
      [DeviceAction("读取电压", ActionType.Read, group: "读取", icon: "⚡", style: ActionStyle.Info, order: 0)]
      public async Task<DeviceActionResult> ReadVoltageAsync(DeviceClientModel device, ParameterConfig config)
      {
         // 读取逻辑
         return DeviceActionResult.Ok(
            "2026-04-03 16:55:35 195 [信息] [外观检测机]-[绝缘测试仪]-[AICEKEJI_VoltogeTest_AC1200]-[1号]：获取结果原始值：5A-A5-98-01-03-62-5C-00-01-00-04-00-00-00-00-00-00-00-02-02-00-98-96-80-00-01-9B-2C-00-98-96-80-00-01-E8-48-00-03-D0-90-00-01-4C-70-00-06-C8-1C-00-06-1A-80-01-00-00-00-02-02-02-02-02-02-02-02-02-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-02-00-00-04-02-02-02-02-02-02-02-02-02-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-1A-02-0A-02-10-33-08-FF-0D-0A\r\n2026-04-03 16:55:35 195 [成功] [取短路测试数据]校验和成功，设备报文：5A-A5-98-01-03-62-6C-00-01-05-DC-00-00-00-00-00-00-00-02-02-00-98-96-80-02-8D-F0-34-00-98-96-80-00-3D-2E-1C-0C-C5-BC-BC-01-46-F5-2C-0C-C5-BC-BC-0C-C5-BC-BC-01-00-00-00-02-02-02-02-02-02-02-02-02-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-02-00-00-00-02-02-02-02-02-02-02-02-02-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-1A-02-0A-02-10-34-06-C1-0D-0A\r\n2026-04-03 16:55:35 195 [信息] [外观检测机]-[绝缘测试仪]-[AICEKEJI_VoltogeTest_AC1200]-[2号]：获取结果原始值：5A-A5-98-01-03-62-6C-00-01-05-DC-00-00-00-00-00-00-00-02-02-00-98-96-80-02-8D-F0-34-00-98-96-80-00-3D-2E-1C-0C-C5-BC-BC-01-46-F5-2C-0C-C5-BC-BC-0C-C5-BC-BC-01-00-00-00-02-02-02-02-02-02-02-02-02-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-02-00-00-00-02-02-02-02-02-02-02-02-02-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-1A-02-0A-02-10-34-06-C1-0D-0A"
         );
      }

      [DeviceAction("读取电阻", ActionType.Read, group: "读取", icon: "🔬", style: ActionStyle.Info, order: 1)]
      public async Task<DeviceActionResult> ReadResistanceAsync(DeviceClientModel device, ParameterConfig config)
      {
         return DeviceActionResult.Ok("100 Ω \r\n 100");
      }

      [DeviceAction(
         "写入电压设定值",
         ActionType.Write,
         group: "写入",
         icon: "✏️",
         style: ActionStyle.Warning,
         placeholder: "输入电压值 (V)",
         order: 0
      )]
      public async Task<DeviceActionResult> WriteVoltageAsync(DeviceClientModel device, ParameterConfig config, string value)
      {
         // 写入逻辑，value 是用户输入的值
         return DeviceActionResult.Ok($"已写入：{value} V");
      }

      [DeviceAction(
         "写入电阻设定值",
         ActionType.Write,
         group: "写入",
         icon: "✏️",
         style: ActionStyle.Warning,
         placeholder: "输入电阻值",
         order: 0
      )]
      public async Task<DeviceActionResult> WriteIAsync(DeviceClientModel device, ParameterConfig config, string value)
      {
         // 写入逻辑，value 是用户输入的值
         return DeviceActionResult.Ok($"已写入：{value} V");
      }
   }

   //[DeviceCommand([ProcessTypeEnum.测短路, ProcessTypeEnum.短路测试RJ692R])]
   //internal class ShortCircuitDeviceCommand : IDeviceCommand
   //{
   //    public async Task<string> ReadAsync(DeviceClientModel device, ParameterConfig parameterConfig)
   //    {
   //        if(device.ProcessesType == ProcessTypeEnum.测短路)
   //        {
   //            AC5100ResultModel? result = null;

   //            await device.WithCreatedDeviceAsync(d =>
   //                Task.Run(() => result = d.ReadClass<AC5100ResultModel>(null))
   //            );

   //            return result != null ? $"{result.总结果}{result.TestMsg}" : "null";
   //        }
   //        else if (device.ProcessesType == ProcessTypeEnum.短路测试RJ692R)
   //        {
   //            RJ6902RResultModel? rj6902RResult = null;

   //            await device.WithCreatedDeviceAsync(d =>
   //                Task.Run(() => rj6902RResult = d.ReadClass<RJ6902RResultModel>(null))
   //            );

   //            var json = JsonSerializer.Serialize(
   //                rj6902RResult,
   //                new JsonSerializerOptions
   //                {
   //                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
   //                });

   //            return rj6902RResult != null ? $"结果：{rj6902RResult.ConverterRJ6902R()}  数据 {json}" : "null";

   //        }

   //        return "";

   //    }

   //    public Task<bool> WriteAsync(DeviceClientModel device, ParameterConfig parameterConfig, string value)
   //    {
   //        throw new NotSupportedException("测短路设备不支持写入操作");
   //    }
   //}
}
