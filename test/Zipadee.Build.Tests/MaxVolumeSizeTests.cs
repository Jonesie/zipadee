using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class MaxVolumeSizeTests(ZipadeeBuildPackFixture pack)
{
    // Deliberately incompressible (random) and much larger than ZipadeeMaxVolumeSize below, so
    // spanning is guaranteed regardless of exactly how well any format compresses the fixture's
    // other small text/binary files - the point of these tests is "does it actually split and
    // does the result reassemble cleanly", not pinning an exact volume count to fragile
    // compression-ratio math.
    private static byte[] LargeContent { get; } = RandomBytes(300_000);

    [Theory]
    [InlineData("Zip", 100_000, "StubArchive.zip.*")]
    [InlineData("SevenZip", 100_000, "StubArchive.7z.*")]
    [InlineData("Rar", 100_000, "StubArchive.part*.rar")]
    [InlineData("Cab", 102_400, "StubArchive*.cab")] // must be a multiple of 512
    public void SplitsIntoMultipleVolumesThatReassembleCleanly(string format, int maxVolumeSize, string volumeGlob)
    {
        using var workspace = new FixtureWorkspace();
        workspace.WriteFile("StubArchive/volume-data/large.bin", LargeContent);

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = format,
                ["ZipadeeMaxVolumeSize"] = maxVolumeSize.ToString(),
            });

        Assert.True(build.Succeeded, build.Output);

        var volumes = Directory.GetFiles(build.OutputDirectory, volumeGlob);
        Assert.True(volumes.Length >= 2, $"Expected at least 2 volumes, found {volumes.Length}: {string.Join(", ", volumes)}{Environment.NewLine}{build.Output}");

        var firstVolume = volumes.OrderBy(v => v).First();
        var test = ArchiveInspector.Test(firstVolume);
        Assert.True(test.ExitCode == 0, test.CombinedOutput);

        var list = ArchiveInspector.List(firstVolume);
        Assert.Contains("large.bin", list.StdOut);
        Assert.Contains("Hello.txt", list.StdOut);
    }

    [Theory]
    [InlineData("Tar")]
    [InlineData("GZip")]
    public void IsIgnoredWithAWarningForFormatsThatDontSupportIt(string format)
    {
        using var workspace = new FixtureWorkspace();
        workspace.WriteFile("StubArchive/volume-data/large.bin", LargeContent);

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = format,
                ["ZipadeeMaxVolumeSize"] = "100000",
            });

        Assert.True(build.Succeeded, build.Output);
        Assert.Contains("ZipadeeMaxVolumeSize is set but ignored", build.Output);

        var extension = format == "Tar" ? ".tar" : ".tar.gz";
        Assert.True(File.Exists(Path.Combine(build.OutputDirectory, "StubArchive" + extension)));
    }

    [Fact]
    public void StaleVolumesFromAPreviousBuildAreCleanedUp()
    {
        using var workspace = new FixtureWorkspace();
        workspace.WriteFile("StubArchive/volume-data/large.bin", LargeContent);

        var small = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Zip", ["ZipadeeMaxVolumeSize"] = "50000" });
        Assert.True(small.Succeeded, small.Output);
        var manyVolumes = Directory.GetFiles(small.OutputDirectory, "StubArchive.zip.*");
        Assert.True(manyVolumes.Length > 2, $"Expected more than 2 volumes to set up this test, found {manyVolumes.Length}.");

        var large = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Zip", ["ZipadeeMaxVolumeSize"] = "1000000" });
        Assert.True(large.Succeeded, large.Output);
        var fewVolumes = Directory.GetFiles(large.OutputDirectory, "StubArchive.zip.*");

        Assert.True(fewVolumes.Length < manyVolumes.Length,
            $"Expected fewer volumes after raising ZipadeeMaxVolumeSize (stale ones should be cleaned up), " +
            $"but found {fewVolumes.Length} (was {manyVolumes.Length}).");
    }

    private static byte[] RandomBytes(int size)
    {
        var buffer = new byte[size];
        Random.Shared.NextBytes(buffer);
        return buffer;
    }
}
