namespace Kinlo.Common.Interfaces;

/// <summary>
/// 用来MES值的转换接口
/// </summary>
public interface IMesValueConverter
{
   /// <summary>
   ///
   /// </summary>
   /// <param name="container"></param>
   /// <param name="battery">电池</param>
   /// <param name="manualParameter">手动传入的参数</param>
   /// <param name="autoParameter">自动传入的参数</param>
   /// <returns></returns>
   object Convert(IContainer container, IBatMainModel battery, string manualParameter, string autoParameter);
   //object Convert(IContainer container, string parameter, params object[] values);
}
