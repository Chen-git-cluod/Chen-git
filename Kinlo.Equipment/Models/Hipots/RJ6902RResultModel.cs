namespace Kinlo.Equipment.Models;

public class RJ6902RResultModel
{
   public ushort 跌落1 { get; set; }
   public ushort 跌落2 { get; set; }
   public ushort VP电压 { get; set; }
   public ushort 升压时间 { get; set; }
   public float 电阻测试数据 { get; set; }
   public byte 开路结果 { get; set; }
   public byte 严重短路结果 { get; set; }
   public byte 欠压结果 { get; set; }
   public byte 过压结果 { get; set; }
   public byte 跌落1结果 { get; set; }
   public byte 跌落2结果 { get; set; }
   public byte TL结果 { get; set; }
   public byte TH结果 { get; set; }
   public byte 电阻测试结果 { get; set; }
   public byte 总结果 { get; set; }
   public string TestMsg { get; set; }
}
