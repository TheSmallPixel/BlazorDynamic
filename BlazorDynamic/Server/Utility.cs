using BlazorDynamic.Server.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorDynamic.Server
{
    public static class Utility
    {
        public static void AddModuleManager(this IServiceCollection service)
        {
            service.AddSingleton<IAssemblyProvider>(new FileAssemblyProvider());
            service.AddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();
            service.AddSingleton<ModuleManager>();
        }
    }
}
