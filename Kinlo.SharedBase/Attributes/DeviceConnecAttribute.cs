namespace Kinlo.SharedBase.Attributes;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public class DeviceConnecAttribute : Attribute
{
   /// <summary>
   //  工序任务名
   /// </summary>
   public ProcessTypeEnum ProcessesTaskType { get; set; }

   /// <summary>
   /// 设备型号
   /// </summary>
   public CommunicationEnum[] DeviceCommunicationTypes { get; set; } = new CommunicationEnum[0];

   ///// <summary>
   ///// TCP,UDP,SerialPort
   ///// </summary>
   //public ConnectTypeEnum CommunicationBaseType { get; set; } = ConnectTypeEnum.TCP;

   //public DeviceConnecAttribute(ConnectTypeEnum communicationBaseType, CommunicationEnum[] deviceCommunicationTypes)
   //{
   //    CommunicationBaseType = communicationBaseType;
   //    DeviceCommunicationTypes = deviceCommunicationTypes;
   //}
   public DeviceConnecAttribute(params CommunicationEnum[] deviceCommunicationTypes)
   {
      DeviceCommunicationTypes = deviceCommunicationTypes;
   }

   public DeviceConnecAttribute(ProcessTypeEnum processesTaskType, params CommunicationEnum[] deviceCommunicationTypes)
   {
      ProcessesTaskType = processesTaskType;
      DeviceCommunicationTypes = deviceCommunicationTypes;
   }
}
