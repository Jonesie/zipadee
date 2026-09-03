using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

/// <summary>
/// Every ZipadeeValidateArchiveSettings error/warning, one per test - except the ones already
/// covered incidentally elsewhere because they're central to that feature's own tests:
/// ZipadeeMaxVolumeSize ignored-for-Tar/GZip warning (MaxVolumeSizeTests),
/// ZipadeeMaxVolumeSize-forces-a-rebuild warning (IncrementalBuildTests's ZipadeeIncrementalBuildFalse
/// test exists specifically because this warning means ZipadeeIncrementalBuild=true can't be
/// relied on with volumes), the Cab MaxDiskSize-override warning (CabDdfTests), and the
/// ZipadeeArchiveOutputPath-overrides-ZipadeeArchiveFileName warning (ArchiveFileNameTests).
/// </summary>
[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class ValidationErrorTests(ZipadeeBuildPackFixture pack)
{
    [Theory]
    [InlineData("Tar")]
    [InlineData("GZip")]
    [InlineData("Cab")]
    public void PasswordFailsTheBuildForFormatsWithNoEncryptionSupport(string format)
    {
        var result = Build(new Dictionary<string, string> { ["ZipadeeOutputFormat"] = format, ["ZipadeePassword"] = "irrelevant" });

        Assert.False(result.Succeeded);
        Assert.Contains("there's no encryption support for the Tar, GZip, or Cab output formats", result.Output);
    }

    [Theory]
    [InlineData("Zip")]
    [InlineData("Tar")]
    [InlineData("GZip")]
    [InlineData("Rar")]
    [InlineData("Cab")]
    public void SfxFailsTheBuildForEveryFormatExceptSevenZip(string format)
    {
        var result = Build(new Dictionary<string, string> { ["ZipadeeOutputFormat"] = format, ["ZipadeeCreateSfx"] = "true" });

        Assert.False(result.Succeeded);
        Assert.Contains("7-Zip's SFX modules can only self-extract a 7z payload", result.Output);
    }

    [Fact]
    public void SfxFailsTheBuildWhenCombinedWithMaxVolumeSize()
    {
        var result = Build(new Dictionary<string, string>
        {
            ["ZipadeeOutputFormat"] = "SevenZip",
            ["ZipadeeCreateSfx"] = "true",
            ["ZipadeeMaxVolumeSize"] = "100000",
        });

        Assert.False(result.Succeeded);
        Assert.Contains("ZipadeeCreateSfx and ZipadeeMaxVolumeSize can't be combined", result.Output);
    }

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("100k")]
    [InlineData("-5")]
    public void MaxVolumeSizeMustBeAPlainPositiveInteger(string value)
    {
        var result = Build(new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Zip", ["ZipadeeMaxVolumeSize"] = value });

        Assert.False(result.Succeeded);
        Assert.Contains("must be a plain positive integer", result.Output);
    }

    [Fact]
    public void MaxVolumeSizeMustBeAMultipleOf512ForCab()
    {
        var result = Build(new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Cab", ["ZipadeeMaxVolumeSize"] = "100000" });

        Assert.False(result.Succeeded);
        Assert.Contains("must be a multiple of 512 for the Cab format", result.Output);
    }

    [Theory]
    [InlineData("ZipadeeProjectOutputExclude")]
    [InlineData("ZipadeeProjectOutputInclude")]
    public void ProjectOutputPatternsCantContainABackslash(string propertyName)
    {
        var result = Build(new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Zip", [propertyName] = "sub\\*.dll" });

        Assert.False(result.Succeeded);
        Assert.Contains($"{propertyName} can't contain \\", result.Output);
    }

    [Fact]
    public void ProjectOutputIncludeWithoutExcludeIsAHarmlessNoOpWarning()
    {
        var result = Build(new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Zip", ["ZipadeeProjectOutputInclude"] = "extra.txt" });

        Assert.True(result.Succeeded, result.Output);
        Assert.Contains("ZipadeeProjectOutputInclude is set but ZipadeeProjectOutputExclude isn't", result.Output);
    }

    [Theory]
    [InlineData("ZipadeeArchiveDateFormat", "yyyy/MM/dd")]
    [InlineData("ZipadeeArchiveTimeFormat", "HH:mm:ss")]
    public void DateAndTimeFormatsCantContainACharacterWindowsDisallowsInFileNames(string propertyName, string badFormat)
    {
        var result = Build(new Dictionary<string, string> { ["ZipadeeOutputFormat"] = "Zip", [propertyName] = badFormat });

        Assert.False(result.Succeeded);
        Assert.Contains($"{propertyName} ('{badFormat}')", result.Output);
    }

    private BuildResult Build(IReadOnlyDictionary<string, string> properties)
    {
        var workspace = new FixtureWorkspace();
        try
        {
            return BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version, properties);
        }
        finally
        {
            workspace.Dispose();
        }
    }
}
