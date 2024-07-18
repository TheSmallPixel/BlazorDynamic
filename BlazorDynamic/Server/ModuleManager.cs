using BlazorDynamic.Models;
using BlazorDynamic.Server.Interfaces;
using NuGet;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System.Collections.Concurrent;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace BlazorDynamic.Server;

public class ModuleManager
{
    private readonly IAssemblyProvider _assemblyProvider;
    private readonly IAssemblyResolver _assemblyResolver;
    private ConcurrentDictionary<AssemblyName, List<AssemblyRawData>> _cache = new();

    public ModuleManager(IAssemblyProvider assemblyProvider, IAssemblyResolver assemblyResolver)
    {
        this._assemblyProvider = assemblyProvider;
        this._assemblyResolver = assemblyResolver;

    }
    public async Task<List<AssemblyRawData>> GetModules(string name)
    {
        var assemblyName = new AssemblyName(name);
        return await GetModules(assemblyName);
    }
    public async Task<List<AssemblyRawData>> GetModules(AssemblyName name)
    {
        if (_cache.ContainsKey(name))
        {
            var modules = new List<AssemblyRawData>();
            if (_cache.TryGetValue(name, out  modules))
                return modules;

            throw new Exception($"{this.GetType().FullName} Module {name.Name} get error!");
        }
        var data = await Load(name);
        _cache.TryAdd(name, data);
        return data;
    }
    private async Task<List<AssemblyRawData>> Load(AssemblyName name)
    {
        var raw = _assemblyProvider.LoadAssembly(name);
        if (raw == null || raw.DllRaw == null)
            throw new Exception($"{this.GetType().FullName} Module {name.Name} not found!");
        using MemoryStream dllStream = new(raw.DllRaw);
        using PEReader peReader = new(dllStream);
        MetadataReader metaReader = peReader.GetMetadataReader(MetadataReaderOptions.None);
        var assemblyDependency = new List<AssemblyRawData>();
        var dependency = await _assemblyResolver.Resolve(metaReader);
        dependency.RemoveAll(x => x.Name.Equals("System.Runtime") || x.Name.Equals("Microsoft.JSInterop") || x.Name.Equals("Microsoft.AspNetCore.Components"));
        foreach (var dep in dependency)
        {
            var rawdep = await Load(dep);
            assemblyDependency.AddRange(rawdep);
        }
        assemblyDependency.Add(raw);
        return assemblyDependency;
    }
    

}


