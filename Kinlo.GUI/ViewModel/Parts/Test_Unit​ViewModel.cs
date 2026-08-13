using System.Collections.Concurrent;
using System.Linq;
using HandyControl.Controls;
using Kinlo.Equipment.Devices.ShortCircuitTester;
using Kinlo.Equipment.Interfaces;
using Kinlo.GUI.DeviceTest;
using Kinlo.GUI.DeviceTest.Commands;
using Kinlo.GUI.DeviceTest.Helpers;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.Streaming.Values;

namespace Kinlo.GUI.ViewModel;

public class Test_UnitViewModel : Screen
{
   // ── 静态缓存，生命周期跟随整个应用，跨页面复用 ──
   private static readonly ConcurrentDictionary<string, Test_UnitViewModel> _cache = new();
   private static bool _isPreloaded = false;
   private static readonly SemaphoreSlim _preloadLock = new(1, 1);

   public string? ReadCode { get; set; } = string.Empty;
   public string WriteCode { get; set; } = string.Empty;
   public DeviceClientModel Device { get; set; }
   public BindableCollection<DeviceActionGroup> ActionGroups { get; set; } = new();

   private readonly ParameterConfig _parameterConfig;
   private readonly object? _command;
   private readonly IContainer _container;

   public Test_UnitViewModel(IContainer container, DeviceClientModel device)
   {
      _container = container;
      Device = device;
      _parameterConfig = container.Get<ParameterConfig>();
      _command = DeviceCommandFactory.GetCommand(device);
      BuildActionGroups();
   }

   // ── 侧边结果面板 ──────────────────────────────────
   private bool _isResultPanelOpen;
   public bool IsResultPanelOpen
   {
      get => _isResultPanelOpen;
      set => SetAndNotify(ref _isResultPanelOpen, value);
   }

   private string _panelResult = "";
   public string PanelResult
   {
      get => _panelResult;
      set => SetAndNotify(ref _panelResult, value);
   }

   private bool _panelIsSuccess = true;
   public bool PanelIsSuccess
   {
      get => _panelIsSuccess;
      set => SetAndNotify(ref _panelIsSuccess, value);
   }

   private string _panelLabel = "";
   public string PanelLabel
   {
      get => _panelLabel;
      set => SetAndNotify(ref _panelLabel, value);
   }

   public void CloseResultPanelCMD() => IsResultPanelOpen = false;

   public void CopyPanelResultCMD()
   {
      if (!string.IsNullOrEmpty(PanelResult))
         Clipboard.SetText(PanelResult);
   }

   // ── 构建动作组时注入回调 ──────────────────────────
   private void BuildActionGroups()
   {
      if (_command == null)
         return;
      var groups = DeviceActionScanner.Scan(_command, Device, _parameterConfig);

      // ✅ 每个 ActionItem 执行完后回调到 VM 显示侧边面板
      foreach (var group in groups)
      foreach (var action in group.Actions)
         action.OnResultReady = ShowResultPanel;

      ActionGroups = new BindableCollection<DeviceActionGroup>(groups);
   }

   private void ShowResultPanel(string label, string result, bool isSuccess)
   {
      PanelLabel = label;
      PanelResult = result;
      PanelIsSuccess = isSuccess;
      IsResultPanelOpen = true;
   }

   // ══════════════════════════════════════════════════
   //  静态工厂方法：取缓存 或 新建
   // ══════════════════════════════════════════════════

   /// <summary>
   /// 外部统一通过此方法获取 VM，命中缓存直接返回
   /// </summary>
   public static Test_UnitViewModel GetOrCreate(IContainer container, DeviceClientModel device)
   {
      var key = GetKey(device);
      return _cache.GetOrAdd(key, _ => new Test_UnitViewModel(container, device));
   }

   // ══════════════════════════════════════════════════
   //  静态预加载：传入设备列表，后台全部预热
   // ══════════════════════════════════════════════════

   /// <summary>
   /// 在任意时机调用，传入全量设备列表，后台预加载所有有测试类的设备
   /// 多次调用安全，只执行一次
   /// </summary>
   public static async Task PreloadAllAsync(IContainer container, IEnumerable<DeviceClientModel> devices)
   {
      if (_isPreloaded)
         return;

      await _preloadLock.WaitAsync();
      try
      {
         if (_isPreloaded)
            return;

         await Task.Run(() =>
         {
            foreach (var device in devices)
            {
               // 没有对应测试类就跳过
               if (DeviceCommandFactory.GetCommand(device) == null)
                  continue;

               var key = GetKey(device);
               _cache.GetOrAdd(
                  key,
                  _ =>
                  {
                     try
                     {
                        return new Test_UnitViewModel(container, device);
                     }
                     catch (Exception ex)
                     {
                        $"预加载 [{key}] 失败：{ex.Message}".LogRun(Log4NetLevelEnum.警告);
                        return null!;
                     }
                  }
               );
            }
         });

         _isPreloaded = true;
      }
      finally
      {
         _preloadLock.Release();
      }
   }

   /// <summary>
   /// 新增单个设备时调用，追加预加载
   /// </summary>
   public static void PreloadOne(IContainer container, DeviceClientModel device)
   {
      if (DeviceCommandFactory.GetCommand(device) == null)
         return;

      var key = GetKey(device);
      _cache.GetOrAdd(
         key,
         _ =>
         {
            try
            {
               return new Test_UnitViewModel(container, device);
            }
            catch
            {
               return null!;
            }
         }
      );
   }

