using Kinlo.Equipment.Devices.CardReaders;

namespace Kinlo.Common.Configurations;

/// <summary>
/// 刷卡验证器
/// </summary>
public partial class UsersStatusConfig
{
   /// <summary>
   /// 注册的刷卡 卡号长度 列表
   /// </summary>
   int[] _cardLengthList = [];

   [JsonIgnore]
   public Action<string>? CardAction { get; set; } = null;

   public void CardLogin(string cardNumber)
   {
      if (IsLogin)
      {
         _ = LocalLoginAsync(string.Empty, string.Empty, cardNumber, 0, LoginAccountTypeEnum.刷卡登陆);
      }
      else
      {
         CardAction?.Invoke(cardNumber);
      }
   }

   /// <summary>
   /// 注册 普通USB刷卡器 刷卡登陆
   /// </summary>
   public void RegisterUsbCardReader()
   {
      //卡号长度 从6注册31个，如果确定卡号长度也可指定长度注册
      _cardLengthList = Enumerable.Range(6, 22).ToArray();

      //注册刷卡登陆
      foreach (var item in _cardLengthList)
      {
         KeyboardHook.Hook.Register(item, barcode => CardLogin(barcode));
      }
      $"[普通USB刷卡器] 注册卡号长度{string.Join(',', _cardLengthList)}完成".LogRun();
   }

   /// <summary>
   /// 注销 普通USB刷卡器 刷卡登陆
   /// </summary>
   public void UnregisterUsbCardReader()
   {
      //注销刷卡登陆
      foreach (var item in _cardLengthList)
      {
         KeyboardHook.Hook.Unregister(item);
      }
      $"[普通USB刷卡器] 注销卡号长度{string.Join(',', _cardLengthList)}完成".LogRun();
   }

   /// <summary>
   /// 初始化hx540刷卡器
   /// </summary>
   /// <param name="devicesConfig"></param>
   private void InitializeHX540CardReader(DevicesConfig devicesConfig)
   {
      try
      {
         if (devicesConfig.DeviceList.FirstOrDefault(x => x.Communication == CommunicationEnum.HX540_H_E刷卡器 && x.IsEnable) != null)
         {
            var _card = new Kinlo.Equipment.Devices.CardReaders.HX540_H_E(GlobalStaticTemporary.GlobalToken);
            _card.Open();
            _card.CardAction = CardLogin;
         }
      }
      catch (Exception ex)
      {
         $"[初始化刷卡器]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   /// <summary>
   /// 初始化通用串口刷卡器
   /// </summary>
   /// <param name="devicesConfig"></param>
   private void InitializeGenericCardReaders(DevicesConfig devicesConfig)
   {
      try
      {
         var cards = devicesConfig.DeviceList.Where(x => x.Communication == CommunicationEnum.通用串口刷卡器 && x.IsEnable);
         if (cards.Any())
         {
            foreach (var card in cards)
            {
               $"[初始化通用串口刷卡器]开始!".LogRun(Log4NetLevelEnum.信息);
               if (card.TryCreateDevice(GlobalStaticTemporary.GlobalCancellationTokenSource, out var device))
               {
                  $"[初始化通用串口刷卡器]Com[{card.IPCOM}] Port[{card.Port}],成功!".LogRun(Log4NetLevelEnum.信息);
                  if (device is ICardReader<string> cardReader)
                  {
                     cardReader.CardAction = CardLogin;
                  }
               }
            }
         }
      }
      catch (Exception ex)
      {
         $"[初始化通用串口刷卡器]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

    /// <summary>
    /// 初始化一卡通读卡器
    /// </summary>
    /// <param name="devicesConfig"></param>
    private void InitializeLibCardFeeReader(DevicesConfig devicesConfig)
    {
        try
        {
            var cfg = devicesConfig.DeviceList.FirstOrDefault(
                x => x.Communication == CommunicationEnum.LibCardFee_一卡通读卡器 && x.IsEnable);

            if (cfg != null)
            {
                var reader = new CardReaderLibCardFee(GlobalStaticTemporary.GlobalToken, cfg.Port);
                reader.CardAction = CardLogin;
                reader.Open();
                $"[一卡通读卡器]初始化完成 串口:{cfg.Port}".LogRun(Log4NetLevelEnum.成功);
            }
        }
        catch (Exception ex)
        {
            $"[一卡通读卡器初始化异常] {ex}".LogRun(Log4NetLevelEnum.错误, true);
        }
    }
}
