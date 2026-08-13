using HandyControl.Controls;
using Kinlo.Services.Handlers;

namespace Kinlo.GUI.ViewModel;

public class ManualTrayLoadingViewModel : Screen
{
   [Inject]
   public IContainer? Container { get; set; }

   [Inject]
   public IBatteryCache? Cache { get; set; }

   [Inject]
   public ParameterConfig? Parameter { get; set; }
   public string TrayCode { get; set; } = string.Empty;
   public string Barcodes { get; set; } = string.Empty;

   public ManualTrayLoadingViewModel() { }

   public async Task OutboundCmd()
   {
      //if (Parameter.AdvancedConfig.LogisticsMesStatus == MESStatusEnum.关闭)
      //{
      //    Growl.Warning("物流出站功能已关闭或处于测试状态，请打开再试！");
      //    return;
      //}
      //if (Parameter.AdvancedConfig.LogisticsMesStatus == MESStatusEnum.测试)
      //{
      //    var rs = HandyControl.Controls.MessageBox.Show($"物流出站功能处于测试状态!是否继续出站？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
      //    if (rs != MessageBoxResult.Yes)
      //    {
      //        return; // 用户选择不继续
      //    }
      //}

      //List<string> lines = new List<string>();

      //using (StringReader reader = new StringReader(Barcodes))
      //{
      //    string? line;
      //    while ((line = reader.ReadLine()) != null)
      //    {
      //        if (!string.IsNullOrWhiteSpace(line))  // 跳过空行和只含空白的行
      //        {
      //            lines.Add(line.Trim());
      //        }
      //    }
      //}

      //if (lines.Count == 0)
      //{
      //    Growl.Warning("请至少输入一个条码！");
      //    return;
      //}

      //List<string> notFoundBarcodes = new List<string>();//未找到数据的条码
      //List<IBatMainModel> mainBatteries = new List<IBatMainModel>();
      //for (int i = 0; i < lines.Count; i++)
      //{
      //    string logHeader = $"[手动出站-{lines[i]}]";
      //    var mainBattery = await Cache.GetByBarcodeAsync(lines[i], logHeader); // 等待异步获取电池数据
      //    if (mainBattery != null)
      //    {
      //        //mainBattery.ExitCrateNumber = TrayCode;
      //        //mainBattery.ExitCrateSlot = i + 1;
      //        mainBattery.MesExitTime = DateTime.Now;
      //        mainBatteries.Add(mainBattery);
      //    }
      //    else
      //    {
      //        notFoundBarcodes.Add(lines[i]);
      //    }
      //}
      //if (notFoundBarcodes.Count > 0)
      //{
      //    var rs = HandyControl.Controls.MessageBox.Show($"以下条码未找到数据：{string.Join(", ", notFoundBarcodes)}\r\n是否继续出站？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
      //    if (rs != MessageBoxResult.Yes)
      //    {
      //        return; // 用户选择不继续
      //    }
      //}
      //var outboundRss = await LoadTraySendWmsHandler.SendWms(mainBatteries, TrayCode, Parameter, Container.Get<IMesService>(), Container.Get<SugarDbHelper>(), $"[手动出站]", MESStatusEnum.启用);
      //if (outboundRss == ResultTypeEnum.OK)
      //{
      //    Growl.Success("物流出站成功");
      //}
      //else
      //{
      //    Growl.Warning("物流出站失败");
      //}
   }
}
