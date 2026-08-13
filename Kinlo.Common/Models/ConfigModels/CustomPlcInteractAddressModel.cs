using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.Common.Models.ConfigModels;

/// <summary>
/// 自定义PLC交互地址
/// </summary>
[AddINotifyPropertyChangedInterface]
public class CustomPlcInteractAddressModel
{
   public int Id { get; set; }

   /// <summary>
   ///  自定义交互名
   /// </summary>
   public CustomInteractNameEnum CustomInteractName { get; set; }

   /// <summary>
   /// 设备数量或数据长度
   /// </summary>
   public int DataLength { get; set; } = 1;

   /// <summary>
   /// 数据交互标签
   /// </summary>
   public SignalAddressModel DataAddress { get; set; } = new();

   /// <summary>
   /// 备用：其它数据
   /// </summary>
   public SignalAddressModel ExtraDataAddress { get; set; } = new();

   /// <summary>
   /// 是否启用
   /// </summary>
   public bool IsEnable { get; set; } = true;
}

/// <summary>
///
/// </summary>
public enum CustomInteractNameEnum
{
   PC至PLC用户权限,
   PC至PLC用户名,
   PC至PLC生产数据,
   PC下发MES参数,

   PLC参数上报MES,
   PLC主动停机原因,
   PLC同步PC时间,
}
