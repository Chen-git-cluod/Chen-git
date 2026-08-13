namespace Kinlo.Common.Dto;

public class CurrentStatisticsData
{
   public ProcessRoleEnum ProcessRole { get; set; } = ProcessRoleEnum.None;
   public string Process { get; set; }
   public ConcurrentBag<StatisticsItem> Data { get; set; } = new ConcurrentBag<StatisticsItem>();

   public CurrentStatisticsData(string process, ProcessRoleEnum processRole)
   {
      Process = process;
      ProcessRole = processRole;
   }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")] //序列化时在json中加入字段用来区分用哪个对象来实现
[JsonDerivedType(typeof(StatisticsResult), "base")] //base对应StatisticsResult
[JsonDerivedType(typeof(InjectStatisticsResult), "inject")] //同上
public interface IStatisticsResult
{
   /// <summary>
   /// 最终结果
   /// </summary>
   ResultTypeEnum FinalResult { get; set; }
}

public class StatisticsResult : IStatisticsResult
{
   public StatisticsResult() { }

   public StatisticsResult(ResultTypeEnum finalResult)
   {
      FinalResult = finalResult;
   }

   public ResultTypeEnum FinalResult { get; set; }
}

public class InjectStatisticsResult : StatisticsResult
{
   public InjectStatisticsResult() { }

   public InjectStatisticsResult(ResultTypeEnum finalResult)
      : base(finalResult) { }

   public InjectStatisticsResult(
      ResultTypeEnum finalResult,
      ResultTypeEnum firstInjectResult,
      ResultTypeEnum autoRefillInjectResult,
      ResultTypeEnum manualRefillInjectResult
   )
      : base(finalResult)
   {
      FirstInjectResult = firstInjectResult;
      AutoRefillInjectResult = autoRefillInjectResult;
      ManualRefillInjectResult = manualRefillInjectResult;
   }

   /// <summary>
   /// 首注
   /// </summary>
   public ResultTypeEnum FirstInjectResult { get; set; }

   /// <summary>
   /// 自动补液
   /// </summary>
   public ResultTypeEnum AutoRefillInjectResult { get; set; }

   /// <summary>
   /// 手动补液
   /// </summary>
   public ResultTypeEnum ManualRefillInjectResult { get; set; }
}

public class StatisticsItem
{
   public StatisticsItem(string borcoe, IStatisticsResult result)
   {
      Barcode = borcoe;
      Result = result;
   }

   public string Barcode { get; set; } = string.Empty;

   /// <summary>
   /// 结果
   /// </summary>
   public IStatisticsResult Result { get; set; }
}
