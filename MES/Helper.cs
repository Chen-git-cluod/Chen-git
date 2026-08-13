using System;
using System.Threading.Tasks;

namespace MES
{
   public class Helper
   {
      #region 设备在线检测
      /// <summary>
      /// 设备在线检测
      /// </summary>
      /// <returns></returns>
      public async Task<bool> EquipmentOnline()
      {
         try
         {
            var _request = GetEquipmentOnlineRequest();
            SendService.RPServicesClient _servicesClient = new SendService.RPServicesClient();
            var _result = await _servicesClient.PutMesHeartDatasAsync(_request);
            return _result.Status;
         }
         catch (Exception ex)
         {
            $"{MesInterfaceNameEnum.设备在线检测}异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         }
         return false;
      }

      public dynamic GetEquipmentOnlineRequest()
      {
         dynamic _request = GetShareHeader();
         _request.CmdID = "001A";
         _request.Data = new dynamic[] { new { Heartbeat = "1" } };
         return _request;
      }

      #endregion
   }
}
