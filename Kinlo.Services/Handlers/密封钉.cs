namespace Kinlo.Services.Handlers;

/// <summary>
///
/// </summary>
[DeviceConnec(ProcessTypeEnum.打钉检测, [CommunicationEnum.None])] //指定工艺，可指定多个
[DeviceConnec(ProcessTypeEnum.回流打钉检测, [CommunicationEnum.None])] //指定工艺，可指定多个
[DeviceConnec(ProcessTypeEnum.全压钉, [CommunicationEnum.None])] //指定工艺，可指定多个
[DeviceConnec(ProcessTypeEnum.预压钉, [CommunicationEnum.None])] //指定工艺，可指定多个
[DeviceConnec(ProcessTypeEnum.拔钉前检测, [CommunicationEnum.None])] //指定工艺，可指定多个
public class NialHandler : ServiceHandlerBase
{
   public NialHandler(IContainer container, IDevice plc, PLCInteractAddressModel plcInteractAddress, CancellationTokenSource taskToken)
      : base(container, plc, plcInteractAddress, taskToken) { }

   protected override async Task HandleCore(short plcValue)
   {
      if (!TryReadPlcData(out List<ReceivePlcDataModel> plcDatas))
         return;
      await Parallel.ForAsync(
         0,
         plcDatas.Count,
         new ParallelOptions { MaxDegreeOfParallelism = plcDatas.Count },
         async (dataIndex, _) =>
         // await Parallel.ForEachAsync(plcDatas, new ParallelOptions { MaxDegreeOfParallelism = plcDatas.Count }, async (plcData, _) =>
         {
            var plcData = plcDatas[dataIndex];
            string logHeader = Context.ToProcessLogHeader(plcData.Index, plcData.ID);
            var mainBattery = await _batteryCache.GetByIdAsync(plcData.ID, logHeader); //取缓存

            if (mainBattery == null)
            {
               plcData.DataAddress.WritePlcResult(ResultTypeEnum.数据库找不到电池, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
               return;
            }
            logHeader = Context.ToProcessLogHeader(plcData.Index, plcData.ID, mainBattery.Barcode);
            ResultTypeEnum result = ResultTypeEnum.OK;

            if (Context.ProcessesType == ProcessTypeEnum.拔钉前检测)
            {
               var nail = (IBatNailBeforePullingModel)mainBattery;
               nail.BeforePullingNailTime = DateTime.Now;
               nail.BeforePullingNailHeight = (float)Math.Round(plcData.PLCData, 2);
               nail.NailHeightDifference = nail.BeforePullingNailHeight - nail.PreProcessNailHeight;
               result = nail.BeforePullingNailResult = plcData.PLCDataType == 1 ? ResultTypeEnum.OK : ResultTypeEnum.拔钉前检测NG;
               //胶钉高度差检测
               if (
                  result == ResultTypeEnum.OK
                  && _parameterConfig.FunctionEnable.IsCheckNailHeightDifference
                  && nail.NailHeightDifference > _parameterConfig.RunParameter.NailHeightDifferenceUpper
               )
               {
                  result = nail.BeforePullingNailResult = ResultTypeEnum.胶钉高度差NG;
               }
            }
            else if (Context.ProcessesType == ProcessTypeEnum.全压钉)
            {
               var nail = (IBatNailAfterModel)mainBattery;
               nail.AfterNailTime = DateTime.Now;
               nail.AfterNailHeight = (float)Math.Round(plcData.PLCData, 2);
               result = nail.AfterNailResult = plcData.PLCDataType == 1 ? ResultTypeEnum.OK : ResultTypeEnum.全压钉NG;
               //  GetScissorsData();
            }
            else if (Context.ProcessesType == ProcessTypeEnum.预压钉)
            {
               var nail = (IBatNailPrepressModel)mainBattery;
               nail.PrepressNailHeight = (float)Math.Round(plcData.PLCData, 2);
               nail.PrepressNailTime = DateTime.Now;
               result = nail.PrepressNailResult = plcData.PLCDataType == 1 ? ResultTypeEnum.OK : ResultTypeEnum.预压钉NG;
            }
            else if (Context.ProcessesType is ProcessTypeEnum.打钉检测 or ProcessTypeEnum.回流打钉检测)
            {
               var nail = (IBatNailModel)mainBattery;
               nail.NailHeight = (float)Math.Round(plcData.PLCData, 2);
               nail.NailTime = DateTime.Now;
               nail.NailIndex = (byte)(dataIndex + 1);

               result = nail.NailResult = plcData.PLCDataType == 1 ? ResultTypeEnum.OK : ResultTypeEnum.打钉NG;
               if (nail.NailResult != ResultTypeEnum.OK) //20260107 加入打钉NG 直接当OK上传MES
               {
                  await MesOutput(mainBattery, logHeader);
               }
               //  GetScissorsData();
            }
            result = ResultTypeEnum.OK;
            if (!await _sugarDB.UpdateByObjectAsync(mainBattery, logHeader))
            {
               if (Context.ProcessesType == ProcessTypeEnum.拔钉前检测)
               {
                  var nail = (IBatNailBeforePullingModel)mainBattery;
                  nail.BeforePullingNailTime = DateTime.Now;
                  result = nail.BeforePullingNailResult = ResultTypeEnum.保存数据库失败;
               }
               else if (Context.ProcessesType == ProcessTypeEnum.全压钉)
               {
                  var nail = (IBatNailAfterModel)mainBattery;
                  nail.AfterNailTime = DateTime.Now;
                  result = nail.AfterNailResult = ResultTypeEnum.保存数据库失败;
               }
               else if (Context.ProcessesType == ProcessTypeEnum.预压钉)
               {
                  var nail = (IBatNailPrepressModel)mainBattery;
                  nail.PrepressNailTime = DateTime.Now;
                  result = nail.PrepressNailResult = ResultTypeEnum.保存数据库失败;
               }
               else if (Context.ProcessesType is ProcessTypeEnum.打钉检测 or ProcessTypeEnum.回流打钉检测)
               {
                  var nail = (IBatNailModel)mainBattery;
                  nail.NailTime = DateTime.Now;
                  result = nail.NailResult = ResultTypeEnum.保存数据库失败;
               }
            }

            AddDisplayData(mainBattery); //更新界面显示
            plcData.DataAddress.WritePlcResult(result, ResultTypeEnum._, _plc, _parameterConfig, logHeader); //写入PLC结果
         }
      );
   }
}
