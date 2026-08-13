using Dm.util;

namespace Kinlo.Services.Handlers;

[DeviceConnec(ProcessTypeEnum.装盘扫码, [CommunicationEnum.ScanCode_SR1000, CommunicationEnum.ScanCode_SR700])]
[DeviceConnec(ProcessTypeEnum.拆盘扫码, [CommunicationEnum.ScanCode_SR1000, CommunicationEnum.ScanCode_SR700])]
public class ScanCodeLoadCrateHandler : ServiceHandlerBase
{
   public ScanCodeLoadCrateHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken) { }

   protected override Task HandleCore(short plcValue)
   {
      string logHeader = Context.ToProcessLogHeader(plcValue);
      var resultAddress = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[{Context.DeviceStartIndex - 1}]", 0);
      if (_parameterConfig.FunctionEnable.IsEmptyLoadMode) //空跑则不去获取设备数据
      {
         $"开启空载模式，直接合格完成;".LogProcess(_taskLogHeader, Log4NetLevelEnum.警告);
         resultAddress.WritePlcResult(ResultTypeEnum.OK, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
         return Task.CompletedTask;
      }

      var device = _devicesConfig.GetRunDevice(Context, Context.DeviceStartIndex);

      if (device == null)
      {
         _isDeviceAlarm = true;
         $"找不到设备，不给PLC发结果及完成信号，请停机检查！;".LogProcess(logHeader, Log4NetLevelEnum.错误, true);
         return Task.CompletedTask;
      }
      var scanResults = ScanBarcodeHelpre.ScanCode(
         device,
         _parameterConfig.RunParameter.RetryCountScanCode,
         Context.DataLength,
         logHeader,
         _parameterConfig.AdvancedConfig.LogisticsTrayCodeValidationRule
      );
      logHeader = Context.ToProcessLogHeader(plcValue, barcode: scanResults[0].Code);

      if (scanResults[0].ScanStatus == ResultTypeEnum.OK)
      {
         var code = scanResults[0].Code;
         var id = _snowflakeHelper.NextId(); //托盘ID

         var barceodeTag = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[0].Code", 0);
         for (int n = 1; n < 4; n++)
         {
            if (_plc.WriteValue(code, barceodeTag, logHeader))
            {
               $"第[{n}]次 地址[{JsonSerializer.Serialize(barceodeTag)}]写入托盘码[{code}] 成功".LogProcess(logHeader);
               break;
            }
            else
            {
               $"第[{n}]次 地址[{JsonSerializer.Serialize(barceodeTag)}]写入托盘码[{code}] 失败".LogProcess(logHeader);
            }
         }
      }
      resultAddress.WritePlcResult(scanResults[0].ScanStatus, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
      return Task.CompletedTask;
   }
}
