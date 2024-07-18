using BlazorDynamic.Server.Interfaces;
using System.Reflection.Metadata;

namespace BlazorDynamic.Server;
public class DefaultAssemblyResolver : IAssemblyResolver
{
    public Task<List<AssemblyName>> Resolve(MetadataReader metadata)
    {
        var _loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName());
        var result = metadata.AssemblyReferences
                .Select(x => metadata.GetAssemblyReference(x).GetAssemblyName())
                .Except(_loadedAssembly)
                .ToList();
        return Task.FromResult(result);
    }
}

