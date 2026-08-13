namespace Kinlo.Services;

public abstract class ServiceHandlerBase : IServiceHandler
{
   protected IPLC _plc;
   protected IContainer _container;
   protected MesService _mesService;
   protected ParameterConfig _parameterConfig;
   protected DevicesConfig _devicesConfig;
   protected PLCSignalConfig _plcSignalConfig;
   protected InspectionConfig _inspectionConfig;
   protected MesInterfaceParameterConfig _mesInterfaceParameterConfig;
   protected DisplayDataCollection _displayDataCollection;
   protected GlobalStaticTemporary _globalStaticTemporary;
   protected PlcStatusConfig _plcStatusConfig;
   protected IBatteryCache _batteryCache;
   protected DbHelper _sugarDB;
   protected Stopwatch _stopwatch;
   protected CancellationTokenSource _cancellationTokenSource;
   protected DisplayDataDto? _processesDatas; //界面显示数据
   protected SnowflakeHelper _snowflakeHelper; //雪花ID生成

   /// 当前工序实时统计数据
   // protected int okCount = 0, ng1Count = 0, ng2Count = 0, injectOKCount = 0, lowElectrolyteCount = 0, highElectorlyteCount = 0;//添加统计界面显示
   protected ProcessRoleEnum _processRole = ProcessRoleEnum.None; //工序顺序，比如进出站
   protected string _taskLogHeader;

   /// <summary>
   /// 称重的重量警戒值，小于这个值应重新称
   /// </summary>
   protected double _weightWarningValue = 10;

   /// <summary>
   /// //找不到设备标记（如无设备停机）,true有报警，false无报警
   /// </summary>
   protected bool _isDeviceAlarm = false;
   private PLCInteractAddressModel _context;
   public PLCInteractAddressModel Context
   {
      get => _context;
   }

   protected ServiceHandlerBase(
      IContainer container,
      IDevice plc,
      PLCInteractAddressModel plcInteractAddress,
      CancellationTokenSource taskToken
   )
   {
      _container = container;
      _cancellationTokenSource = taskToken;
      _plc = (IPLC)plc;
      _stopwatch = new Stopwatch();
      _mesService = container.Get<MesService>();
      _parameterConfig = container.Get<ParameterConfig>();
      _devicesConfig = container.Get<DevicesConfig>();
      _mesInterfaceParameterConfig = container.Get<MesInterfaceParameterConfig>();
      _plcSignalConfig = container.Get<PLCSignalConfig>();
      _plcStatusConfig = container.Get<PlcStatusConfig>();
      _displayDataCollection = container.Get<DisplayDataCollection>();
      _globalStaticTemporary = container.Get<GlobalStaticTemporary>();
      _batteryCache = container.Get<IBatteryCache>();
      _sugarDB = container.Get<DbHelper>();
      _snowflakeHelper = container.Get<SnowflakeHelper>();
      _inspectionConfig = container.Get<InspectionConfig>();
      _context = plcInteractAddress;
      _taskLogHeader = _context.ToProcessLogHeader();
      _processesDatas = _displayDataCollection.ProcessesDatas.FirstOrDefault(x => x.Processes == Context.ProcessesType);

      #region 判断当前工序是否进出站
      var group = _plcSignalConfig
         .PLCInteractAddresses.Where(x => x.ProcessesType != ProcessTypeEnum.设备状态)
         .GroupBy(x => x.ProcessesType)
         .ToList();
      if (group.Count == 1) //如果只有一个工序
      {
         _processRole = ProcessRoleEnum.进出站;
      }
      else
      {
         if (
            _plcSignalConfig
               .PLCInteractAddresses.Where(x => x.ProcessesType != ProcessTypeEnum.设备状态)
               .MinBy(x => x.ProductionIndex)
               ?.ProcessesType == Context.ProcessesType
         )
         {
            _processRole = ProcessRoleEnum.进站;
         }
         else
         {
            if (_plcSignalConfig.PLCInteractAddresses.Any(x => x.ProcessesType == ProcessTypeEnum.出站)) //有配置单独出站工序
            {
               if (Context.ProcessesType == ProcessTypeEnum.出站)
                  _processRole = ProcessRoleEnum.出站;
            }
            else
            {
               if (_plcSignalConfig.PLCInteractAddresses.MaxBy(x => x.ProductionIndex)?.ProcessesType == Context.ProcessesType)
                  _processRole = ProcessRoleEnum.出站;
            }
         }
      }
      #endregion
   }

