//namespace Kinlo.SharedBase.Attributes;

///// <summary>
///// 用于统计电池工序结果的特性
///// </summary>
//[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
//public class StatisticsAttribute : Attribute
//{
//    /// <summary>
//    /// 指定结果及合格率统计是用于哪个工序(有些工序有多个结果,比如后称有抽真空结果、注液结果等)
//    /// </summary>
//    public ProcessTypeEnum Process { get; set; }
//    /// <summary>
//    /// 功能
//    /// </summary>
//    public StatisticsFuncEnum Func { get; set; }
//    public StatisticsAttribute()
//    {

//    }

//    public StatisticsAttribute(ProcessTypeEnum process)
//    {
//        Process = process;
//    }
//    public StatisticsAttribute(ProcessTypeEnum process,StatisticsFuncEnum func) : this(process)
//    {
//        Func = func;
//    }
//}
