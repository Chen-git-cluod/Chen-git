using System.ComponentModel;

namespace Kinlo.SharedBase.Enums;

[Languages]
public enum WarningLevelEnum
{
   [Languages("一级报警"), Description("P0")]
   一级报警,

   [Languages("二级报警"), Description("P1")]
   二级报警,

   [Languages("三级报警"), Description("P2")]
   三级报警,

   [Languages("四级报警"), Description("P3")]
   四级报警,
}

[Languages]
public enum PlcAalrmLevelEnum
{
   [Languages("报警"), Description("P0")]
   报警,

   [Languages("警告"), Description("P1")]
   警告,

   [Languages("提示"), Description("P2")]
   提示,
}
