using System.Dynamic;

namespace Kinlo.MESDocking;

[Languages(["国轩MES接口"], IsScanMethod = true)]
public class MesRequestBuildNJGX
{
   protected IContainer _container;
   protected ParameterConfig _parameterConfig;
   protected MesParameterConfig _mesParameterConfig;
   protected OtherParameterConfig _otherParameterConfig;
   protected UsersStatusConfig _usersStatusConfig;

   public MesRequestBuildNJGX(IContainer container)
   {
      _container = container;
      _parameterConfig = container.Get<ParameterConfig>();
      _mesParameterConfig = container.Get<MesParameterConfig>();
      _otherParameterConfig = container.Get<OtherParameterConfig>();
      _usersStatusConfig = container.Get<UsersStatusConfig>();
   }

   #region MES登陆
   /// <summary>
   /// MES登陆参数
   /// </summary>
   /// <param name="account"></param>
   /// <param name="password"></param>
   public record ArgsMesLogin(string account, string password) : IMesArgs;

   /// <summary>
   /// MES登陆报文
   /// </summary>
   /// <param name="account"></param>
   /// <param name="password"></param>
   /// <returns></returns>
   [Languages("设备上位机账号登录接口"), MesInterfaceInfo("/api/ipccheck/account")]
   public string GetMesRequest(ArgsMesLogin args)
   {
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         deviceCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         account = args.account,
         password = MD5Encrypt.MD5Encrypt32(args.password),
      };

      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 获取一注数据

   /// <summary>
   /// 获取一注数据报文参数
   /// </summary>
   /// <param name="barcode"></param>
   public record ArgsGetPrimaryEntry(params string[] barcodes) : IMesArgs;

