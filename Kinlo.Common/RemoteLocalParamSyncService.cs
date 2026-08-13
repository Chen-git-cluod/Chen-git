using System.Windows.Shapes;
using Dm.util;
using Kinlo.Common.Tools.ExpressionHelpers;
using NPOI.SS.UserModel;

namespace Kinlo.Common;

/// <summary>
/// 远程（MES）与本地参数同步
/// </summary>
public class RemoteLocalParamSyncService : ConfigurationBase
{
   /// <summary>
   /// 远程（MES）与PLC需同步参数列表
   /// </summary>
   public List<ParamChangNotifyModel> RemotePlcSyncList { get; set; } = new();

   //public List<SyncParameterDto> RemotePlcSyncList { get; set; } = new();

   /// <summary>
   /// 远程（MES）与PC需同步参数列表
   /// </summary>
   public List<ParamChangNotifyModel> RemotePcSyncList { get; set; } = new();

   /// <summary>
   /// 远程（MES）与仪器需同步参数列表
   /// </summary>
   public List<ParamChangNotifyModel> RemoteInstrumentSyncList { get; set; } = new();

   [JsonIgnore]
   private Lazy<DevicesConfig> _devicesConfig;

   [JsonIgnore]
   private Lazy<PLCSignalConfig> _plcSignalConfig;

   [JsonIgnore]
   private Lazy<ParameterConfig> _parameterConfig;
   private Lazy<RoleConfig> _poleConfig;

   public RemoteLocalParamSyncService(IContainer container, bool isStartup)
      : base(container, isStartup) { }

