namespace Kinlo.Common.Models.ConfigModels;

[AddINotifyPropertyChangedInterface]
[Languages]
public class FunctionEnableModel
{
   /// <summary>
   /// 启用变量注液
   /// </summary>
   [Languages(["启用变量注液", "Aktifkan suntikan variabel", "Enable variable injection"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public bool IsEnableVariableInjection { get; set; }

   /// <summary>
   /// 启用前称重量管控
   /// </summary>
   [Languages(["启用前称重量管控", "Aktifkan pengendalian berat", "enable before Weight Control"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public bool IsEnableIncomingWeightControl { get; set; }

   /// <summary>
   /// 启用最终重管控,
   /// </summary>
   [Languages(["启用最终重管控", "Deteksi bobot setelah diaktifkan", "final enabling weight detection"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public bool IsEnableAfterWeightCheck { get; set; }

   /// <summary>
   /// 启用注液量管控
   /// </summary>
   [Languages(["启用注液量管控", "Aktifkan pengujian volume injeksi", "Enable injection volume detection"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.清洗机], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public bool IsEnableInjectionCheck { get; set; }

   private bool _isRewrokMode;

   /// <summary>
   /// 复投模式，其实只有一注需要查旧数据，二三注只需前称不管控
   /// </summary>
   [Languages(["复投模式(不管控前称重)", "复投模式(不管控前称重)", "Rework mode"])]
   [UIDisplay(Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.清洗机], IsRunEdit = false, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public bool IsRewrokMode
   {
      get { return _isRewrokMode; }
      set
      {
         if (_isRewrokMode != value)
         {
            _isRewrokMode = value;
            if (value)
            {
               IsEnableVariableInjection = true;
               IsEnableReuseOldTest = true;
            }
         }
      }
   }

   /// <summary>
   /// 复投使用旧短路数据
   /// </summary>
   [Languages(["复投复用旧短路数据", "Menggunakan kembali data sirkuit pendek lama", "Reuse old test data"])]
   [UIDisplay(
      Hiddens = [ProductionTypeEnum.回氦, ProductionTypeEnum.二次注液, ProductionTypeEnum.三次注液, ProductionTypeEnum.清洗机],
      IsRunEdit = false,
      EditLevel = (uint)DefaultRoleEnum.工艺
   )]
   public bool IsEnableReuseOldTest { get; set; } = true;

   /// <summary>
   /// 启用虚拟码
   /// </summary>
   [Languages(["启用虚拟码", "Aktifkan kode virtual", "Enable virtual code"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public bool IsEnablePseudoCode { get; set; }

   /// <summary>
   /// 空载模式
   /// </summary>
   [Languages(["空载模式", "Mode beban kosong", "Empty mode"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public bool IsEmptyLoadMode { get; set; }

   /// <summary>
   /// PC->PLC结果强制合格（所有）
   /// </summary>
   [Languages(["PC->PLC结果强制合格（所有）", "Hasil Wajib Lulus", "Forced result OK"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public bool IsForceOK { get; set; }

   /// <summary>
   /// PC->PLC结果强制合格（MES）
   /// </summary>
   [Languages(["PC->PLC结果强制合格（MES）", "", "ignore mes result"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public bool IsIgnoreMesNg { get; set; }

   /// <summary>
   /// 同步PLC权限时询问
   /// </summary>
   [Languages(["同步PLC权限时询问", "Batasi waktu akses PLC", "Sync PLC level inquire"])]
   [UIDisplay(IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.工艺)]
   public bool IsEnableSyncPLCInquire { get; set; }

   ///// <summary>
   ///// 校验老化压降
   ///// </summary>
   //[Languages(["校验老化压降", "Buck Tegangan Kalibrasi", "Check Voltage drop"])]
   //[UIDisplay(Hiddens = [ProductionTypeEnum.一次注液, ProductionTypeEnum.二次注液], IsRunEdit = true, EditLevel = (uint)DefaultRoleEnum.设备)]
   //public bool IsCheckVoltageDrop { get; set; }
   /// <summary>
   /// 启用胶钉高度差检测
   /// </summary>
   [Languages(["启用胶钉高度差检测", "启用胶钉高度差检测", "Check nail height difference"])]
   [UIDisplay(
      Hiddens = [ProductionTypeEnum.一次注液, ProductionTypeEnum.二次注液, ProductionTypeEnum.三次注液, ProductionTypeEnum.清洗机],
      IsRunEdit = true,
      EditLevel = (uint)DefaultRoleEnum.设备
   )]
   public bool IsCheckNailHeightDifference { get; set; }

   /// <summary>
   /// 一扫多时，PLC指定前扫码通道有无电池
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["启用PLC指定进站通道有无电池", "", ""])]
   public bool IsLaneHaveBattery { get; set; } = false;

   /// <summary>
   /// 启用MES补传
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["启用MES补传", "", ""])]
   public bool IsEnableMesResend { get; set; } = false;

   /// <summary>
   /// 读取泵温度
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["读取泵温度", "", ""])]
   public bool IsEnableReadPumpTemperature { get; set; } = true;

   /// <summary>
   /// 使用实时范围值
   /// </summary>
   [UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   [Languages(["使用实时上下限", "", ""])]
   public bool IsEnableCurrentRange { get; set; } = false;
   ///// <summary>
   ///// PC启动短路(临时)
   ///// </summary>
   //[UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   //[Languages([" PC启动短路(临时)", "", ""])]
   //public bool IsPcStartShortCircuitTemp { get; set; } = false;
   ///// <summary>
   ///// 短路取两次(临时)
   ///// </summary>
   //[UIDisplay(IsRunEdit = true, EditLevel = (ulong)1 << 62)]
   //[Languages([" 短路取两次(临时)", "", ""])]
   //public bool IsEnableTemp { get; set; } = false;
}
