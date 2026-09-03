using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class ArchiveFileNameTests(ZipadeeBuildPackFixture pack)
{
    [Fact]
    public void SubstitutesProjectNameVersionDateAndTimeTokens()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = "Zip",
                ["Version"] = "9.9.9",
                ["ZipadeeArchiveFileName"] = "{ProjectName}-v{Version}-{Date}-{Time}",
            });

        Assert.True(build.Succeeded, build.Output);

        var now = DateTime.Now;
        var files = Directory.GetFiles(build.OutputDirectory, "StubArchive-v9.9.9-*.zip");
        Assert.True(files.Length == 1, $"Expected exactly one matching file, found {files.Length}: {string.Join(", ", files)}{Environment.NewLine}{build.Output}");

        var name = Path.GetFileName(files[0]);
        Assert.Contains(now.ToString("yyyyMMdd"), name); // default ZipadeeArchiveDateFormat
    }

    [Fact]
    public void RespectsCustomDateAndTimeFormats()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = "Zip",
                ["ZipadeeArchiveFileName"] = "{ProjectName}-{Date}",
                ["ZipadeeArchiveDateFormat"] = "yyyy-MM-dd",
            });

        Assert.True(build.Succeeded, build.Output);

        var expectedName = $"StubArchive-{DateTime.Now:yyyy-MM-dd}.zip";
        Assert.True(File.Exists(Path.Combine(build.OutputDirectory, expectedName)), build.Output);
    }

    [Fact]
    public void ZipadeeArchiveOutputPathTakesOverTheWholePathIgnoringFileName()
    {
        using var workspace = new FixtureWorkspace();
        var customPath = Path.Combine(workspace.Root, "StubArchive", "bin", "Debug", "Manual.zip");

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = "Zip",
                ["ZipadeeArchiveFileName"] = "{ProjectName}-ignored",
                ["ZipadeeArchiveOutputPath"] = customPath,
            });

        Assert.True(build.Succeeded, build.Output);
        Assert.True(File.Exists(customPath), build.Output);
        Assert.Contains("ZipadeeArchiveOutputPath is set, which takes over the whole output path", build.Output);
    }
}
