using HandyControl.Controls;

namespace Kinlo.GUI.View;

/// <summary>
/// PlcTestView.xaml 的交互逻辑
/// </summary>
public partial class PlcTestView : System.Windows.Window
{
   public ObservableCollection<PlcTestModel> PlcTests { get; set; } = new();
   public ObservableCollection<PlcDataTypeEnum> PlcDataTypes
   {
      get => new ObservableCollection<PlcDataTypeEnum>(Enum.GetValues(typeof(PlcDataTypeEnum)).Cast<PlcDataTypeEnum>());
   }
   IContainer _container;

   public PlcTestView(IContainer container, ObservableCollection<PLCInteractAddressModel> plcInteractAddresses)
   {
      InitializeComponent();
      this.DataContext = this;
      _container = container;
      foreach (var item in plcInteractAddresses)
      {
         PlcTestModel plcTest = new PlcTestModel
         {
            ServiceName = item.ServiceName,
            PlcDataType = PlcDataTypeEnum.Short,
            WriteValue = "1",
            StartCommand = new GenericCommandModel
            {
               Index = item.StartCommand.Index,
               IsEnabled = item.StartCommand.IsEnabled,
               IsExcision = item.StartCommand.IsExcision,
               Tag = new SignalAddressModel { Address = item.StartCommand.Tag.Address, Lable = item.StartCommand.Tag.Lable },
            },
            ProcessesType = item.ProcessesType,
            DeviceCommunicationType = item.DeviceCommunicationType,
         };
         PlcTests.Add(plcTest);
      }
   }

   private void Write_Click(object sender, RoutedEventArgs e)
   {
      try
      {
         var dataContext = ((Button)sender).DataContext as PlcTestModel;

         object writeVal = dataContext.PlcDataType switch
         {
            PlcDataTypeEnum.Short => (object)Int16.Parse(dataContext.WriteValue),
            PlcDataTypeEnum.Long => (object)long.Parse(dataContext.WriteValue),
            PlcDataTypeEnum.Float => (object)float.Parse(dataContext.WriteValue),
            _ => (object)int.Parse(dataContext.WriteValue),
         };

         var device = _container
            .Get<DevicesConfig>()
            .DeviceList.FirstOrDefault(x => x.ServiceName == dataContext.ServiceName && x.ProcessesType == ProcessTypeEnum.PLC);
         Task.Run(async () =>
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(async () =>
            {
               await device.WithCreatedDeviceAsync(async d =>
                  await Task.Run(() =>
                  {
                     if (d.WriteValue(writeVal, dataContext.StartCommand.Tag, "[Test]"))
                        Growl.Success("写入成功！");
                     else
                        Growl.Warning("写入失败！");
                  })
               );
            });
         });
      }
      catch (Exception ex)
      {
         Growl.Warning($"写入异常：{ex}！");
      }
   }

   private void Read_Click(object sender, RoutedEventArgs e)
   {
      try
      {
         Task.Run(async () =>
         {
            await UIThreadHelper.InvokeOnUiThreadAsync(async () =>
            {
               var dataContext = ((Button)sender).DataContext as PlcTestModel;
               var device = _container
                  .Get<DevicesConfig>()
                  .DeviceList.FirstOrDefault(x => x.ServiceName == dataContext.ServiceName && x.ProcessesType == ProcessTypeEnum.PLC);
               await device.WithCreatedDeviceAsync(async d =>
                  await Task.Run(() =>
                     dataContext.ReadValue = dataContext.PlcDataType switch
                     {
                        PlcDataTypeEnum.Short => d.ReadValue<short>(dataContext.StartCommand.Tag, "[Test]"),
                        PlcDataTypeEnum.Long => d.ReadValue<long>(dataContext.StartCommand.Tag, "[Test]"),
                        PlcDataTypeEnum.Float => d.ReadValue<float>(dataContext.StartCommand.Tag, "[Test]"),
                        _ => d.ReadValue<int>(dataContext.StartCommand.Tag, "[Test]"),
                     }
                  )
               );
            });
         });
      }
      catch (Exception ex)
      {
         Growl.Warning(ex.Message);
      }
   }
}

[AddINotifyPropertyChangedInterface]
public class PlcTestModel
{
   /// <summary>
   /// 服务名
   /// </summary>
   public string ServiceName { get; set; } = string.Empty;

   /// <summary>
   ///  启动命令
   /// </summary>
   public GenericCommandModel StartCommand { get; set; } = new();

   /// <summary>
   ///  工序类型
   /// </summary>
   public ProcessTypeEnum ProcessesType { get; set; }

   /// <summary>
   /// 设备通信类型
   /// </summary>
   public CommunicationEnum DeviceCommunicationType { get; set; }

   public PlcDataTypeEnum PlcDataType { get; set; } = PlcDataTypeEnum.Short;
   public object ReadValue { get; set; } = string.Empty;
   public string WriteValue { get; set; } = string.Empty;
}

public enum PlcDataTypeEnum
{
   Short,
   Int,
   Float,
   Long,
}
