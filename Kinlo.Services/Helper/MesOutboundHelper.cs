namespace Kinlo.Services.Helper;

public class MesOutboundHelper
{
   #region 上传MES
   /// <summary>
   /// 生产过程上传MES结果数据
   /// </summary>
   /// <param name="container"></param>
   /// <param name="mesService"></param>
   /// <param name="batt"></param>
   /// <param name="logHeader">日志头</param>
   /// <returns></returns>
   public static async Task ProductionMesOutput(IContainer container, MesService mesService, IBatMainModel batt, string logHeader)
   {
      var result = await MesOutput(container, mesService, batt, logHeader);
      if (result is OutputStatus.未上传 or OutputStatus.生成报文失败 or OutputStatus.通讯错误 or OutputStatus.异常)
         await SaveMesResend(container, batt, logHeader);
   }

   /// <summary>
   /// 上传MES结果数据
   /// </summary>
   /// <param name="container"></param>
   /// <param name="mesService"></param>
   /// <param name="batt"></param>
   /// <param name="logHeader">日志头</param>
   /// <returns></returns>
   public static async Task<OutputStatus> MesOutput(IContainer container, MesService mesService, IBatMainModel batt, string logHeader)
   {
      batt.OutputTime = DateTime.Now;

      try
      {
         var mesInterfaceParameterConfig = container.Get<MesInterfaceParameterConfig>();
         var call = mesInterfaceParameterConfig.GetApiCall(new MesRequestBuildNJGX.ArgsMesExit(batt));
         if (call == null || !call.IsEnable)
         {
            return OutputStatus.未上传;
         }
         var result = await mesService.SendAsync(call, batt.Barcode, receive => receive.MesCommonParse(logHeader));

         batt.MesOutputStatus = result.ResultStatus.ToResult();

         return result.ResultStatus switch
         {
            MesResultStatusEnum.成功 => OutputStatus.成功,
            MesResultStatusEnum.生成报文失败 => OutputStatus.生成报文失败,
            MesResultStatusEnum.通讯错误 => OutputStatus.通讯错误,
            _ => OutputStatus.MES判定NG,
         };
      }
      catch (Exception ex)
      {
         $"MES出站异常：{ex}".LogProcess(logHeader, Log4NetLevelEnum.错误);
         batt.MesOutputStatus = ResultTypeEnum.MES异常;
         return OutputStatus.异常;
      }
   }

   /// <summary>
   /// 插入补传表
   /// </summary>
   /// <param name="container"></param>
   /// <param name="batt"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static async Task SaveMesResend(IContainer container, IBatMainModel batt, string logHeader)
   {
      var paramterConfig = container.Get<ParameterConfig>();
      if (!paramterConfig.FunctionEnable.IsEnableMesResend)
         return;

      var db = container.Get<DbHelper>();
      var resend = new MesResendModel
      {
         Barcode = batt.Barcode,
         Id = batt.Id,
         CreateTime = DateTime.Now,
         LastResult = ResultTypeEnum._,
         LastUpdateTime = DateTime.Now,
         ResendStatus = ResendStatusEnum.未上传,
      };
      //插入补传列表
      await db.InsertOrUpdateMesResendAsync(resend, logHeader);
   }
   #endregion
}

public enum OutputStatus
{
   成功,
   MES判定NG,
   未上传,
   生成报文失败,
   通讯错误,
   异常,
}
