using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

public class WorkOrderViewModel : Screen
{
   public string Barcode { get; set; } = string.Empty;
   public ObservableCollection<WorkOrderDto> WorkOrders { get; set; } = new ObservableCollection<WorkOrderDto>();

   public ObservableCollection<string> WorkOrdersCode { get; set; } = new ObservableCollection<string>();

   public bool IsSendFinish { get; set; } = true;
   ParameterConfig _parameterConfig;
   MesService _mesService;
   UsersStatusConfig _usersStatus;
   IContainer _container;

   public WorkOrderViewModel(StyletIoC.IContainer container)
   {
      _container = container;
      _parameterConfig = container.Get<ParameterConfig>();
      _mesService = container.Get<MesService>();
      _usersStatus = container.Get<UsersStatusConfig>();
   }

   public async Task GetCmd()
   {
      WorkOrdersCode.Clear();
      IsSendFinish = false;
      var mesParam = new MesRequestBuildNJGX.ArgsWorkOrder(Barcode);
      var call = _container.Get<MesInterfaceParameterConfig>().GetApiCall(mesParam);
      if (call == null || !call.IsEnable)
      {
         Growl.Warning("接口未启用或未找到相应接口!");
         return;
      }
      var mesWorkOrderResult = await _mesService.SendAsync(
         call,
         Barcode,
         receiveMes => receiveMes.MesCommonParse("手动获取工单").WorkOrderParse()
      );

      if (mesWorkOrderResult.ResultStatus == MesResultStatusEnum.成功 && mesWorkOrderResult.Data != null) //更新数据
      {
         foreach (var item in mesWorkOrderResult.Data)
         {
            WorkOrders.Add(item);
         }
      }
      IsSendFinish = true;
   }

   public async Task SelectCmd(object sender)
   {
      //WorkOrders.Clear();
      //var reRequest = _mesService.GetRequestMessage(MesInterfaceNameEnum.工单信息, [sender]);
      //var mesRevice = await _mesService.SendRequestAsync(MesInterfaceNameEnum.工单信息, reRequest, "", _parameterConfig.AdvancedConfig.MESStatus);
      //if (mesRevice.Status == HttpResultStatusEnum.成功 && mesRevice.Data.Count > 0)
      //{
      //    WorkOrders.Add(new WorkOrderDto
      //    {
      //        result = mesRevice.Data["result"].ToString(),
      //        message = mesRevice.Data["message"].ToString(),
      //        shop_order = mesRevice.Data["shop_order"].ToString(),
      //        tech_no = mesRevice.Data["tech_no"].ToString(),
      //        item_no = mesRevice.Data["item_no"].ToString(),
      //        qty_order = mesRevice.Data["qty_order"].ToString(),
      //        qty_over = mesRevice.Data["qty_over"].ToString(),
      //        qty_sfc = mesRevice.Data["qty_sfc"].ToString(),
      //        qty_left = mesRevice.Data["qty_left"].ToString(),
      //        standard = mesRevice.Data["standard"].ToString(),
      //        qty_cell = mesRevice.Data["qty_cell"].ToString(),
      //    });
      //}

      //var vm = _container.Get<ConfigurationParameterViewModel>();
      //string msg = $"[{nameof(ParameterConfig.DeviceParameter.WorkOrderNo)}] {_parameterConfig.DeviceParameter.WorkOrderNo} ==> {WorkOrders[0].shop_order}；";
      //_parameterConfig.DeviceParameter.WorkOrderNo = WorkOrders[0].shop_order;

      //if (vm.ParameterCopy != null)
      //    vm.ParameterCopy.DeviceParameter.WorkOrderNo = WorkOrders[0].shop_order;

      //_parameterConfig.Save(_usersStatus.LocalLoggedinUser.Name, msg);
      //this.RequestClose();
   }
}
