using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class IncrementalBuildTests(ZipadeeBuildPackFixture pack)
{
    private static readonly Dictionary<string, string> ZipFormat = new() { ["ZipadeeOutputFormat"] = "Zip" };

    [Fact]
    public void SkipsRebuildingWhenNothingChanged()
    {
        using var workspace = new FixtureWorkspace();

        var first = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version, ZipFormat);
        Assert.True(first.Succeeded, first.Output);
        var archivePath = Path.Combine(first.OutputDirectory, "StubArchive.zip");
        var firstWriteTime = File.GetLastWriteTimeUtc(archivePath);

        Thread.Sleep(1100); // past filesystem timestamp resolution, so a real rewrite would be visible

        var second = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version, ZipFormat);
        Assert.True(second.Succeeded, second.Output);

        Assert.Equal(firstWriteTime, File.GetLastWriteTimeUtc(archivePath));
    }

    [Fact]
    public void RebuildsWhenTheReferencedProjectsActualOutputChanges()
    {
        using var workspace = new FixtureWorkspace();

        var first = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version, ZipFormat);
        Assert.True(first.Succeeded, first.Output);
        var archivePath = Path.Combine(first.OutputDirectory, "StubArchive.zip");
        var firstWriteTime = File.GetLastWriteTimeUtc(archivePath);

        Thread.Sleep(1100);

        // Changing StubApp's own source (not StubArchive.zparchproj) is the point: incremental
        // build has to notice the referenced project's real output changed, not just react to
        // edits to the archive project file itself.
        File.WriteAllText(
            Path.Combine(workspace.Root, "StubApp", "Program.cs"),
            "Console.WriteLine(\"stub-app-changed\");" + Environment.NewLine);

        var second = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version, ZipFormat);
        Assert.True(second.Succeeded, second.Output);
        Assert.NotEqual(firstWriteTime, File.GetLastWriteTimeUtc(archivePath));

        var extractDir = Path.Combine(workspace.Root, "extracted");
        var extract = ArchiveInspector.Extract(archivePath, extractDir);
        Assert.True(extract.ExitCode == 0, extract.CombinedOutput);

        var run = ProcessRunner.Run(Path.Combine(extractDir, "StubApp.exe"), string.Empty, extractDir);
        Assert.Contains("stub-app-changed", run.StdOut);
    }

    [Fact]
    public void ZipadeeIncrementalBuildFalseForcesARebuildEveryTime()
    {
        using var workspace = new FixtureWorkspace();
        var props = new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Zip", ["ZipadeeIncrementalBuild"] = "false" };

        var first = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version, props);
        Assert.True(first.Succeeded, first.Output);
        var archivePath = Path.Combine(first.OutputDirectory, "StubArchive.zip");
        var firstWriteTime = File.GetLastWriteTimeUtc(archivePath);

        Thread.Sleep(1100);

        // Nothing changed at all - a real dependency-tracked build would skip this one too, per
        // SkipsRebuildingWhenNothingChanged above.
        var second = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version, props);
        Assert.True(second.Succeeded, second.Output);

        Assert.NotEqual(firstWriteTime, File.GetLastWriteTimeUtc(archivePath));
    }
}
