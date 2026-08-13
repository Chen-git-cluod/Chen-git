using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kinlo.Common.DAL;
using Kinlo.Equipment.Models.PLCInteractiveDTU;

namespace Kinlo.Common.Tools.RestoreData;

public static class RestareDataHelper
{
   /// <summary>
   /// 从日志中恢复数据并导出到 Excel
   /// </summary>
   /// <returns></returns>
   public static async Task RestoreDataFramLogs(IContainer container)
   {
      string folderPath = @"D:\User\Desktop\2025-06-12";
      string folderPath2 = @"D:\User\Desktop\2025-06-13";
      var jsonList = new List<PlcToPcEzInjectStationDTU>();

      // 用平衡组匹配“注液站数据：{...}”
      var regex = new Regex(@"注液站数据：(?'json'\{(?:[^{}]|(?<open>\{)|(?<-open>\}))*\}(?(open)(?!)))", RegexOptions.Compiled);

      foreach (var file in Directory.GetFiles(folderPath, "*.txt"))
      {
         foreach (var line in File.ReadLines(file))
         {
            var match = regex.Match(line);
            if (match.Success)
            {
               string jsonStr = match.Groups["json"].Value;

               try
               {
                  // 验证是否为合法 JSON
                  using var doc = JsonDocument.Parse(jsonStr);

                  jsonList.Add(JsonSerializer.Deserialize<PlcToPcEzInjectStationDTU>(jsonStr));
               }
               catch
               {
                  // JSON 不合法，跳过
               }
            }
         }
      }

      foreach (var file in Directory.GetFiles(folderPath2, "*.txt"))
      {
         foreach (var line in File.ReadLines(file))
         {
            var match = regex.Match(line);
            if (match.Success)
            {
               string jsonStr = match.Groups["json"].Value;

               try
               {
                  // 验证是否为合法 JSON
                  using var doc = JsonDocument.Parse(jsonStr);

                  jsonList.Add(JsonSerializer.Deserialize<PlcToPcEzInjectStationDTU>(jsonStr));
               }
               catch
               {
                  // JSON 不合法，跳过
               }
            }
         }
      }

      var bats = await container.Get<DbHelper>().GetBattereyListAsync();
      //foreach (var item in bats)
      //{
      //    var inj = jsonList.FirstOrDefault(x => x.ID == item.Id);
      //    if (inj != null)
      //    {
      //        ((IBatEzInjectStationModel)item).InjectionStardTime = inj.InjectionStardTime.ToLocalTimeFromSeconds().AddHours(-8);
      //        ((IBatEzInjectStationModel)item).InjectionEndTime = inj.InjectionEndTime.ToLocalTimeFromSeconds().AddHours(-8);
      //    }
      //}
      List<ExpandoObject> list = new List<ExpandoObject>();
      foreach (var item in bats)
      {
         var expando = new ExpandoObject() as IDictionary<string, object>;
         foreach (var binding in container.Get<DisplayDataCollection>().CompleteBatteryDatas.RuntimeBatteryType.GetProperties())
         {
            if (item.GetType().GetProperty(binding.Name) != null)
            {
               expando.Add(binding.Name, item.GetType().GetProperty(binding.Name)?.GetValue(item, null));
            }
         }
         list.Add((ExpandoObject)expando);
      }
      ExcelHelper.ExportBattery(
         list,
         container.Get<OtherParameterConfig>(),
         container.Get<DisplayDataCollection>().CompleteBatteryDatas.PropertyBindings
      );
   }
}
