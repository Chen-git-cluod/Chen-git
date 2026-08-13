namespace Kinlo.Common.Models.ConfigModels.UserModels;

[AddINotifyPropertyChangedInterface]
public class RoleModel
{
   /// <summary>
   /// 等级（ID），每个位代表一个部门，ulong.MaxValue 为默认超级管理员，ulong.MaxValue >> 1, 为默认管理员
   /// </summary>
   public ulong Level { get; set; } = 1;
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// 对应PLC权限等级
   /// </summary>
   public short PlcLevel { get; set; }

   /// <summary>
   /// 对应MES权限等级
   /// </summary>
   public string MESLevel { get; set; } = string.Empty;

   public RoleModel() { }

   /// <summary>
   ///
   /// </summary>
   /// <param name="level">权限</param>
   /// <param name="name"></param>
   /// <param name="plcLeve">对应发给plc的权限</param>
   public RoleModel(ulong level, string name, short plcLeve)
   {
      Level = level;
      Name = name;
      PlcLevel = plcLeve;
   }
}