   #region Handle
   /// <summary>
   /// 处理逻辑的核心代码
   /// </summary>
   /// <param name="plcValue"></param>
   /// <returns></returns>
   protected abstract Task HandleCore(short plcValue);

   public virtual async Task<int> Handle(short plcValue)
   {
      if (!StartTask())
         return await TaskFinish(true);
      try
      {
         await HandleCore(plcValue);
      }
      catch (Exception ex)
      {
         _isDeviceAlarm = true; //如果出现异常，不给PLC发完成信号
         $"出现异常:{ex};".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
      }

      return await TaskFinish(false);
   }

   string? startCmd = null;

   /// <summary>
   /// 开始任务时调用
   /// </summary>
   /// <returns></returns>
   protected bool StartTask()
   {
      try
      {
         _batteryToDisplay.Clear();
         _isDeviceAlarm = false;
         _stopwatch.Restart();
         startCmd ??= JsonSerializer.Serialize(Context.StartCommand);
         $"收到PLC信号，地址[{startCmd}]".LogProcess(_taskLogHeader);
         return WritePlcSingle(-1, Context.StartCommand.Tag, _taskLogHeader);
      }
      catch (Exception ex)
      {
         $"{JsonSerializer.Serialize(Context.StartCommand.Tag)}写入-1异常：{ex}".LogProcess(_taskLogHeader, Log4NetLevelEnum.警告, true);
         return false;
      }
   }

   /// <summary>
   /// 任务完成
   /// </summary>
   /// <param name="isStart"></param>
   /// <returns></returns>
   public async Task<int> TaskFinish(bool isStart)
   {
      try
      {
         if (!isStart)
         {
            var batteriesForDisplay = _batteryToDisplay.ToArray(); //快照

            _ = _processesDatas?.AddDisplayData(batteriesForDisplay); //当前工序完成后一次性添加显示数据

            if (_processRole == ProcessRoleEnum.出站) //如果当前工序是最后工序，就显示完整电芯
            {
               _ = _displayDataCollection.CompleteBatteryDatas.AddDisplayData(batteriesForDisplay);
            }
         }

         if (_isDeviceAlarm)
         {
            $"出现处理错误，需停机检验，不给PLC发完成信号！;".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
         }
         else
         {
            short plcValue = isStart ? (short)-1 : (short)-2;
            WritePlcSingle(plcValue, Context.StartCommand.Tag, _taskLogHeader);
         }
      }
      catch (Exception e)
      {
         $"完成任务时发生异常：{e}".LogProcess(_taskLogHeader, Log4NetLevelEnum.错误, true);
      }
      finally
      {
         _batteryToDisplay.Clear();
         _stopwatch.Stop();
         $"完成任务,用时:{_stopwatch.ElapsedMilliseconds}ms".LogProcess(_taskLogHeader);
      }
      return await Task.FromResult(Context.Key);
   }
   #endregion

   #region 添加UI数据
   /// <summary>
   /// 待发送给UI显示数据
   /// </summary>
   List<IBatMainModel> _batteryToDisplay = new List<IBatMainModel>();
   static readonly object _toDisplayLock = new object();

   /// <summary>
   /// 添加待发送给UI显示的数据
   /// </summary>
   public void AddDisplayData(params IBatMainModel[] battery)
   {
      lock (_toDisplayLock)
      {
         _batteryToDisplay.AddRange(battery);
      }
   }

   #endregion

