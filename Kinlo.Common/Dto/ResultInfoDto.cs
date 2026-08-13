namespace Kinlo.Common.Dto;

//public class ResultInfoDto
//{
//    public ResultTypeEnum Detitme { get; set; }
//    public ResultTypeEnum IsMesStatus { get; set; }
//    public Dictionary<ResultProcessEnum, ResultTypeEnum> ResultItems { get; set; } = new();
//}

public class ResultInfoItemDto
{
   public ResultInfoItemDto(ProcessTypeEnum processes, ResultTypeEnum result)
   {
      Processes = processes;
      Result = result;
   }

   public ProcessTypeEnum Processes { get; set; }
   public ResultTypeEnum Result { get; set; }
}
