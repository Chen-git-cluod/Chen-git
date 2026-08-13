using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Kinlo.Common.Tools.ScriptCreation;

public static class DynamicLocalCodeMerger
{
   private static PluginLoadContext? _pluginContext = null;

   /// <summary>
   /// 生成合并类
   /// </summary>
   /// <param name="code"></param>
   /// <returns></returns>
   public static Assembly GenerateMergedClass(this string code)
   {
      try
      {
         var dllPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
         IEnumerable<MetadataReference> _metadataReference = new[]
         {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location), //mscorlib.dll
            MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location), //System.dll
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), //System.Core.dll
            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location), //
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "System.ObjectModel.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "System.Collections.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "netstandard.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "System.Data.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "System.Linq.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "System.XML.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "System.IO.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "Microsoft.CSharp.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "System.Runtime.Serialization.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "System.Dynamic.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dllPath, "System.Reflection.dll")),
            MetadataReference.CreateFromFile(Path.Combine(AppContext.BaseDirectory, "Kinlo.Common.dll")),
            MetadataReference.CreateFromFile(Path.Combine(AppContext.BaseDirectory, "Kinlo.SharedBase.dll")),
            MetadataReference.CreateFromFile(Path.Combine(AppContext.BaseDirectory, "SqlSugar.dll")),
         };

         var syntaxTree = CSharpSyntaxTree.ParseText(code); // 编译代码

         //构建编译
         CSharpCompilation compilation = CSharpCompilation
            .Create(Helper.AssemblyName)
            .WithOptions(new CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary)) //DLL
            .AddReferences(_metadataReference) //编译依赖
            .AddSyntaxTrees(syntaxTree);

         UnAssembly(_pluginContext);
         _pluginContext = new PluginLoadContext(
            [],
            [typeof(BatMainModel).Assembly, typeof(ResultTypeEnum).Assembly, typeof(SugarTable).Assembly]
         );
         using (var ms = new MemoryStream())
         {
            var emitResult = compilation.Emit(ms);
            if (!emitResult.Success)
            {
               StringBuilder stringBuilder = new StringBuilder();
               var failures = emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
               foreach (var failure in failures)
               {
                  stringBuilder.AppendLine(failure.GetMessage());
               }
               $"[编译动态类代码]失败：{stringBuilder}".LogRun();
               return null;
            }

            ms.Seek(0, SeekOrigin.Begin);
            // return Assembly.Load(ms.ToArray()); // 返回动态生成的程序集(加载到默认上下文)
            return _pluginContext.LoadFromStream(ms); // 返回动态生成的程序集(加载到指定上下文，和主程序隔离)
         }
      }
      catch (Exception ex)
      {
         $"[编译动态类代码]异常：{ex}".LogRun();
      }
      return null;
   }

   public static void UnAssembly(PluginLoadContext? pluginContext)
   {
      try
      {
         pluginContext?.Unload();
         pluginContext = null;
         GC.Collect();
         GC.WaitForPendingFinalizers(); // 等待资源释放
      }
      catch (Exception ex)
      {
         $"[卸载动态类]异常：{ex}".LogRun();
      }
   }

   public static Type? GetCalssType(this Assembly? assembly, string className)
   {
      //Type? _type = assembly?.GetType($"{DynamicHelper.NamespaceName}.{className}");
      Type? _type = assembly?.GetTypes().FirstOrDefault(x => x.Name == $"{className}");
      return _type;
   }
}