   #region 读写PLC
   protected bool WritePlcSingle(short value, SignalAddressModel tag, string logHeader)
   {
      try
      {
         for (int i = 0; i < 3; i++)
         {
            if (_plc.WriteValue(value, tag, logHeader))
            {
               $"[{JsonSerializer.Serialize(tag)}] 写入[{value}] 成功".LogProcess(logHeader);
               return true;
            }
            else
            {
               $"[{JsonSerializer.Serialize(tag)}] 第{i + 1}次 写入[{value}] 失败".LogProcess(logHeader, Log4NetLevelEnum.错误);
            }
         }
      }
      catch (Exception ex)
      {
         $"[{tag}] 写入[{value}] 异常:\r\n{ex}".LogProcess(logHeader, Log4NetLevelEnum.警告);
      }
      return false;
   }

   ///// <summary>
   ///// 写入结果
   ///// </summary>
   ///// <param name="resultTypes">结果</param>
   ///// <param name="label"></param>
   ///// <param name="barcode"></param>
   ///// <param name="index"></param>
   //public void WriteResult(ResultTypeEnum resultTypes, SignalAddressModel label, string barcode, int index)
   //=> resultTypes.WritePlcResult(new SignalAddressModel($"{label.Lable}.PCResult", 0), barcode, index, _plc, _parameterConfig, Context);

   ///// <summary>
   ///// 写入PLC报警
   ///// </summary>
   //protected void WritePLCMemoryLossAlarm()
   //{
   //    if (_plcSignalConfig.PLCAlarmAddresses.PLCMemoryLossAlarms.TryGetValue(Context.ProcessesType, out var alarm_address))
   //        WritePlcSingle(1, alarm_address);
   //    else
   //        TaskLog($"注意：记忆丢失，但未找到PLC报警标签！", 0, Log4NetLevelEnum.错误);
   //}

   /// <summary>
   /// 读取PLC ID统一方法
   /// </summary>
   /// <param name="plcDatas"></param>
   /// <param name="length">长度，默认为 Context.DataLength </param>
   /// <param name="showErrorDialog">错误是否弹窗</param>
   /// <returns></returns>
   public bool TryReadPlcData(out List<ReceivePlcDataModel> plcDatas, int length = 0, bool showErrorDialog = true)
   {
      length = length <= 0 ? Context.DataLength : length;
      bool state = false;
      if (length == 1)
      {
         state = ReadPLCDataSingle(out ReceivePlcDataModel plcData, _taskLogHeader);
         plcDatas = [plcData];
      }
      else
      {
         //_state = ReadPLCDataList(out plcDatas, length);
         // _state = ReadPLCDataList2(out plcDatas, length);
         state = ReadPLCDataList3(out plcDatas, length, _taskLogHeader);
      }
      if (state)
      {
         JsonSerializer.Serialize(plcDatas).LogProcess(_taskLogHeader);
      }
      else
      {
         if (showErrorDialog)
         {
            string _msg =
               $"==>>{Context.ProcessesType}<<==收到PLC启动信号，没收到PLC电池ID，请联系电气工程师！！！标签：{JsonSerializer.Serialize(Context.DataAddress)}";
            UIThreadHelper.InvokeOnUiThreadAsync(
               new Action(() => Dialog.Show(new AlarmDialog(new AlarmDialogDto(_msg)), GenericHelper.FullScreenAlarmToken))
            );
            _msg.LogProcess(_taskLogHeader);
            // WritePLCMemoryLossAlarm();
         }
      }
      return state;
   }

   #region 读取单个记忆
   /// <summary>
   /// 读取单个记忆
   /// </summary>
   /// <param name="address_struct">结构体起始地址</param>
   /// <param name="alarm_address">报警地址</param>
   /// <returns></returns>
   public bool ReadPLCDataSingle(out ReceivePlcDataModel plcData, string logHeader)
   {
      var _tag = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPCData[0]");
      PlcGeneric2DTU _plcGenericData = new PlcGeneric2DTU();
      try
      {
         _plc.ReadClass(_tag, _plcGenericData, logHeader);
         if (_plcGenericData.ID != 0)
         {
            plcData = new ReceivePlcDataModel()
            {
               Index = Context.DeviceStartIndex,
               DataAddress = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[0]", 0),
               ID = _plcGenericData.ID,
               PLCDataType = _plcGenericData.PLCDataType,
               PLCData = _plcGenericData.PLCData,
            };
            return true;
         }
      }
      catch (Exception e)
      {
         $"读取 [{JsonSerializer.Serialize(_tag)}] 记忆异常：{e}".LogProcess(logHeader, Log4NetLevelEnum.警告);
      }
      plcData = new ReceivePlcDataModel();
      return false;
   }
   #endregion

