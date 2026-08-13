using Kinlo.LogNet.Models;
using Microsoft.AspNetCore.Http.Extensions;

namespace Kinlo.WebApi;

public static class WebHelper
{
   public static WebLogModel GetWebLogInfo(this string apiName, HttpRequest httpRequest) =>
      new WebLogModel
      {
         InterfaceName = apiName,
         Status = StatusTypeEnum.失败,
         StartTime = DateTime.Now,
         Level = LogNet.Enums.Log4NetLevelEnum.错误,
         Url = httpRequest.GetDisplayUrl(), //Request.Path;
      };
}
