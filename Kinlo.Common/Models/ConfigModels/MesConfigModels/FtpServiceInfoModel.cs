using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.Common.Models.ConfigModels.MesConfigModels;

public class FtpServiceInfoModel
{
   public string Host { get; set; } = "10.2.100.71";
   public string Name { get; set; } = "cellplant6cl";
   public string Password { get; set; } = "lzgx@1234";
   public int Port { get; set; } = 21;

   /// <summary>
   /// 主机目录
   /// </summary>
   public string RemoteDirectory { get; set; } = "/02三工段-L200/C2000二次注液/";

   /// <summary>
   /// 上传文件超时时间(ms)
   /// </summary>
   public int UploadTimeoutMs { get; set; } = 30000;
   public bool Enable { get; set; } = false;
}
