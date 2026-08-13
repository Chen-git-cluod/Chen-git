namespace Kinlo.Common.Dto;

public class SyncParameterDto
{
   public string RowNumber { get; set; } = string.Empty;
   public string Name { get; set; } = string.Empty;
   public string MesCode { get; set; } = string.Empty;
   public string Unit { get; set; } = string.Empty;
   public ParamTypeEnum ParamType { get; set; } = ParamTypeEnum.标准值;

   /// <summary>
   /// 标准值
   /// </summary>
   public string ParamterValue { get; set; } = string.Empty;

   /// <summary>
   /// 如果类型是上下限，则为下限
   /// </summary>
   public string ParamterMin { get; set; } = string.Empty;

   /// <summary>
   /// 如果类型是上下限，则为上限
   /// </summary>
   public string ParamterMax { get; set; } = string.Empty;
}

public enum ParamTypeEnum
{
   无需管控 = 0,
   标准值 = 1,
   范围上下限 = 2,
}
