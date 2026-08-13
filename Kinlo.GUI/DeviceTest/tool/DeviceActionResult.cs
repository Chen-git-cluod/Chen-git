using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.GUI.DeviceTest.tool
{
   public class DeviceActionResult
   {
      public bool IsSuccess { get; init; }
      public string Message { get; init; } = "";

      // 成功
      public static DeviceActionResult Ok(string message = "") => new() { IsSuccess = true, Message = message };

      // 失败
      public static DeviceActionResult Fail(string message) => new() { IsSuccess = false, Message = message };
   }
}
