namespace Kinlo.SharedBase.Attributes;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public class BatteryDisplayAttribute : Attribute
{
   //public int Index  { get; set; }
   ///// <summary>
   ///// 描述
   ///// </summary>
   //public string Description { get; set; } = string.Empty;

   /// <summary>
   /// 忽略
   /// </summary>
   public bool IsIgnore { get; set; }

   /// <summary>
   /// 工序
   /// </summary>
   public ProcessTypeEnum[] DisplayProcesses { get; set; } = new ProcessTypeEnum[0];

   /// <summary>
   ///
   /// </summary>
   public CommunicationEnum[] DeviceCommunicationType { get; set; } = new CommunicationEnum[0];
}
