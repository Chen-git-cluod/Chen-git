namespace Kinlo.Common.Dto;

public class MesInterfaceInfoDto
{
   public string MesName { get; set; } = "默认MES";
   public List<MesInterfaceDescription> InterfaceDescriptions { get; set; } = new();
}

public record MesInterfaceDescription(string interfaceName, string url, int pollingInterval, Type parameterType);
