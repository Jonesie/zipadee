using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class ProjectReferenceTests(ZipadeeBuildPackFixture pack)
{
    [Fact]
    public void ReferencedProjectOutputLandsAtTheArchiveRootNotANestedFolder()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Zip" });

        Assert.True(build.Succeeded, build.Output);

        var archivePath = Path.Combine(build.OutputDirectory, "StubArchive.zip");
        var list = ArchiveInspector.List(archivePath);

        // Root-level, not "StubApp\StubApp.dll" - a project reference's output is flattened into
        // the archive root, it doesn't recreate the referenced project's own folder structure.
        foreach (var line in list.StdOut.Split('\n'))
        {
            Assert.DoesNotContain(@"StubApp\", line);
        }

        Assert.Contains("StubApp.dll", list.StdOut);
        Assert.Contains("StubApp.exe", list.StdOut);
        Assert.Contains("StubApp.deps.json", list.StdOut);
        Assert.Contains("StubApp.runtimeconfig.json", list.StdOut);
        // StubApp's own CopyToOutputDirectory content item, pulled in via the same mechanism.
        Assert.Contains("extra.txt", list.StdOut);
    }

    [Fact]
    public void TheReferencedProjectIsBuiltBeforeTheArchive()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Zip" });

        Assert.True(build.Succeeded, build.Output);

        // If StubApp hadn't actually been built first, its .dll/.exe wouldn't exist yet for the
        // archive step to pick up at all, and the build above would have failed outright rather
        // than succeeding with an incomplete archive - so success here already proves the
        // ordering. Extracting and running the assembly is the deepest possible confirmation that
        // what got archived is a real, working build output, not a stale or partial one.
        var archivePath = Path.Combine(build.OutputDirectory, "StubArchive.zip");
        var extractDir = Path.Combine(workspace.Root, "extracted");
        var extract = ArchiveInspector.Extract(archivePath, extractDir);
        Assert.True(extract.ExitCode == 0, extract.CombinedOutput);

        var run = ProcessRunner.Run(Path.Combine(extractDir, "StubApp.exe"), string.Empty, extractDir);
        Assert.Equal(0, run.ExitCode);
        Assert.Contains("stub-app", run.StdOut);
    }
}