   /// <summary>
   /// 获取一注数据报文
   /// </summary>
   /// <returns></returns>
   [Languages("获取一注数据接口"), MesInterfaceInfo("/aiipcapi/api/station/private/produce/paramQueryV2")]
   public string GetMesRequest(ArgsGetPrimaryEntry args)
   {
      string[] paramCodes = ["RYCZY10022", "RYCZY10028"];
      dynamic[] processes =
      [
         new
         {
            processCode = _parameterConfig.DeviceParameter.ProcessOperationName,
            stepCode = "",
            paramCodes = paramCodes,
         },
      ];

      List<dynamic> list = new List<dynamic>();
      foreach (var barcode in args.barcodes)
      {
         list.Add(new { productCode = barcode, processes = processes });
      }
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         products = list,
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 产品进站报文
   /// <summary>
   /// 产品进站报文参数
   /// </summary>
   /// <param name="barcode"></param>
   public record ArgsProductEntry(string barcode) : IMesArgs;

   /// <summary>
   /// 产品进站报文
   /// </summary>
   /// <returns></returns>
   [Languages("在制品投料防呆校验接口"), MesInterfaceInfo("/api/station/private/produce/feeding/preventionCheck")]
   public string GetMesRequest(ArgsProductEntry args)
   {
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         technicsProcessCode = _parameterConfig.DeviceParameter.ProcessOperationName, //工序编码
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         productCodeList = new[] { new { productCode = args.barcode } },
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 二注防呆检验接口报文
   /// <summary>
   /// 二注防呆检验报文参数
   /// </summary>
   /// <param name="barcode"></param>
   public record ArgsSecondInjValidation(string barcode) : IMesArgs;

   /// <summary>
   /// 二注防呆检验报文
   /// </summary>
   /// <returns></returns>
   [
      Languages("二注防呆检验接口"),
      MesInterfaceInfo("http://10.2.100.8:13308/aiipcapi/api/station/private/produce/feeding/multiProcessPreventionCheck")
   ]
   public string GetMesRequest(ArgsSecondInjValidation args)
   {
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         technicsProcessCode = _parameterConfig.DeviceParameter.ProcessOperationName, //工序编码
         targetTechnicsProcessCodeList = new[] { "C1700" },
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         productCodeList = new[] { new { productCode = args.barcode } },
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 工单获取报文
   /// <summary>
   /// 工单获取报文参数
   /// </summary>
   /// <param name="barcode"></param>
   public record ArgsWorkOrder(string barcode) : IMesArgs;

   /// <summary>
   /// 工单获取报文
   /// </summary>
   /// <param name="barcode"></param>
   /// <returns></returns>
   [Languages("工单获取接口"), MesInterfaceInfo("/aiipcapi/api/station/private/produce/order/getByProductCode")]
   public string GetMesRequest(ArgsWorkOrder args)
   {
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         technicsProcessCode = _parameterConfig.DeviceParameter.ProcessOperationName, //工序编码
         productCode = args.barcode,
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 原材料上料报文
   /// <summary>
   /// 原材料上料参数
   /// </summary>
   /// <param name="barcode"></param>
   /// <param name="count"></param>
   public record ArgsMaterialIn(string barcode, double count) : IMesArgs;

   /// <summary>
   /// 原材料上料报文
   /// </summary>
   /// <param name="barcode"></param>
   /// <returns></returns>
   [Languages("原材料投料校验接口"), MesInterfaceInfo("/aiipcapi/api/station/private/produce/material/in")]
   public string GetMesRequest(ArgsMaterialIn args)
   {
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         produceOrderCode = _parameterConfig.DeviceParameter.WorkOrderNo, //工单号
         technicsProcessCode = _parameterConfig.DeviceParameter.ProcessOperationName, //工序编码
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         isForeign = "0", //是否外来料，非必填，空的话默认为0
         uploadTime = DateTime.Now.ToMesDateTime(),
         productCodeList = new[] { new { productCode = args.barcode, productCount = args.count } },
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 原材料卸料接口报文
   /// <summary>
   /// 原材料卸料接口参数
   /// </summary>
   /// <param name="barcode"></param>
   /// <param name="count"></param>
   public record ArgsMaterialOut(string barcode, double count) : IMesArgs;

   /// <summary>
   /// 原材料卸料接口报文
   /// </summary>
   /// <param name="barcode"></param>
   /// <returns></returns>
   [Languages("原材料卸料接口"), MesInterfaceInfo("/api/station/private/produce/material/out")]
   public string GetMesRequest(ArgsMaterialOut args)
   {
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         technicsProcessCode = _parameterConfig.DeviceParameter.ProcessOperationName, //工序编码
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         uploadTime = DateTime.Now.ToMesDateTime(),
         productCodeList = new[] { new { productCode = args.barcode, productCount = args.count } },
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 获取产品结果数据收集接口（单条）报文
   /// <summary>
   /// 获取产品结果数据收集接口（单条）报文参数
   /// </summary>
   /// <param name="batMain"></param>
   public record ArgsMesExit(IBatMainModel batMain) : IMesArgs;

   /// <summary>
   /// 获取产品结果数据收集接口（单条）报文
   /// </summary>
   /// <param name="batMain"></param>
   /// <param name="ngCode"></param>
   /// <returns></returns>
   [Languages("产品结果数据收集接口（单条）"), MesInterfaceInfo("/aiipcapi/api/produce/produce/open/add")]
   public string GetMesRequest(ArgsMesExit args)
   {
      DateTime exitTime = DateTime.Now;
      List<ExpandoObject> list = new List<ExpandoObject>();
      foreach (var item in _mesParameterConfig.ResultParameters)
      {
         try
         {
            if (!item.CheckExpired())
               continue;
            var mesValue = item.ValueConverter switch
            {
               null => args.batMain[item.LocalPropertyName].ToMesString(),
               _ => item.ValueConverter.Convert(_container, args.batMain, item.ConverterParam, item.LocalPropertyName),
            };
            dynamic obj = new ExpandoObject();
            obj.productCode = args.batMain.Barcode; //产品编码
            obj.technicsParamName = item.MesName; //工艺参数名称
            obj.technicsParamCode = item.MesCode; //工艺参数编码
            obj.technicsParamQuality = "1"; //工艺参数质量(结果判定)：0 不合格；1 合格；
            obj.technicsParamValue = mesValue;
            list.Add(obj);
         }
         catch (Exception ex)
         {
            $"取结果参数异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         }
      }
      List<dynamic> produceInEntityList = new List<dynamic>(); //原材料，加入电芯条码
      produceInEntityList.Add(new { productCode = args.batMain.Barcode, productCount = 1 });

      if (
         _parameterConfig.AdvancedConfig.ProductionType
         is ProductionTypeEnum.一次注液
            or ProductionTypeEnum.二次注液
            or ProductionTypeEnum.三次注液
      )
      {
         var vol = _parameterConfig.AdvancedConfig.ProductionType switch
         {
            ProductionTypeEnum.一次注液 => Math.Round(((IBatWeightAfterModel)args.batMain).TotalInjectionVolume, 3),
            _ => new Func<double>(() => //二次注液,三注
            {
               if (args.batMain is IBatWeightManualRefillModel replenish)
               {
                  return Math.Round(((IBatWeightAfterModel)args.batMain).ActualInjectionVolume + replenish.ManualRefillVolume, 3);
               }
               return Math.Round(((IBatWeightAfterModel)args.batMain).ActualInjectionVolume, 3);
            }).Invoke(),
         };

         produceInEntityList.Add(
            new //加入电解液
            {
               productCode = !string.IsNullOrEmpty(args.batMain.ElectrolyteBatch)
                  ? args.batMain.ElectrolyteBatch
                  : _parameterConfig.DeviceParameter.ElectrolyteLotCode,
               productCount = vol < 0 ? 0 : vol,
            }
         );

         //produceInEntityList.Add(new //加入胶钉批次
         //{
         //    productCode = !string.IsNullOrEmpty(args.batMain.GlueNailBatch) ? args.batMain.GlueNailBatch : _parameterConfig.DeviceParameter.GlueNailCode,
         //    productCount = 1
         //});
      }
      var ngCode = GetNgCode(_container, args.batMain);
      var entity = new
      {
         produceOrderCode = string.IsNullOrEmpty(args.batMain.WorkOrderNumber)
            ? _parameterConfig.DeviceParameter.WorkOrderNo
            : args.batMain.WorkOrderNumber,
         technicsProcessCode = _parameterConfig.DeviceParameter.ProcessOperationName, //工序编码
         technicsProcessName = _parameterConfig.DeviceParameter.ProcessName, //工序名称
         technicsStepCode = _parameterConfig.DeviceParameter.StepCode, //工步编码
         technicsStepName = _parameterConfig.DeviceParameter.StepNmae, //工步名称
         productCode = args.batMain.Barcode, //产品编码（模组）
         productCount = 1, //产品数量
         productQuality = string.IsNullOrWhiteSpace(ngCode) ? 1 : 0, //产品质量(结果判定)：0 不合格；1 合格；
         //  productQuality = args.batMain.FinalStatus.GetResultArea() == ResultArea.NG ? 0 : 1, //产品质量(结果判定)：0 不合格；1 合格；
         ngType = ngCode, //工艺提供
         produceDate = exitTime.ToString("yyyy-MM-dd"), //生产日期
         startTime = args.batMain.CreateTime.ToMesDateTime(), //生产开始时间
         endTime = exitTime.ToMesDateTime(), //生产结束时间
         userName = string.IsNullOrEmpty(_usersStatusConfig.LocalLoggedinUser.Name) ? "未登陆" : _usersStatusConfig.LocalLoggedinUser.Name, //用户名称
         userAccount = string.IsNullOrEmpty(_usersStatusConfig.LocalLoggedinUser.Account)
            ? "未登陆"
            : _usersStatusConfig.LocalLoggedinUser.Account, //用户账号
         deviceCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         deviceName = _parameterConfig.DeviceParameter.DeviceName, //设备名称
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         produceInEntityList = produceInEntityList,
         produceParamEntityList = list,
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }

   private string GetNgCode(IContainer container, IBatMainModel batMain)
   {
      if (batMain.FinalStatus.GetResultArea() != ResultArea.NG)
         return string.Empty;

      var param = container.Get<ParameterConfig>();
      if (param.AdvancedConfig.ProductionType == ProductionTypeEnum.一次注液)
      {
         return batMain.NgProcesses switch
         {
            ProcessTypeEnum.测短路 => batMain.FinalStatus
               is ResultTypeEnum.E01_开路
                  or ResultTypeEnum.E01_开路1
                  or ResultTypeEnum.外壳开路
                  or ResultTypeEnum.开路
               ? "NG-E-YCZY-004"
               : "NG-E-YCZY-004",
            ProcessTypeEnum.前称重 => "NG-E-YCZY-001",
            ProcessTypeEnum.测漏 => "NG-E-YCZY-005",
            ProcessTypeEnum.注液 => new Func<string>(() =>
            {
               var inj = ((IBatWeightAfterModel)batMain).FinalInjectResult;
               return inj switch
               {
                  ResultTypeEnum.注液量偏少 => "NG-E-YCZY-003",
                  ResultTypeEnum.注液量偏多 => "NG-E-YCZY-003",
                  _ => inj.ToString(),
               };
            }).Invoke(),
            //  ProcessTypeEnum.打钉检测 => "NG-FYCZY0006",
            ProcessTypeEnum.打钉检测 => "", //20260119客户要求修改打钉NG当合格
            ProcessTypeEnum.注液后称重 => "NG-E-YCZY-002",
            ProcessTypeEnum.静置站=> "NG-E-YCZY-007",
            //_ => "未知NG类型"
            _ => "",
         };
      }
      else if (param.AdvancedConfig.ProductionType == ProductionTypeEnum.二次注液)
      {
         return batMain.NgProcesses switch
         {
            ProcessTypeEnum.前称重 => "NG-E-ECZY-001",
            ProcessTypeEnum.测漏 => "NG-E-ECZY-004",
            ProcessTypeEnum.注液 => new Func<string>(() =>
            {
               var inj = ((IBatWeightAfterModel)batMain).FinalInjectResult;
               return inj switch
               {
                  ResultTypeEnum.注液量偏少 => "NG-E-ECZY-003",
                  ResultTypeEnum.注液量偏多 => "NG-E-ECZY-003",
                  _ => "未知注液NG类型",
               };
            }).Invoke(),
            // ProcessTypeEnum.打钉检测 => "NG-FECZY0006",
            ProcessTypeEnum.打钉检测 => "", //20260119客户要求修改打钉NG当合格
            ProcessTypeEnum.后称重 => "NG-E-ECZY-002",
            ProcessTypeEnum.回氦=> "NG-E-ECZY-005",
            //_ => "未知NG类型"
            _ => "",
         };
      }
      else if (param.AdvancedConfig.ProductionType == ProductionTypeEnum.三次注液) //三注要加
      {
         return "";
         //return batMain.NgProcesses switch
         //{
         //   ProcessTypeEnum.前称重 => "NG-FECZY0001",
         //   ProcessTypeEnum.测电压 => "NG-FECZY0002",
         //   ProcessTypeEnum.测漏 => "NG-FECZY0003",
         //   ProcessTypeEnum.注液 => new Func<string>(() =>
         //   {
         //      var inj = ((IBatWeightAfterModel)batMain).InjectResult;
         //      return inj switch
         //      {
         //         ResultTypeEnum.注液量偏少 => "NG-FECZY0004",
         //         ResultTypeEnum.注液量偏多 => "NG-FECZY0005",
         //         _ => "未知注液NG类型",
         //      };
         //   }).Invoke(),
         //   // ProcessTypeEnum.打钉检测 => "NG-FECZY0006",
         //   ProcessTypeEnum.打钉检测 => "", //20260119客户要求修改打钉NG当合格
         //   ProcessTypeEnum.后称重 => "NG-FECZY0007",
         //   //_ => "未知NG类型"
         //   _ => "",
         //};
      }
      // return "未定义NG类型";
      return "";
   }

   #endregion

   #region OEE主动停机上报接口报文
   /// <summary>
   /// OEE主动停机上报接口参数
   /// </summary>
   /// <param name="status"></param>
   /// <param name="guid"></param>
   /// <param name="warningType"></param>
   /// <param name="startTime"></param>
   /// <param name="endTime"></param>
   /// <param name="responseTime"></param>
   public record ArgsActiveShutdown(int status, string guid, string warningType, DateTime startTime, DateTime? endTime, string responseTime)
      : IMesArgs;

   /// <summary>
   /// OEE主动停机上报接口报文
   /// </summary>
   /// <param name="args"></param>
   /// <returns></returns>
   [Languages("OEE主动停机上报接口"), MesInterfaceInfo("http://10.1.100.9:19091/api/tab/")]
   public string GetMesRequest(ArgsActiveShutdown args)
   {
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         operateId = _usersStatusConfig.LocalLoggedinUser.Account, //操作者
         status = args.status, //0为主动停机开始，1为主动停机结束
         warningSerial = args.guid, //报警开始和结束使用同一个序列号UUID
         warningType = args.warningType, //主动停机原因（吃饭(休息)、5S(清洁)、验证测试、设备清洁、设备点检、设备维保、换型(清批)、治具更换、物料更换、缺料停机、动力异常、参数调整、返工作业、其他停机、设备故障）
         warningStartTime = args.startTime.ToMesDateTime(), //主动停机开始时间(YYYY-MM-DD HH:MM:SS)
         warningEndTime = args.endTime?.ToMesDateTime() ?? "", //主动停机结束时间(YYYY-MM-DD HH:MM:SS)  主动停机开始时值为null，主动停机结束时必传主动停机结束时间
         responseTime = args.responseTime, //主动停机时长（单位秒） 是，主动停机开始时值为null，主动停机结束时为主动停机结束时间-主动停机开始时间
         uploadTime = DateTime.Now.ToMesDateTime(),
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region OEE被动停机及报警上报接口报文
   /// <summary>
   /// OEE被动停机及报警上报接口参数
   /// </summary>
   public record ArgsPassiveShutdown(int status, string guid, string warningCode, DateTime startTime, DateTime? endTime, int? responseTime)
      : IMesArgs;

   /// <summary>
   /// OEE被动停机及报警上报接口
   /// </summary>
   /// <param name="args"></param>
   /// <returns></returns>
   [Languages("OEE被动停机及报警上报接口"), MesInterfaceInfo("http://10.1.100.9:19090/api/tab/")]
   public string GetMesRequest(ArgsPassiveShutdown args)
   {
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         operateId = _usersStatusConfig.LocalLoggedinUser.Account, //操作者
         status = args.status.ToString(), //0为主动停机开始，1为主动停机结束
         warningSerial = args.guid, //UUID
         warningCode = args.warningCode, //报警码
         warningStartTime = args.startTime.ToMesDateTime(), //主动停机开始时间(YYYY-MM-DD HH:MM:SS)
         warningEndTime = args.endTime == null ? "" : args.endTime.Value.ToMesDateTime(), //主动停机结束时间(YYYY-MM-DD HH:MM:SS)  主动停机开始时值为null，主动停机结束时必传主动停机结束时间
         responseTime = args.responseTime == null ? "" : args.responseTime.Value.ToString(), //主动停机时长（单位秒） 是，主动停机开始时值为null，主动停机结束时为主动停机结束时间-主动停机开始时间
         uploadTime = DateTime.Now.ToMesDateTime(),
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 待料、堵料上报接口报文
   /// <summary>
   /// 待料、堵料上报接口参数
   /// </summary>
   /// <param name="status"></param>
   /// <param name="guid"></param>
   /// <param name="waitingType"></param>
   /// <param name="startTime"></param>
   /// <param name="endTime"></param>
   /// <param name="waitingTime">等待相应时间（单位秒）</param>
   public record ArgsMaterialShortage(
      int status,
      string guid,
      string waitingType,
      DateTime startTime,
      DateTime? endTime,
      string waitingTime
   ) : IMesArgs;

   /// <summary>
   /// 待料、堵料上报接口报文
   /// </summary>
   /// <param name="args"></param>
   /// <returns></returns>
   [Languages("待料、堵料上报接口"), MesInterfaceInfo("http://10.1.100.9:19092/api/tab/")]
   public string GetMesRequest(ArgsMaterialShortage args)
   {
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         operateId = _usersStatusConfig.LocalLoggedinUser.Account, //操作者
         status = args.status, //0为待料/堵料开始，1为待料/堵料结束
         waitingSerial = args.guid, //等待序列号（唯一）
         waitingType = args.waitingType, //等待原因（待料/堵料）
         waitingStartTime = args.startTime.ToMesDateTime(), //等待开始时间
         waitingEndTime = args.endTime?.ToMesDateTime() ?? "", //等待结束时间
         waitingTime = args.waitingTime, //等待相应时间（单位秒） 是，主动停机开始时值为null，主动停机结束时为主动停机结束时间-主动停机开始时间
         uploadTime = DateTime.Now.ToMesDateTime(),
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region  设备状态上报接口报文
   /// <summary>
   /// 设备状态上报接口参数
   /// </summary>
   /// <param name="manualStatus">手自动状态（0：手动；1：自动）</param>
   /// <param name="runStatus">运行状态（0：非运行；1：运行）</param>
   /// <param name="waitStatus">待机状态（0：非待机；1：待机）</param>
   /// <param name="faultStatus">故障状态（0：非故障；1：故障）</param>
   /// <param name="repairStatus">维修状态（0：非维修；1：维修）</param>
   /// <param name="stopStatus">急停状态（0：非急停；1：急停）</param>
   /// <param name="equipSign">设备状态指示牌（带反馈）（0：停机；1：运行；2：待机；3：故障；4：维修；5：急停；6：其它）</param>
   /// <param name="warningStatus">报警喇叭状态（0：喇叭停止；1：喇叭报警）</param>
   public record ArgsDeviceStatus(
      string manualStatus,
      string runStatus,
      string waitStatus,
      string faultStatus,
      string repairStatus,
      string stopStatus,
      string equipSign,
      string warningStatus
   ) : IMesArgs;

   /// <summary>
   /// 设备状态上报接口报文
   /// </summary>
   /// <param name="args"></param>
   /// <returns></returns>
   [Languages("设备状态上报接口"), MesInterfaceInfo("/api/equip/oeeStatus")]
   public string GetMesRequest(ArgsDeviceStatus args)
   {
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         operateId = _usersStatusConfig.LocalLoggedinUser.Account, //操作者
         manualStatus = args.manualStatus, //手自动状态（0：手动；1：自动）
         runStatus = args.runStatus, //运行状态（0：非运行；1：运行）
         waitStatus = args.waitStatus, //待机状态（0：非待机；1：待机）
         faultStatus = args.faultStatus, //故障状态（0：非故障；1：故障）
         repairStatus = args.repairStatus, //维修状态（0：非维修；1：维修）
         stopStatus = args.stopStatus, //急停状态（0：非急停；1：急停）
         mesConnectStatus = "1", //与MES通讯状态（0：状态异常；1：状态正常）
         equipMesStatus = "0", //设备MES状态（0：未屏蔽MES上位机或者扫码枪；1：屏蔽）
         equipSign = args.equipSign, //设备状态指示牌（带反馈）（0：停机；1：运行；2：待机；3：故障；4：维修；5：急停；6：其它）
         warningStatus = args.warningStatus, //报警喇叭状态（0：喇叭停止；1：喇叭报警）
         uploadTime = DateTime.Now.ToMesDateTime(),
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 开机上传参数接口报文
   /// <summary>
   /// 开机上传参数接口参数
   /// </summary>
   public record ArgsRunUploadParam(IList<UploadParamDto> paramList) : IMesArgs;

   /// <summary>
   /// 开机上传参数接口报文
   /// </summary>
   /// <param name="args"></param>
   /// <returns></returns>
   [Languages("开机上传参数接口"), MesInterfaceInfo("/api/station/private/produce/equip/uploadParam")]
   public string GetMesRequest(ArgsRunUploadParam args)
   {
      List<ExpandoObject> list = new List<ExpandoObject>();
      foreach (var item in args.paramList)
      {
         dynamic obj = new ExpandoObject();
         obj.paramCode = item.ParamCode; //参数编码
         obj.paramName = item.ParamName; //参数名称
         obj.setValue = item.CurrentValue; //设置值
         list.Add(obj);
      }
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         uploadTime = DateTime.Now.ToMesDateTime(),
         upperCompTime = DateTime.Now.ToMesDateTime(), //上位机当前时间
         plc1Time = DateTime.Now.ToMesDateTime(), //PLC1当前时间
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         paramList = list,
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 修改参数上传接口报文
   /// <summary>
   /// 修改参数上传接口参数 ,修改参数时，上传最新所有参数信息至AI工控
   /// </summary>
   public record ArgsChangeParam(IList<UploadParamDto> paramList) : IMesArgs;

   /// <summary>
   /// 修改参数上传接口报文
   /// </summary>
   /// <param name="args"></param>
   /// <returns></returns>
   [Languages("修改参数上传接口"), MesInterfaceInfo("/api/station/private/produce/equip/changeParam")]
   public string GetMesRequest(ArgsChangeParam args)
   {
      List<ExpandoObject> list = new List<ExpandoObject>();
      foreach (var item in args.paramList)
      {
         dynamic obj = new ExpandoObject();
         obj.paramCode = item.ParamCode; //参数编码
         obj.paramName = item.ParamName; //参数名称
         obj.oldValue = item.OriginalValue; //原始值
         obj.setValue = item.CurrentValue; //设置值
         list.Add(obj);
      }
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         userAccout = "jlxg", //用户账号,MES要求写死, Accout MES写错了
         //  userAccount = _usersStatusConfig.LocalLoggedinUser.Account, //用户账号
         uploadTime = DateTime.Now.ToMesDateTime(),
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         paramList = list,
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 设备产能上报接口报文
   /// <summary>
   /// 设备产能上报接口参数 （ 1分钟和单个产品产出）
   /// </summary>
   /// <param name="singleBeat">单个产品节拍值（浮点值）</param>
   /// <param name="minutePPM">1分钟节拍值（浮点值）</param>
   public record ArgsOeeCapacity(double singleBeat, double minutePPM) : IMesArgs;

   /// <summary>
   /// 设备产能上报接口报文
   /// </summary>
   /// <param name="args"></param>
   /// <returns></returns>
   [Languages("设备产能上报接口"), MesInterfaceInfo("/api/equip/oeeCapacity")]
   public string GetMesRequest(ArgsOeeCapacity args)
   {
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         operateId = _usersStatusConfig.LocalLoggedinUser.Account, //操作者
         singleBeat = args.singleBeat.ToString(), //单个产品节拍值（浮点值）
         minutePPM = args.minutePPM.ToString(), //1分钟节拍值（浮点值）
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 工装夹具寿命上传接口报文
   /// <summary>
   /// 工装夹具寿命上传接口参数
   /// </summary>
   /// <param name="frockList"></param>
   public record ArgsFixtureLifeCount(IList<(string frockCode, string useCount)> frockList) : IMesArgs;

   /// <summary>
   /// 工装夹具寿命上传接口报文
   /// </summary>
   /// <param name="args"></param>
   /// <returns></returns>
   [Languages("工装夹具寿命上传接口"), MesInterfaceInfo("/api/station/private/produce/frock/life")]
   public string GetMesRequest(ArgsFixtureLifeCount args)
   {
      List<ExpandoObject> list = new List<ExpandoObject>();
      foreach (var item in args.frockList)
      {
         dynamic obj = new ExpandoObject();
         obj.frockCode = item.frockCode; //工装夹具码
         obj.useCount = item.useCount; //工装夹具码
         list.Add(obj);
      }
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         technicsProcessCode = _parameterConfig.DeviceParameter.ProcessOperationName, //工序编码
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         uploadTime = DateTime.Now.ToMesDateTime(),
         frockList = list,
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 工装夹具更换上传接口报文
   /// <summary>
   /// 工装夹具更换上传接口参数
   /// </summary>
   /// <param name="frockList"></param>
   public record ArgsReplaceFixture(IList<(string frockCode, string changefrockCode)> frockList) : IMesArgs;

   /// <summary>
   /// 工装夹具更换上传接口报文
   /// </summary>
   /// <param name="args"></param>
   /// <returns></returns>
   [Languages("工装夹具更换上传接口"), MesInterfaceInfo("/api/station/private/produce/frock/change")]
   public string GetMesRequest(ArgsReplaceFixture args)
   {
      List<ExpandoObject> list = new List<ExpandoObject>();
      foreach (var item in args.frockList)
      {
         dynamic obj = new ExpandoObject();
         obj.frockCode = item.frockCode; //工装夹具码
         obj.changefrockCode = item.changefrockCode; //更换工装夹具码
         list.Add(obj);
      }
      var entity = new
      {
         tenantID = _parameterConfig.DeviceParameter.LineName, //产线编号
         technicsProcessCode = _parameterConfig.DeviceParameter.ProcessOperationName, //工序编码
         equipCode = _parameterConfig.DeviceParameter.DeviceCode, //设备编码
         uploadTime = DateTime.Now.ToMesDateTime(),
         frockList = list,
      };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion

   #region 发送电池干重
   /// <summary>
   /// 发送电池干重
   /// </summary>
   /// <param name="frockList"></param>
   public record ArgsSendNetWeight(string barcode, double netweigh) : IMesArgs;

   /// <summary>
   /// 发送电池干重接口报文
   /// </summary>
   /// <param name="args"></param>
   /// <returns></returns>
   [Languages("发送电池干重"), MesInterfaceInfo("http://172.26.232.173:5082/WeightBattery")]
   public string GetMesRequest(ArgsSendNetWeight args)
   {
      var entity = new { BatCode = args.barcode, Weigh = args.netweigh };
      return JsonSerializer.Serialize(entity, GenericHelper.SerializerOptions);
   }
   #endregion
}
