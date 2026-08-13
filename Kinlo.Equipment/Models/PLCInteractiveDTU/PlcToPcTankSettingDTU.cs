namespace Kinlo.Equipment.Model.PLCInteractive;

public class PlcToPcTankSettingDTU
{
   private static int _length = 32;
   public PlcToPcTankSettingItemDTU[] Datas { get; set; }

   public PlcToPcTankSettingDTU()
   {
      Datas = new PlcToPcTankSettingItemDTU[_length];
   }

   public PlcToPcTankSettingDTU(int length)
   {
      _length = length;
      Datas = new PlcToPcTankSettingItemDTU[_length];
   }
}

public class PlcToPcTankSettingItemDTU
{
   /// <summary>
   /// 设定压力
   /// </summary>
   public float SetPressure { get; set; }

   /// <summary>
   /// 设定保压时间
   /// </summary>
   public short SetRetentionTime { get; set; }

   /// <summary>
   /// 功能
   /// </summary>
   public short Func { get; set; }
   // public int MyProperty { get; set; }
}

public class PlcToTank
{
   /// <summary>
   /// 缸号
   /// </summary>
   public short TankNo { get; set; }

   /// <summary>
   /// 循环时间
   /// </summary>
   public float CycleTime { get; set; }

   /// <summary>
   /// 设置循环次数
   /// </summary>
   public short SetCycleNo { get; set; }

   /// <summary>
   /// 实际循环次数
   /// </summary>
   public short actualCycleNo { get; set; }

   /// <summary>
   /// 第一层托盘ID
   /// </summary>
   public long FirstLayerTrayID { get; set; }

   /// <summary>
   /// 第二层ID
   /// </summary>
   public long SecondLayerID { get; set; }

   /// <summary>
   /// 第三层ID
   /// </summary>
   public long ThirdLayerID { get; set; }

   /// <summary>
   /// 第四层ID
   /// </summary>
   public long FourthLayerID { get; set; }

   /// <summary>
   /// 保压设置时间
   /// </summary>
   public ushort[] StepTime_Set { get; set; } = new ushort[30];

   /// <summary>
   /// 功能设置选择
   /// </summary>
   public ushort[] Step_Function { get; set; } = new ushort[30];

   /// <summary>
   /// 功能压力参数
   /// </summary>
   public float[] Step_Function_Set { get; set; } = new float[30];

   /// <summary>
   /// 开阀超时时间
   /// </summary>
   public short[] Step_Function_OVT { get; set; } = new short[30];

   /// <summary>
   /// 实际开阀时间
   /// </summary>
   public short[] Step_Function_Time { get; set; } = new short[30];

   /// <summary>
   /// 实际保压时间
   /// </summary>
   public short[] ActualKeepTime { get; set; } = new short[30];

   /// <summary>
   /// 实际压力
   /// </summary>
   public float[] ActualPressure { get; set; } = new float[30];

   /// <summary>
   /// 排队动作时间
   /// </summary>
   public short[] LineUpTime { get; set; } = new short[30];
}