   #region 读取多个记忆
   /// <summary>
   /// 读取多个记忆,
   /// </summary>
   /// <param name="alarm_address"></param>
   /// <returns></returns>
   public bool ReadPLCDataList(out List<ReceivePlcDataModel> plcDatas, int length, string logHeader)
   {
      plcDatas = new List<ReceivePlcDataModel>();
      PlcGenericArrayDTU batteryArray = new PlcGenericArrayDTU(length);
      var _tag = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPCData");
      try
      {
         plcDatas.Clear();
         _plc.ReadClass(_tag, batteryArray, logHeader);
         for (int index = 0; index < batteryArray.Batterys.Length; index++)
         {
            if (batteryArray.Batterys[index] == null)
               continue;
            if (batteryArray.Batterys[index].ID != 0)
            {
               plcDatas.Add(
                  new ReceivePlcDataModel()
                  {
                     Index = (byte)(index + 1),
                     DataAddress = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[{index}]", 0),
                     ID = batteryArray.Batterys[index].ID,
                     PLCDataType = batteryArray.Batterys[index].PLCDataType,
                     PLCData = batteryArray.Batterys[index].PLCData,
                  }
               );
            }
         }
         return plcDatas.Count > 0;
      }
      catch (Exception e)
      {
         $"读取 [{JsonSerializer.Serialize(_tag)}] 记忆异常：{e}".LogProcess(logHeader, Log4NetLevelEnum.警告);
      }
      return false;
   }
   #endregion

   #region 读取多个记忆 ,相对第一个方法较慢，约8ms左路
   /// <summary>
   /// 读取多个记忆,相对第一个方法较慢，约8ms左路
   /// </summary>
   /// <param name="plcDatas"></param>
   /// <param name="length"></param>
   /// <returns></returns>
   public bool ReadPLCDataList2(out List<ReceivePlcDataModel> plcDatas, int length, string logHeader)
   {
      List<SignalAddressModel> _tags = new List<SignalAddressModel>();
      for (int i = 0; i < length; i++)
      {
         _tags.Add(new SignalAddressModel($"{Context.DataAddress.Lable}.ToPCData[{i}]"));
      }
      $"读取数据，标签:{JsonSerializer.Serialize(_tags)}".LogProcess(logHeader);
      plcDatas = new List<ReceivePlcDataModel>();
      try
      {
         var rs = _plc.ReadClasses<PlcGeneric2DTU>(_tags.ToArray(), logHeader);
         if (rs.Status != DeviceStatus.Success)
            return false;
         List<PlcGeneric2DTU> _plcGenericDatas = rs.Value;
         plcDatas.Clear();
         for (int index = 0; index < _plcGenericDatas.Count; index++)
         {
            if (_plcGenericDatas[index] == null)
               continue;
            if (_plcGenericDatas[index].ID != 0)
            {
               plcDatas.Add(
                  new ReceivePlcDataModel()
                  {
                     Index = (byte)(index + 1),
                     DataAddress = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[{index}]", 0),
                     ID = _plcGenericDatas[index].ID,
                     PLCDataType = _plcGenericDatas[index].PLCDataType,
                     PLCData = _plcGenericDatas[index].PLCData,
                  }
               );
            }
         }
         return plcDatas.Count > 0;
      }
      catch (Exception e)
      {
         $"读取记忆[{JsonSerializer.Serialize(_tags)}]异常：{e}".LogProcess(logHeader, Log4NetLevelEnum.警告);
      }
      return false;
   }
   #endregion

