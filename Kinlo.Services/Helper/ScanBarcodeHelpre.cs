using System.Text.RegularExpressions;

namespace Kinlo.Services.Helper;

public static class ScanBarcodeHelpre
{
   /// <summary>
   /// 扫码枪通用扫码方法
   /// </summary>
   /// <param name="device">扫码设备接口</param>
   /// <param name="retryCount"> 重试次数 </param>
   /// <param name="count">期望接收的条码个数</param>
   /// <param name="logHeader">日志前缀</param>
   /// <param name="pattern">条码正则校验规则</param>
   /// <returns>每个通道的扫码结果数组</returns>
   public static ScanBarcodeResultDto[] ScanCode(IDevice device, int retryCount, int count, string logHeader, string pattern)
   {
      $"开始扫码;".LogProcess(logHeader);
      var scanRs = device.ReadValue<string>(null!, logHeader, new DeviceOperationOptions(retryCount: retryCount));
      if (scanRs.Status != DeviceStatus.Success)
         return Enumerable.Range(0, count).Select(_ => new ScanBarcodeResultDto { ScanStatus = scanRs.Status.ToProductResult() }).ToArray();

      string deviceBarcode = scanRs.Value ?? "";
      $"扫码枪返回原始数据：{deviceBarcode}".LogProcess(logHeader);

      string[] barcodes = count > 1 ? deviceBarcode.Split(',') : [deviceBarcode];
      if (barcodes.Length < count)
      {
         $"扫码枪条码数量({barcodes.Length})与期望数量({count})不符，接收到的条码：{deviceBarcode}".LogProcess(
            logHeader,
            Log4NetLevelEnum.错误,
            true
         );
         return Enumerable
            .Range(0, count)
            .Select(_ => new ScanBarcodeResultDto { ScanStatus = ResultTypeEnum.扫码成功但个数不符 })
            .ToArray();
      }

      var results = Enumerable.Range(0, count).Select(_ => new ScanBarcodeResultDto()).ToArray();
      // 逐通道处理条码数据
      for (int k = 0; k < results.Length; k++)
      {
         results[k].Code = barcodes[k];

         //扫码失败
         if (string.IsNullOrWhiteSpace(results[k].Code) || results[k].Code == "ERROR")
            continue;

         if (string.IsNullOrWhiteSpace(pattern))
         {
            results[k].ScanStatus = ResultTypeEnum.OK;
            $"条码[{barcodes[k]}]未配置校验规则，默认校验合格;".LogProcess(logHeader);
            continue;
         }

         if (!ValidationBarcode(barcodes[k], pattern))
         {
            results[k].ScanStatus = ResultTypeEnum.扫码成功但规则验证失败;
            $"条码[{barcodes[k]}]扫码成功但正则校验失败;".LogProcess(logHeader, Log4NetLevelEnum.警告);
            continue;
         }

         results[k].ScanStatus = ResultTypeEnum.OK;
         $"条码[{barcodes[k]}]正则校验合格;".LogProcess(logHeader);
      }

      // 判断是否成功拿到这一批数据
      if (results.All(x => x.ScanStatus is ResultTypeEnum.OK))
         return results;

      // }

      $"扫码达到最大重试次数仍然失败;".LogProcess(logHeader, Log4NetLevelEnum.错误, true);

      if (results == null)
      {
         return Enumerable.Range(0, count).Select(_ => new ScanBarcodeResultDto()).ToArray();
      }

      return results;
   }

   public static bool ValidationBarcode(string barcode, string pattern) => Regex.IsMatch(barcode, pattern, RegexOptions.Compiled);
}

public class ScanBarcodeResultDto
{
   public ResultTypeEnum ScanStatus { get; set; } = ResultTypeEnum.扫码失败;
   public string Code { get; set; } = string.Empty;
}
