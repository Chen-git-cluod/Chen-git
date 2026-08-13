using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.Equipment.Interfaces;

/// <summary>
/// 读卡器接口
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICardReader<T>
{
   Action<T>? CardAction { get; set; }
}
