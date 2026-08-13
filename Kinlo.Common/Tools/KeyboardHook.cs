namespace Kinlo.Common.Tools;

/// <summary>
/// 全局键盘钩子
/// </summary>
public sealed class KeyboardHook //sealed 此修饰符会阻止其他类从该类继承
{
   #region 单例模式
   private static readonly Lazy<KeyboardHook> lazy = new Lazy<KeyboardHook>(() => new KeyboardHook());
   public static KeyboardHook Hook
   {
      get { return lazy.Value; }
   }

   private KeyboardHook() { }
   #endregion

   #region 定义字段
   private bool _isStarted = false;
   private delegate int HookProc(int nCode, int wParam, IntPtr lParam);
   private HookProc _hookCallback;
   private static int _hookType = WH_KEYBOARD_LL; //全局钩子
   private static int _handleToHook = 0; //声明键盘钩子处理的初始值
   private static List<char> _list = new List<char>();
   private static List<ShortcutKeyModel> _ShortcutKeys = new List<ShortcutKeyModel>();

   private readonly object _lock = new();

   /// <summary>
   /// 当前处理器
   /// key：条码长度，不能为0
   /// </summary>
   private readonly Dictionary<int, Action<string>> _handlers = [];

   /// <summary>
   /// 独占模式备份
   /// </summary>
   private readonly Dictionary<int, Action<string>> _backupHandlers = [];

   /// <summary>
   /// 是否处于独占模式
   /// </summary>
   private bool _isExclusive;
   #endregion

   #region 系统用常量
   const int WH_MOUSE_LL = 14;
   const int WH_KEYBOARD_LL = 13;
   const int WH_MOUSE = 7;
   const int WH_KEYBOARD = 2;
   const int WM_MOUSEMOVE = 0x200;
   const int WM_LBUTTONDOWN = 0x201;
   const int WM_RBUTTONDOWN = 0x204;
   const int WM_MBUTTONDOWN = 0x207;
   const int WM_LBUTTONUP = 0x202;
   const int WM_RBUTTONUP = 0x205;
   const int WM_MBUTTONUP = 0x208;
   const int WM_LBUTTONDBLCLK = 0x203;
   const int WM_RBUTTONDBLCLK = 0x206;
   const int WM_MBUTTONDBLCLK = 0x209;
   const int WM_MOUSEWHEEL = 0x020A;
   const int WM_KEYDOWN = 0x100;
   const int WM_KEYUP = 0x101;
   const int WM_SYSKEYDOWN = 0x104;
   const int WM_SYSKEYUP = 0x105;
   const byte VK_SHIFT = 0x10;
   const byte VK_CAPITAL = 0x14;
   const byte VK_NUMLOCK = 0x90;
   const byte VK_LSHIFT = 0xA0;
   const byte VK_RSHIFT = 0xA1;
   const byte VK_LCONTROL = 0xA2;
   const byte VK_RCONTROL = 0x3;
   const byte VK_LALT = 0xA4;
   const byte VK_RALT = 0xA5;
   const byte LLKHF_ALTDOWN = 0x20;
   #endregion

   #region 输出方法
   /// <summary>
   /// 注册处理器，已存在则覆盖
   /// </summary>
   /// <param name="length">条码长度，0表示任意长度</param>
   /// <param name="handler">处理器</param>
   public void Register(int length, Action<string> handler)
   {
      // 等价于 throw new ArgumentNullException(nameof(handler));
      // handlero 为null时抛出 ArgumentNullException 异常
      ArgumentNullException.ThrowIfNull(handler);

      lock (_lock)
      {
         _handlers[length] = handler;
      }
   }

   /// <summary>
   /// 注销指定长度的处理器
   /// </summary>
   /// <param name="length">条码长度</param>
   public void Unregister(int length)
   {
      lock (_lock)
      {
         _handlers.Remove(length);
      }
   }

   /// <summary>
   /// 进入独占模式
   /// 当前所有处理器将被暂存，仅保留当前处理器
   /// </summary>
   /// <param name="length">条码长度，0表示任意长度</param>
   /// <param name="handler">处理器</param>
   public void RegisterExclusive(int length, Action<string> handler)
   {
      // 等价于 throw new ArgumentNullException(nameof(handler));
      // handlero 为null时抛出 ArgumentNullException 异常
      ArgumentNullException.ThrowIfNull(handler);

      lock (_lock)
      {
         if (_isExclusive)
         {
            var currentLength = _handlers.Count > 0 ? _handlers.Keys.First() : -1;
            throw new InvalidOperationException($"当前已处于{currentLength}独占模式。");
         }

         _backupHandlers.Clear();

         foreach (var item in _handlers)
         {
            _backupHandlers.Add(item.Key, item.Value);
         }

         _handlers.Clear();
         _handlers[length] = handler;

         _isExclusive = true;
      }
   }

