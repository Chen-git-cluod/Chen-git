namespace Kinlo.Equipment.Devices.CardReaders;

public class CardReaderLibCardFee
{
    public Action<string>? CardAction { get; set; }

    CancellationToken _cancellationToken;
    private bool _sdkInited;
    private readonly int _comPort;
    private const string Dll = "LibCardFee.dll";

    // 初始化SDK，回调与用户对象传NULL
    [DllImport(Dll, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern void LibCardFee_Init(IntPtr lpCallBack, IntPtr pUser);
    // 释放SDK资源
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern void LibCardFee_Uninit();
    // 打开指定串口 nSerial：串口号
    [DllImport(Dll, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int LibCardFee_DB_OnOpen(int nSerial);
    // 关闭串口
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern void LibCardFee_DB_OnClose();
    // 读取卡片用户信息，输出卡号、卡索引、卡计数
    [DllImport(Dll, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int LibCardFee_DB_OnReadCustomerCard(StringBuilder szCustomerCardCode, out int nCustomrCardIndex, out int nCustomerCardSN);
    // 根据错误码获取文字描述
    [DllImport(Dll, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int LibCardFee_GetErrString(int nErrCode, StringBuilder szRetErrString);
    public CardReaderLibCardFee(CancellationToken token, int comPort)
    {
        _cancellationToken = token;
        _comPort = comPort;
    }

    public void Open()
    {
        try
        {
            LibCardFee_Init(IntPtr.Zero, IntPtr.Zero);
            _sdkInited = true;

            int openRet = LibCardFee_DB_OnOpen(_comPort);
            if (openRet != 0)
            {
                var errSb = new StringBuilder(256);
                LibCardFee_GetErrString(openRet, errSb);
                $"[一卡通读卡器]串口打开失败 Err={openRet} Msg={errSb}".LogRun(Log4NetLevelEnum.错误);
                return;
            }

            $"[一卡通读卡器]打开成功 Com{_comPort}".LogRun(Log4NetLevelEnum.成功);
            ThreadPool.QueueUserWorkItem(ReadLoop);
        }
        catch (Exception ex)
        {
            $"[一卡通读卡器]Open异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
        }
    }

    private void ReadLoop(object? state)
    {
        var cardBuf = new StringBuilder(64);
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                cardBuf.Clear();
                int ret = LibCardFee_DB_OnReadCustomerCard(cardBuf, out _, out _);
                if (ret == 0 && cardBuf.Length > 0)
                {
                    string cardId = cardBuf.ToString().Trim();
                    $"[一卡通读卡器]读到卡片ID：{cardId}".LogRun(Log4NetLevelEnum.信息);
                    CardAction?.Invoke(cardId);
                }
                Thread.Sleep(200);
            }
            catch (Exception ex)
            {
                $"[一卡通读卡器]读卡循环异常：{ex}".LogRun(Log4NetLevelEnum.错误);
                Thread.Sleep(500);
            }
        }
        LibCardFee_DB_OnClose();
    }

    public void Close()
    {
        try
        {
            LibCardFee_DB_OnClose();
            if (_sdkInited)
            {
                LibCardFee_Uninit();
                _sdkInited = false;
                "[一卡通读卡器]SDK卸载完成".LogRun();
            }
        }
        catch (Exception ex)
        {
            $"[一卡通读卡器]Close异常：{ex}".LogRun(Log4NetLevelEnum.错误);
        }
    }
}