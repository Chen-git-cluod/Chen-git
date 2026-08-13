using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Kinlo.WebApi;

/// <summary>
/// 过滤控制器
/// </summary>
public class SpecificControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
   private readonly HashSet<string> _allowedNames;

   public SpecificControllerFeatureProvider(string[] allowedNames)
   {
      _allowedNames = new HashSet<string>(allowedNames, StringComparer.OrdinalIgnoreCase);
   }

   public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
   {
      for (int i = feature.Controllers.Count - 1; i >= 0; i--)
      {
         var controller = feature.Controllers[i];
         var att = controller.GetCustomAttribute<TagsAttribute>();
         if (att == null || att.Tags.Count == 0 || !_allowedNames.Contains(att.Tags[0]))
         {
            feature.Controllers.RemoveAt(i);
         }
      }
   }
}
