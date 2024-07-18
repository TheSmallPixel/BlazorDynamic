using System.Reflection.Metadata;

namespace BlazorDynamic.Server.Interfaces;
public interface IAssemblyResolver
{
    public Task<List<AssemblyName>> Resolve(MetadataReader metadata);
}