namespace Kinlo.Equipment.Devices.CardReaders;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public class HX540_H_E
{
   IntPtr HIDHandle;
   int HIDAddress;

   /// <summary>
   /// 读卡器委托
   /// </summary>
   public Action<string>? CardAction { get; set; } = null;
   CancellationToken _cancellationToken;

   public HX540_H_E(CancellationToken token)
   {
      _cancellationToken = token;
      HIDHandle = IntPtr.Zero;
      HIDAddress = 0;
   }

   public void Open()
   {
      int device_num = 0;
      int ret = GetHIDDevice(ref device_num);
      if (ret == 0)
      {
         ret = OpenHIDDevice(device_num - 1);
         if (ret == 0)
         {
            if (IntPtr.Zero == GetHIDHandle())
            {
               "刷卡器 刷卡器故障！！！".LogRun(Log4NetLevelEnum.错误);
            }
            else
            {
               "刷卡器打开成功...".LogRun(Log4NetLevelEnum.成功);
               ThreadPool.QueueUserWorkItem(
                  new WaitCallback(x =>
                  {
                     string strTmp = default;
                     byte[] buf = new byte[128];
                     //byte[] key;
                     byte flag = 2;
                     while (!_cancellationToken.IsCancellationRequested)
                     {
                        // key = CPublic.CharToByte("FF FF FF FF FF FF");
                        //int ret = _hX540_H_E.HID_Read_14443A(0, 0, 1, key, buf);
                        int ret = HID_GetSerialNum_14443A(0, 0, ref flag, buf);
                        if (ret == 0)
                        {
                           strTmp = default;
                           for (int i = 1; i < buf[0] + 1; i++)
                           {
                              //卡数据
                              strTmp += string.Format("{0:X2}", buf[i]);
                           }
                           CardAction?.Invoke(strTmp);
                           $"读取到的卡数据：{strTmp}".LogRun(Log4NetLevelEnum.信息);
                           SetHIDBuzzer(0x03, 0x01, ref buf[0]);
                        }
                        Thread.Sleep(200);
                     }
                     CloseHIDPort();
                  })
               );
            }
         }
         else if (ret == -1)
         {
            "刷卡器 找不到HID设备".LogRun(Log4NetLevelEnum.错误);
         }
         else
         {
            "刷卡器 未知误差".LogRun(Log4NetLevelEnum.错误);
         }
      }
      else if (ret == -1)
      {
         "刷卡器 找不到HID设备".LogRun(Log4NetLevelEnum.错误);
      }
      else
      {
         "刷卡器 未知误差".LogRun(Log4NetLevelEnum.错误);
      }
   }

