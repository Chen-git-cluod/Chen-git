using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

internal class RfidViewModel : Screen
{
   public string? ReadCode { get; set; } = string.Empty;
   public string WriteCode { get; set; } = string.Empty;
   public DeviceClientModel Device { get; set; }

   public RfidViewModel(IContainer container, DeviceClientModel device)
   {
      Device = device;
   }

   public async Task ReadCmd()
   {
      try
      {
         await Device.WithCreatedDeviceAsync(async d =>
            await Task.Run(() =>
            {
               var deviceResult = d.ReadValue<string>(null, "");
               UIThreadHelper.InvokeOnUiThreadAsync(() => ReadCode = deviceResult.Status == DeviceStatus.Success ? deviceResult.Value : "");
            })
         );
      }
      catch (Exception ex)
      {
         Growl.Warning(ex.Message);
      }
   }

   public async Task WriteCmd()
   {
      if (WriteCode.Length != 6)
      {
         Growl.Warning("长度必须是6！");
         return;
      }

      try
      {
         await Device.WithCreatedDeviceAsync(async d =>
            await Task.Run(() =>
            {
               if (d.WriteValue(WriteCode, null, ""))
                  Growl.Success("写入成功！");
               else
                  Growl.Warning("写入失败！");
            })
         );
      }
      catch (Exception ex)
      {
         Growl.Warning(ex.Message);
      }
   }
}