   public override void Load()
   {
      _devicesConfig = new Lazy<DevicesConfig>(() => _container.Get<DevicesConfig>());
      _plcSignalConfig = new Lazy<PLCSignalConfig>(() => _container.Get<PLCSignalConfig>());
      _parameterConfig = new Lazy<ParameterConfig>(() => _container.Get<ParameterConfig>());
      _poleConfig = new Lazy<RoleConfig>(() => _container.Get<RoleConfig>());
      if (_parameterConfig.Value.AdvancedConfig.ProductionType == ProductionTypeEnum.一次注液)
      {
         RemotePlcSyncList.AddRange([
            new ParamChangNotifyModel
            {
               Index = 0,
               MesCode = "YCZY_JDJC_JDJCSXZ",
               MesName = "一次注液_胶钉检测_胶钉检测上限值",
            },
            new ParamChangNotifyModel
            {
               Index = 1,
               MesCode = "YCZY_JDJC_JDJCXXZ",
               MesName = "一次注液_胶钉检测_胶钉检测下限值",
            },
         ]);
         RemotePcSyncList.AddRange([
            new ParamChangNotifyModel
            {
               MesCode = "YCZY_ZYJ_ZYL_SX",
               MesName = "一次注液_注液机_注液量_上限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.InjectionUpper),
            },
            new ParamChangNotifyModel
            {
               MesCode = "YCZY_ZYJ_ZYL_XX",
               MesName = "一次注液_注液机_注液量_下限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.InjectionLower),
            },
            new ParamChangNotifyModel
            {
               MesCode = "YCZY_DZC_QCZ_SX",
               MesName = "一次注液_电子秤_前称重_上限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.IncomingWeightUpper),
            },
            new ParamChangNotifyModel
            {
               MesCode = "YCZY_DZC_QCZ_XX",
               MesName = "一次注液_电子秤_前称重_下限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.IncomingWeightLower),
            },
            new ParamChangNotifyModel
            {
               MesCode = "YCZY_DZC_HCZ_SX",
               MesName = "一次注液_电子秤_后称重_上限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.AfterWeightUpper),
            },
            new ParamChangNotifyModel
            {
               MesCode = "YCZY_DZC_HCZ_XX",
               MesName = "一次注液_电子秤_后称重_下限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.AfterWeightLower),
            },
         ]);
      }
      else
      {
         RemotePlcSyncList.AddRange([
            new ParamChangNotifyModel
            {
               Index = 0,
               MesCode = "ECZY_JDJC_JDJCSXZ",
               MesName = "二次注液_胶钉检测_胶钉检测上限值",
            },
            new ParamChangNotifyModel
            {
               Index = 1,
               MesCode = "ECZY_JDJC_JDJCXXZ",
               MesName = "二次注液_胶钉检测_胶钉检测下限值",
            },
         ]);
         RemotePcSyncList.AddRange([
            new ParamChangNotifyModel
            {
               MesCode = "ECZY_ZYJ_ZYJZBYL_SX",
               MesName = "二次注液_注液机_注液机总保液量_上限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.InjectionUpper),
            },
            new ParamChangNotifyModel
            {
               MesCode = "ECZY_ZYJ_ZYJZBYL_XX",
               MesName = "二次注液_注液机_注液机总保液量_下限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.InjectionLower),
            },
            new ParamChangNotifyModel
            {
               MesCode = "ECZY_DZC_QCZ_SX",
               MesName = "二次注液_电子秤_前称重_上限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.IncomingWeightUpper),
            },
            new ParamChangNotifyModel
            {
               MesCode = "ECZY_DZC_QCZ_XX",
               MesName = "二次注液_电子秤_前称重_下限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.IncomingWeightLower),
            },
            new ParamChangNotifyModel
            {
               MesCode = "ECZY_DZC_DCZL_SX",
               MesName = "二次注液_电子秤_电池重量_上限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.AfterWeightUpper),
            },
            new ParamChangNotifyModel
            {
               MesCode = "ECZY_DZC_DCZL_XX",
               MesName = "二次注液_电子秤_电池重量_下限",
               PtyFullName = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter.AfterWeightLower),
            },
         ]);
      }
      _ = LoadPlcParameterFromExcelAsync();
   }

   private async Task LoadPlcParameterFromExcelAsync()
   {
      //string directory = @$"{ExternalTablesDirectory}\PLC修改参数上报表";
      //await directory.UseFirstExcelFromDirectoryAsync(async w => await Task.Run(() => LoadPlcParameter(w)));
   }

   private void LoadPlcParameter(IWorkbook workbook)
   {
      //try
      //{
      //   ISheet sheet = workbook.GetSheetAt(0);
      //   for (int i = 1; i <= sheet.LastRowNum; i++)
      //   {
      //      string? code = ExcelHelper.GetCellValue(sheet.GetRow(i).GetCell(0));
      //      string? parameterName = ExcelHelper.GetCellValue(sheet.GetRow(i).GetCell(1));
      //      string? mesCode = ExcelHelper.GetCellValue(sheet.GetRow(i).GetCell(2));
      //      string? unit = ExcelHelper.GetCellValue(sheet.GetRow(i).GetCell(3));
      //      string? paramType = ExcelHelper.GetCellValue(sheet.GetRow(i).GetCell(4));

      //      if (code == null || parameterName == null || mesCode == null || unit == null)
      //      {
      //         $"第[{i + 1}]行，代码、消息或位置有空，不加载;".LogRun(Log4NetLevelEnum.警告);
      //         continue;
      //      }

      //      RemotePlcSyncList.Add(
      //         new SyncParameterDto
      //         {
      //            RowNumber = code.Trim(),
      //            Name = parameterName,
      //            MesCode = mesCode,
      //            Unit = unit,
      //            ParamType = paramType switch
      //            {
      //               null => ParamTypeEnum.标准值,
      //               var t when t == "2" || t.Contains("上下限") => ParamTypeEnum.范围上下限,
      //               var t when t == "3" || t.Contains("无需管控") => ParamTypeEnum.无需管控,
      //               _ => ParamTypeEnum.标准值,
      //            },
      //         }
      //      );
      //   }
      //}
      //catch (Exception ex)
      //{
      //   $"[导入PLC修改参数列表]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      //}
   }

   public record DownloadParamItem(string code, string name, object value);

   /// <summary>
   /// MES下发参数至设备
   /// </summary>
   /// <param name="downloadParams"></param>
   /// <returns></returns>
   public async Task<(bool status, string msg)> MesDownloadParamAsync(List<DownloadParamItem> downloadParams)
   {
      var plcParams = new List<(int index, float value)>(); //发给PLC的参数
      var pcParams = new List<(ParamChangNotifyModel info, object value)>(); //发给PC的参数
      var instrumentParams = new List<(ParamChangNotifyModel info, object value)>(); //发给仪器仪表的参数

      foreach (var param in downloadParams)
      {
         if (param.value == null)
         {
            $"名称 [{param.name}]，编码 [{param.code}] 值为空，不下发".LogRun(Log4NetLevelEnum.警告, false);
            continue;
         }
         var plcSysnc = RemotePlcSyncList.FirstOrDefault(x => x.MesCode == param.code);
         if (plcSysnc != null)
         {
            if (param.value is float f)
            {
               plcParams.Add((plcSysnc.Index, f));
            }
            else
            {
               if (float.TryParse(param.value.ToString(), out var v))
               {
                  plcParams.Add((plcSysnc.Index, v));
               }
               else
               {
                  $"名称 [{param.name}]，编码 [{param.code}]，值 [{param.value}]无法转为float，不下发".LogRun(Log4NetLevelEnum.警告, false);
               }
            }
            continue;
         }
         var pcSysnc = RemotePcSyncList.FirstOrDefault(x => x.MesCode == param.code);
         if (pcSysnc != null)
         {
            pcParams.add((pcSysnc, param.value));
            continue;
         }

         //if (RemoteInstrumentSyncList.Any(x => x.MesCode == param.code))
         //{
         //   instrumentParams.Add(param);
         //   continue;
         //}
      }

      StringBuilder stringBuilder = new StringBuilder();
      await DownParamToPcAsync(pcParams);
      var plcResult = await DownParamToPlcAsync(plcParams);
      if (!plcResult)
         stringBuilder.Append("传PLC值失败！");

      return (stringBuilder.Length > 0 ? false : true, plcResult.ToString());
   }

   #region 更新上位机配置
   /// <summary>
   /// 更新上位机配置
   /// </summary>
   /// <param name="downloadParams"></param>
   /// <returns></returns>
   private async Task DownParamToPcAsync(List<(ParamChangNotifyModel info, object value)> downloadParams)
   {
      //需锁定的本地属性
      List<string> interlockProps = new List<string>();
      string header = "[MES参数下发PC]";
      StringBuilder stringBuilder = new StringBuilder(header);
      var paramConfig = _parameterConfig.Value;
      var properties = paramConfig.RunParameter.GetType().GetProperties();
      var basePath = PathHelper.GetFullPath((ParameterConfig p) => p.RunParameter);
      bool isUpdated = false;
      foreach (var item in downloadParams)
      {
         var info = properties.FirstOrDefault(x => $"{basePath}.{x.Name}" == item.info.PtyFullName);
         if (info == null)
         {
            stringBuilder.AppendLine($"未找到 {item.info.PtyFullName}-{item.value} 本地属性！");
            continue;
         }
         else
         {
            //取路径最后名字
            int startIndex = item.info.PtyFullName.LastIndexOf('.') + 1;
            if (startIndex < item.info.PtyFullName.Length)
            {
               var name = item.info.PtyFullName[startIndex..];
               interlockProps.add(name);
            }

            Type targetType = info.PropertyType;
            Type sucType = item.value.GetType();
            if (targetType == sucType)
            {
               await UpProperty(info, item.value, paramConfig);
               stringBuilder.AppendLine($"更新{item.info.PtyFullName}-{item.info.MesName}-{item.info.MesName}-{item.value}成功！");
               isUpdated = true;
               continue;
            }

            try
            {
               // Convert.ChangeType 支持大部分基础类型转换 (string -> int, double, bool, datetime 等)
               object? convertedValue = Convert.ChangeType(item.value, targetType);
               await UpProperty(info, convertedValue, paramConfig);
               // info.SetValue(paramConfig.RunParameter, convertedValue);
               stringBuilder.AppendLine($"更新{item.info.PtyFullName}-{item.info.MesName}-{item.info.MesName}-{item.value}成功！");
               isUpdated = true;
            }
            catch (Exception ex)
            {
               // 转换失败（例如：把 "abc" 转成 int）
               stringBuilder.AppendLine($"属性 {info.Name} 转换失败: {item.value} -> {targetType.Name}");
            }
         }
      }
      if (isUpdated)
         paramConfig.Save(header, stringBuilder.ToString());
      stringBuilder.ToString().LogRun();

      //远程锁定本地属性让其不可本地修改
      if (interlockProps.Count > 0)
         _poleConfig.Value.InterlockState.Lock(interlockProps.ToArray());
   }

   private async Task UpProperty(PropertyInfo info, object value, ParameterConfig parameter)
   {
      await parameter.UpdateParameterAsync(
         (p, d, t) =>
         {
            info?.SetValue(p.RunParameter, value);
            info?.SetValue(d.RunParameter, value);
            t?.ClearChanges();
         }
      );
   }
   #endregion

   #region 更新 PLC 配置
   /// <summary>
   /// 发送 PLC 参数
   /// </summary>
   /// <param name="downloadParams1"></param>
   /// <returns></returns>
   private async Task<bool> DownParamToPlcAsync(List<(int index, float value)> downloadParams1)
   {
      string header = "[MES参数下发PLC]";
      try
      {
         if (!downloadParams1.Any())
            return true;

         var sendParas = Enumerable.Repeat(-999.0f, 30).ToList();
         foreach (var par in downloadParams1)
         {
            if (par.index < sendParas.Count)
            {
               sendParas[par.index] = par.value;
            }
         }

         var devicesConfig = _devicesConfig.Value;
         var device = devicesConfig.GetRunDevice(x => x.DeviceInfo.ProcessesType == ProcessTypeEnum.PLC);

         if (device != null)
         {
            return await DownloadPlcAsync((IPLC)device);
         }

         var client = devicesConfig.DeviceList.FirstOrDefault(x => x.ProcessesType == ProcessTypeEnum.PLC);
         if (client == null)
         {
            $"{header} 未找到设备[PLC]设备配置信息！".LogRun(Log4NetLevelEnum.错误, true);
            return false;
         }

         return await client.WithCreatedDeviceAsync(async d => await DownloadPlcAsync((IPLC)d));

         async Task<bool> DownloadPlcAsync(IPLC plc)
         {
            var address = _plcSignalConfig.Value.CustomPlcInteractAddresses.FirstOrDefault(x =>
               x.IsEnable && x.CustomInteractName == CustomInteractNameEnum.PC下发MES参数
            );
            if (address == null)
            {
               $"{header} 未找到PC下发MES参数标签！".LogRun(Log4NetLevelEnum.错误, true);
               return false;
            }

            return await Task.Run(() =>
            {
               bool res1 = true;
               StringBuilder stringBuilder = new StringBuilder(header);
               for (int i = 0; i < sendParas.Count; i++)
               {
                  var sendValue = sendParas[i];
                  // var res1 = plc.WriteLargeValues(senParas, address.DataAddress, "PC下发MES参数");
                  if (sendValue != -999.0)
                  {
                     var tempRes = res1 = plc.WriteValue(
                        sendValue,
                        new SignalAddressModel($"{address.DataAddress.Lable}[{i}]"),
                        "PC下发MES参数"
                     );
                     stringBuilder.AppendLine($"PLC索引[{i}],值[{sendValue}] 写入{(tempRes ? "成功" : "失败")}");
                     if (res1)
                        res1 = tempRes;
                  }
               }
               var res2 = plc.WriteValue((short)1, address.ExtraDataAddress, "PC下发MES参数");
               stringBuilder.AppendLine($"通知PLC{(res2 ? "成功" : "失败")}");
               stringBuilder.ToString().LogRun();
               return res2 && res1;
            });
         }
      }
      catch (Exception ex)
      {
         $"{header} 异常：{ex}".LogRun(Log4NetLevelEnum.警告);
      }
      return false;
   }
   #endregion
}
