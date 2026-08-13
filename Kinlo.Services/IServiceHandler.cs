namespace Kinlo.Services;

public interface IServiceHandler
{
   public PLCInteractAddressModel Context { get; }

   /// <summary>
   ///  开始处理
   /// </summary>
   /// <param name="plcValue">读取的plc CMD值 </param>
   /// <returns></returns>
   Task<int> Handle(short plcValue);

   /// <summary>
   /// 添加UI界面显示数据
   /// </summary>
   /// <param name="batterys"></param>
   void AddDisplayData(params IBatMainModel[] battery);
}
