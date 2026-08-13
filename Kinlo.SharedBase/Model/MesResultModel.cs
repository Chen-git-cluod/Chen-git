namespace Kinlo.SharedBase.Model;

public class MesResultModel
{
   /// <summary>MES返回状态</summary>
   public MesResultStatusEnum ResultStatus { get; set; }

   /// <summary>MES原始报文</summary>
   public string Response { get; set; } = string.Empty;

   /// <summary>返回消息（错误提示）</summary>
   public string ErrMsg { get; set; } = string.Empty;

   /// <summary>成功</summary>
   public static MesResultModel OK(string response, string message = "成功") =>
      new()
      {
         ResultStatus = MesResultStatusEnum.成功,
         Response = response,
         ErrMsg = message,
      };

   /// <summary>MES判定NG</summary>
   public static MesResultModel MesNg(string message, string response) =>
      new()
      {
         ResultStatus = MesResultStatusEnum.MES判定NG,
         Response = response,
         ErrMsg = message,
      };

   /// <summary>通讯错误</summary>
   public static MesResultModel CommFail(string message, string response) =>
      new()
      {
         ResultStatus = MesResultStatusEnum.通讯错误,
         Response = response,
         ErrMsg = message,
      };

   /// <summary>生成MES报文失败</summary>
   public static MesResultModel RequestBuildError(string message = "生成MES报文失败") =>
      new() { ResultStatus = MesResultStatusEnum.生成报文失败, ErrMsg = message };
}

public class MesResultModel<TData> : MesResultModel
{
   /// <summary>MES返回处理后的数据</summary>
   public TData? Data { get; set; }

   /// <summary>成功（含数据）</summary>
   public static MesResultModel<TData> OK(TData data, string response, string message = "成功") =>
      new()
      {
         ResultStatus = MesResultStatusEnum.成功,
         Data = data,
         Response = response,
         ErrMsg = message,
      };

   /// <summary>MES判定NG（无数据）</summary>
   public static new MesResultModel<TData> MesNg(string message, string response) =>
      new()
      {
         ResultStatus = MesResultStatusEnum.MES判定NG,
         Response = response,
         ErrMsg = message,
      };

   /// <summary>通讯错误（无数据）</summary>
   public static new MesResultModel<TData> CommFail(string message, string response) =>
      new()
      {
         ResultStatus = MesResultStatusEnum.通讯错误,
         Response = response,
         ErrMsg = message,
      };

   /// <summary>接口未启用（无数据）</summary>
   public static new MesResultModel<TData> RequestBuildError(string message = "生成MES报文失败") =>
      new() { ResultStatus = MesResultStatusEnum.生成报文失败, ErrMsg = message };
}
