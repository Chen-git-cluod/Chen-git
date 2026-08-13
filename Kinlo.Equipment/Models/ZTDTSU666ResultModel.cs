namespace Kinlo.Equipment.Models;

[AddINotifyPropertyChangedInterface]
[Languages]
public class ZTDTSU666ResultModel
{
   /// <summary>
   /// A相电压 单位：V
   /// </summary>
   [Languages(["A相电压", "A相電壓", "A-phase voltage"])]
   public float Ua { get; set; }

   /// <summary>
   /// B相电压 单位：V
   /// </summary>
   [Languages(["B相电压", "B相電壓", "B-phase voltage"])]
   public float Ub { get; set; }

   /// <summary>
   /// C相电压 单位：V
   /// </summary>
   [Languages(["C相电压", "C相電壓", "C-phase voltage"])]
   public float Uc { get; set; }

   /// <summary>
   /// A相电流 单位：A
   /// </summary>
   [Languages(["A相电流", "A相电流", "A-phase curren"])]
   public float Ia { get; set; }

   /// <summary>
   /// B相电流 单位：A
   /// </summary>
   [Languages(["B相电流", "B相电流", "B-phase curren"])]
   public float Ib { get; set; }

   /// <summary>
   /// C相电流 单位：A
   /// </summary>
   [Languages(["C相电流", "C相电流", "C-phase curren"])]
   public float Ic { get; set; }

   /// <summary>
   /// 正向有功总电能 单位：kWh
   /// </summary>
   [Languages(["有功总电能", "有功總電能", "Total electrical energy"])]
   public float ImpEp { get; set; }

   /// <summary>
   ///  合相有功功率，单位 W
   /// </summary>
   [Languages(["有功功率", "有功功率", "Active power"])]
   public float Pt { get; set; }
}
