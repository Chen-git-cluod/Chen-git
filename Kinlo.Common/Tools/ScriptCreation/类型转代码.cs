using Microsoft.CodeAnalysis;

namespace Kinlo.Common.Tools.ScriptCreation;

public static class TypesToCode
{
   public static string BuildHeaderCode()
   {
      return $"using System;\r\n"
         + $"using System.ComponentModel;\r\n"
         + $"using System.Runtime.CompilerServices;\r\n"
         + $"using Kinlo.Common.Tools;\r\n"
         + $"using Kinlo.SharedBase.Attributes;\r\n"
         + $"using Kinlo.SharedBase.Enums;\r\n"
         + $"using Kinlo.Common.Attributes;\r\n"
         + $"using SqlSugar;\r\n\r\n"
         + $"namespace Kinlo.Common.Models.BatteryModels;\r\n\r\n";
   }

   /// <summary>
   /// 在现有多个类中取属性生成新动态类
   /// </summary>
   /// <param name="types">基类</param>
   /// <param name="propertyNames">属性名</param>
   /// <param name="className">生成动态类后的类名</param>
   /// <returns></returns>
   public static string BuildClassCode(this List<Type> types, string className)
   {
      #region 接口名
      List<string> interfaceNames = new List<string>();

      foreach (var type in types) //加接口
      {
         var interfaces = type.GetInterfaces();
         if (interfaces != null)
         {
            foreach (var interfac in interfaces)
            {
               // if (interfac.Name!= "INotifyPropertyChanged" && !interfaceNames.Any(x => x == interfac.Name))
               if (!interfaceNames.Any(x => x == interfac.Name))
                  interfaceNames.Add(interfac.Name);
            }
         }
      }

      string interfaceNamesStr = string.Empty;
      if (interfaceNames.Count > 0)
      {
         interfaceNamesStr = string.Join(",", interfaceNames);
      }
      #endregion

      List<ResultInfo> resultList = new(); //用于 生成 UpdateFinalResult 方法
      List<PropertyInfo> properties = new(); //用于 swicth 生成
      StringBuilder classCode = new StringBuilder(); //类代码
      List<object?> entitys = types.Select(x => Activator.CreateInstance(x)).ToList(); //创建实体，取默认值

      Dictionary<string, int> propertyNames = new Dictionary<string, int>();
      string classAttributeStr = string.Empty; //类的特性
      foreach (var entity in entitys)
      {
         if (entity.GetType().Name == "BatMainModel")
         {
            var attStr = entity.GetType().GetAttributeToStringList();
            classAttributeStr = string.Join(" ", attStr);
         }

         foreach (var property in entity.GetType().GetProperties())
         {
            if (property.Name == "Item")
               continue;

            var dynamicClassAtt = property.GetCustomAttributes<DynamicClassAttribute>().FirstOrDefault();

            if (dynamicClassAtt != null && dynamicClassAtt.IsIgnoreProperty)
               continue;

            if (propertyNames.TryAdd(property.Name, 0))
            {
               if (
                  property.PropertyType == typeof(ResultTypeEnum)
                  && property.Name != nameof(BatMainModel.FinalStatus)
                  && dynamicClassAtt != null
                  && !dynamicClassAtt.IsIgnoreForFinalStatus
               )
               {
                  resultList.Add(
                     new ResultInfo(property.Name, dynamicClassAtt.Process, dynamicClassAtt.ParentResult, dynamicClassAtt.Rule)
                  );
               }
               properties.Add(property);
               classCode.Append(CreatePropertyCode(property, entity));
            }
         }
      }

      return $"\r\n    {classAttributeStr}"
         + $"    public class {className} : {interfaceNamesStr}\r\n"
         + $"    {{\r\n"
         + $"{classCode}\r\n"
         + $"{resultList.BuildUpdateFinalResultCode()}"
         + $"{properties.BuildSwitchCode()}"
         + $"{Helper.ViewModelBase}\r\n"
         + $"}}\r\n";
   }

   record ResultInfo(string propertyName, ProcessTypeEnum process, string parentResult, ProductResultRuleEnum childResultRule);

