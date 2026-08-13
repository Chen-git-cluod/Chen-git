using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.MESDocking.南京国轩.Dto;

public class MesUserDto
{
   public string accountDept { get; set; }
   public string account { get; set; }
   public string accountName { get; set; }
   public string accountUser { get; set; }
   public string accountTel { get; set; }
   public Authlist[] authList { get; set; }
}

public class Authlist
{
   public string authName { get; set; }
   public string authCode { get; set; }
}
