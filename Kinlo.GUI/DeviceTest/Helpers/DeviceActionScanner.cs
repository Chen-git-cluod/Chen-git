using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinlo.GUI.DeviceTest.Commands;
using Kinlo.GUI.DeviceTest.tool;

namespace Kinlo.GUI.DeviceTest.Helpers
{
   public static class DeviceActionScanner
   {
      public static List<DeviceActionGroup> Scan(
         object command, // ✅ 改成 object
         DeviceClientModel device,
         ParameterConfig config
      )
      {
         var methods = command
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(m => (Method: m, Attr: m.GetCustomAttribute<DeviceActionAttribute>()))
            .Where(x => x.Attr != null)
            .OrderBy(x => x.Attr!.Order)
            .ToList();

         return methods
            .GroupBy(x => x.Attr!.Group)
            .Select(g => new DeviceActionGroup
            {
               GroupName = g.Key,
               Actions = g.Select(x => BuildItem(x.Method, x.Attr!, command, device, config)).ToList(),
            })
            .ToList();
      }

      private static DeviceActionItem BuildItem(
         MethodInfo method,
         DeviceActionAttribute attr,
         object command, // ✅ 改成 object
         DeviceClientModel device,
         ParameterConfig config
      )
      {
         var item = new DeviceActionItem
         {
            Label = attr.Label,
            Group = attr.Group,
            Icon = attr.Icon,
            ActionStyle = attr.Style,
            ActionType = attr.ActionType,
            Placeholder = attr.Placeholder,
            Order = attr.Order,
         };

         item.Execute = async () =>
         {
            var result = method.Invoke(command, BuildArgs(method, device, config, null));

            if (result is Task<DeviceActionResult> tr)
            {
               var r = await tr;
               return r; // ✅ 直接返回结果对象
            }
            if (result is Task<string> ts)
               return DeviceActionResult.Ok(await ts); // 兼容旧的 string 返回
            if (result is Task t)
            {
               await t;
               return DeviceActionResult.Ok("");
            }
            return DeviceActionResult.Ok(result?.ToString() ?? "");
         };

         item.ExecuteWithInput = async input =>
         {
            var result = method.Invoke(command, BuildArgs(method, device, config, input));

            if (result is Task<DeviceActionResult> tr)
            {
               var r = await tr;
               return r;
            }
            if (result is Task<string> ts)
               return DeviceActionResult.Ok(await ts);
            if (result is Task t)
            {
               await t;
               return DeviceActionResult.Ok("");
            }
            return DeviceActionResult.Ok(result?.ToString() ?? "");
         };

         return item;
      }

      private static object?[] BuildArgs(MethodInfo method, DeviceClientModel device, ParameterConfig config, string? inputValue)
      {
         return method
            .GetParameters()
            .Select(p =>
            {
               if (p.ParameterType == typeof(DeviceClientModel))
                  return (object?)device;
               if (p.ParameterType == typeof(ParameterConfig))
                  return config;
               if (p.ParameterType == typeof(string))
                  return inputValue ?? "";
               if (p.ParameterType == typeof(ProcessTypeEnum))
                  return device.ProcessesType;
               return null;
            })
            .ToArray();
      }
   }
}
