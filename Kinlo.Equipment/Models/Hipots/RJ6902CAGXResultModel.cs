namespace Kinlo.Equipment.Models;

public class RJ6902CAGXResultModel
{
   public ushort 跌落1 { get; set; }
   public ushort 跌落2 { get; set; }
   public ushort 跌落3 { get; set; }
   public ushort VP电压 { get; set; }
   public ushort 升压时间 { get; set; }
   public float 电阻测试数据 { get; set; }
   public float 电容测试数据 { get; set; }

   /// <summary>
   /// E01 检测到电池开路，可能是接触不好或者连接线断开
   /// </summary>
   public byte 开路结果 { get; set; }

   /// <summary>
   /// E02：严重短路，电压无法爬升
   /// </summary>
   public byte 放电1结果 { get; set; }

   /// <summary>
   /// E04：检测到电压超出设置电压正常范围 FF 为不合格，1 为合格
   /// </summary>
   public byte VP结果 { get; set; }

   /// <summary>
   /// E03：电压未在时间允许范围内爬升至设置电压
   /// </summary>
   public byte 放电2结果 { get; set; }

   /// <summary>
   /// E05：在升压阶段，检测到跌落电压低于设定的跌落门限（Vd1）
   /// </summary>
   public byte 跌落1结果 { get; set; }

   /// <summary>
   /// E06：在电压持续阶段，检测到跌落电压低于设定的跌落门限（Vd2）
   /// </summary>
   public byte 跌落2结果 { get; set; }

   /// <summary>
   /// E11：在电压放电阶段，检测到跌落电压低于设定的跌落门限（Vd3）
   /// </summary>
   public byte 跌落3结果 { get; set; }

   /// <summary>
   /// E07：实际上升时间（Tp）小于电压上升时间的下限值（TL）
   /// </summary>
   public byte TL结果 { get; set; }

   /// <summary>
   /// E08：实际上升时间（Tp）大于电压上升时间的上限值（TH）
   /// </summary>
   public byte TH结果 { get; set; }

   /// <summary>
   /// E09： 实际测量电阻值小于设定的电阻限值（R） FF 为不合格，1 为合格
   /// </summary>
   public byte 电阻测试结果 { get; set; }

   /// <summary>
   /// E10：实际测试电容值不在设定的电容测试范围内（C） FF 为不合格，1 为合格
   /// </summary>
   public byte 电容测试结果 { get; set; }

   /// <summary>
   /// FF 为不合格，1 为合格
   /// </summary>
   public byte 总结果 { get; set; }

   public string TestMsg { get; set; } = string.Empty;
}
