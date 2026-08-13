namespace Kinlo.Equipment.Models;

/// <summary>
/// 浮子流量计，贺德克(HDK-LDZ-DN50)
/// </summary>
[AddINotifyPropertyChangedInterface]
[Languages(["流量计", "流量计", "Flow"])]
public class HDK_LDZ_DN50DTO
{
   /// <summary>
   /// 瞬时流量
   /// </summary>
   [Languages(["瞬时流量", "瞬时流量", "Instantaneous flow rate"])]
   public float 瞬时流量 { get; set; }

   /// <summary>
   /// 流量百分比
   /// </summary>
   [Languages(["流量百分比", "流量百分比", "Traffic percentage"])]
   public float 流量百分比 { get; set; }

   /// <summary>
   /// 总量
   /// </summary>
   [Languages(["总量", "总量", "Total"])]
   public float 总量 { get; set; }

   /// <summary>
   /// 瞬时流量单位
   /// </summary>
   [Languages(["瞬时流量单位"])]
   public string 瞬时流量单位 { get; set; } = GetInstantaneousFlowUnit(0);

   /// <summary>
   /// 总量单位
   /// </summary>
   [Languages(["总量单位"])]
   public string 总量单位 { get; set; } = GetTotalnit(0);

   public static string GetInstantaneousFlowUnit(int code)
   {
      return code switch
      {
         0 => "m3_h",
         1 => "m3_m",
         2 => "m3_s",
         3 => "L_h",
         4 => "L_m",
         5 => "L_s",
         6 => "mL_h",
         7 => "mL_m",
         8 => "mL_s",
         9 => "kg_h",
         10 => "kg_m",
         11 => "kg_s",
         12 => "t_h",
         13 => "t_m",
         14 => "t_s",
         15 => "Nm3_h",
         16 => "Nm3_m",
         17 => "Nm3_s",
         _ => "无单位",
      };
   }

   public static string GetTotalnit(int code)
   {
      return code switch
      {
         0 => "m3",
         1 => "L",
         2 => "mL",
         3 => "kg",
         4 => "t",
         5 => "Nm3",
         _ => "无单位",
      };
   }
}
