// See https://aka.ms/new-console-template for more information
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

Console.WriteLine("Hello, World!");
await DownloadPackageAsync(CancellationToken.None);



static async Task DownloadPackageAsync(CancellationToken cancellationToken)
{
    ILogger logger = NullLogger.Instance;


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

    var targetFramework = NuGetFramework.Parse("net6.0");
    var dependencies = NuGetFrameworkUtility.GetNearest(nuspecReader.GetDependencyGroups(),
                                                    targetFramework,
                                                    item => item.TargetFramework);

    var frameworkAssemblies = NuGetFrameworkUtility.GetNearest(nuspecReader.GetFrameworkReferenceGroups(),
                                                     targetFramework,
                                                     item => item.TargetFramework);
    var nearestWithSelector = NuGetFrameworkUtility.GetNearest(nuspecReader.GetFrameworkRefGroups(), targetFramework);
    var nuGetFramework = NuGetFramework.ParseFrameworkName(dependencies.TargetFramework.DotNetFrameworkName, DefaultFrameworkNameProvider.Instance);
    var shortFolderName = nuGetFramework.GetShortFolderName();
    foreach (var content in await packageReader.GetPackageDependenciesAsync(cancellationToken))
    {
        Console.WriteLine($"{content.TargetFramework} {content.TargetFramework}");
        foreach (var target in content.Packages)
        {
            Console.WriteLine($"    > {target.Id}");
        }

    }
    foreach (var test in await packageReader.GetLibItemsAsync(cancellationToken))
    {
        Console.WriteLine($"{test.TargetFramework} ");
        foreach (var target in test.Items)
        {
            Console.WriteLine($"{target}");
        }
    }
    var path = "lib/" + shortFolderName;
    var files = await packageReader.GetFilesAsync(cancellationToken);
    var results = new Dictionary<string, byte[]>();
    foreach (var file in files)
    {
        if (file.StartsWith(path))
        {
            var stream = await packageReader.GetStreamAsync(file, cancellationToken);
            if (stream == null) continue;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                var resultbyte = memoryStream.ToArray();
                results.Add(file, resultbyte);
            }
        }
    }



    Console.WriteLine($"Tags: {nuspecReader.GetTags()}");
    Console.WriteLine($"Description: {nuspecReader.GetDescription()}");
}