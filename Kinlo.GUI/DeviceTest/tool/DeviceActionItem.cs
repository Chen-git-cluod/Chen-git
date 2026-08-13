using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Kinlo.GUI.DeviceTest.tool;

namespace Kinlo.GUI.DeviceTest
{
   public class DeviceActionItem : PropertyChangedBase
   {
      public string Label { get; set; } = "";
      public string Group { get; set; } = "";
      public string Icon { get; set; } = "";
      public ActionStyle ActionStyle { get; set; }
      public ActionType ActionType { get; set; }
      public int Order { get; set; }
      public string Placeholder { get; set; } = "";

      // ── 状态 ──────────────────────────────────────────
      private bool _isRunning;
      public bool IsRunning
      {
         get => _isRunning;
         set
         {
            SetAndNotify(ref _isRunning, value);
            NotifyOfPropertyChange(nameof(CanRun));
         }
      }

      // ── Write 类型的用户输入值 ──────────────────────────
      public string InputValue { get; set; } = "";

      // ── 结果输出 ───────────────────────────────────────
      private string _result = "";
      public string Result
      {
         get => _result;
         set
         {
            SetAndNotify(ref _result, value);
            NotifyOfPropertyChange(nameof(HasResult));
         }
      }

      private bool _isSuccess = true;
      public bool IsSuccess
      {
         get => _isSuccess;
         set => SetAndNotify(ref _isSuccess, value);
      }

      public bool HasResult => !string.IsNullOrEmpty(Result);
      public bool CanRun => !IsRunning;

      // ── 是否显示输入框 ─────────────────────────────────
      public bool NeedsInput => ActionType == ActionType.Write;

      // ── 委托（由 Scanner 注入） ────────────────────────
      /// <summary>Read/Action 类型：直接执行，返回结果字符串</summary>
      public Func<Task<DeviceActionResult>> Execute { get; set; } = () => Task.FromResult(DeviceActionResult.Ok());

      public Func<string, Task<DeviceActionResult>> ExecuteWithInput { get; set; } = _ => Task.FromResult(DeviceActionResult.Ok());

      // ── ICommand ──────────────────────────────────────
      public ICommand RunCommand { get; }

      public DeviceActionItem()
      {
         RunCommand = new AsyncRelayCommand(RunAsync);
      }

      // ✅ 执行完后通知外部显示结果面板
      public Action<string, string, bool>? OnResultReady { get; set; }

      private async Task RunAsync()
      {
         if (IsRunning)
            return;
         IsRunning = true;

         try
         {
            DeviceActionResult actionResult;

            if (ActionType == ActionType.Write)
            {
               if (string.IsNullOrWhiteSpace(InputValue))
               {
                  OnResultReady?.Invoke(Label, "请先输入值", false);
                  return;
               }
               actionResult = await ExecuteWithInput(InputValue);
            }
            else
            {
               actionResult = await Execute();
            }

            // ✅ 通知 VM 弹出侧边面板
            OnResultReady?.Invoke(Label, actionResult.Message, actionResult.IsSuccess);
         }
         catch (Exception ex)
         {
            OnResultReady?.Invoke(Label, $"异常：{ex.Message}", false);
         }
         finally
         {
            IsRunning = false;
         }
      }
   }

   public class DeviceActionGroup
   {
      public string GroupName { get; set; } = "";
      public List<DeviceActionItem> Actions { get; set; } = new();
   }
}
