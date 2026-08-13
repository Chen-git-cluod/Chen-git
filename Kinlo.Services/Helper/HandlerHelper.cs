namespace Kinlo.Services.Helper;

public static class HandlerHelper
{
   #region 获取重量
   /// <summary>
   /// 获取重量（重量对比已修改至仪器内部，不在业务层对比）
   /// </summary>
   /// <param name="device"></param>
   /// <param name="barcode"></param>
   /// <returns></returns>
   public static DeviceResult<double> GetWeiging(IDevice device, string logHeader) => device.ReadValue<double>(null, logHeader);
   #endregion

   #region  写注液量至注液泵
   /// <summary>
   /// 写注液量至注液泵
   /// </summary>
   /// <param name="value"></param>
   /// <param name="address"></param>
   /// <param name="device"></param>
   /// <returns></returns>
   public static bool SendInj(this float value, SignalAddressModel address, IDevice device, string logHeader)
   {
      for (int i = 0; i < 3; i++)
      {
         switch (device.DeviceInfo.Communication)
         {
            case CommunicationEnum.FinsTcp:
            case CommunicationEnum.FinsUdpLongConn:
            case CommunicationEnum.FinsUdpShortConn:
               if (device.DeviceInfo.HardwareType == HardwareTypeEnum.飞升_FSH_CF系列四通道恒流泵)
               {
                  float senVal = (float)value;
                  if (device.WriteValue(senVal, address, logHeader))
                  {
                     var reviceVal = device.ReadValue<float>(address, logHeader);
                     return senVal == reviceVal.Value;
                  }
               }
               else
               {
                  var sendVal = (ushort)(value * 100);
                  if (device.WriteValue(sendVal, address, logHeader))
                  {
                     var reviceVal = device.ReadValue<ushort>(address, logHeader);
                     return sendVal == reviceVal.Value;
                  }
               }
               break;
            case CommunicationEnum.Modbus_TCP_DCBA:
               float snedVal = (float)value;
               if (device.WriteValue(snedVal, address, logHeader))
               {
                  var reviceVal = device.ReadValue<float>(address, logHeader);
                  //  RunLog($"写入注液量为：{inj};读取泵内注液量为：{defultInf}", MessageLevelType.警告);
                  return snedVal == reviceVal.Value;
               }
               break;
         }
         Thread.Sleep(200);
      }
      return false;
   }
   #endregion


   /// <summary>
   /// 转换锐捷6902CAGX总结果
   /// </summary>
   /// <returns></returns>
   public static ResultTypeEnum ConverterRJ6902CAGXRS(this RJ6902CAGXResultModel rj6902CAGXResult)
   {
      if (rj6902CAGXResult.总结果 == 0xFF) //不合格
      {
         return rj6902CAGXResult switch
         {
            var r when r.开路结果 == 255 => ResultTypeEnum.E01_开路1,
            var r when r.放电1结果 != 1 => ResultTypeEnum.E02_严重短路,
            var r when r.放电2结果 != 1 => ResultTypeEnum.E03_电压欠压,
            var r when r.VP结果 == 255 => ResultTypeEnum.E04_电压过压,
            var r when r.跌落1结果 != 1 => ResultTypeEnum.E05_升压阶段Vd1超限,
            var r when r.跌落2结果 != 1 => ResultTypeEnum.E06_电压保持Vd2超限,
            var r when r.跌落3结果 != 1 => ResultTypeEnum.E11_自由放电Vd3超限,
            var r when r.TL结果 != 1 => ResultTypeEnum.E07_TLNg_Tp超下限,
            var r when r.TH结果 != 1 => ResultTypeEnum.E08_THNg_Tp超上限,
            var r when r.电阻测试结果 == 255 => ResultTypeEnum.E09_电阻超下限,
            var r when r.电容测试结果 == 255 => ResultTypeEnum.E10_电容Ng,
            _ => ResultTypeEnum.NG,
         };
      }
      else
      {
         return ResultTypeEnum.OK;
      }
   }

