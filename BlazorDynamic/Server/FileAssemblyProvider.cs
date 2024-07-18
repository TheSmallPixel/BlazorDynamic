using BlazorDynamic.Models;
using BlazorDynamic.Server.Interfaces;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace BlazorDynamic.Server;
public class FileAssemblyProvider : IAssemblyProvider
{
    private readonly string _path = string.Empty;
   // private readonly ILogger _logger;
    public FileAssemblyProvider()
    {
        this._path = "../plugins/";
        var dir = Directory.CreateDirectory(string.Concat(AppContext.BaseDirectory,_path));
        this._path = dir.FullName;

        var files = Directory.GetFiles(_path, "*.nupkg");
        ReadPackage(files.FirstOrDefault());
    }
    private AssemblyRawData GetRawData(string file)
    {
        var path = new String(file.SkipLast(4).ToArray());
        var dll = System.IO.File.ReadAllBytes(file);
        var pdb = System.IO.File.ReadAllBytes(path + ".pdb");

        return new AssemblyRawData()
        {
            DllRaw = dll,
            PdbRaw = pdb
        };
    }

    public AssemblyRawData? LoadAssembly(AssemblyName name)
    {
        if (name == null || string.IsNullOrWhiteSpace(name.Name))
            throw new Exception($"FileAssemblyProvider, null or without name.");
        var files = Directory.GetFiles(_path, "*.dll");
        if (files == null || files.Count() == 0)
            return null;
        List<AssemblyName> MissingDependency = new();
        var path = files.FirstOrDefault(x => x.ToLower().Contains(name.Name.ToLower()));
        if (path == null)
            throw new Exception($"FileAssemblyProvider, missing this assembly: {name.FullName}");
        var data = GetRawData(path);
        return data;
    }
    public static async Task DownloadPackageAsync()
    {
        // This code region is referenced by the NuGet docs. Please update the docs if you rename the region
        // or move it to a different file.
        #region DownloadPackage
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

        string packageId = "Newtonsoft.Json";
        NuGetVersion packageVersion = new NuGetVersion("12.0.1");
        using MemoryStream packageStream = new MemoryStream();

        await resource.CopyNupkgToStreamAsync(
            packageId,
            packageVersion,
            packageStream,
            cache,
            logger,
            cancellationToken);

        Console.WriteLine($"Downloaded package {packageId} {packageVersion}");

        using PackageArchiveReader packageReader = new PackageArchiveReader(packageStream);
        
        NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);
        //packageReader.GetStream(path);
        Console.WriteLine($"Tags: {nuspecReader.GetTags()}");
        Console.WriteLine($"Description: {nuspecReader.GetDescription()}");
        #endregion
    }


    private async Task ReadPackage(string packagePath)
    {
        using FileStream inputStream = new FileStream(packagePath, FileMode.Open);
        using PackageArchiveReader reader = new PackageArchiveReader(inputStream);
        NuspecReader nuspec = reader.NuspecReader;
        Console.WriteLine($"ID: {nuspec.GetId()}");
        Console.WriteLine($"Version: {nuspec.GetVersion()}");
        Console.WriteLine($"Description: {nuspec.GetDescription()}");
        Console.WriteLine($"Authors: {nuspec.GetAuthors()}");
        
        NugetController nugetController = new();
       
        await nugetController.Load(nuspec.GetIdentity());
        Console.WriteLine("Dependencies:");
        foreach (var dependencyGroup in nuspec.GetDependencyGroups())
        {
            Console.WriteLine($" - {dependencyGroup.TargetFramework.GetShortFolderName()}");
            foreach (var dependency in dependencyGroup.Packages)
            {
                Console.WriteLine($"   > {dependency.Id} {dependency.VersionRange}");
            }
        }

        Console.WriteLine("Files:");
        var filesToExtract = reader.GetFiles().Where(x => x.Contains(".dll"));
        var logger = new LogInterface();
        foreach (var file in filesToExtract)
        {
            Console.WriteLine($" - {file}");
            var result = reader.ExtractFile(file, _path, logger);


        }

        //reader.CopyFilesAsync(path, filesToExtract, ExtractAssembly,null,null);

    }


    private void ReadAssemblyFromPackage(PackageArchiveReader reader)
    {

    }
    private class LogInterface : ILogger
    {
        public void Log(LogLevel level, string data)
        {
            Console.WriteLine(data);
        }

        public void Log(ILogMessage message)
        {
            Console.WriteLine(message.Message);
        }

        public async Task LogAsync(LogLevel level, string data)
        {
            Console.WriteLine(data);
            await Task.CompletedTask;
        }

        public Task LogAsync(ILogMessage message)
        {
            Console.WriteLine(message.Message);
            return Task.CompletedTask;
        }

        public void LogDebug(string data)
        {
            Console.WriteLine(data);
        }

        public void LogError(string data)
        {
            Console.WriteLine(data);
        }

        public void LogInformation(string data)
        {
            Console.WriteLine(data);
        }

        public void LogInformationSummary(string data)
        {
            Console.WriteLine(data);
        }

        public void LogMinimal(string data)
        {
            Console.WriteLine(data);
        }

        public void LogVerbose(string data)
        {
            Console.WriteLine(data);
        }

        public void LogWarning(string data)
        {
            Console.WriteLine(data);
        }
    }
}
