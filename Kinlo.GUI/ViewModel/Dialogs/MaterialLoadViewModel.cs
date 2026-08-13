using Dm.util;
using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

public class MaterialLoadViewModel : Screen
{
   ParameterConfig _parameterConfig;

   public List<string> MaterialTypes { get; set; } = ["电解液", "胶钉"];
   public string MaterialType { get; set; } = "电解液";
   public string Code { get; set; } = "";

   /// <summary>
   /// 原料批次号
   /// </summary>
   public string MaterialBatchCode { get; set; } = string.Empty;

   /// <summary>
   /// 原材料编码
   /// </summary>
   public string MaterialCode { get; set; } = string.Empty;

   /// <summary>
   /// 数量
   /// </summary>
   public float Quantity { get; set; }
   public bool IsSendMES { get; set; } = false;
   public bool IsSendFinish { get; set; } = true;
   MesService mesService;
   UsersStatusConfig _usersStatus;
   MesInterfaceParameterConfig _mesInterfaceParameters;
   IContainer _container;

   public MaterialLoadViewModel(StyletIoC.IContainer container)
   {
      _container = container;
      _parameterConfig = container.Get<ParameterConfig>();
      mesService = container.Get<MesService>();
      _usersStatus = container.Get<UsersStatusConfig>();
      _mesInterfaceParameters = container.Get<MesInterfaceParameterConfig>();
   }

   /// <summary>
   /// 解析
   /// </summary>
   public void AnalysisCMD()
   {
      if (string.IsNullOrEmpty(Code))
      {
         Growl.Warning("请先扫码！");
         return;
      }
      char split = Code switch
      {
         var _ when Code.Contains("；") => '；',
         _ => ';',
      };
      var dodes = Code.Split(split);
      MaterialBatchCode = dodes[0];
   }

   /// <summary>
   /// mes_原材料上料接口
   /// </summary>
   public async Task MaterialLoadCMD()
   {
      if (string.IsNullOrEmpty(MaterialBatchCode))
      {
         Growl.Warning("请输入原材料批次号！");
         return;
      }

      try
      {
         if (!IsSendMES)
         {
            await Task.Run(async () => await UpdateAsync());
            string msg = $"未开启MES，原材料直接写入本地，编号：{MaterialCode}，批次号：{MaterialBatchCode}，数量：{Quantity}！";
            msg.LogRun(Log4NetLevelEnum.警告);
            Growl.Success(msg);
            _parameterConfig.Save(_usersStatus.LocalLoggedinUser.Name, msg);
            this.RequestClose();
            return;
         }

         IsSendFinish = false;
         var call = _mesInterfaceParameters.GetApiCall(new MesRequestBuildNJGX.ArgsMaterialIn(MaterialBatchCode, Quantity));
         if (call == null || !call.IsEnable)
         {
            Growl.Warning($"[原材料上料]接口未启用或未找到接口信息！");
            return;
         }
         var mesRevice = await mesService.SendAsync(call, MaterialBatchCode, receive => receive.MesCommonParse("手动上传原材料"));

         if (mesRevice.ResultStatus == MesResultStatusEnum.成功)
         {
            await UpdateAsync();
            string msg = $"[原材料上料] 成功，编号：{MaterialCode}，批次号：{MaterialBatchCode}，数量：{Quantity}！";
            msg.LogRun(Log4NetLevelEnum.信息);
            _parameterConfig.Save(_usersStatus.LocalLoggedinUser.Name, msg);
            Growl.Success(msg);
            this.RequestClose();
         }
         else if (mesRevice.ResultStatus == MesResultStatusEnum.生成报文失败)
         {
            Growl.Error($"[原材料上料] 生成报文失败！");
         }
         else
         {
            Growl.Error($"[原材料上料]失败，详情请查看EMS日志！");
         }
      }
      catch (Exception ex)
      {
         string msg = $"[原材料上料] 异常：{ex}！";
         msg.LogRun(Log4NetLevelEnum.错误, true);
      }
      finally
      {
         IsSendFinish = true;
      }

      async Task UpdateAsync()
      {
         await _parameterConfig.UpdateParameterAsync(
            (param, display, t) => //修改参数（包括显示页）
            {
               param.DeviceParameter.ElectrolyteCode = MaterialCode;
               param.DeviceParameter.ElectrolyteLotCode = MaterialBatchCode;
               param.DeviceParameter.ElectrolyteQuantity = Quantity;

               if (display != null)
               {
                  display.DeviceParameter.ElectrolyteCode = MaterialCode;
                  display.DeviceParameter.ElectrolyteLotCode = MaterialBatchCode;
                  display.DeviceParameter.ElectrolyteQuantity = Quantity;
               }
               t?.ClearChanges();
            }
         );
      }
   }

