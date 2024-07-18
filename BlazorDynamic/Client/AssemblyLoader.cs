using BlazorDynamic.Models;

namespace BlazorDynamic.Client;
public static class AssemblyLoader
{

    public static Assembly? LoadAssembly(this List<AssemblyRawData> rawData, AppDomain domain)
    {
        Assembly? current = null;
        foreach (var assembly in rawData)
        {
            if (assembly == null || assembly.DllRaw == null)
                continue;
            current = ((assembly.PdbRaw == null) ? domain.Load(assembly.DllRaw) : domain.Load(assembly.DllRaw, assembly.PdbRaw));

        }
        return current;
    }

    public static Assembly? LoadAssembly(this List<AssemblyRawData> rawData)
    {
        return rawData.LoadAssembly(AppDomain.CurrentDomain);
    }
}
