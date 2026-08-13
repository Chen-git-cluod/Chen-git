namespace Kinlo.SharedBase.Interfacs;

public interface IExpand
{
   /// <summary>
   /// 加载时接收的数据
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   bool LoadData(Dictionary<string, object> dataDic);

   /// <summary>
   /// 生产过程中接收的数据
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   bool ProcessData(Dictionary<string, object> dataDic);
}