   /// <summary>
   /// mes_原材料卸料接口
   /// </summary>
   public async Task MaterialUnLoadCMD()
   {
      if (string.IsNullOrEmpty(MaterialBatchCode))
      {
         Growl.Warning("请输入原材料批次号！");
         return;
      }

      var mesInterfaceName = "原材料卸料";
      try
      {
         if (!IsSendMES)
         {
            await UpdateAsync();
            string msg =
               $"[{mesInterfaceName}] 未开启MES，直接写入本地，编号：{MaterialCode}，批次号：{MaterialBatchCode}，数量：{Quantity}！";
            msg.LogRun(Log4NetLevelEnum.警告, true);
            _parameterConfig.Save(_usersStatus.LocalLoggedinUser.Name, msg);
            this.RequestClose();
            return;
         }

         IsSendFinish = false;
         var call = _mesInterfaceParameters.GetApiCall(new MesRequestBuildNJGX.ArgsMaterialOut(MaterialBatchCode, Quantity));
         if (call == null || !call.IsEnable)
         {
            Growl.Warning($"[原材料卸料]接口未启用或未找到接口信息！");
            return;
         }
         var mesRevice = await mesService.SendAsync(call, MaterialBatchCode, receive => receive.MesCommonParse("手动上传原材料"));

         if (mesRevice.ResultStatus == MesResultStatusEnum.成功)
         {
            await UpdateAsync();
            string msg = $"[{mesInterfaceName}] 成功，编号：{MaterialCode}，批次号：{MaterialBatchCode}，数量：{Quantity}！";
            msg.LogRun(Log4NetLevelEnum.信息);
            _parameterConfig.Save(_usersStatus.LocalLoggedinUser.Name, msg);
            Growl.Success(msg);
            this.RequestClose();
         }
         else if (mesRevice.ResultStatus == MesResultStatusEnum.生成报文失败)
         {
            Growl.Error($"[{mesInterfaceName}] 生成报文失败！");
         }
         else
         {
            Growl.Error($"[{mesInterfaceName}]失败，详情请查看EMS日志！");
         }
      }
      catch (Exception ex)
      {
         string msg = $"[{mesInterfaceName}] 异常：{ex}！";
         msg.LogRun(Log4NetLevelEnum.错误, true);
      }
      finally
      {
         IsSendFinish = true;
      }

      async Task UpdateAsync()
      {
         var remainingQuantity = _parameterConfig.DeviceParameter.ElectrolyteQuantity - Quantity;
         if (remainingQuantity < 0)
            remainingQuantity = 0;
         await _parameterConfig.UpdateParameterAsync(
            (param, display, t) => //修改参数（包括显示页）
            {
               param.DeviceParameter.ElectrolyteQuantity = remainingQuantity;
               if (remainingQuantity <= 0)
               {
                  param.DeviceParameter.ElectrolyteCode = "";
                  param.DeviceParameter.ElectrolyteLotCode = "";
               }

               if (display != null)
               {
                  display.DeviceParameter.ElectrolyteQuantity = param.DeviceParameter.ElectrolyteQuantity;
                  display.DeviceParameter.ElectrolyteCode = param.DeviceParameter.ElectrolyteCode;
                  display.DeviceParameter.ElectrolyteLotCode = param.DeviceParameter.ElectrolyteLotCode;
               }
               t?.ClearChanges();
            }
         );
      }
   }
}