   /// <summary>
   /// 设备删除时清理缓存
   /// </summary>
   public static void RemoveCache(DeviceClientModel device) => _cache.TryRemove(GetKey(device), out _);

   /// <summary>
   /// 判断设备是否有测试页面
   /// </summary>
   public static bool HasTestSupport(DeviceClientModel device) => DeviceCommandFactory.GetCommand(device) != null;

   private static string GetKey(DeviceClientModel device) => $"{device.ProcessesType}_{device.Index}_{device.Communication}";
}


//internal class Test_UnitViewModel : Screen
//{
//    // ── 原有属性 ────────────────────────────────────────
//    public string? ReadCode { get; set; } = string.Empty;
//    public string WriteCode { get; set; } = string.Empty;
//    public DeviceClientModel Device { get; set; }

//    // ── 动态动作组 ──────────────────────────────────────
//    public BindableCollection<DeviceActionGroup> ActionGroups { get; set; } = new();

//    // ── 结果输出（所有动作共用一个输出区） ──────────────
//    public string ResultText { get; set; } = "";

//    private readonly ParameterConfig _parameterConfig;
//    private readonly object? _command;   // ✅ 改成 object

//    private Dialog? _dialog;

//    public Test_UnitViewModel(IContainer container, DeviceClientModel device)
//    {
//        Device = device;
//        _parameterConfig = container.Get<ParameterConfig>();
//        _command = DeviceCommandFactory.GetCommand(device);

//        BuildActionGroups();
//    }

//    private void BuildActionGroups()
//    {
//        if (_command == null) return;
//        var groups = DeviceActionScanner.Scan(_command, Device, _parameterConfig);
//        ActionGroups = new BindableCollection<DeviceActionGroup>(groups);
//    }

//    /// <summary>动态按钮点击入口（XAML CommandParameter 传入 DeviceActionItem）</summary>
//    public async Task ExecuteActionCMD(DeviceActionItem action)
//    {
//        if (action.IsRunning) return;

//        action.IsRunning = true;
//        _dialog = Dialog.Show(GenericHelper.CreateLoadingCircle(), "DialogQuery");

//        try
//        {
//            var sw = Stopwatch.StartNew();
//            await action.Execute();
//            ResultText = $"[{action.Label}] 执行完成，耗时 {sw.ElapsedMilliseconds} ms";
//        }
//        catch (NotSupportedException ex)
//        {
//            Growl.Warning(ex.Message);
//        }
//        catch (Exception ex)
//        {
//            Growl.Warning(ex.Message);
//            $"[{action.Label}] 异常：{ex}".LogRun();
//        }
//        finally
//        {
//            action.IsRunning = false;
//            Application.Current.Dispatcher.Invoke(() => _dialog?.Close());
//        }
//    }

//    // ── 原有读写保留兼容 ─────────────────────────────────
//    public async Task ReadCmd() { /* 原逻辑不动 */ }
//    public async Task WriteCmd() { /* 原逻辑不动 */ }
//}

//internal class Test_UnitViewModel : Screen
//{
//    public string? ReadCode { get; set; } = string.Empty;
//    public string WriteCode { get; set; } = string.Empty;
//    public DeviceClientModel Device { get; set; }
//    private ParameterConfig _parameterConfig { get; set; }
//    private Dialog _dialog;

//    public Test_UnitViewModel(IContainer container, DeviceClientModel device)
//    {
//        Device = device;
//        _parameterConfig = container.Get<ParameterConfig>();
//    }

//    public async Task ReadCmd()
//    {
//        _dialog = Dialog.Show(GenericHelper.CreateLoadingCircle(), "DialogQuery");
//        try
//        {
//            var stopwatch = Stopwatch.StartNew();

//            var command = DeviceCommandFactory.GetCommand(Device);
//            if(command == null)
//            {
//                var tip = $"{Device.ProcessesType}没找到测试读取类";
//                tip.LogRun();
//                Growl.Warning(tip);
//                return;
//            }
//            string result = await command.ReadAsync(Device, _parameterConfig);

//            Application.Current.Dispatcher.Invoke(() => ReadCode = result);

//            $"本次读取使用时间：{stopwatch.ElapsedMilliseconds}".LogRun();
//        }
//        catch (Exception ex)
//        {
//            Growl.Warning(ex.Message);
//        }
//        finally
//        {
//            Application.Current.Dispatcher.Invoke(() => _dialog.Close());
//        }
//    }

//    public async Task WriteCmd()
//    {
//        try
//        {
//            var command = DeviceCommandFactory.GetCommand(Device);
//            if (command == null)
//            {
//                $"{Device.ProcessesType}没找到测试写入类".LogRun();
//                return;
//            }

//            bool success = await command.WriteAsync(Device, _parameterConfig, WriteCode);
//            Growl.Warning(success ? "写入成功" : "写入失败");
//        }
//        catch (NotSupportedException ex)
//        {
//            Growl.Warning(ex.Message);
//        }
//        catch (Exception ex)
//        {
//            Growl.Warning(ex.Message);
//        }
//    }
//}
