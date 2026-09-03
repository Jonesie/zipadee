using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class LinkedFilesTests(ZipadeeBuildPackFixture pack)
{
    [Fact]
    public void ALinkedFileIsArchivedAtItsLinkPathNotItsOnDiskPath()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Zip" });

        Assert.True(build.Succeeded, build.Output);

        var archivePath = Path.Combine(build.OutputDirectory, "StubArchive.zip");
        var list = ArchiveInspector.List(archivePath);

        // Linked.txt lives at Fixtures/Shared/Linked.txt on disk (outside the project folder
        // entirely) but StubArchive.zparchproj links it in at linked\Linked.txt.
        Assert.Contains(@"linked\Linked.txt", list.StdOut);
    }

    [Fact]
    public void AWildcardLinkedWholeFolderPreservesItsStructure()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Zip" });

        Assert.True(build.Succeeded, build.Output);

        var archivePath = Path.Combine(build.OutputDirectory, "StubArchive.zip");
        var list = ArchiveInspector.List(archivePath);

        // Fixtures/ExternalFolder/{Root.txt, sub/Nested.txt} is linked in as a whole folder via
        // %(RecursiveDir) - both the root file and the nested one should land under fromexternal\,
        // with the nested one keeping its sub\ subfolder.
        Assert.Contains(@"fromexternal\Root.txt", list.StdOut);
        Assert.Contains(@"fromexternal\sub\Nested.txt", list.StdOut);
    }
}
