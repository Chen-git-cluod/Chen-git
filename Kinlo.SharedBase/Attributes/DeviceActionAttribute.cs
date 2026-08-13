using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.SharedBase.Attributes
{
   public enum ActionType
   {
      /// <summary>只读，无需输入，结果显示在输出区</summary>
      Read,

      /// <summary>需要用户输入一个值再执行</summary>
      Write,

      /// <summary>纯操作，无输入无输出（如清零、复位）</summary>
      Action,
   }

   [AttributeUsage(AttributeTargets.Method)]
   public class DeviceActionAttribute : Attribute
   {
      public string Label { get; }
      public string Group { get; }
      public string Icon { get; }
      public ActionType ActionType { get; }
      public ActionStyle Style { get; }
      public int Order { get; }

      /// <summary>Write 类型时，输入框的占位提示</summary>
      public string Placeholder { get; }

      public DeviceActionAttribute(
         string label,
         ActionType actionType = ActionType.Action,
         string group = "操作",
         string icon = "",
         ActionStyle style = ActionStyle.Primary,
         string placeholder = "请输入值",
         int order = 0
      )
      {
         Label = label;
         ActionType = actionType;
         Group = group;
         Icon = icon;
         Style = style;
         Placeholder = placeholder;
         Order = order;
      }
   }

   public enum ActionStyle
   {
      Primary,
      Info,
      Success,
      Warning,
      Danger,
   }
}
