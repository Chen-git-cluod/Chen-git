using System;
using System.Collections.Generic;
using System.Text;

namespace MES
{
   /// <summary>
   /// 瑞普三期，机场
   /// </summary>
   public class RP3PhaseVerify
   {
      MesInterfaceNameEnum _mesInterfaceType;

      public RP3PhaseVerify(MesInterfaceNameEnum mesInterfaceType)
      {
         _mesInterfaceType = mesInterfaceType;
      }

      public HttpResultModel Verify(string json)
      {
         return _mesInterfaceType switch
         {
            MesInterfaceNameEnum.获取指纹权限级别 => Fingerprint(json),
            MesInterfaceNameEnum.员工卡号校验 => CardNumber(json),
            MesInterfaceNameEnum.一次注液进站 or MesInterfaceNameEnum.二次注液进站 => InjectionInbound(json, _mesInterfaceType),
            MesInterfaceNameEnum.获取一注信息接口 => GetOneInjectionData(json),
            _ => General(json),
         };
      }

      /// <summary>
      /// 常规接口验证
      /// </summary>
      /// <param name="json"></param>
      /// <returns></returns>
      private HttpResultModel General(string json)
      {
         HttpResultModel _result = new HttpResultModel(true, json, string.Empty);
         try
         {
            var _data = JsonDocument.Parse(json).RootElement.GetProperty("Data").EnumerateArray().FirstOrDefault();
            JsonElement _retJE = _data.GetProperty("Ret");
            string _res =
               _retJE.ValueKind == JsonValueKind.String
                  ? _data.GetProperty("Ret").GetString()!
                  : _data.GetProperty("Ret").GetInt32().ToString();

            if (_res != "0")
            {
               _result.Status = false;
               _result.ErrMsg = _data.GetProperty("Msg").GetString();
            }
         }
         catch (Exception ex)
         {
            _result.Status = false;
            _result.ErrMsg = $"MES返回报文解析异常：{ex};";
         }
         return _result;
      }

      /// <summary>
      /// 获取指纹权限级别
      /// </summary>
      /// <param name="json"></param>
      /// <returns></returns>
      private HttpResultModel Fingerprint(string json)
      {
         HttpResultModel _result = new HttpResultModel(true, json, string.Empty);
         try
         {
            var _data = JsonDocument.Parse(json).RootElement.GetProperty("Data").EnumerateArray().FirstOrDefault();
            if (
               _data.TryGetProperty("StaffNumber", out JsonElement StaffNumberJE)
               && _data.TryGetProperty("PermissionLevel", out JsonElement PermissionLevelJE)
            )
            {
               _result.Data.Add("StaffNumber", StaffNumberJE.GetString());
               _result.Data.Add("PermissionLevel", PermissionLevelJE.GetString());
            }
            else
            {
               _result.Status = false;
               _result.ErrMsg = "缺少必要字段[StaffNumber]或[PermissionLevel]!";
            }
         }
         catch (Exception ex)
         {
            _result.Status = false;
            _result.ErrMsg = $"MES返回报文解析异常：{ex};";
         }
         return _result;
      }

      /// <summary>
      /// 员工卡号校验
      /// </summary>
      /// <param name="json"></param>
      /// <returns></returns>
      private HttpResultModel CardNumber(string json)
      {
         HttpResultModel _result = new HttpResultModel(true, json, string.Empty);
         try
         {
            var _data = JsonDocument.Parse(json).RootElement;
            if (_data.TryGetProperty("Result", out JsonElement resultJE) && _data.TryGetProperty("Level", out JsonElement levelJE))
            {
               if (resultJE.GetString().ToUpper() == "OK")
               {
                  _result.Data.Add("Level", levelJE.GetString());
               }
               else
               {
                  _result.Status = false;
                  _result.ErrMsg = _data.GetProperty("Message").GetString();
               }
            }
            else
            {
               _result.Status = false;
               _result.ErrMsg = "缺少必要字段[Result]或[Level]!";
            }
         }
         catch (Exception ex)
         {
            _result.Status = false;
            _result.ErrMsg = $"MES返回报文解析异常：{ex};";
         }
         return _result;
      }

      /// <summary>
      /// 一次(二次)注液进站
      /// </summary>
      /// <param name="json"></param>
      /// <returns></returns>
      private HttpResultModel InjectionInbound(string json, MesInterfaceNameEnum mesInterfaceName)
      {
         HttpResultModel _result = new HttpResultModel(true, json, string.Empty);
         try
         {
            var _data = JsonDocument.Parse(json).RootElement;
            if (_data.TryGetProperty("Result", out JsonElement resultJE))
            {
               if (resultJE.GetString().ToUpper() == "OK")
               {
                  if (mesInterfaceName == MesInterfaceNameEnum.一次注液进站)
                  {
                     if (_data.TryGetProperty("MarkingCode", out JsonElement markingCodeJE))
                        _result.Data.Add("MarkingCode", markingCodeJE.GetString().Split(";").ToArray());
                  }
               }
               else
               {
                  _result.Status = false;
                  _result.ErrMsg = _data.GetProperty("Message").GetString();
               }
            }
            else
            {
               _result.Status = false;
               _result.ErrMsg = "缺少必要字段[Result]!";
            }
         }
         catch (Exception ex)
         {
            _result.Status = false;
            _result.ErrMsg = $"MES返回报文解析异常：{ex};";
         }
         return _result;
      }

      /// <summary>
      /// 获取一注信息接口
      /// </summary>
      /// <param name="json"></param>
      /// <returns></returns>
      private HttpResultModel GetOneInjectionData(string json)
      {
         HttpResultModel _result = new HttpResultModel(true, json, string.Empty);
         try
         {
            var _data = JsonDocument.Parse(json).RootElement;
            if (_data.TryGetProperty("beforeWeight", out JsonElement beforeWeightJE))
            {
               if (beforeWeightJE.TryGetDouble(out double beforeWeight))
               {
                  _result.Data.Add("beforeWeight", beforeWeight);
               }
               else
               {
                  _result.Status = false;
                  // _result.ErrMsg = _data.GetProperty("Message").GetString();
               }
            }
            else
            {
               _result.Status = false;
               _result.ErrMsg = "缺少必要字段[Result]!";
            }
         }
         catch (Exception ex)
         {
            _result.Status = false;
            _result.ErrMsg = $"MES返回报文解析异常：{ex};";
         }
         return _result;
      }
   }
}