   /// <summary>
   /// 退出独占模式并恢复之前的处理器
   /// </summary>
   public void UnregisterExclusive()
   {
      lock (_lock)
      {
         if (!_isExclusive)
         {
            return;
         }

         _handlers.Clear();

         foreach (var item in _backupHandlers)
         {
            _handlers.Add(item.Key, item.Value);
         }

         _backupHandlers.Clear();

         _isExclusive = false;
      }
   }

   /// <summary>
   /// 清空所有处理器
   /// </summary>
   public void Clear()
   {
      lock (_lock)
      {
         _handlers.Clear();
         _backupHandlers.Clear();
         _isExclusive = false;
      }
   }

   /// <summary>
   /// 快捷键
   /// </summary>
   /// <param name="shortcutKey"></param>
   public void RegistrationListening(ShortcutKeyModel shortcutKey) => _ShortcutKeys.Add(shortcutKey);

   #endregion

   #region 引用DLL
   //设置钩子
   [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
   private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

   //卸载钩子
   [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
   private static extern bool UnhookWindowsHookEx(int idHook);

   //继续下个钩子
   [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
   private static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

   //ToAscii职能的转换指定的虚拟键码和键盘状态的相应字符或字符
   [DllImport("user32", EntryPoint = "ToAscii")]
   private static extern bool ToAscii(int VirtualKey, int ScanCode, byte[] lpKeySate, ref uint lpChar, int uFlags);

   //用于判断vKey的状态。用返回值的最高位表示，最高位为1表示当前键处于down的状态；最高位为0当前键处于up状态,是从windows消息队列中取得键盘消息，返回key status.
   [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
   static extern short GetKeyState(int vKey);

   //获得所有的256个键（键盘按键、鼠标按键等等）的状态,当从windows消息队列中移除键盘消息时，才返回key status
   [DllImport("user32", EntryPoint = "GetKeyboardState")]
   private static extern int GetKeyboardState(byte[] pbKeyState);

   //根据键盘消息获取按键名称
   [DllImport("user32", EntryPoint = "GetKeyNameText")]
   private static extern int GetKeyNameText(int IParam, StringBuilder lpBuffer, int nSize);

   //使用WINDOWS API函数代替获取当前实例的函数,防止钩子失效
   [DllImport("kernel32.dll")]
   public static extern IntPtr GetModuleHandle(string name);
   #endregion

   #region Method
   public bool Start()
   {
      try
      {
         if (_handleToHook == 0)
         {
            _hookCallback = new HookProc(HookCallbackProcedure);
            _handleToHook = SetWindowsHookEx(
               _hookType, //全局钩子
               _hookCallback,
               GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName),
               0
            );
            //GetModuleHandle 函数 替代 Marshal.GetHINSTANCE
            //防止在 framework4.0中 注册钩子不成功
            //_handleToHook = SetWindowsHookEx(13, hookproc, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
         }
      }
      catch (Exception ex)
      {
         $"[HOOK] 启动监听异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         return false;
      }
      if (_handleToHook != 0 && _handleToHook != -1)
      {
         $"[HOOK] 启动监听成功".LogRun();
         _isStarted = true;
         Polling();
         return true;
      }
      else
      {
         $"[HOOK] 输入监听启动失败,请联系工程师！！！".LogRun(Log4NetLevelEnum.错误);
         return false;
      }
   }

   public bool Stop()
   {
      bool retKeyboard = true;
      try
      {
         if (_handleToHook != 0)
         {
            retKeyboard = UnhookWindowsHookEx(_handleToHook);
            _handleToHook = 0;
            if (retKeyboard)
            {
               // _stopwatch.Stop();
               _isStarted = false;
               $"[HOOK] 停止成功！".LogRun();
            }
            else
               $"[HOOK] 停止失败！".LogRun(Log4NetLevelEnum.错误, true);
         }
      }
      catch (Exception ex)
      {
         $"[HOOK] 停止异常:{ex.Message}".LogRun(Log4NetLevelEnum.错误, true);
      }
      return retKeyboard;
   }

   /// <summary>
   /// 处理
   /// </summary>
   /// <param name="nCode"></param>
   /// <param name="wParam"></param>
   /// <param name="lParam"></param>
   /// <returns></returns>
   static int HookCallbackProcedure(int nCode, Int32 wParam, IntPtr lParam)
   {
      if (lParam == IntPtr.Zero)
      {
         $"[HOOK] 失败，IntPtr(指针句柄)为{lParam}！".LogRun(Log4NetLevelEnum.错误, true);
         return -1;
      }
      if (nCode > -1)
      {
         KeyboardHookStruct? keyboardHookStruct = Marshal.PtrToStructure<KeyboardHookStruct>(lParam);
         if (keyboardHookStruct == null)
         {
            $"[HOOK] 失败，keyboardHookStruct为null ！".LogRun(Log4NetLevelEnum.错误, true);
            return -1;
         }
         if (CharDic.TryGetValue(keyboardHookStruct.vkCode, out char _char))
         {
            switch (wParam)
            {
               case WM_KEYDOWN:
                  _list.Add(_char);

                  bool _isCapslock = GetKeyState(VK_CAPITAL) != 0; //按下大写键，暂时未用；
                  bool _isControl = ((GetKeyState(VK_LCONTROL) & 0x80) != 0) || ((GetKeyState(VK_RCONTROL) & 0x80) != 0);
                  bool _isShift = ((GetKeyState(VK_LSHIFT) & 0x80) != 0) || ((GetKeyState(VK_RSHIFT) & 0x80) != 0);
                  bool _isAlt = ((GetKeyState(VK_LALT) & 0x80) != 0) || ((GetKeyState(VK_RALT) & 0x80) != 0);

                  var model = _ShortcutKeys.FirstOrDefault(x =>
                     x.IsAlt == _isAlt && x.IsCrtl == _isControl && x.IsShift == _isShift && x.Key == _char
                  );
                  if (model != null)
                  {
                     model.Action?.Invoke();
                  }
                  break;
            }
         }
      }
      //  _stopwatch.Restart();
      return CallNextHookEx(_handleToHook, nCode, wParam, lParam);
   }

   public static int _intervalTime = 100; // 指定输入间隔为100毫秒以内时为连续输入

   private void Polling()
   {
      Task.Run(() =>
      {
         while (_isStarted)
         {
            int _key = _list.Count;
            Thread.Sleep(_intervalTime);
            if (_list.Count == _key)
            {
               try
               {
                  if (_key > 9)
                  {
                     string _resultCode = string.Join(null, _list);
                     //0 为大于9的任一长度
                     if (_handlers.TryGetValue(0, out var handler))
                        handler?.Invoke(_resultCode);

                     if (_handlers.TryGetValue(_key, out var keyHandler))
                        keyHandler?.Invoke(_resultCode);
                  }
               }
               catch (Exception ex)
               {
                  $"[HOOK] 输入触发异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
               }
               finally
               {
                  _list.Clear();
               }
            }
            Thread.Sleep(30);
         }
      });
   }
   #endregion

   #region 结构体
   [StructLayout(LayoutKind.Sequential)]
   private class KeyboardHookStruct
   {
      public int vkCode;
      public int scanCode;
      public int flags;
      public int time;
      public int dwExtraInfo;
   }
   #endregion

   #region 虚拟键盘
   static Dictionary<int, char> CharDic = new Dictionary<int, char>
   {
      { 0x20, ' ' },
      { 0x30, '0' },
      { 0x31, '1' },
      { 0x32, '2' },
      { 0x33, '3' },
      { 0x34, '4' },
      { 0x35, '5' },
      { 0x36, '6' },
      { 0x37, '7' },
      { 0x38, '8' },
      { 0x39, '9' },
      { 0x41, 'A' },
      { 0x42, 'B' },
      { 0x43, 'C' },
      { 0x44, 'D' },
      { 0x45, 'E' },
      { 0x46, 'F' },
      { 0x47, 'G' },
      { 0x48, 'H' },
      { 0x49, 'I' },
      { 0x4A, 'J' },
      { 0x4B, 'K' },
      { 0x4C, 'L' },
      { 0x4D, 'M' },
      { 0x4E, 'N' },
      { 0x4F, 'O' },
      { 0x50, 'P' },
      { 0x51, 'Q' },
      { 0x52, 'R' },
      { 0x53, 'S' },
      { 0x54, 'T' },
      { 0x55, 'U' },
      { 0x56, 'V' },
      { 0x57, 'W' },
      { 0x58, 'X' },
      { 0x59, 'Y' },
      { 0x5A, 'Z' },
      { 0x60, '0' },
      { 0x61, '1' },
      { 0x62, '2' },
      { 0x63, '3' },
      { 0x64, '4' },
      { 0x65, '5' },
      { 0x66, '6' },
      { 0x67, '7' },
      { 0x68, '8' },
      { 0x69, '9' },
      { 0x6A, '*' },
      { 0x6B, '+' },
      { 0x6D, '-' },
      { 0x6E, '.' },
      { 0x6F, '/' },
      { 0xBA, ';' },
      { 0xBB, '+' },
      { 0xBC, ',' },
      { 0xBD, '-' },
      { 0xBE, '.' },
   };
   #endregion
}
