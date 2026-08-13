namespace Kinlo.Common.Tools.ScriptCreation;

/// <summary>
/// 自定义上下文，承载插件Assembly
/// </summary>
public class PluginLoadContext : AssemblyLoadContext
{
   private readonly IReadOnlyList<AssemblyDependencyResolver> _resolvers;
   private readonly IReadOnlyDictionary<string, Assembly> _sharedAssemblies;

   public PluginLoadContext(IEnumerable<string> pluginPaths, IEnumerable<Assembly> sharedAssemblies)
      : base(isCollectible: true) //可卸载
   {
      _resolvers = pluginPaths.Select(x => new AssemblyDependencyResolver(x)).ToList(); //路径加载dll
      _sharedAssemblies = sharedAssemblies.ToDictionary(a => a.GetName().Name!, a => a);
   }

   protected override Assembly? Load(AssemblyName assemblyName)
   {
      // 返回主程序中已经加载的共享程序集（接口、工具等）
      if (_sharedAssemblies.TryGetValue(assemblyName.Name!, out var sharedAssembly))
      {
         return sharedAssembly;
      }

      // 加载插件自己的依赖
      foreach (var item in _resolvers)
      {
         string? path = item.ResolveAssemblyToPath(assemblyName);
         if (path != null)
         {
            return LoadFromAssemblyPath(path);
         }
      }
      return null;
   }
}
