using Xunit;

namespace Zipadee.Build.Tests.Infrastructure;

/// <summary>
/// Packs the CURRENT Zipadee.Build source into the repo's LocalPackages feed once per test run,
/// under a version generated at run time (e.g. 0.0.0-test20260904013000000) that no NuGet cache
/// has ever seen. Every fixture project reads that exact version back via a
/// ZipadeeBuildTestVersion MSBuild property passed on the `dotnet build` command line (see
/// BuildHarness), so tests always exercise today's code - never a stale cached package, and never
/// needing the manual version-bump-and-sed dance the sample projects require.
/// </summary>
public sealed class ZipadeeBuildPackFixture
{
    public string Version { get; }

    public ZipadeeBuildPackFixture()
    {
        Version = $"0.0.0-test{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        Directory.CreateDirectory(RepoPaths.LocalPackages);

        var result = ProcessRunner.Run(
            "dotnet",
            $"pack \"{RepoPaths.ZipadeeBuildCsproj}\" -o \"{RepoPaths.LocalPackages}\" -p:Version={Version}",
            RepoPaths.Root);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Failed to pack Zipadee.Build ({RepoPaths.ZipadeeBuildCsproj}) for testing:{Environment.NewLine}{result.CombinedOutput}");
        }
    }
}

[CollectionDefinition(Name)]
public sealed class ZipadeeBuildPackCollection : ICollectionFixture<ZipadeeBuildPackFixture>
{
    public const string Name = "Zipadee.Build pack";

    // Every test class in this project uses this same collection - not just to share the one
    // pack, but because it also serializes all of them (xUnit runs collections in parallel with
    // each other but tests within one collection sequentially). Concurrent `dotnet build`
    // invocations across dozens of tests would each spin up their own MSBuild node and restore
    // concurrently, which is more likely to help than hurt in principle, but correctness on a
    // first pass matters more than speed here - see the test project's README for how to revisit
    // this if the suite gets slow.
}
