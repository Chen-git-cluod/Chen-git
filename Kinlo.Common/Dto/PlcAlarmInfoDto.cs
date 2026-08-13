namespace Kinlo.Common.Dto;

public class PlcAlarmInfoDto
{
   /// <summary>
   /// MES代码
   /// </summary>
   public string MesCode { get; set; } = string.Empty;

   /// <summary>
   /// PLC报警信息
   /// </summary>
   public string AlarmMessage { get; set; } = string.Empty;

   /// <summary>
   /// PLC从表格导入的原始标签
   /// </summary>
   public string OriginalTag { get; set; } = string.Empty;
}