   public IntPtr GetHIDHandle()
   {
      return HIDHandle;
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBGetConnectedDeviceNum")]
   static extern int API_USBGetConnectedDeviceNum();

   public int GetHIDDevice(ref int device_num)
   {
      device_num = API_USBGetConnectedDeviceNum();
      if (device_num > 0)
         return 0; //有设备
      else
         return -1; //没有设备
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBOpenWithNum")]
   static extern int API_USBOpenWithNum(ref IntPtr HIDHandle, int DeviceIndex, short NumInputBuffers);

   public int OpenHIDDevice(int DeviceIndex)
   {
      int status = API_USBOpenWithNum(ref HIDHandle, DeviceIndex, 0x40);
      if (status == 0)
      {
         return 0; //成功
      }
      else if (status == 1)
      {
         return -1; //没有找到设备
      }
      else
      {
         return -2; //未知错误
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBCloseComm")]
   static extern bool API_USBCloseComm(IntPtr HidPortHandle);

   public int CloseHIDPort()
   {
      if (IntPtr.Zero != HIDHandle)
      {
         bool status = API_USBCloseComm(HIDHandle);
         if (status)
         {
            HIDHandle = IntPtr.Zero;
            return 0; //操作成功
         }
         else
         {
            return -2;
            ///关闭不成功
         }
      }
      else
      {
         return -1; //前一次设备打开不成功
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBGetSerNum")]
   static extern int API_USBGetSerNum(IntPtr HIDHandle, int DeviceAddress, byte[] buf);

   public int GetHIDSerNum(int DeviceAddress, byte[] buf)
   {
      if (IntPtr.Zero == HIDHandle)
         return -1;
      HIDAddress = DeviceAddress; //保存地址
      return API_USBGetSerNum(HIDHandle, DeviceAddress, buf);
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_SetDeviceAddress")]
   static extern int API_SetDeviceAddress(IntPtr HIDHandle, int DeviceAddress, int NewAddress, ref byte buf);

   public int SetHIDAddress(int NewAddress, ref byte buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         if (NewAddress != HIDAddress)
         {
            int ret = API_SetDeviceAddress(HIDHandle, HIDAddress, NewAddress, ref buf);
            if (ret == 0)
            {
               HIDAddress = NewAddress;
               return 0;
            }
            else
            {
               return ret;
            }
         }
         else
         {
            return -2; //设备地址没有更换
         }
      }
      else
      {
         return -1; //没有打开设备
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBControlLED")]
   static extern int API_USBControlLED(IntPtr HIDHandle, int DeviceAddress, byte led_time, byte led_num, ref byte buf);

   public int SetHIDLED(byte led_time, byte led_num, ref byte buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         if (led_time < 0 || led_time > 0x32)
         {
            return -2; //超出了led亮的范围，大于的话只会亮一次
         }

         return API_USBControlLED(HIDHandle, HIDAddress, led_time, led_num, ref buf);
      }
      else
      {
         return -1; //打开了设备
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBControlBuzzer")]
   static extern int API_USBControlBuzzer(IntPtr HIDHandle, int DeviceAddress, byte buzzer_time, byte buzzer_num, ref byte buf);

   public int SetHIDBuzzer(byte buzzer_time, byte buzzer_num, ref byte buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         if (buzzer_time < 0 || buzzer_time > 0x32)
         {
            return -2; //超出了led亮的范围，大于的话只会亮一次
         }

         return API_USBControlBuzzer(HIDHandle, HIDAddress, buzzer_time, buzzer_num, ref buf);
      }
      else
      {
         return -1; //没有打开设备
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBMF_Request")]
   static extern int API_USBMF_Request(IntPtr HIDHandle, int HIDAddress, byte mode, ref byte buf);

   public int HID_Request_14443A(byte mode, ref byte buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         return API_USBMF_Request(HIDHandle, HIDAddress, mode, ref buf);
      }
      else
      {
         return -1;
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBMF_Anticoll")]
   static extern int API_USBMF_Anticoll(IntPtr HIDHandle, int HIDAddress, ref byte flag, byte[] buf);

   public int HID_Anticoll_14443A(ref byte flag, byte[] buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         return API_USBMF_Anticoll(HIDHandle, HIDAddress, ref flag, buf);
      }
      else
      {
         return -1;
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBMF_Select")]
   static extern int API_USBMF_Select(IntPtr HIDHandle, int HIDAddress, byte[] uid, byte len, byte[] buf);

   public int HID_Select_14443A(byte[] uid, byte len, byte[] buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         return API_USBMF_Select(HIDHandle, HIDAddress, uid, len, buf);
      }
      else
      {
         return -1;
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBMF_Halt")]
   static extern int API_USBMF_Select(IntPtr HIDHandle, int HIDAddress);

   public int HID_Halt_14443A()
   {
      if (HIDHandle != IntPtr.Zero)
      {
         return API_USBMF_Select(HIDHandle, HIDAddress);
      }
      else
      {
         return -1;
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBMF_GET_SNR")]
   static extern int API_USBMF_GET_SNR(IntPtr HIDHandle, int HIDAddress, byte mode, byte halt, ref byte flag, byte[] buf);

   public int HID_GetSerialNum_14443A(byte mode, byte halt, ref byte flag, byte[] buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         return API_USBMF_GET_SNR(HIDHandle, HIDAddress, mode, halt, ref flag, buf);
      }
      else
      {
         return -1;
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBMF_Read")]
   static extern int API_USBMF_Read(IntPtr HIDHandle, int HIDAddress, byte mode, byte blockaddress, byte blocknum, byte[] key, byte[] buf);

   public int HID_Read_14443A(byte mode, byte blockaddress, byte blocknum, byte[] key, byte[] buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         return API_USBMF_Read(HIDHandle, HIDAddress, mode, blockaddress, blocknum, key, buf);
      }
      else
      {
         return -1;
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBMF_Write")]
   static extern int API_USBMF_Write(
      IntPtr HIDHandle,
      int HIDAddress,
      byte mode,
      byte blockaddress,
      byte blocknum,
      byte[] key,
      byte[] text,
      byte[] buf
   );

   public int HID_Write_14443A(byte mode, byte blockaddress, byte blocknum, byte[] key, byte[] text, byte[] buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         return API_USBMF_Write(HIDHandle, HIDAddress, mode, blockaddress, blocknum, key, text, buf);
      }
      else
      {
         return -1;
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBMF_InitVal")]
   static extern int API_USBMF_InitVal(IntPtr HIDHandle, int HIDAddress, byte mode, byte sector, byte[] key, int EP_value, byte[] buf);

   public int HID_InitEP_14443A(byte mode, byte sector, byte[] key, int EP_value, byte[] buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         return API_USBMF_InitVal(HIDHandle, HIDAddress, mode, sector, key, EP_value, buf);
      }
      else
      {
         return -1;
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBMF_Inc")]
   static extern int API_USBMF_Inc(IntPtr HIDHandle, int HIDAddress, byte mode, byte sector, byte[] key, ref int EP_value, byte[] buf);

   public int HID_IncreaseEP_14443A(byte mode, byte sector, byte[] key, ref int EP_value, byte[] buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         return API_USBMF_Inc(HIDHandle, HIDAddress, mode, sector, key, ref EP_value, buf);
      }
      else
      {
         return -1;
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBMF_Dec")]
   static extern int API_USBMF_Dec(IntPtr HIDHandle, int HIDAddress, byte mode, byte sector, byte[] key, ref int EP_value, byte[] buf);

   public int HID_DecreaseEP_14443A(byte mode, byte sector, byte[] key, ref int EP_value, byte[] buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         return API_USBMF_Dec(HIDHandle, HIDAddress, mode, sector, key, ref EP_value, buf);
      }
      else
      {
         return -1;
      }
   }

   [DllImport("HIDdll.dll", EntryPoint = "API_USBMF_TransferCMD")]
   static extern int API_USBMF_TransferCMD(IntPtr HIDHandle, int HIDAddress, byte crc, byte[] cmd, byte[] lenght, byte[] buf);

   public int HID_TFCMD_14443A(byte crc, byte[] cmd, byte[] length, byte[] buf)
   {
      if (HIDHandle != IntPtr.Zero)
      {
         return API_USBMF_TransferCMD(HIDHandle, HIDAddress, crc, cmd, length, buf);
      }
      else
      {
         return -1;
      }
   }
}
