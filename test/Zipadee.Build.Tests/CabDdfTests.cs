using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class CabDdfTests(ZipadeeBuildPackFixture pack)
{
    [Fact]
    public void UserDdfSettingsAreMergedIntoTheGeneratedDdf()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchiveWithDdf/StubArchiveWithDdf.zparchproj", pack.Version,
            new Dictionary<string, string> { ["ZipadeeCompressionLevel"] = "Ultra" });

        Assert.True(build.Succeeded, build.Output);

        // StubArchiveWithDdf.ddf forces CompressionType=MSZIP, which should win over whatever
        // ZipadeeCompressionLevel=Ultra would otherwise compute (LZX).
        var ddf = File.ReadAllText(Path.Combine(build.IntermediateDirectory, "StubArchiveWithDdf.ddf"));
        Assert.Contains(".Set CompressionType=MSZIP", ddf);
    }

    [Fact]
    public void TheDdfFileItselfIsExcludedFromTheCabsOwnContentsEvenThoughItsTrackedAsContent()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchiveWithDdf/StubArchiveWithDdf.zparchproj", pack.Version, null);
        Assert.True(build.Succeeded, build.Output);

        var archivePath = Path.Combine(build.OutputDirectory, "StubArchiveWithDdf.cab");
        Assert.True(File.Exists(archivePath), build.Output);

        var list = ArchiveInspector.List(archivePath);
        Assert.Contains("Hello.txt", list.StdOut);
        // Tracked as a Content item in the fixture project, but never part of the cab's own
        // contents - unlike every other format, where a same-named file just follows the normal
        // Content/None rule.
        Assert.DoesNotContain("StubArchiveWithDdf.ddf", list.StdOut);
    }

    [Fact]
    public void ZipadeeMaxVolumeSizeOverridesTheUserDdfsOwnMaxDiskSizeWithAWarning()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchiveWithDdf/StubArchiveWithDdf.zparchproj", pack.Version,
            new Dictionary<string, string> { ["ZipadeeMaxVolumeSize"] = "102400" });

        Assert.True(build.Succeeded, build.Output);
        Assert.Contains("ZipadeeMaxVolumeSize is set, which overwrites the MaxDiskSize directive", build.Output);

        var ddf = File.ReadAllText(Path.Combine(build.IntermediateDirectory, "StubArchiveWithDdf.ddf"));
        // The fixture's own ".Set MaxDiskSize=0" line is still present (Zipadee appends its own
        // mandatory settings after the user DDF's content rather than stripping anything out of
        // it) - but its own, later ".Set MaxDiskSize=102400" line has to come after it, since
        // makecab applies whichever occurrence of a directive it scans last.
        var userLineIndex = ddf.IndexOf(".Set MaxDiskSize=0", StringComparison.Ordinal);
        var forcedLineIndex = ddf.IndexOf(".Set MaxDiskSize=102400", StringComparison.Ordinal);
        Assert.True(userLineIndex >= 0, ddf);
        Assert.True(forcedLineIndex > userLineIndex, ddf);
    }
}
