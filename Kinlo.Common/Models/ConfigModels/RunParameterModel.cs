namespace Kinlo.Common.Models.ConfigModels;

[AddINotifyPropertyChangedInterface]
[Languages]
public class RunParameterModel
{
   /// <summary>
   /// MES连续NG报警次数
   /// </summary>
   //[UIDisplay(DisplayName = "MES连续NG次数", IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   //public int MESContinuousNGCount { get; set; }
   /// <summary>
   /// 保液量标准(g)
   /// </summary>
   [Languages(["保液量标准(g)", "Standar retensi cairan(g)", "Injection standard(g)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float InjectionStandard { get; set; }

   /// <summary>
   /// 保液量上限(g)
   /// </summary>
   [Languages(["保液量上限(g)", "Batas penampungan cairan(g)", "Injection upper upper(g)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float InjectionUpper { get; set; }

   /// <summary>
   /// 保液量下限(g)
   /// </summary>
   [Languages(["保液量下限(g)", "Batas bawah penampungan cairan(g)", "Injection lower limit(g)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float InjectionLower { get; set; }

   /// <summary>
   /// 注液泵上限(g)
   /// </summary>
   [Languages(["注液泵上限(g)", "Batas atas pompa injeksi cairan(g)", "Injection pump upper upper(g)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float VariableInjectionUpper { get; set; } = 500;

   /// <summary>
   /// 注液泵下限(g)
   /// </summary>
   [Languages(["注液泵下限(g)", "Batas bawah pompa injeksi cairan(g)", "Injection pump lower limit(g)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float VariableInjectionLower { get; set; } = 1;

   /// <summary>
   /// 补液泵上限(g)
   /// </summary>
   [Languages(["补液泵上限(g)", "Batas atas pompa pengisian ulang(g)", "replenishment pump upper upper(g)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float ReplenishInjectionUpper { get; set; } = 50;

   /// <summary>
   /// 来料重量上限(g)
   /// </summary>
   [Languages(["前称重上限(g)", "Batas berat yang masuk(g)", "before weight upper limit(g)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float IncomingWeightUpper { get; set; }

   /// <summary>
   /// 前称重下限(g)
   /// </summary>
   [Languages(["前称重下限(g)", "Batas berat lebih rendah untuk material masuk(g)", "before weight lower limit(g)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float IncomingWeightLower { get; set; }

   /// <summary>
   /// 后称重上限(g)
   /// </summary>
   [Languages(["后称重上限(g)", "Batas penimbangan belakang(g)", "After weighing upper upper(g)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float AfterWeightUpper { get; set; }

   /// <summary>
   /// 后称重下限(g)
   /// </summary>
   [Languages(["后称重下限(g)", "Batas pasca-penimbangan yang lebih rendah(g)", "After weighing lower limit(g)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float AfterWeightLower { get; set; }

   ///// <summary>
   ///// 称重点检上限(g)
   ///// </summary>
   //[UIDisplay(DisplayName = "称重点检上限(g)", IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   //public float SpotCheckWeightUpper { get; set; }
   ///// <summary>
   ///// 称重点检下限(g)
   ///// </summary>
   //[UIDisplay(DisplayName = "称重点检下限(g)", IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   //public float SpotCheckWeightLower { get; set; }

   /// <summary>
   /// 电压复测次数
   /// </summary>
   [Languages(["电压复测次数", "Jumlah pengujian ulang tegangan", "Voltage retest times"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.一次注液, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public int VoltageTestCount { get; set; } = 3;

   /// <summary>
   /// 电压上限(V)
   /// </summary>
   [Languages(["电压上限(V)", "Batas Tegangan", "Voltage lower upper(V)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.一次注液, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float VoltageUpper { get; set; } = 6;

   /// <summary>
   ///电压下限(V)
   /// </summary>
   [Languages(["电压下限(V)", "Batas tegangan yang lebih rendah", "Voltage lower limit(V)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.一次注液, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float VoltageLower { get; set; } = 2;

   /// <summary>
   /// 失液量上限(g)
   /// </summary>
   [Languages(["失液量上限(g)", "Batas atas kehilangan cairan(g)", "loss of fluid upper(g)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.一次注液, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public float LossOfFluidUpper { get; set; } = 30;

   /// <summary>
   /// 胶钉高度差上限
   /// </summary>
   [Languages(["胶钉高度差上限", "", "nail height difference upper(g)"])]
   [UIDisplay(
      Hiddens = [ProductionTypeEnum.一次注液, ProductionTypeEnum.二次注液, ProductionTypeEnum.三次注液, ProductionTypeEnum.清洗机],
      IsRunEdit = true,
      EditLevel = (uint)DefaultRoleEnum.工艺
   )]
   public float NailHeightDifferenceUpper { get; set; } = 0.5f;

   /// <summary>
   /// 化成钉重量(g)
   /// </summary>
   [Languages(["化成钉重量(g)", "Berat kuku(g)", "Weight of nail"])]
   [UIDisplay(
      Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.一次注液, ProductionTypeEnum.清洗机],
      IsRunEdit = true,
      EditLevel = (uint)DefaultRoleEnum.工艺
   )]
   public float NailWeight { get; set; } = 0.0f;

   /// <summary>
   /// 电压极差补偿
   /// </summary>
   [Languages(["电压极差补偿", "Kompensasi untuk Tegangan Ekstrem", "Voltage range compensation"])]
   [UIDisplay(
      Hiddens = [ProductionTypeEnum.二次注液, ProductionTypeEnum.一次注液, ProductionTypeEnum.三次注液, ProductionTypeEnum.清洗机],
      IsRunEdit = true,
      EditLevel = (uint)DefaultRoleEnum.工艺
   )]
   public int VoltageRangeCompensation { get; set; } = 1;

   /// <summary>
   /// 老化压降上限(mv)
   /// </summary>
   [Languages(["老化压降上限(mv)", "Batas atas penurunan tekanan penuaan(mv)", "Voltage drop upper(mv)"])]
   [UIDisplay(
      Hiddens = [ProductionTypeEnum.二次注液, ProductionTypeEnum.一次注液, ProductionTypeEnum.三次注液, ProductionTypeEnum.清洗机],
      IsRunEdit = true,
      EditLevel = (uint)DefaultRoleEnum.工艺
   )]
   public float VoltageDropUpper { get; set; } = 500f;

   /// <summary>
   /// 少液回流上限(次)
   /// </summary>
   [Languages(["少液回流上限(次)", "少液回流上限(次)", "Inject less rework count"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public int InjectLessReworkCount { get; set; } = 3;

   /// <summary>
   /// 测漏回流上限(次)
   /// </summary>
   [Languages(["测漏回流上限(次)", "测漏回流上限(次)", "Test leak rework count"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public int TestLeakReworkCount { get; set; } = 3;

   /// <summary>
   /// 扫码重试次数
   /// </summary>
   [Languages(["扫码重试次数", "Memindai Hitungan Coba Ulang", "Scan retry times"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public int RetryCountScanCode { get; set; } = 2;

   /// <summary>
   /// 称重超时
   /// </summary>
   [Languages(["称重超时(ms)", "Batas waktu penimbangan(ms)", "Weighing timeout(ms)"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public int WeighingTimeout { get; set; } = 1000;

   ///// <summary>
   ///// 参数上传MES间隔（秒）
   ///// </summary>
   //[Languages(["参数上传MES间隔(秒)", "参数上传MES间隔(秒)", "Param Notify MES Span(s)"])]
   //[UIDisplay(Hiddens = [ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   //public int ParamNotifySpan { get; set; } = 600;
}
