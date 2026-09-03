using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class ProjectOutputFilterTests(ZipadeeBuildPackFixture pack)
{
    [Fact]
    public void ExcludeLeavesMatchingProjectOutputFilesOut()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = "Zip",
                ["ZipadeeProjectOutputExclude"] = "*.deps.json;extra.txt",
            });

        Assert.True(build.Succeeded, build.Output);

        var list = ArchiveInspector.List(Path.Combine(build.OutputDirectory, "StubArchive.zip"));
        Assert.DoesNotContain("StubApp.deps.json", list.StdOut);
        Assert.DoesNotContain("extra.txt", list.StdOut);
        // Unmatched project output still comes through untouched.
        Assert.Contains("StubApp.dll", list.StdOut);
        Assert.Contains("StubApp.exe", list.StdOut);
        // The project's own Content items are a completely separate mechanism, unaffected by
        // this filter, which only ever applies to ProjectReference output.
        Assert.Contains("Hello.txt", list.StdOut);
    }

    [Fact]
    public void IncludeOverridesExcludeForSpecificFiles()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = "Zip",
                ["ZipadeeProjectOutputExclude"] = "*.txt;*.json",
                ["ZipadeeProjectOutputInclude"] = "extra.txt",
            });

        Assert.True(build.Succeeded, build.Output);

        var list = ArchiveInspector.List(Path.Combine(build.OutputDirectory, "StubArchive.zip"));
        // extra.txt matches the exclude pattern (*.txt) but is forced back in by the include.
        Assert.Contains("extra.txt", list.StdOut);
        // StubApp.deps.json matches *.json and isn't covered by the include - still excluded.
        Assert.DoesNotContain("StubApp.deps.json", list.StdOut);
    }
}
