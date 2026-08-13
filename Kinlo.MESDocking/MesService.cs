using Kinlo.LogNet.Models;

namespace Kinlo.MESDocking;

public partial class MesService
{
   protected IContainer _container;
   protected ParameterConfig _parameterConfig;
   protected MesInterfaceParameterConfig _mesInterfacParameterConfig;
   protected MesParameterConfig _mesParameterConfig;
   protected HttpClientSingleHelper _httpClientSingle;
   protected OtherParameterConfig _otherParameterConfig;
   protected UsersStatusConfig _usersStatusConfig;

   // 创建Request委托的集合
   private static readonly ConcurrentDictionary<Type, Func<IMesArgs, string>> _buildRequestDic = new();

   /// <summary>
   /// 如有些服务器一定要加一个固定的参数，可在此设定，无需每一次都加
   /// </summary>
   private Dictionary<string, string>? _queryParams = null;

   public MesService(IContainer container)
   {
      _container = container;
      _httpClientSingle = container.Get<HttpClientSingleHelper>();
      _parameterConfig = container.Get<ParameterConfig>();
      _mesInterfacParameterConfig = container.Get<MesInterfaceParameterConfig>();
      _mesParameterConfig = container.Get<MesParameterConfig>();
      _otherParameterConfig = container.Get<OtherParameterConfig>();
      _usersStatusConfig = container.Get<UsersStatusConfig>();
      _queryParams = new Dictionary<string, string> { { "tenantID", _parameterConfig.DeviceParameter.LineName.ToString() } };
   }

   #region 发送MES请求
   /// <summary>
   /// 发送MES请求（无额外返回Data）
   /// </summary>
   /// <param name="call">请求体</param>
   /// <param name="barcode"></param>
   /// <param name="validator">验证器（无额外返回Data）</param>
   /// <param name="queryParams">有部分接口需要Url加参数</param>
   /// <param name="isFullScreenOnFail">是否全屏弹窗，是就全屏弹窗，否就左下角弹窗</param>
   /// <returns></returns>
   public async Task<MesResultModel> SendAsync(
      MesApiCall call,
      string? barcode,
      Func<string, MesResultModel>? validator,
      Dictionary<string, string>? queryParams = null,
      bool isFullScreenOnFail = true
   )
   {
      if (call.MesArgs != null)
      {
         var requestResult = RequestBuild(call.MesArgs);
         if (requestResult.isSuccess) //获取mes报文失败，退出；
            call.Request = requestResult.request;
         else
            return MesResultModel.RequestBuildError();
      }

      var httpResult = await SendCoreAsync(call.InterfaceName, call.Request, call.Url, barcode, queryParams);

      MesResultModel? mesResult = null;
      if (!httpResult.isSuccess) //失败，直接返回报文
      {
         mesResult = MesResultModel.CommFail(httpResult.response, httpResult.response);
      }
      else
      {
         if (validator == null) //手工测试或无验证器，直接返回报文
            mesResult = MesResultModel.OK(httpResult.response);
         else
            mesResult = validator(httpResult.response);
      }

      //不等待弹窗
      _ = ShowDialog(
         mesResult.ResultStatus == MesResultStatusEnum.成功,
         call.InterfaceName,
         mesResult.Response,
         mesResult.ErrMsg,
         isFullScreenOnFail
      );
      return mesResult;
   }

   /// <summary>
   ///  发送MES请求（有额外返回Data）
   /// </summary>
   /// <typeparam name="TData"></typeparam>
   /// <param name="call">请求体</param>
   /// <param name="barcode"></param>
   /// <param name="validator">验证器（有额外返回Data）</param>
   /// <param name="queryParams">有部分接口需要Url加参数</param>
   /// <param name="isFullScreenOnFail">是否全屏弹窗，是就全屏弹窗，否就左下角弹窗</param>
   /// <returns></returns>
   public async Task<MesResultModel<TData>> SendAsync<TData>(
      MesApiCall call,
      string? barcode,
      Func<string, MesResultModel<TData>>? validator,
      Dictionary<string, string>? queryParams = null,
      bool isFullScreenOnFail = true
   )
   {
      if (call.MesArgs != null)
      {
         var requestResult = RequestBuild(call.MesArgs);
         if (requestResult.isSuccess) //获取mes报文失败，退出；
            call.Request = requestResult.request;
         else
            return MesResultModel<TData>.RequestBuildError();
      }

      var httpResult = await SendCoreAsync(call.InterfaceName, call.Request, call.Url, barcode, queryParams);

      MesResultModel<TData>? mesResult = null;
      if (!httpResult.isSuccess) //失败，直接返回报文
      {
         mesResult = MesResultModel<TData>.CommFail(httpResult.response, httpResult.response);
      }
      else
      {
         if (validator == null) //手工测试或无验证器，直接返回报文
            mesResult = MesResultModel<TData>.OK(default, httpResult.response);
         else
            mesResult = validator(httpResult.response);
      }

      //不等待弹窗
      _ = ShowDialog(
         mesResult.ResultStatus == MesResultStatusEnum.成功,
         call.InterfaceName,
         mesResult.Response,
         mesResult.ErrMsg,
         isFullScreenOnFail
      );
      return mesResult;
   }

