using System.ComponentModel;

namespace Kinlo.SharedBase.Enums;

[Languages]
public enum ProductTypeEnum
{
   [Languages("量产", "Produksi massal", "Mass production"), Description("MASS")]
   量产,

   [Languages("试产", "Uji produksi", "Trial production"), Description("TRIAL")]
   试产,

   [Languages("调机", "Penyesuaian mesin", "Adjusting the machine"), Description("COMMISSIONING")]
   调机,

   [Languages("返修", "Perbaikan", "Repair"), Description("REWORK")]
   返修,
}