   #region clss3有连接读取多个记忆
   /// <summary>
   /// clss3有连接读取多个记忆
   /// </summary>
   /// <param name="plcDatas"></param>
   /// <param name="length"></param>
   /// <param name="logHeader"></param>
   /// <param name="cycleNumbe">循环次数（有些可能是没有ID的就不用多次读取了）</param>
   /// <returns></returns>
   public bool ReadPLCDataList3(out List<ReceivePlcDataModel> plcDatas, int length, string logHeader)
   {
      var address = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPCData");
      plcDatas = new List<ReceivePlcDataModel>();
      try
      {
         plcDatas.Clear();
         var rs = _plc.ReadLargeObjects<PlcGeneric2DTU>(address, logHeader);
         if (rs.Status != DeviceStatus.Success)
            return false;
         var plcGenericDatas = rs.Value!;
         for (int index = 0; index < length; index++)
         {
            if (index >= plcGenericDatas.Count)
               break;
            if (plcGenericDatas[index] == null)
               continue;
            if (plcGenericDatas[index].ID != 0)
            {
               plcDatas.Add(
                  new ReceivePlcDataModel
                  {
                     ID = plcGenericDatas[index].ID,
                     PLCDataType = plcGenericDatas[index].PLCDataType,
                     PLCData = plcGenericDatas[index].PLCData,
                     Index = (byte)(index + 1),
                     DataAddress = new SignalAddressModel($"{Context.DataAddress.Lable}.ToPLCData[{index}]", 0),
                  }
               );
            }
         }
         return plcDatas.Count > 0;
      }
      catch (Exception e)
      {
         $"读取记忆[ {JsonSerializer.Serialize(address)} ]异常：{e}".LogProcess(logHeader, Log4NetLevelEnum.警告);
      }
      return false;
   }
   #endregion
   #endregion

   #region 上传MES
   /// <summary>
   /// 上传MES结果数据
   /// </summary>
   /// <param name="battery"></param>
   /// <returns></returns>
   protected async Task MesOutput(IBatMainModel battery, string logHeader) =>
      await MesOutboundHelper.ProductionMesOutput(_container, _mesService, battery, logHeader);
   #endregion

   #region 发送值到PLC的 PCData1 (注液量等)
   protected ResultTypeEnum OnSupplementaryElectrolyteToPlc(ReceivePlcDataModel plcData, float value, string logHeader, string sendType)
   {
      for (int n = 1; n < 4; n++)
      {
         var tag = new SignalAddressModel($"{plcData.DataAddress.Lable}.PCData1", 0);
         if (_plc.WriteValue(value, tag, logHeader))
         {
            $"{sendType}：{value}; 第[{n}]次写入标签 [{tag.Lable}] 成功".LogProcess(logHeader);
            return ResultTypeEnum.OK;
         }
         else
         {
            $"{sendType}：{value}; 第[{n}]次写入标签 [{tag.Lable}] 失败".LogProcess(logHeader);
         }
      }
      return ResultTypeEnum.注液量发送失败;
   }
   #endregion

   #region PLC指定扫码通道是否有电池
   /// <summary>
   /// PLC指定扫码通道是否有电池,如果开户了验证，PLC未报到数据，应该直接停机
   /// </summary>
   /// <param name="isReadPlcSuccess">读取PLC数据是否成功</param>
   /// <param name="plcDatas">读取的PLC数据</param>
   /// <param name="logHeader"></param>
   /// <param name="laneHasBattery"></param>
   /// <returns></returns>
   protected bool HasBatteryInLane(bool isReadPlcSuccess, List<ReceivePlcDataModel> plcDatas, string logHeader, ref bool[] laneHasBattery)
   {
      if (!_parameterConfig.FunctionEnable.IsLaneHaveBattery) //未启用PLC指定是否有电池
         return true;

      if (!isReadPlcSuccess)
      {
         $"已启用PLC指定通道是否有电池，但PLC未给数据，不给PLC发结果及完成信号，请停机检查！;".LogProcess(
            logHeader,
            Log4NetLevelEnum.错误,
            true
         );
         return false;
      }
      for (int i = 0; i < laneHasBattery.Length; i++)
      {
         var plcData = plcDatas.FirstOrDefault(x => x.Index == i + 1);
         if (plcData == null)
            laneHasBattery[i] = false; //如果ID等于0,此处就会为null,代表该通道无电池，否则就有电池
      }
      return true;
   }
   #endregion
}
/// <summary>
