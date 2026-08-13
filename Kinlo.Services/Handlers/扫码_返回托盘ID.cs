using MathNet.Numerics.Random;

namespace Kinlo.Services.Handlers;

[DeviceConnec(ProcessTypeEnum.返回托盘ID, [CommunicationEnum.None])]
public class SendTrayIDHandler : ServiceHandlerBase
{
   //添加出站界面显示
   Random ran = new Random();

   public SendTrayIDHandler(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
      : base(container, plc, plcInteractAddress, taskToken) { }

   protected override Task HandleCore(short plcValue)
   {
      string? _strCode = ran.Next().ToString();

      var _barceodeTag = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[{0}].Code", 0);
      for (int n = 1; n < 4; n++)
      {
         if (_plc.WriteValue(_strCode!, _barceodeTag, _taskLogHeader))
         {
            $"第[{n}]次 地址[{JsonSerializer.Serialize(_barceodeTag)}]写入{Context.ProcessesType}字符型条码[{_strCode}] 成功".LogProcess(
               _taskLogHeader
            );
            break;
         }
         else
         {
            $"第[{n}]次 地址[{JsonSerializer.Serialize(_barceodeTag)}]写入{Context.ProcessesType}字符型条码[{_strCode}] 失败".LogProcess(
               _taskLogHeader
            );
         }
      }

      long _trayId = _snowflakeHelper.NextId();
      var _idTag = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPCData[{0}].ID", 0);
      for (int n = 1; n < 4; n++)
      {
         if (_plc.WriteValue(_trayId, _idTag, _taskLogHeader))
         {
            $"第[{n}]次地址[{JsonSerializer.Serialize(_idTag)}]托盘ID[{_trayId}] 成功;".LogProcess(_taskLogHeader);
            break;
         }
         else
         {
            $"第[{n}]次地址[{JsonSerializer.Serialize(_idTag)}]托盘ID[{_trayId}] 失败;".LogProcess(_taskLogHeader);
         }
      }
      new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[0]").WritePlcResult(
         string.IsNullOrEmpty(_strCode) ? ResultTypeEnum.NG : ResultTypeEnum.OK,
         ResultTypeEnum._,
         _plc,
         _parameterConfig,
         _taskLogHeader
      );
      return Task.CompletedTask;
   }
}
