namespace Kinlo.Equipment.Model.PLCInteractive;

[Obsolete("弃用")]
public class PlcBackHeliumDTU2
{
   /// <summary>
   /// 回氦前真空度
   /// </summary>
   public float HeliumBeforeVacuum { get; set; }

   /// <summary>
   /// 箱体真空值
   /// </summary>
   public float Box { get; set; }

   /// <summary>
   /// 回氦前真空保压时间1
   /// </summary>
   public float HeliumBeforeVacuumKeepTime1 { get; set; }

   /// <summary>
   /// 回氦前真空保压时间2
   /// </summary>
   public float HeliumBeforeVacuumKeepTime2 { get; set; }

   /// <summary>
   /// 回氦后真空度
   /// </summary>
   public float HeliumAfterVacuum { get; set; }

   /// <summary>
   /// 回氦后压力保压时间
   /// </summary>
   public float HeliumAfterPressureKeepTime { get; set; }

   /// <summary>
   /// 回氦平压值
   /// </summary>
   public float ReturnHeliumFlatPressureValue { get; set; }

   /// <summary>
   /// 前密封塞高度值
   /// </summary>
   public float BeforeSealingHeight { get; set; }

   /// <summary>
   /// 后密封塞高度值
   /// </summary>
   public float AfterSealingHeight { get; set; }

   /// <summary>
   /// 回氦时间
   /// </summary>
   public float HeliumTime { get; set; }

   /// <summary>
   /// 回氦站号
   /// </summary>
   public ushort HeliumStationNo { get; set; }

   /// <summary>
   /// 回氦位置
   /// </summary>
   public ushort HeliumPosition { get; set; }

   /// <summary>
   /// 回氦结果
   /// </summary>
   public ushort HeliumResult { get; set; }

   /// <summary>
   /// 前密封塞结果
   /// </summary>
   public ushort BeforeSealingResult { get; set; }

   /// <summary>
   /// 后密封塞结果
   /// </summary>
   public ushort AfterSealingResult { get; set; }

   /// <summary>
   /// CCD结果
   /// </summary>
   public ushort CCDResult { get; set; }
   public ushort 占位对齐 { get; set; }
   public short MyProperty1 { get; set; }
   public long ID { get; set; }
   public float[] Reserve { get; set; } = [0, 0, 0, 0];
}

/// <summary>
/// 南京国轩回氦
/// </summary>
public class PlcBackHeliumDtu
{
   private static int _rowCount = 8;

   public PlcBackHeliumDtu()
   {
      Actuals = new HeliumActualDtu[_rowCount];
      for (int i = 0; i < _rowCount; i++)
      {
         Actuals[i] = new HeliumActualDtu();
      }
   }

   public PlcBackHeliumDtu(int rowCount)
   {
      _rowCount = rowCount;
      Actuals = new HeliumActualDtu[_rowCount];
      for (int i = 0; i < _rowCount; i++)
      {
         Actuals[i] = new HeliumActualDtu();
      }
   }

   public HeliumSetDtu Set { get; set; } = new HeliumSetDtu();
   public HeliumActualDtu[] Actuals { get; set; }
}

/// <summary>
/// 回氦设定值
/// </summary>
public class HeliumSetDtu
{
   /// <summary>
   /// 回氦前第1段真空度设置
   /// </summary>
   public float HeliumBeforeVacuum1 { get; set; }

   /// <summary>
   /// 回氦前第2段真空度设置
   /// </summary>
   public float HeliumBeforeVacuum2 { get; set; }

   /// <summary>
   /// 回氦后真空度设置
   /// </summary>
   public float HeliumAfterVacuum { get; set; }

   /// <summary>
   /// 回氦前真空上限设置
   /// </summary>
   public float HeliumBeforeVacuumUL { get; set; }

   /// <summary>
   /// 回氮前真空下限设置
   /// </summary>
   public float HeliumBeforeVacuumLL { get; set; }

   /// <summary>
   /// 回氦后真空上限设置
   /// </summary>
   public float HeliumAfterVacuumUL { get; set; }

   /// <summary>
   /// 回氦后真空下限设置
   /// </summary>
   public float HeliumAfterVacuumLL { get; set; }

   /// <summary>
   /// 回氮前第1段保压时间
   /// </summary>
   public short HeliumBeforeKeepTime1 { get; set; }

   /// <summary>
   /// 回氮前第2段保压时间
   /// </summary>
   public short HeliumBeforeKeepTime2 { get; set; }

   /// <summary>
   /// 回氦后保压时间
   /// </summary>
   public short HeliumAfterKeepTime { get; set; }

   /// <summary>
   /// 回氦抽真空段数设置
   /// </summary>
   public short VacuumNumber { get; set; }
   public float MyProperty { get; set; }
}

