using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.SharedBase.Attributes
{
   [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
   public class DeviceCommandAttribute : Attribute
   {
      public ProcessTypeEnum[] ProcessTypes { get; }
      public CommunicationEnum[] Communications { get; }

      public DeviceCommandAttribute(ProcessTypeEnum[] processTypes)
      {
         ProcessTypes = processTypes;
         Communications = Array.Empty<CommunicationEnum>();
      }

      public DeviceCommandAttribute(CommunicationEnum[] communications)
      {
         Communications = communications;
         ProcessTypes = Array.Empty<ProcessTypeEnum>();
      }
   }
}