   /// <summary>
   /// 更新结果方法，MES独立出来，生产状态列
   /// </summary>
   /// <param name="results"></param>
   /// <returns></returns>
   private static string BuildUpdateFinalResultCode(this List<ResultInfo> results)
   {
      //此处改为在工序结果标记是否参数最终判定，不写死
      //var mesEntryResult = results.FirstOrDefault(x => x.Key == nameof(BatMainModel.MesEntryStatus));
      //if (mesEntryResult.Key != null)
      //{
      //    results.Remove(mesEntryResult);
      //}
      //var mesExitResult = results.FirstOrDefault(x => x.Key == nameof(BatMainModel.MesExitStatus));
      //if (mesExitResult.Key != null)
      //{
      //    results.Remove(mesExitResult);
      //}

      var childResultCode = results
         .Where(x => !string.IsNullOrWhiteSpace(x.parentResult))
         .GroupBy(x => x.childResultRule)
         .Select(ruleGroup =>
         {
            var resultGroups = ruleGroup.GroupBy(r => r.parentResult);
            StringBuilder code = new();
            if (ruleGroup.Key == ProductResultRuleEnum.由最后子结果决定父结果)
            {
               foreach (var rg in resultGroups)
               {
                  string clildName = string.Join(" or ", rg.Select(g => $"\"{g.propertyName}\""));
                  if (!string.IsNullOrWhiteSpace(clildName))
                  {
                     code.Append($"if(propertyName is {clildName})\r\n" + $"{rg.Key} = propertyValue;\r\n");
                  }
               }
            }
            else
            {
               //..其它规则有需要再写
            }
            var codeStr = code.ToString();
            return codeStr;
         });

      var array = results
         .Where(x => string.IsNullOrWhiteSpace(x.parentResult))
         .Select(x =>
         {
            // return $"if({x.Key}!= {nameof(ResultTypeEnum)}.{nameof(ResultTypeEnum.MES未启用)} && {x.Key}!= {nameof(ResultTypeEnum)}.{nameof(ResultTypeEnum.发送测试数据)} && (int){x.Key} >1 )" +
            //return $"if((int){x.propertyName} >20 )" +
            return $"if({x.propertyName}.GetResultArea()==ResultArea.NG)"
               + $"    {{\r\n"
               + $"    {nameof(BatMainModel.FinalStatus)} = {x.propertyName};\r\n"
               + $"    {nameof(BatMainModel.NgProcesses)} = {nameof(ProcessTypeEnum)}.{x.process};\r\n"
               + $"    return;\r\n"
               + $"    }}\r\n";
         });
      return $" public void UpdateFinalResult(string propertyName,ResultTypeEnum propertyValue)\r\n    "
         + $"{{"
         + $"{string.Join("\r\n", childResultCode)}\r\n"
         + $"{string.Join("", array)}\r\n"
         + $"{nameof(BatMainModel.FinalStatus)}={nameof(ResultTypeEnum)}.{nameof(ResultTypeEnum.OK)};\r\n"
         + $"{nameof(BatMainModel.NgProcesses)} = {nameof(ProcessTypeEnum)}.{ProcessTypeEnum._};\r\n"
         + $"}}";
   }

   /// <summary>
   /// 生成属性代码
   /// </summary>
   /// <param name="propertyTypeName"></param>
   /// <param name="propertyName"></param>
   /// <param name="propertyAttribute"></param>
   /// <param name="value">值</param>
   /// <returns></returns>
   private static string CreatePropertyCode(PropertyInfo property, object entity)
   {
      try
      {
         var attributes = property.GetAttributeToStringList();
         string attributePropertyStr = string.Join(' ', attributes); //属性的特性

         string propertyName = property.Name;
         string getDateTimeFromId =
            property.Name == nameof(BatMainModel.Id) ? "CreateTime = SnowflakeHelper.GetDateTimeFromId(value);" : string.Empty;
         string updateFun =
            property.PropertyType == typeof(ResultTypeEnum) && property.Name != $"{nameof(BatMainModel.FinalStatus)}"
               ? $"UpdateFinalResult(\"{property.Name}\",value);"
               : "";
         string _variable = $"_{propertyName[0].ToString().ToLower()}{propertyName.Substring(1, propertyName.Length - 1)}";
         var _propertyInfo = property.CreatePropertyInfo(entity);

         var _property =
            $@"        public {_propertyInfo.propertyTypeStr} {propertyName} 
          {{
              get {{ return {_variable}; }}
              set {{
                     if( {_variable} != value)
                     {{ 
                          {_variable} = value;
                          {getDateTimeFromId}  
                          {updateFun}  
                          OnPropertyChanged(); 
                     }}                     
                  }}
          }}";
         return $"        private {_propertyInfo.propertyTypeStr} {_variable} {_propertyInfo.valueStr};\r\n        {attributePropertyStr}\r\n{_property}\r\n";
      }
      catch (Exception ex)
      {
         $"生成属性代码异常:{ex}".LogRun(Log4NetLevelEnum.警告);
      }
      return string.Empty;
   }

   /// <summary>
   /// 生成 Switch 索引器代码
   /// </summary>
   /// <param name="properties"></param>
   /// <returns></returns>
   private static string BuildSwitchCode(this IEnumerable<PropertyInfo> properties)
   {
      StringBuilder codes = new StringBuilder();
      codes.AppendLine("        [SugarColumn(IsIgnore = true)]");
      codes.AppendLine("        [ExpressionDeepSync(true)]");
      codes.AppendLine("        public object this[string name]");
      codes.AppendLine("        {");
      codes.AppendLine("            get");
      codes.AppendLine("            {");
      codes.AppendLine("                return name switch");
      codes.AppendLine("                {");

      foreach (var prop in properties)
      {
         codes.AppendLine($"                    \"{prop.Name}\" => this.{prop.Name},");
      }

      codes.AppendLine("                    _ => throw new global::System.ArgumentException($\"未找到属性 {name}\")");
      codes.AppendLine("                };");
      codes.AppendLine("            }");

      codes.AppendLine("            set");
      codes.AppendLine("            {");
      codes.AppendLine("                switch (name)");
      codes.AppendLine("                {");

      foreach (var prop in properties)
      {
         codes.AppendLine($"                    case \"{prop.Name}\": this.{prop.Name} = ({prop.PropertyType.FullName})value!; break;");
      }

      codes.AppendLine("                    default: throw new global::System.ArgumentException($\"未找到属性 {name}\");");
      codes.AppendLine("                }");
      codes.AppendLine("            }");
      codes.AppendLine("        }");

      return codes.ToString();
   }
}