/// <summary>
/// 回氦实际值
/// </summary>
public class HeliumActualDtu
{
   public HeliumActualDtu()
   {
      Reserve = new float[4];
   }

   public long ID { get; set; }

   /// <summary>
   /// 回氮前第1段真空度
   /// </summary>
   public float HeliumBeforeVacuum1 { get; set; }

   /// <summary>
   /// 回氦前第2段真空度
   /// </summary>
   public float HeliumBeforeVacuum2 { get; set; }

   /// <summary>
   /// 回氦后真空度
   /// </summary>
   public float HeliumAfterVacuum { get; set; }

   /// <summary>
   /// 回氦总时间
   /// </summary>
   public short HeliumTime { get; set; }

   /// <summary>
   /// 回氦站号
   /// </summary>
   public short HeliumStationNo { get; set; }

   /// <summary>
   /// 回氦位置
   /// </summary>
   public short HeliumPosition { get; set; }

   /// <summary>
   /// 回氦结果
   /// </summary>
   public short HeliumResult { get; set; }
   public float[] Reserve { get; set; }
   public float MyProperty { get; set; }
}

/// <summary>
/// 南京国轩回氦，弃用
/// </summary>
[Obsolete("已弃用，请使用PlcBackHeliumNJGXDtu")]
public class PlcBackHeliumNJGXDtu1
{
   /// <summary>
   /// 回氦前真空超时时间设置
   /// </summary>
   public short PreBackHeliumVacuumTimeoutTime { get; set; }

   /// <summary>
   /// 回氮前第1段真空保持时间
   /// </summary>
   public short PreBackHeliumVacuumKeepTimePhase1 { get; set; }

   /// <summary>
   /// 回氮前第2段真空保持时间
   /// </summary>
   public short PreBackHeliumVacuumKeepTimePhase2 { get; set; }

   /// <summary>
   /// 回氮超时时间
   /// </summary>
   public short BackHeliumTimeout { get; set; }

   /// <summary>
   /// 回氦后保持时间设置
   /// </summary>
   public short AfterBackHeliumKeepTime { get; set; }

   /// <summary>
   /// 泄压时间设置
   /// </summary>
   public short ReleasePressureTime { get; set; }

   /// <summary>
   /// 泄压超时时间设置
   /// </summary>
   public short ReleasePressureTimeoutTime { get; set; }

   /// <summary>
   /// 排液时间设置
   /// </summary>
   public short PushoutLiquidTime { get; set; }

   /// <summary>
   /// 排液周期设置
   /// </summary>
   public short PushoutLiquidPeriod { get; set; }

   /// <summary>
   /// 接残液时间设置
   /// </summary>
   public short TakeRemainLiquidTime { get; set; }

   /// <summary>
   /// 清洗漫泡时间设置
   /// </summary>
   public short WashingTime { get; set; }

   /// <summary>
   /// 清洗后接液时间设置
   /// </summary>
   public short AfterWashingTakeLiquidTime { get; set; }

   /// <summary>
   /// 清洗周期设定值
   /// </summary>
   public short WashingPeriod { get; set; }
   public short MyProperty { get; set; }

   /// <summary>
   /// 常压上限设定值
   /// </summary>
   public float NormalPressureUpperlimit { get; set; }

   /// <summary>
   /// 常压下限设定值
   /// </summary>
   public float NormalPressureLowerLimit { get; set; }

   /// <summary>
   /// 回氮前真空上限设定们
   /// </summary>
   public float PreBackHeliumVacuumUpperLimit { get; set; }

   /// <summary>
   /// 回氮前真空下限设定值
   /// </summary>
   public float PreBackHeliumVacuumLowerLimit { get; set; }

   /// <summary>
   /// 回氦后真空上限设定们
   /// </summary>
   public float AfterBackHeiumVacuumUpperLimit { get; set; }

   /// <summary>
   /// 回氮后真空下限设定信
   /// </summary>
   public float AfterBackHeiumVacuumLowerLimit { get; set; }

   /// <summary>
   /// 回氦抽真空段数设置
   /// </summary>
   public short BackHeliumTakeVacuumSections { get; set; }
   public short MyProperty1 { get; set; }

   /// <summary>
   /// 回氨前第1段真空度值
   /// </summary>
   public float[] PreBackHeliumVacuumValuePhase1 { get; set; } = new float[4];

   /// <summary>
   /// 回氨前第2段真空度值
   /// </summary>
   public float[] PreBackHeliumVacuumValuePhase2 { get; set; } = new float[4];

   /// <summary>
   /// 回氦后真空度
   /// </summary>
   public float[] AfterBackHeliumVacuumValue { get; set; } = new float[4];
   public long[] ID { get; set; } = new long[4];

   /// <summary>
   /// 回氦位置
   /// </summary>
   public short[] BackHeliumLocation { get; set; } = new short[4];
}
