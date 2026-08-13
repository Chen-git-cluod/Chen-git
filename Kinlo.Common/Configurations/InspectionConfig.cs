using System.Threading.Tasks;

namespace Kinlo.Common.Configurations;

/// <summary>
/// 点检相关设置
/// </summary>
public class InspectionConfig : ConfigurationBase
{
   /// <summary>
   /// 弹出点检窗口委托
   /// </summary>
   public event Action<ProcessTypeEnum>? ShowInspectionWindow;

   /// <summary>
   /// 弹出点检窗口
   /// </summary>
   /// <param name="process"></param>
   public async Task OnShowInspectionWindow(ProcessTypeEnum process) =>
      await UIThreadHelper.InvokeOnUiThreadAsync(() => ShowInspectionWindow?.Invoke(process));

   public InspectionItem BeforeScanBarcodeParameter { get; set; }
   public InspectionItem AfterScanBarcodeParameter { get; set; }
   public InspectionItem ShortCircuitParameter { get; set; }
   public InspectionItem TestVoltageParameter { get; set; }
   public InspectionItem BeforeWeighParameter { get; set; }
   public InspectionItem AfterWeighParameter { get; set; }
   IContainer _container;

   public InspectionConfig(IContainer container, bool isStartup)
      : base(container, isStartup)
   {
      _container = container;
   }

   public override void Load()
   {
      var json = FileHelper.LoadToString(this.GetType().Name);
      if (!string.IsNullOrEmpty(json))
      {
         json.ParseJson(root =>
         {
            JsonElement element;
            if (root.TryGetProperty(nameof(BeforeScanBarcodeParameter), out element))
               BeforeScanBarcodeParameter = element.Deserialize<InspectionItem>();

            if (root.TryGetProperty(nameof(AfterScanBarcodeParameter), out element))
               AfterScanBarcodeParameter = element.Deserialize<InspectionItem>();

            if (root.TryGetProperty(nameof(ShortCircuitParameter), out element))
               ShortCircuitParameter = element.Deserialize<InspectionItem>();

            if (root.TryGetProperty(nameof(TestVoltageParameter), out element))
               TestVoltageParameter = element.Deserialize<InspectionItem>();

            if (root.TryGetProperty(nameof(BeforeWeighParameter), out element))
               BeforeWeighParameter = element.Deserialize<InspectionItem>();

            if (root.TryGetProperty(nameof(AfterWeighParameter), out element))
               AfterWeighParameter = element.Deserialize<InspectionItem>();

            return true;
         });
      }
      if (BeforeScanBarcodeParameter == null || !BeforeScanBarcodeParameter.Lanes.Any())
         BeforeScanBarcodeParameter = InspectionItem.Init(ProcessTypeEnum.前扫码);

      if (AfterScanBarcodeParameter == null || !AfterScanBarcodeParameter.Lanes.Any())
         AfterScanBarcodeParameter = InspectionItem.Init(ProcessTypeEnum.后扫码);

      if (ShortCircuitParameter == null || !ShortCircuitParameter.Lanes.Any())
         ShortCircuitParameter = InspectionItem.Init(ProcessTypeEnum.测短路);

      if (TestVoltageParameter == null || !TestVoltageParameter.Lanes.Any())
         TestVoltageParameter = InspectionItem.Init(ProcessTypeEnum.测电压);

      if (BeforeWeighParameter == null || !BeforeWeighParameter.Lanes.Any())
         BeforeWeighParameter = InspectionItem.Init(ProcessTypeEnum.前称重);

      if (AfterWeighParameter == null || !AfterWeighParameter.Lanes.Any())
         AfterWeighParameter = InspectionItem.Init(ProcessTypeEnum.后称重);
   }
}

[AddINotifyPropertyChangedInterface]
public class InspectionItem
{
   public ProcessTypeEnum Process { get; set; }
   public Visibility Visible { get; set; } = Visibility.Visible;

   /// <summary>
   /// 通道数设置
   /// </summary>
   public int SetLaneCount { get; set; } = 4;

   /// <summary>
   /// 一对几（如扫码枪可以一扫2）
   /// </summary>
   public int Count { get; set; } = 1;

   /// <summary>
   /// 各通道
   /// </summary>
   public ObservableRangeCollection<InspectionLaneModel> Lanes { get; set; } = new();

   public static InspectionItem Init(ProcessTypeEnum process, int length = 4)
   {
      var entity = new InspectionItem();
      entity.Process = process;
      entity.Lanes.Clear();
      for (int i = 0; i < length; i++)
      {
         entity.Lanes.Add(new InspectionLaneModel() { Index = i + 1 });
      }
      return entity;
   }
}

/// <summary>
///
/// </summary>
[AddINotifyPropertyChangedInterface]
public class InspectionLaneModel
{
   public int Index { get; set; }

   public string CurrentValue { get; set; } = string.Empty;

   /// <summary>
   /// 目标值
   /// </summary>
   public string TragetValue { get; set; } = string.Empty;

   /// <summary>
   /// 上限
   /// </summary>
   public double Upper { get; set; }

   /// <summary>
   /// 下限
   /// </summary>
   public double Lower { get; set; }

   public bool IsSuccess { get; set; }

   public DateTime UpdateTime { get; set; } = DateTime.MinValue;
}
