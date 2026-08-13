using System.Windows.Media;

namespace Kinlo.SharedBase.Enums;

/// <summary>
/// PLC设备状态
/// </summary>
[Languages]
[Flags]
public enum DeviceStateEnum : ushort
{
   /// <summary>
   /// 报警为被动停机
   /// </summary>
   [DeviceStateAttribute("#FF6384")] //红色 2
   [Languages("报警")]
   报警 = 1,

   [DeviceStateAttribute("#FFFFC1")] //黄色 4
   [Languages("堵料")]
   堵料 = 2,

   [DeviceStateAttribute("#00BBF0")] //蓝色 8
   [Languages("待料")]
   待料 = 3,

   [DeviceStateAttribute("#29CC9F")] //绿色 16
   [Languages("运行")]
   运行 = 4,

   /// <summary>
   /// 待机为主动停机
   /// </summary>
   [DeviceStateAttribute("#8395A2")] //灰色 32
   [Languages("待机")]
   待机 = 5,
}

//(Brush) new BrushConverter().ConvertFromString("#29CC9F"),//绿色
//            (Brush) new BrushConverter().ConvertFromString("#FF6384"), //红色
//            (Brush) new BrushConverter().ConvertFromString("#FFFFC1"), //黄色
//            (Brush) new BrushConverter().ConvertFromString("#00BBF0"), //蓝色
//            (Brush) new BrushConverter().ConvertFromString("#8395A2"),//灰色
//            (Brush) new BrushConverter().ConvertFromString("#E9B4F8"),
//            (Brush) new BrushConverter().ConvertFromString("#4DF3FF"),
//            (Brush) new BrushConverter().ConvertFromString("#940078"),
