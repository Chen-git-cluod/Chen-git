using Kinlo.Common.Models.ConfigModels.UserModels;
using Kinlo.MESDocking.南京国轩.Dto;
using static Kinlo.Common.DAL.DbHelper;

namespace Kinlo.MESDocking;

public static class MesValidatorNJGX
{
   const string codeName = "code",
      errorMsgName = "errorMsg";

   /// <summary>
   /// 通用验证器
   /// </summary>
   /// <param name="json"></param>
   /// <param name="logHeader"></param>
   /// <returns></returns>
   public static MesResultModel MesCommonParse(this string json, string logHeader)
   {
      return json.ParseJson(root =>
      {
         try
         {
            if (root.TryGetProperty(codeName, out var codeElement))
            {
               string errMsg = string.Empty;
               var isSuccess = codeElement.ToString() == "200"; //最外围验证

               if (!isSuccess) //外围失败，找MSG
               {
                  if (root.TryGetProperty(errorMsgName, out var msgElement))
                  {
                     errMsg = msgElement.GetString() ?? "";
                  }
                  else
                  {
                     errMsg = $"未找到[{errorMsgName}]字段！";
                  }
               }
               return isSuccess ? MesResultModel.OK(json) : MesResultModel.MesNg(errMsg, json);
            }
            else
            {
               return MesResultModel.MesNg($"未找到[{codeName}]字段", json);
            }
         }
         catch (Exception ex)
         {
            string msg = $"解析MES报文异常：{ex}";
            msg.LogProcess(logHeader, Log4NetLevelEnum.错误);
            return MesResultModel.MesNg(msg, json);
         }
      });
   }

   const double defualt = -0.0001;

   /// <summary>
   /// 获取一注数据解析
   /// </summary>
   /// <param name="mesResult"></param>
   /// <returns></returns>
   public static MesResultModel<PreProcessData[]> GetPrimaryInjectDataParse(this MesResultModel mesResult) =>
      ThenParse(
         mesResult,
         json =>
            json.ParseJson(root =>
            {
               try
               {
                  if (!root.TryGetProperty("data", out var data))
                     return MesResultModel<PreProcessData[]>.MesNg("未找到[data]字段！", mesResult.Response);
                  if (!data.TryGetProperty("products", out var products))
                     return MesResultModel<PreProcessData[]>.MesNg("未找到[products]字段！", mesResult.Response);

                  List<PreProcessData> injectDatas = new();

                  var batArray = products.EnumerateArray();

                  foreach (var product in batArray)
                  {
                     string barcode = product.GetProperty("productCode").GetString() ?? "";
                     double netWeight = defualt,
                        finalWeight = defualt;
                     JsonElement jsonElement;
                     foreach (var process in product.GetProperty("processes").EnumerateArray())
                     {
                        foreach (var paramCode in process.GetProperty("paramCodes").EnumerateArray())
                        {
                           if (paramCode.TryGetProperty("technicsParamCode", out jsonElement) && jsonElement.GetString() == "RYCZY10022")
                           {
                              var technicsParamValue = paramCode.GetProperty("technicsParamValue").GetString();
                              double.TryParse(technicsParamValue, out netWeight);
                           }
                           if (paramCode.TryGetProperty("technicsParamCode", out jsonElement) && jsonElement.GetString() == "RYCZY10028")
                           {
                              var technicsParamValue = paramCode.GetProperty("technicsParamValue").GetString();
                              double.TryParse(technicsParamValue, out finalWeight);
                           }
                        }
                     }
                     var isSuccess = (netWeight, finalWeight) switch
                     {
                        var p when p.netWeight == defualt || p.finalWeight == defualt => PrePrcessDataEnum.失败,
                        var p when p.netWeight < 10 => PrePrcessDataEnum.前工序数据不在范围,
                        _ => PrePrcessDataEnum.成功,
                     };
                     injectDatas.Add(new PreProcessData(isSuccess, netWeight, finalWeight, 0, 0, barcode));
                  }
                  return MesResultModel<PreProcessData[]>.OK(injectDatas.ToArray(), mesResult.Response);
               }
               catch (Exception ex)
               {
                  return MesResultModel<PreProcessData[]>.MesNg(ex.ToString(), mesResult.Response);
               }
            })
      );

   /// <summary>
   /// 提取工单
   /// </summary>
   /// <param name="mesResult"></param>
   /// <returns></returns>
   public static MesResultModel<WorkOrderDto[]> WorkOrderParse(this MesResultModel mesResult) =>
      ThenParse(
         mesResult,
         json =>
            json.ParseJson(root =>
            {
               if (!root.TryGetProperty("data", out var data))
                  return MesResultModel<WorkOrderDto[]>.MesNg("未找到[data]字段！", mesResult.Response);

               WorkOrderDto[] works = JsonSerializer.Deserialize<WorkOrderDto[]>(data.GetRawText()) ?? []; //取工单
               return MesResultModel<WorkOrderDto[]>.OK(works, mesResult.Response);
            })
      );

   /// <summary>
   /// 提取MES用户
   /// </summary>
   /// <param name="mesResult"></param>
   /// <returns></returns>
   public static MesResultModel<UserModel> MesLoginParse(this MesResultModel mesResult) =>
      ThenParse(
         mesResult,
         json =>
            json.ParseJson(root =>
            {
               if (!root.TryGetProperty("data", out var data))
                  return MesResultModel<UserModel>.MesNg("未找到[data]字段！", mesResult.Response);

               string dataStr = data.GetRawText();
               MesUserDto? mesUser = JsonSerializer.Deserialize<MesUserDto>(dataStr);
               if (mesUser == null)
                  return MesResultModel<UserModel>.MesNg($"反序列化失败:{dataStr}", mesResult.Response);

               var loginUser = new UserModel
               {
                  Account = mesUser.account,
                  Name = mesUser.accountName,
                  LoginTime = DateTime.Now,
                  Level = (ulong)DefaultRoleEnum.生产,
               };
               return MesResultModel<UserModel>.OK(loginUser, mesResult.Response);
            })
      );

   /// <summary>
   /// 成功后继续解析
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="mesResult"></param>
   /// <param name="func"></param>
   /// <returns></returns>
   private static MesResultModel<T> ThenParse<T>(this MesResultModel mesResult, Func<string, MesResultModel<T>> func)
   {
      switch (mesResult.ResultStatus)
      {
         case MesResultStatusEnum.成功:
            return func(mesResult.Response);
         case MesResultStatusEnum.生成报文失败:
            return MesResultModel<T>.RequestBuildError();
         default:
            return MesResultModel<T>.MesNg(mesResult.ErrMsg, mesResult.Response);
      }
   }

   /// <summary>
   /// 发送干重验证器
   /// </summary>
   /// <param name="json"></param>
   /// <returns></returns>
   public static MesResultModel SendDryWeight(this string json)
   {
      return json.ParseJson(root =>
      {
         if (root.TryGetProperty("message", out var message))
         {
            if (message.ToString() == "success")
               return MesResultModel.OK(json);
            else
               return MesResultModel.MesNg("", json);
         }
         else
         {
            return MesResultModel.MesNg($"未找到[{message}]字段", json);
         }
      }); //发送干重给分容
   }
}
