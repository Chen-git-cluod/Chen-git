namespace Kinlo.Common.Dto;

public class UploadParamDto
{
   public string ParamCode { get; set; } = string.Empty;
   public string ParamName { get; set; } = string.Empty;

   /// <summary>
   /// 原始值
   /// </summary>
   public string OriginalValue { get; set; } = string.Empty;

   /// <summary>
   /// 实时值
   /// </summary>
   public string CurrentValue { get; set; } = string.Empty;

   public UploadParamDto(string code, string name)
   {
      ParamCode = code;
      ParamName = name;
   }
}