   /// <summary>
   /// 发送MES方法
   /// </summary>
   /// <param name="mesInterfaceName"></param>
   /// <param name="requestMessage"></param>
   /// <param name="url"></param>
   /// <param name="barcode"></param>
   /// <param name="queryParams"></param>
   /// <returns></returns>
   private async Task<(bool isSuccess, string response)> SendCoreAsync(
      string mesInterfaceName,
      string requestMessage,
      string url,
      string? barcode,
      Dictionary<string, string>? queryParams = null
   )
   {
      barcode ??= string.Empty;
      queryParams ??= _queryParams;
      try
      {
         HttpContent httpContent = new StringContent(requestMessage, Encoding.UTF8, "application/json");
         if (queryParams != null && queryParams.Count > 0)
         {
            if (string.IsNullOrEmpty(requestMessage))
               httpContent = new FormUrlEncodedContent(queryParams);
            else
            {
               // 构造带参数的 URL
               var query = string.Join("&", queryParams.Select(kv => $"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value)}"));
               url += "?" + query;
            }
         }

         $"[{mesInterfaceName}]开始上传;".LogRun();
         DateTime startTime = DateTime.Now;
         var httpResult = await _httpClientSingle.PostAsync(url, httpContent);
         DateTime endTime = DateTime.Now;

         //打印MES日志
         mesInterfaceName.LogMes(
            state: httpResult.isSuccess ? StatusTypeEnum.成功 : StatusTypeEnum.失败,
            barcode: barcode,
            startTime: startTime,
            endTime: endTime,
            url: url,
            requestedMsg: requestMessage, //请求报文
            receiveMsg: httpResult.content, //MES返回报文
            languageDic: _otherParameterConfig.CurrentLanguagesDictionary, //语言字典
            level: httpResult.isSuccess ? Log4NetLevelEnum.成功 : Log4NetLevelEnum.错误
         ); //log等级

         return httpResult;
      }
      catch (Exception ex)
      {
         var errMsg = $"[{mesInterfaceName}]上传MES异常：{ex};";
         errMsg.LogRun(Log4NetLevelEnum.错误, true);
         return (false, errMsg);
      }
   }

   /// <summary>
   /// 失败弹窗
   /// </summary>
   /// <param name="isSuccess"></param>
   /// <param name="interfaceName"></param>
   /// <param name="response"></param>
   /// <param name="errmsg"></param>
   /// <param name="isFullScreenOnFail">是否全屏弹窗，是就全屏弹窗，否就左下角弹窗</param>
   /// <returns></returns>
   private async Task ShowDialog(bool isSuccess, string interfaceName, string response, string errmsg, bool isFullScreenOnFail)
   {
      try
      {
         if (!isSuccess) //失败
         {
            if (isFullScreenOnFail) //失败全屏弹窗
               await UIThreadHelper.InvokeOnUiThreadAsync(() =>
                  Dialog.Show(
                     new AlarmDialog(new AlarmDialogDto($"错误：{errmsg}\r\n返回内容：{response}", $"MES [{interfaceName}] 失败")),
                     GenericHelper.FullScreenAlarmToken
                  )
               );
            else
               Growl.Warning($"MES==={interfaceName}===失败：{errmsg}\r\n返回内容：{response}");
         }
         $"[{interfaceName}]上传MES完成，结果：{(isSuccess ? "成功" : "失败")};".LogRun(
            isSuccess ? Log4NetLevelEnum.成功 : Log4NetLevelEnum.错误
         );
      }
      catch (Exception ex)
      {
         $"[解析MES弹窗异常]{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }
   #endregion

   /// <summary>
   /// 创建MES请求报文
   /// </summary>
   /// <param name="args"></param>
   /// <returns></returns>
   public (bool isSuccess, string request) RequestBuild(IMesArgs args)
   {
      try
      {
         var argsType = args.GetType();

         if (_buildRequestDic.TryGetValue(argsType, out var handler))
         {
            string request = handler(args);
            return (true, request ?? "");
         }
         string msg = $"未找到 [{argsType.Name}] 相关获取MES报文方法！";
         msg.LogRun(Log4NetLevelEnum.错误, true);
         return (false, msg);
      }
      catch (Exception ex)
      {
         string msg = $"获取MES请求报文异常：{ex}";
         msg.LogRun(Log4NetLevelEnum.错误, true);
         return (false, msg);
      }
   }

   /// <summary>
   /// 注册MES接口
   /// </summary>
   /// <returns></returns>
   public MesInterfaceInfoDto RegisterMesInterface()
   {
      //可以按条件切换不同MES
      var mesRequestBuild = new MesRequestBuildNJGX(_container);
      Type RequestType = mesRequestBuild.GetType();

      MesInterfaceInfoDto interfaceInfo = new MesInterfaceInfoDto();

      var lange = RequestType.GetCustomAttribute<LanguagesAttribute>()?.Languages;
      if (lange != null && lange.Length > 0)
         interfaceInfo.MesName = lange[0];

      var handlerMethods = mesRequestBuild.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).ToList();
      foreach (var handlerMethod in handlerMethods)
      {
         var infoAttr = handlerMethod.GetCustomAttribute<MesInterfaceInfoAttribute>();
         var langAttr = handlerMethod.GetCustomAttribute<LanguagesAttribute>();
         if (infoAttr == null || langAttr == null || langAttr.Languages.Length < 1)
            continue;

         if (handlerMethod.ReturnType != typeof(string))
            continue; //需返回string类型

         var parameters = handlerMethod.GetParameters();
         if (parameters.Length != 1)
            continue; //需只有一个参数
         Type paramType = parameters[0].ParameterType;
         if (!typeof(IMesArgs).IsAssignableFrom(paramType))
            continue; //参数要继承自IMesArgs

         string interfaceNmae = langAttr.Languages[0];

         interfaceInfo.InterfaceDescriptions.Add(
            new MesInterfaceDescription(interfaceNmae, infoAttr.Url, infoAttr.PollingInterval, paramType)
         );

         _buildRequestDic[paramType] = args => (string)handlerMethod.Invoke(mesRequestBuild, [args])!;
      }
      return interfaceInfo;
   }
}