   /// <summary>
   /// 转换锐捷6902CAGX总结果
   /// </summary>
   /// <returns></returns>
   public static ResultTypeEnum ConverterRJ6902R(this RJ6902RResultModel rj6902RResult)
   {
      if (rj6902RResult.总结果 == 0xFF) //不合格
      {
         return rj6902RResult switch
         {
            var r when r.开路结果 == 255 => ResultTypeEnum.E01_开路1,
            var r when r.严重短路结果 != 1 => ResultTypeEnum.E02_严重短路,
            var r when r.欠压结果 != 1 => ResultTypeEnum.E03_电压欠压,
            var r when r.过压结果 == 255 => ResultTypeEnum.E04_电压过压,
            var r when r.跌落1结果 != 1 => ResultTypeEnum.E05_升压阶段Vd1超限,
            var r when r.跌落2结果 != 1 => ResultTypeEnum.E06_电压保持Vd2超限,
            var r when r.TL结果 != 1 => ResultTypeEnum.E07_TLNg_Tp超下限,
            var r when r.TH结果 != 1 => ResultTypeEnum.E08_THNg_Tp超上限,
            var r when r.电阻测试结果 == 255 => ResultTypeEnum.E09_电阻超下限,
            _ => ResultTypeEnum.NG,
         };
      }
      else
      {
         return ResultTypeEnum.OK;
      }
   }

   #region PLC写结果
   /// <summary>
   /// 结果写入PLC
   /// </summary>
   /// <param name="address"></param>
   /// <param name="productResult">生产过程中的结果</param>
   /// <param name="mesResult">MES的结果</param>
   /// <param name="plc"></param>
   /// <param name="parameterConfig"></param>
   /// <param name="logHeader"></param>
   public static void WritePlcResult(
      this SignalAddressModel address,
      ResultTypeEnum productResult,
      ResultTypeEnum mesResult,
      IPLC plc,
      ParameterConfig parameterConfig,
      string logHeader
   )
   {
      short sendValue = GetSendPlcResultValue(productResult, mesResult, parameterConfig, logHeader);
      var resultAddress = new SignalAddressModel($"{address.Lable}.PCResult", address.Address);
      for (int i = 1; i < 4; i++)
      {
         if (plc.WriteValue(sendValue, resultAddress, logHeader))
         {
            $"结果[{sendValue}]第[{i}]次写入[{JsonSerializer.Serialize(resultAddress)}]成功".LogProcess(logHeader);
            return;
         }
         else
         {
            $"结果[{sendValue}]第[{i}]次写入[{JsonSerializer.Serialize(resultAddress)}]失败".LogProcess(logHeader, Log4NetLevelEnum.错误);
         }
      }
   }

   /// <summary>
   /// 生产结果和MES结果转换为PLC结果值
   /// </summary>
   /// <param name="productResult"></param>
   /// <param name="mesResult"></param>
   /// <param name="parameterConfig"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   static short GetSendPlcResultValue(
      ResultTypeEnum productResult,
      ResultTypeEnum mesResult,
      ParameterConfig parameterConfig,
      string logHeader
   )
   {
      if ((int)productResult >= 21) //如果生产结果为NG，优先使用生产结果
      {
         return productResult.ToPlcResultValue(parameterConfig, logHeader);
      }

      if ((int)mesResult >= 21 && parameterConfig.FunctionEnable.IsIgnoreMesNg) //MES 结果为NG并开启忽略MES NG
      {
         $"注意：MES结果为 [{mesResult}]，但开启了MES失败不排出，将忽略MES结果；".LogProcess(logHeader);
         return productResult.ToPlcResultValue(parameterConfig, logHeader);
      }

      return mesResult.ToPlcResultValue(parameterConfig, logHeader);
   }

   /// <summary>
   /// 结果转换为PLC结果值
   /// </summary>
   /// <param name="result"></param>
   /// <param name="parameterConfig"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   static short ToPlcResultValue(this ResultTypeEnum result, ParameterConfig parameterConfig, string logHeader)
   {
      if ((int)result < 21)
         return 1;

      if (parameterConfig.FunctionEnable.IsForceOK) //强制PLC OK信号
      {
         $"由于开启PLC强制OK,结果[{result}]直接发送PLC合格!".LogProcess(logHeader, Log4NetLevelEnum.警告);
         return 1;
      }
      return result switch
      {
         ResultTypeEnum.MES判定NG or ResultTypeEnum.MES异常 => 3,
         ResultTypeEnum.注液量偏多 => 4,
         ResultTypeEnum.注液量偏少 => 5,
         ResultTypeEnum.PLC和PC注液量不对应 => 6,
         ResultTypeEnum.条码重复 => 7,
         ResultTypeEnum.少液回流次数超上限 => 8,
         ResultTypeEnum.测漏回流次数超上限 => 9,
         ResultTypeEnum.E01_开路 or ResultTypeEnum.E01_开路1 => 10,
         ResultTypeEnum.称重异常 => 97,
         ResultTypeEnum.未取到进料框槽位信息 or ResultTypeEnum.进料框槽位不对应 or ResultTypeEnum.上位机处理失败 => 99,

         _ => 2,
      };
   }
   #endregion
}
