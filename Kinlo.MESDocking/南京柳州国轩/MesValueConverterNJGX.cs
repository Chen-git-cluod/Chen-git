using System.ComponentModel;
using Kinlo.Common.Interfaces;

namespace Kinlo.MESDocking;

[Description("结果转OK或NG")]
public class MesValueResultToString : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter)
   {
      var value = battery[autoParameter];
      if (value is ResultTypeEnum result)
      {
         var resultInt = (int)result;
         return resultInt switch
         {
            < 11 => "未加工",
            < 21 => "OK",
            _ => "NG",
         };
      }
      return "";
   }
}

[Description("Hipot转TH结果")]
public class Hipot转TH结果 : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter) =>
      battery[autoParameter].GetMesHipotResult(ResultTypeEnum.E04_电压上升过慢_TH报警);
}

[Description("Hipot转TL结果")]
public class Hipot转TL结果 : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter) =>
      battery[autoParameter].GetMesHipotResult(ResultTypeEnum.E03_电压上升过快_TL报警);
}

[Description("Hipot转跌落1结果")]
public class Hipot转跌落1结果 : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter) =>
      battery[autoParameter].GetMesHipotResult(ResultTypeEnum.E05_上升阶段VD1超限);
}

[Description("Hipot转跌落2结果")]
public class Hipot转跌落2结果 : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter) =>
      battery[autoParameter].GetMesHipotResult(ResultTypeEnum.E06_电压保持VD2超限);
}

[Description("Hipot转跌落3结果")]
public class Hipot转跌落3结果 : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter) =>
      battery[autoParameter].GetMesHipotResult(ResultTypeEnum.E07_自由放电VD3超限);
}

[Description("Hipot转开路结果")]
public class Hipot转开路结果 : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter) =>
      battery[autoParameter].GetMesHipotResult(ResultTypeEnum.E01_开路);
}

[Description("Hipot转VP结果")]
public class Hipot转VP结果 : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter) =>
      battery[autoParameter]
         .GetMesHipotResult(
            ResultTypeEnum.E02_未达到设定电压值,
            ResultTypeEnum.E03_电压上升过快_TL报警,
            ResultTypeEnum.E04_电压上升过慢_TH报警,
            ResultTypeEnum.E05_上升阶段VD1超限,
            ResultTypeEnum.E06_电压保持VD2超限,
            ResultTypeEnum.E07_自由放电VD3超限
         );
}

[Description("Hipot转电容结果")]
public class Hipot转电容结果 : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter) =>
      battery[autoParameter].GetMesHipotResult(ResultTypeEnum.E10_电容超下限报警, ResultTypeEnum.E11_电容超上限报警);
}

[Description("Hipot转电阻结果")]
public class Hipot转电阻结果 : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter) =>
      battery[autoParameter].GetMesHipotResult(ResultTypeEnum.E08_电阻超下限报警, ResultTypeEnum.E09_电阻超上限报警);
}

/// <summary>
/// 解析加压缸数据
/// </summary>
public class TankPressureValueConverter : IMesValueConverter
{
   /// <summary>
   ///
   /// </summary>
   /// <param name="container"></param>
   /// <param name="battery"></param>
   /// <param name="manualParameter">手动传入类型  [ 1:真空，2：正压：3：大气 ]</param>
   /// <param name="autoParameter"></param>
   /// <returns></returns>
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter)
   {
      var value = battery[autoParameter];
      var func = battery[nameof(IBatTankModel.Func)].ToString(); //1:真空，2：正压：3：大气
      var funcArr = func?.Split(',').ToList() ?? [];

      var index = funcArr.IndexOf(manualParameter);

      if (value is string result)
      {
         var arr = result.Split(',');
         return arr.Length > index ? arr[index] : "0";
      }
      return "0";
   }
}

#region 通用
[Description("转换为字符")]
public class MesValueToString : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter) =>
      battery[autoParameter].ToMesString();
}

[Description("转换为小数")]
public class MesValueToDouble : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter)
   {
      var value = battery[autoParameter];

      if (value is double i)
         return i;
      else
      {
         if (double.TryParse(value.ToString(), out var d))
         {
            return d;
         }
      }

      return 0.0;
   }
}

[Description("转换为整数")]
public class MesValueToInt : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter)
   {
      var value = battery[autoParameter];

      if (value is int i)
         return i;
      else
      {
         if (int.TryParse(value.ToString(), out var d))
         {
            return d;
         }
      }

      return 0;
   }
}

[Description("返回手动输入参数")]
public class ReturnParameter : IMesValueConverter
{
   public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter) =>
      manualParameter ?? "";
}

[Description("转换为时间格式YY-MM-DD HH:MM:SS")]
public class MesValueToDateTimeString : IMesValueConverter
{
    public object Convert(StyletIoC.IContainer container, IBatMainModel battery, string manualParameter, string autoParameter)
    {
        var value = battery[autoParameter];
        if (value == null)
            return string.Empty;

     
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("yy-MM-dd HH:mm:ss");
        }

        string strVal = value.ToString();
        if (DateTime.TryParse(strVal, out DateTime parseDt))
        {
            return parseDt.ToString("yy-MM-dd HH:mm:ss");
        }
        return string.Empty;
    }
}
#endregion
