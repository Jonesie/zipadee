using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class FormatTests(ZipadeeBuildPackFixture pack)
{
    [Theory]
    [InlineData("Zip", "StubArchive.zip")]
    [InlineData("SevenZip", "StubArchive.7z")]
    [InlineData("Tar", "StubArchive.tar")]
    [InlineData("GZip", "StubArchive.tar.gz")]
    [InlineData("Rar", "StubArchive.rar")]
    [InlineData("Cab", "StubArchive.cab")]
    public void ProducesTheExpectedArchiveFile(string format, string expectedFileName)
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string> { ["ZipadeeOutputFormat"] = format });

        Assert.True(build.Succeeded, build.Output);

        var archivePath = Path.Combine(build.OutputDirectory, expectedFileName);
        Assert.True(File.Exists(archivePath), $"Expected archive not found at '{archivePath}'.{Environment.NewLine}{build.Output}");

        var test = ArchiveInspector.Test(archivePath);
        Assert.True(test.ExitCode == 0, test.CombinedOutput);

        var list = ArchiveInspector.List(archivePath);
        AssertContainsStubArchiveContents(list.StdOut);
    }

    /// <summary>
    /// Every StubArchive-based test that doesn't otherwise filter/limit content should see this
    /// exact set - shared here so the expected file list only needs to change in one place if the
    /// fixture itself changes.
    /// </summary>
    internal static void AssertContainsStubArchiveContents(string listing)
    {
        foreach (var expected in new[]
        {
            "Hello.txt",
            "Notes.txt",
            "Linked.txt",
            "Root.txt",
            "Nested.txt",
            "StubApp.dll",
            "StubApp.exe",
            "StubApp.deps.json",
            "StubApp.runtimeconfig.json",
            "extra.txt",
        })
        {
            Assert.Contains(expected, listing);
        }
    }
}
