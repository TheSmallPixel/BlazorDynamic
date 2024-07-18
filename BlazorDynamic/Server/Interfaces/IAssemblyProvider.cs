using BlazorDynamic.Models;

namespace BlazorDynamic.Server.Interfaces;
public interface IAssemblyProvider
{
    public AssemblyRawData? LoadAssembly(AssemblyName name);
}

